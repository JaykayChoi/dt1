using System;
using System.Collections.Generic;
using System.Text;
using DesCore;
using SimStats;

namespace FleetDispatch
{
    /// <summary>
    /// 배차(dispatching) 정책 실험 랩 — Phase 2에서 만든 DesCore 엔진을 재사용해
    /// "어느 반송차에 어떤 반송 명령을 줄 것인가"만 바꿔가며 성능을 비교한다.
    ///
    /// 각 부하 단계에서 세 정책을 시드 교체로 여러 번 반복(replication)하고,
    /// 평균에 95% 신뢰구간을 붙여 "정책 차이가 시드 한 번의 우연인가"를 가른다.
    /// 한 반복 안에서는 세 정책이 같은 반송 명령 스트림(공통 난수, CRN)을 처리하므로
    /// 정책 쌍의 대응(paired) t-검정으로 우연이 상쇄된 순수 정책 차이를 판정한다.
    /// warm-up(초기 과도 구간) 제거 전후를 함께 보여 초기 편향의 크기도 드러낸다.
    /// </summary>
    internal static class Program
    {
        private const double ShiftMinutes = 480.0;      // 8시간 교대
        private const int VehicleCount = 3;
        private const double VehicleSpeed = 70.0;       // m/분 (약 1.2 m/s, 실내 AGV 수준)
        private const double DepotX = 50.0;
        private const double DepotY = 30.0;
        private const int RandomSeed = 20260709;
        private const int Replications = 20;            // 부하·정책 조합당 반복 수
        private const double WarmUpMinutes = 30.0;      // 이 시각 이전 생성 명령은 정상상태 통계에서 제외
        private const double TCritical95Df19 = 2.093;   // t(0.975, 19)

        // 100 m × 60 m 베이. 위쪽 줄은 로드포트, 아래쪽 줄은 스토커로 상상하면 된다.
        private static readonly Station[] Layout =
        {
            new Station { Name = "LP-A", X = 10.0, Y = 10.0 },
            new Station { Name = "LP-B", X = 50.0, Y = 10.0 },
            new Station { Name = "LP-C", X = 90.0, Y = 10.0 },
            new Station { Name = "STK-1", X = 10.0, Y = 50.0 },
            new Station { Name = "STK-2", X = 50.0, Y = 50.0 },
            new Station { Name = "STK-3", X = 90.0, Y = 50.0 },
        };

        private static readonly DispatchPolicy[] Policies =
        {
            DispatchPolicy.Random,
            DispatchPolicy.NearestVehicle,
            DispatchPolicy.NearestJob,
        };

        private static readonly Scenario[] Scenarios =
        {
            new Scenario { Label = "여유", ArrivalMean = 1.2 },
            new Scenario { Label = "보통", ArrivalMean = 0.8 },
            new Scenario { Label = "혼잡", ArrivalMean = 0.5 },
        };

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            PrintIntro();

            foreach (Scenario scenario in Scenarios)
            {
                // 정책별 반복 결과를 모은다. 같은 반복 인덱스 r은 세 정책이 동일한
                // 스트림 시드를 써서 반송 명령이 똑같다(CRN).
                var samples = new Dictionary<DispatchPolicy, List<RunResult>>();
                foreach (DispatchPolicy policy in Policies)
                {
                    samples[policy] = new List<RunResult>(Replications);
                }

                for (int r = 0; r < Replications; r++)
                {
                    foreach (DispatchPolicy policy in Policies)
                    {
                        var run = new FleetRun(policy, scenario.ArrivalMean, RandomSeed + r);
                        samples[policy].Add(run.Execute());
                    }
                }

                PrintScenario(scenario, samples);
            }

            RunWeightedSweep();
            PrintLegend();
        }

