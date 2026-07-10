using System;
using System.Collections.Generic;
using System.Text;
using DesCore;
using SimStats;

namespace FleetTraffic
{
    /// <summary>
    /// 교통 제어와 데드락 실험 랩 — DesCore 위에서 구간 점유(zone control)가 처리량을
    /// 어떻게 바꾸는지(실험 A)와, 양방향 corridor가 데드락을 어떻게 만들고 탐지·해소되는지
    /// (실험 B)를 본다. 실험 A는 Phase 6 방식(20회 반복, 평균 ± 95% CI)으로 보고한다.
    /// </summary>
    internal static class Program
    {
        private const int LoopLen = 16;
        private const double Speed = 2.5;           // m/s
        private const double JobIntervalMean = 8.0; // s (고부하)
        private const double ShiftSeconds = 3600.0;
        private const int Replications = 20;
        private const int BaseSeed = 20260709;
        private static readonly int[] VehicleCounts = { 2, 4, 8, 12 };
        private static readonly int[] Ports = { 0, 3, 6, 8, 11, 13 };

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("교통 제어·데드락 실험 랩 — DesCore 재사용");
            Console.WriteLine(
                $"단일 방향 루프 {LoopLen}구간 · 포트 {Ports.Length}개 · 속도 {Speed:F1} m/s · " +
                $"{ShiftSeconds / 60:F0}분 · {Replications}회 반복");

            RunExperimentA(true);
            RunExperimentA(false);
            RunExperimentB();
        }

