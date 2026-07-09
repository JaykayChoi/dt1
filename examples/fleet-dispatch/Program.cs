using System;
using System.Collections.Generic;
using System.Text;
using DesCore;

namespace FleetDispatch
{
    /// <summary>
    /// 배차(dispatching) 정책 실험 랩 — Phase 2에서 만든 DesCore 엔진을 재사용해
    /// "어느 반송차에 어떤 반송 명령을 줄 것인가"만 바꿔가며 성능을 비교한다.
    ///
    /// 같은 반송 명령 스트림(도착 시각·픽업/드롭 위치가 동일)을 세 정책에 똑같이
    /// 흘려보내고, 처리량·대기·공차주행(deadhead)·가동률을 표로 뽑는다. 부하를
    /// 세 단계로 스윕해, 팹 Fleet Management의 핵심 트레이드오프가 부하에 따라
    /// 어떻게 달라지는지를 드러낸다 — 여유일 땐 공차, 혼잡일 땐 큐 재정렬이 승부처다.
    /// </summary>
    internal static class Program
    {
        private const double ShiftMinutes = 480.0;      // 8시간 교대
        private const int VehicleCount = 3;
        private const double VehicleSpeed = 70.0;       // m/분 (약 1.2 m/s, 실내 AGV 수준)
        private const double DepotX = 50.0;
        private const double DepotY = 30.0;
        private const int RandomSeed = 20260709;

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
                var results = new List<RunResult>();
                foreach (DispatchPolicy policy in Policies)
                {
                    var run = new FleetRun(policy, scenario.ArrivalMean);
                    results.Add(run.Execute());
                }

                PrintScenario(scenario, results);
            }

            PrintLegend();
        }

        private static void PrintIntro()
        {
            Console.WriteLine("배차 정책 실험 랩 — DesCore 재사용");
            Console.WriteLine(
                $"베이 {Layout.Length}개 스테이션 · 반송차 {VehicleCount}대 · " +
                $"속도 {VehicleSpeed:F0} m/분 · {ShiftMinutes / 60.0:F0}시간 교대");
            Console.WriteLine("각 부하 단계에서 세 정책은 동일한 반송 명령 스트림(같은 시드)을 처리한다.");
        }

        private static void PrintScenario(Scenario scenario, List<RunResult> results)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"[{scenario.Label}] 도착간격 평균 {scenario.ArrivalMean:F1}분 " +
                $"(offered ≈ {ShiftMinutes / scenario.ArrivalMean:F0}건/교대)");
            Console.WriteLine(
                "  정책          완료   처리량   평균대기  최대대기  평균공차  가동률");
            Console.WriteLine("  " + new string('-', 60));
            foreach (RunResult r in results)
            {
                Console.WriteLine(
                    $"  {r.PolicyName,-12}{r.Completed,5}{r.Throughput,9:F1}" +
                    $"{r.AvgWait,10:F2}{r.MaxWait,10:F1}{r.AvgDeadhead,10:F1}" +
                    $"{r.Utilization,9:F3}");
            }
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
                "공차(deadhead) = 명령을 받은 차량이 짐 없이 픽업 지점까지 가는 빈 주행[m].");
            Console.WriteLine(
                "여유 부하에선 유휴 차량이 많아 '최근접차량'이 공차를 줄이고, 혼잡 부하에선");
            Console.WriteLine(
                "고를 차량이 없어 큐를 재정렬하는 '최근접작업'이 대기·처리량에서 앞선다.");
        }

        private static double Distance(double ax, double ay, double bx, double by)
        {
            double dx = ax - bx;
            double dy = ay - by;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private enum DispatchPolicy
        {
            Random,
            NearestVehicle,
            NearestJob,
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
            public double AvgWait;
            public double MaxWait;
            public double AvgDeadhead;
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
            private readonly Simulation simulation = new Simulation();

            // 반송 명령 스트림용 난수(모든 정책이 공유)와 배차 결정용 난수(정책 내부용)를
            // 분리해, 정책을 바꿔도 처리해야 할 명령들이 똑같이 나오도록 한다.
            private readonly Random streamRandom = new Random(RandomSeed);
            private readonly Random policyRandom = new Random(RandomSeed + 1);

            private readonly Station[] stations = Layout;
            private readonly List<Vehicle> vehicles = new List<Vehicle>();
            private readonly List<Job> pendingJobs = new List<Job>();
            private readonly List<Vehicle> idleScratch = new List<Vehicle>();

            private int nextJobId;
            private int completed;
            private int pickedUp;
            private int assigned;
            private double totalWait;
            private double maxWait;
            private double totalDeadhead;

            public FleetRun(DispatchPolicy policy, double arrivalMean)
            {
                this.policy = policy;
                this.arrivalMean = arrivalMean;
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

                simulation.Schedule(deadhead / VehicleSpeed, () =>
                {
                    vehicle.X = job.Pickup.X;
                    vehicle.Y = job.Pickup.Y;

                    double wait = simulation.Now - job.CreatedAt;
                    totalWait += wait;
                    pickedUp++;
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
                    AvgWait = pickedUp > 0 ? totalWait / pickedUp : 0.0,
                    MaxWait = maxWait,
                    AvgDeadhead = assigned > 0 ? totalDeadhead / assigned : 0.0,
                    Utilization = busySum / (ShiftMinutes * VehicleCount),
                };
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
                    default:
                        return policy.ToString();
                }
            }
        }
    }
}