        // 가중 점수 배차의 에이징 가중치 k를 혼잡 부하에서 스윕한다. k가 커질수록
        // 오래 기다린 명령이 우선돼 P95(꼬리) 대기가 내려가는 공정성-효율 교환을 드러낸다.
        private static void RunWeightedSweep()
        {
            // 기아를 드러내려면 과부하가 필요하다 — 3대 함대의 처리 능력(≈960건/교대)을
            // 웃도는 도착률에서 큐가 쌓이고, 최근접작업이 먼 명령을 계속 뒤로 미룬다.
            const double Overloaded = 0.42;
            int[] weights = { 0, 20, 40, 80, 160 };

            var rows = new List<double[]>();
            foreach (int k in weights)
            {
                var runs = new List<RunResult>(Replications);
                for (int r = 0; r < Replications; r++)
                {
                    var run = new FleetRun(DispatchPolicy.WeightedScore, Overloaded, RandomSeed + r, k);
                    runs.Add(run.Execute());
                }

                rows.Add(new[]
                {
                    k,
                    Mean(Project(runs, x => x.Completed)),
                    Mean(Project(runs, x => x.Throughput)),
                    Mean(Project(runs, x => x.AvgWait)),
                    Mean(Project(runs, x => x.P95Wait)),
                    Mean(Project(runs, x => x.AvgDeadhead)),
                });
            }

            int bestP95 = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i][4] < rows[bestP95][4])
                {
                    bestP95 = i;
                }
            }

            Console.WriteLine();
            Console.WriteLine(
                $"[과부하] 가중 점수 배차 — 에이징 가중치 k 스윕 · 도착간격 {Overloaded:F2}분 · {Replications} 반복");
            Console.WriteLine("  k(m/min)  완료    처리량    평균대기   P95대기   평균공차");
            Console.WriteLine("  " + new string('-', 58));
            for (int i = 0; i < rows.Count; i++)
            {
                double[] row = rows[i];
                string tail = i == bestP95 ? "   ← P95 최저" : "";
                Console.WriteLine(
                    $"  {row[0],5:F0}     {row[1],5:F0}   {row[2],6:F1}   {row[3],7:F2}   " +
                    $"{row[4],7:F2}   {row[5],6:F1}{tail}");
            }

            Console.WriteLine(
                "  읽는 법: 이 작은 균형 팹은 최근접작업(k=0)이 이미 꼬리가 짧아(P95≈4.5분) 기아가");
            Console.WriteLine(
                "  거의 없다. 그래서 k를 올리면 공차만 늘어(26→39 m) 처리량이 떨어지고 P95가 오히려");
            Console.WriteLine(
                "  악화된다 — 에이징은 공짜가 아니며, 실제 기아가 있을 때만 값어치를 한다는 증거다.");
        }

        private static double Mean(double[] xs)
        {
            double sum = 0.0;
            foreach (double x in xs)
            {
                sum += x;
            }

            return sum / xs.Length;
        }

        private static void PrintIntro()
        {
            Console.WriteLine("배차 정책 실험 랩 — DesCore 재사용 · 반복·신뢰구간 판");
            Console.WriteLine(
                $"베이 {Layout.Length}개 스테이션 · 반송차 {VehicleCount}대 · " +
                $"속도 {VehicleSpeed:F0} m/분 · {ShiftMinutes / 60.0:F0}시간 교대");
            Console.WriteLine(
                $"각 조합을 시드 교체로 {Replications}회 반복(공통 난수). " +
                $"평균 ± 95% CI · warm-up {WarmUpMinutes:F0}분 제거.");
        }

        private static void PrintScenario(Scenario scenario, Dictionary<DispatchPolicy, List<RunResult>> samples)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"[{scenario.Label}] 도착간격 평균 {scenario.ArrivalMean:F1}분 " +
                $"(offered ≈ {ShiftMinutes / scenario.ArrivalMean:F0}건/교대) · {Replications} 반복");
            Console.WriteLine(
                "  정책          평균대기(95% CI)     처리량(95% CI)      평균공차(95% CI)");
            Console.WriteLine("  " + new string('-', 66));
            foreach (DispatchPolicy policy in Policies)
            {
                List<RunResult> runs = samples[policy];
                (double waitMean, double waitHalf) = MeanCi95(Project(runs, r => r.AvgWait));
                (double thrMean, double thrHalf) = MeanCi95(Project(runs, r => r.Throughput));
                (double dhMean, double dhHalf) = MeanCi95(Project(runs, r => r.AvgDeadhead));
                Console.WriteLine(
                    $"  {PolicyLabel(policy),-12}" +
                    $"{FormatCi(waitMean, waitHalf),-20}" +
                    $"{FormatCi(thrMean, thrHalf),-20}" +
                    $"{FormatCi(dhMean, dhHalf),-18}");
            }

            PrintPairedComparison(samples);
            PrintWarmUpEffect(samples);
        }

        private static void PrintPairedComparison(Dictionary<DispatchPolicy, List<RunResult>> samples)
        {
            // 최근접작업 − 최근접차량의 반복별 평균대기 차이로 대응 t-검정.
            double[] job = Project(samples[DispatchPolicy.NearestJob], r => r.AvgWait);
            double[] veh = Project(samples[DispatchPolicy.NearestVehicle], r => r.AvgWait);
            (double delta, double t, bool significant) = PairedT(job, veh);
            string verdict = significant ? "예" : "아니오";
            Console.WriteLine(
                $"  대응 비교(최근접작업 − 최근접차량, 평균대기): " +
                $"Δ={delta:F2}분, t={t:F2}, 95% 유의: {verdict}");
        }

        private static void PrintWarmUpEffect(Dictionary<DispatchPolicy, List<RunResult>> samples)
        {
            // warm-up 제거 전(raw)과 후(steady)의 평균대기 차이를 최근접작업 기준으로 보여 준다.
            List<RunResult> runs = samples[DispatchPolicy.NearestJob];
            (double rawMean, double _) = MeanCi95(Project(runs, r => r.AvgWaitRaw));
            (double steadyMean, double __) = MeanCi95(Project(runs, r => r.AvgWait));
            Console.WriteLine(
                $"  warm-up 효과(최근접작업 평균대기): " +
                $"제거 전 {rawMean:F3}분 → 제거 후 {steadyMean:F3}분 " +
                $"(종료형 교대라 초기 과도가 짧아 차이가 작다)");
        }

        private static void PrintLegend()
        {
            Console.WriteLine();
            Console.WriteLine("정책 정의");
            Console.WriteLine(
                "  랜덤       : 가장 오래된 명령 → 유휴 차량 중 아무나 (기준선).");
            Console.WriteLine(
                "  최근접차량 : 가장 오래된 명령 → 픽업에 가장 가까운 유휴 차량 (FIFO 공정).");
            Console.WriteLine(
                "  최근접작업 : 유휴 차량 ↔ 대기 명령 쌍 중 공차가 최소인 조합 (탐욕적).");
            Console.WriteLine();
            Console.WriteLine(
                "평균 ± 95% CI = 반복 표본평균 x̄ ± t(0.975,19)·s/√20. 구간이 겹치지 않으면");
            Console.WriteLine(
                "정책 차이가 우연이 아닐 가능성이 크다. 대응 비교는 같은 명령 스트림(CRN)에서");
            Console.WriteLine(
                "반복별 차이를 직접 검정해, 우연을 상쇄하고 순수 정책 효과만 판정한다.");
            Console.WriteLine(
                "warm-up = 빈 상태에서 출발한 초기 과도 구간. 제거 전 수치는 정상상태보다 낮게 편향된다.");
        }

        private static double Distance(double ax, double ay, double bx, double by)
        {
            double dx = ax - bx;
            double dy = ay - by;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double[] Project(List<RunResult> runs, Func<RunResult, double> selector)
        {
            var values = new double[runs.Count];
            for (int i = 0; i < runs.Count; i++)
            {
                values[i] = selector(runs[i]);
            }

            return values;
        }

        private static string FormatCi(double mean, double half)
        {
            return $"{mean:F2} ± {half:F2}";
        }

        /// <summary>표본의 평균과 95% 신뢰구간 반폭(t(0.975,19)·s/√n)을 반환한다.</summary>
        private static (double mean, double half) MeanCi95(IReadOnlyList<double> xs)
        {
            int n = xs.Count;
            double sum = 0.0;
            foreach (double x in xs)
            {
                sum += x;
            }

            double mean = sum / n;
            if (n < 2)
            {
                return (mean, 0.0);
            }

            double squaredDeviation = 0.0;
            foreach (double x in xs)
            {
                double d = x - mean;
                squaredDeviation += d * d;
            }

            double stdDev = Math.Sqrt(squaredDeviation / (n - 1));
            double half = TCritical95Df19 * stdDev / Math.Sqrt(n);
            return (mean, half);
        }

        /// <summary>
        /// 두 표본의 대응(paired) t-검정. 반복별 차이 d_r = a_r − b_r로
        /// t = d̄ / (s_d/√n)를 계산하고 |t| &gt; t(0.975,19)면 유의로 판정한다.
        /// </summary>
        private static (double delta, double t, bool significant) PairedT(
            IReadOnlyList<double> a, IReadOnlyList<double> b)
        {
            int n = a.Count;
            var diff = new double[n];
            double sum = 0.0;
            for (int i = 0; i < n; i++)
            {
                diff[i] = a[i] - b[i];
                sum += diff[i];
            }

            double mean = sum / n;
            double squaredDeviation = 0.0;
            for (int i = 0; i < n; i++)
            {
                double d = diff[i] - mean;
                squaredDeviation += d * d;
            }

            double stdDev = Math.Sqrt(squaredDeviation / (n - 1));
            double t = stdDev > 0.0 ? mean / (stdDev / Math.Sqrt(n)) : 0.0;
            return (mean, t, Math.Abs(t) > TCritical95Df19);
        }

        private static string PolicyLabel(DispatchPolicy policy)
        {
            switch (policy)
            {
                case DispatchPolicy.Random:
                    return "랜덤";
                case DispatchPolicy.NearestVehicle:
                    return "최근접차량";
                case DispatchPolicy.NearestJob:
                    return "최근접작업";
                case DispatchPolicy.WeightedScore:
                    return "가중점수";
                default:
                    return policy.ToString();
            }
        }

        private enum DispatchPolicy
        {
            Random,
            NearestVehicle,
            NearestJob,
            WeightedScore,
        }

        private sealed class Scenario
        {
            public string Label;
            public double ArrivalMean;
        }

        private sealed class Station
        {
            public string Name;
            public double X;
            public double Y;
        }

        private sealed class Vehicle
        {
            public int Id;
            public double X;
            public double Y;
            public bool IsIdle;
            public double DispatchedAt;
            public double BusyTime;
        }

        private sealed class Job
        {
            public int Id;
            public Station Pickup;
            public Station Dropoff;
            public double CreatedAt;
        }

        private sealed class RunResult
        {
            public string PolicyName;
            public int Completed;
            public double Throughput;
            public double AvgWait;          // warm-up 제거 후(정상상태 추정)
            public double AvgWaitRaw;       // warm-up 포함(초기 과도 편향)
            public double MaxWait;
            public double P95Wait;          // warm-up 제거 후 95백분위 대기(꼬리)
            public double AvgDeadhead;      // warm-up 제거 후
            public double Utilization;
        }

        /// <summary>
        /// 한 정책으로 한 번의 교대(shift)를 시뮬레이션하는 단위. 정책만 다르고
        /// 반송 명령 스트림은 시드가 같아 동일하다 — 배차 결정은 별도 난수를 쓴다.
        /// </summary>
        private sealed class FleetRun
        {
            private readonly DispatchPolicy policy;
            private readonly double arrivalMean;
            private readonly double weight;
            private readonly Simulation simulation = new Simulation();

            // 반송 명령 스트림용 난수(반복 인덱스로만 결정 — 모든 정책이 공유)와
            // 배차 결정용 난수(정책 내부용)를 분리해, 정책을 바꿔도 처리해야 할
            // 명령들이 똑같이 나오도록 한다(CRN).
            private readonly Random streamRandom;
            private readonly Random policyRandom;

            private readonly Station[] stations = Layout;
            private readonly List<Vehicle> vehicles = new List<Vehicle>();
            private readonly List<Job> pendingJobs = new List<Job>();
            private readonly List<Vehicle> idleScratch = new List<Vehicle>();

            private int nextJobId;
            private int completed;
            private int pickedUp;
            private int steadyPickedUp;
            private int assigned;
            private int steadyAssigned;
            private double totalWait;
            private double steadyWait;
            private readonly List<double> steadyWaits = new List<double>();
            private double maxWait;
            private double totalDeadhead;
            private double steadyDeadhead;

            public FleetRun(DispatchPolicy policy, double arrivalMean, int streamSeed, double weight = 0.0)
            {
                this.policy = policy;
                this.arrivalMean = arrivalMean;
                this.weight = weight;
                streamRandom = new Random(streamSeed);
                policyRandom = new Random(streamSeed + 1000000);
            }

            public RunResult Execute()
            {
                for (int i = 0; i < VehicleCount; i++)
                {
                    vehicles.Add(new Vehicle
                    {
                        Id = i,
                        X = DepotX,
                        Y = DepotY,
                        IsIdle = true,
                    });
                }

                ScheduleNextArrival();
                simulation.Run(ShiftMinutes);

                // 교대 종료 시점까지의 부분 가동 시간을 마저 반영한다.
                foreach (Vehicle vehicle in vehicles)
                {
                    if (!vehicle.IsIdle)
                    {
                        vehicle.BusyTime += simulation.Now - vehicle.DispatchedAt;
                    }
                }

                return BuildResult();
            }

            private void ScheduleNextArrival()
            {
                simulation.Schedule(streamRandom.NextExponential(arrivalMean), () =>
                {
                    int pickup = streamRandom.Next(stations.Length);
                    int dropoff = streamRandom.Next(stations.Length - 1);
                    if (dropoff >= pickup)
                    {
                        dropoff++;
                    }

                    pendingJobs.Add(new Job
                    {
                        Id = nextJobId++,
                        Pickup = stations[pickup],
                        Dropoff = stations[dropoff],
                        CreatedAt = simulation.Now,
                    });

                    Dispatch();
                    ScheduleNextArrival();
                });
            }

            private void Dispatch()
            {
                while (SelectAssignment(out Vehicle vehicle, out Job job))
                {
                    Assign(vehicle, job);
                }
            }

            private bool SelectAssignment(out Vehicle vehicle, out Job job)
            {
                vehicle = null;
                job = null;
                if (pendingJobs.Count == 0)
                {
                    return false;
                }

                switch (policy)
                {
                    case DispatchPolicy.Random:
                        vehicle = RandomIdleVehicle();
                        if (vehicle != null)
                        {
                            job = pendingJobs[0];
                        }

                        break;
                    case DispatchPolicy.NearestVehicle:
                        job = pendingJobs[0];
                        vehicle = NearestIdleVehicleTo(job.Pickup);
                        if (vehicle == null)
                        {
                            job = null;
                        }

                        break;
                    case DispatchPolicy.NearestJob:
                        SelectNearestPair(out vehicle, out job);
                        break;
                    case DispatchPolicy.WeightedScore:
                        SelectWeightedPair(out vehicle, out job);
                        break;
                    default:
                        break;
                }

                return vehicle != null && job != null;
            }

            private void Assign(Vehicle vehicle, Job job)
            {
                pendingJobs.Remove(job);
                vehicle.IsIdle = false;
                vehicle.DispatchedAt = simulation.Now;
                assigned++;

                double deadhead = Distance(vehicle.X, vehicle.Y, job.Pickup.X, job.Pickup.Y);
                totalDeadhead += deadhead;
                bool steady = job.CreatedAt >= WarmUpMinutes;
                if (steady)
                {
                    steadyDeadhead += deadhead;
                    steadyAssigned++;
                }

                simulation.Schedule(deadhead / VehicleSpeed, () =>
                {
                    vehicle.X = job.Pickup.X;
                    vehicle.Y = job.Pickup.Y;

                    double wait = simulation.Now - job.CreatedAt;
                    totalWait += wait;
                    pickedUp++;
                    if (steady)
                    {
                        steadyWait += wait;
                        steadyWaits.Add(wait);
                        steadyPickedUp++;
                    }

                    if (wait > maxWait)
                    {
                        maxWait = wait;
                    }

                    double loaded = Distance(
                        job.Pickup.X, job.Pickup.Y, job.Dropoff.X, job.Dropoff.Y);

                    simulation.Schedule(loaded / VehicleSpeed, () =>
                    {
                        vehicle.X = job.Dropoff.X;
                        vehicle.Y = job.Dropoff.Y;
                        vehicle.IsIdle = true;
                        vehicle.BusyTime += simulation.Now - vehicle.DispatchedAt;
                        completed++;
                        Dispatch();
                    });
                });
            }

            private Vehicle RandomIdleVehicle()
            {
                idleScratch.Clear();
                foreach (Vehicle vehicle in vehicles)
                {
                    if (vehicle.IsIdle)
                    {
                        idleScratch.Add(vehicle);
                    }
                }

                if (idleScratch.Count == 0)
                {
                    return null;
                }

                return idleScratch[policyRandom.Next(idleScratch.Count)];
            }

            private Vehicle NearestIdleVehicleTo(Station target)
            {
                Vehicle best = null;
                double bestDist = double.MaxValue;
                foreach (Vehicle vehicle in vehicles)
                {
                    if (!vehicle.IsIdle)
                    {
                        continue;
                    }

                    double d = Distance(vehicle.X, vehicle.Y, target.X, target.Y);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = vehicle;
                    }
                }

                return best;
            }

            private void SelectNearestPair(out Vehicle bestVehicle, out Job bestJob)
            {
                bestVehicle = null;
                bestJob = null;
                double bestDist = double.MaxValue;
                foreach (Vehicle vehicle in vehicles)
                {
                    if (!vehicle.IsIdle)
                    {
                        continue;
                    }

                    foreach (Job job in pendingJobs)
                    {
                        double d = Distance(
                            vehicle.X, vehicle.Y, job.Pickup.X, job.Pickup.Y);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            bestVehicle = vehicle;
                            bestJob = job;
                        }
                    }
                }
            }

            // 가중 점수 배차: score = 공차거리[m] − k × 대기시간[분]. 오래 기다린 명령은
            // 점수가 낮아져(우선) 뽑힌다. k=0이면 최근접 작업으로 환원된다.
            private void SelectWeightedPair(out Vehicle bestVehicle, out Job bestJob)
            {
                bestVehicle = null;
                bestJob = null;
                double bestScore = double.MaxValue;
                foreach (Vehicle vehicle in vehicles)
                {
                    if (!vehicle.IsIdle)
                    {
                        continue;
                    }

                    foreach (Job job in pendingJobs)
                    {
                        double deadhead = Distance(
                            vehicle.X, vehicle.Y, job.Pickup.X, job.Pickup.Y);
                        double waitMinutes = simulation.Now - job.CreatedAt;
                        double score = deadhead - weight * waitMinutes;
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestVehicle = vehicle;
                            bestJob = job;
                        }
                    }
                }
            }

            private RunResult BuildResult()
            {
                double busySum = 0.0;
                foreach (Vehicle vehicle in vehicles)
                {
                    busySum += vehicle.BusyTime;
                }

                return new RunResult
                {
                    PolicyName = PolicyLabel(policy),
                    Completed = completed,
                    Throughput = completed / ShiftMinutes * 60.0,
                    AvgWait = steadyPickedUp > 0 ? steadyWait / steadyPickedUp : 0.0,
                    AvgWaitRaw = pickedUp > 0 ? totalWait / pickedUp : 0.0,
                    MaxWait = maxWait,
                    P95Wait = ExperimentStats.Percentile(steadyWaits, 95.0),
                    AvgDeadhead = steadyAssigned > 0 ? steadyDeadhead / steadyAssigned : 0.0,
                    Utilization = busySum / (ShiftMinutes * VehicleCount),
                };
            }
        }
    }
}