        private static void RunExperimentA(bool useControl)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"[교통 제어 {(useControl ? "ON — 한 구간에 한 대" : "OFF — 차량이 통과")}] " +
                $"{ShiftSeconds:F0}s × {Replications}회 반복");
            Console.WriteLine("  차량수   처리량(건/h)          평균반송(s)");
            Console.WriteLine("  " + new string('-', 48));

            foreach (int vehicleCount in VehicleCounts)
            {
                var throughput = new List<double>(Replications);
                var delivery = new List<double>(Replications);
                for (int r = 0; r < Replications; r++)
                {
                    RailGraph graph = BuildLoop();
                    var model = new TrafficModel(
                        graph, LoopLen, Ports, vehicleCount, Speed, JobIntervalMean,
                        useControl, BaseSeed + r);
                    model.Start();
                    model.Simulation.Run(ShiftSeconds);
                    throughput.Add(model.ThroughputPerHour(ShiftSeconds));
                    delivery.Add(model.AverageDeliveryTime());
                }

                ExperimentStats.Summary th = ExperimentStats.Summarize(throughput);
                ExperimentStats.Summary dl = ExperimentStats.Summarize(delivery);
                Console.WriteLine(
                    $"  {vehicleCount,4}     {th.Format("F1"),-20} {dl.Format("F1"),-16}");
            }
        }

        private static void RunExperimentB()
        {
            Console.WriteLine();
            Console.WriteLine("[데드락 실험] 양방향 corridor(2구간) · 마주 오는 차량 2대");
            Console.WriteLine("  구성                     데드락 발생   최초 탐지 시각   관련 차량");
            Console.WriteLine("  " + new string('-', 62));
            PrintDeadlockRow("양방향(무대책)", Resolution.None);
            PrintDeadlockRow("단방향화", Resolution.Unidirectional);
            PrintDeadlockRow("자원 순서화", Resolution.ResourceOrdering);

            Console.WriteLine();
            Console.WriteLine("정체 ≠ 데드락 — 정체는 느려진 것, 데드락은 멈춘 것이다.");
            Console.WriteLine(
                "실험 A: 교통 제어 ON은 차량이 늘수록 처리량이 오르다 꺾인다(구간 대기). OFF는");
            Console.WriteLine(
                "차량이 서로 통과해 단조 증가한다. 실험 B: 마주 오는 두 차량이 서로의 구간을 쥔 채");
            Console.WriteLine(
                "상대 구간을 기다리면(순환 대기) 데드락 — 단방향화·자원 순서화가 순환을 끊는다.");
        }

        private static void PrintDeadlockRow(string label, Resolution resolution)
        {
            (bool deadlock, double time, IReadOnlyList<int> vehicles) = RunDeadlockScenario(resolution);
            string occurred = deadlock ? "예" : "아니오";
            string timeText = deadlock ? $"t={time:F1} s" : "—";
            string vehicleText = deadlock ? "[" + string.Join(", ", vehicles) + "]" : "—";
            Console.WriteLine($"  {label,-20}     {occurred,-10}    {timeText,-14}   {vehicleText}");
        }

        private enum Resolution
        {
            None,
            Unidirectional,
            ResourceOrdering,
        }

        /// <summary>
        /// 두 구간(seg0, seg1)으로 된 단선 corridor에서 마주 오는 두 차량을 태워
        /// 데드락 발생·탐지 여부를 확인한다. 각 차량은 hold-and-wait로 구간을 순서대로
        /// 획득한다(현재 구간을 쥔 채 다음 구간 예약).
        /// </summary>
        private static (bool, double, IReadOnlyList<int>) RunDeadlockScenario(Resolution resolution)
        {
            var sim = new Simulation();
            var seg = new SegmentController();
            const double Travel = 4.0;

            // 물리적 진행 방향: 차량0은 seg0→seg1, 차량1은 seg1→seg0.
            int[] order0 = { 0, 1 };
            int[] order1 = resolution == Resolution.None ? new[] { 1, 0 } : new[] { 0, 1 };
            // 단방향화: 두 차량을 같은 방향(0→1)으로. 자원 순서화: 방향과 무관하게 id 오름차순 획득.
            // 둘 다 결과적으로 order [0,1]이라 교차 점유가 생기지 않는다.

            RunCorridorVehicle(sim, seg, 0, order0, 0, Travel);
            RunCorridorVehicle(sim, seg, 1, order1, 0, Travel);

            double detectedAt = -1.0;
            IReadOnlyList<int> cycle = Array.Empty<int>();
            for (double t = 0.5; t <= 60.0; t += 0.5)
            {
                sim.Run(t);
                IReadOnlyList<int> found = seg.DetectDeadlockCycle();
                if (found.Count > 0 && detectedAt < 0.0)
                {
                    detectedAt = sim.Now;
                    cycle = found;
                    break;
                }
            }

            return (detectedAt >= 0.0, detectedAt, cycle);
        }

        // hold-and-wait: order[idx] 구간을 획득하면(이전 구간을 쥔 채) 이동, 다음 구간 요청.
        private static void RunCorridorVehicle(
            Simulation sim, SegmentController seg, int vehicleId, int[] order, int idx, double travel)
        {
            if (idx >= order.Length)
            {
                foreach (int s in order)
                {
                    seg.Release(s, vehicleId);
                }

                return;
            }

            seg.Acquire(order[idx], vehicleId, () =>
            {
                sim.Schedule(travel, () => RunCorridorVehicle(sim, seg, vehicleId, order, idx + 1, travel));
            });
        }

        // 16노드를 원 위에 균등 배치해 인접 구간 길이가 10 m가 되게 한다.
        private static RailGraph BuildLoop()
        {
            var graph = new RailGraph();
            double radius = 10.0 / (2.0 * Math.Sin(Math.PI / LoopLen));
            for (int i = 0; i < LoopLen; i++)
            {
                double angle = 2.0 * Math.PI * i / LoopLen;
                graph.AddNode((float)(radius * Math.Cos(angle)), 0f, (float)(radius * Math.Sin(angle)));
            }

            for (int i = 0; i < LoopLen; i++)
            {
                graph.AddEdge(i, (i + 1) % LoopLen);
            }

            return graph;
        }
    }
}
