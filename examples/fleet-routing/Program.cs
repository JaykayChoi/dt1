using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SimStats;

namespace FleetRouting
{
    /// <summary>
    /// 경로 탐색 심화 랩 — 다익스트라 vs A*의 효율 비교(실험 A)와, 정적 최단 경로 vs
    /// 정체 인지 동적 경로의 반송시간 비교(실험 B). 6×6 격자에서 돈다. 실험 B는 Phase 6
    /// 방식(20회 반복, 평균 ± 95% CI)으로 보고한다.
    /// </summary>
    internal static class Program
    {
        private const int GridSize = 6;
        private const double Spacing = 10.0;
        private const double Speed = 2.5;               // m/s
        private const int PairSamples = 200;            // 실험 A 표본 쌍 수
        private const int VehiclesPerRep = 60;          // 실험 B 수요 차량 수
        private const int Replications = 20;
        private const int BaseSeed = 20260709;
        private const double CongestionK = 0.15;        // 정체 페널티 계수

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("경로 탐색 심화 랩 — 다익스트라 vs A*, 정적 vs 정체 인지 라우팅");
            Console.WriteLine(
                $"{GridSize}×{GridSize} 격자 · 노드 {GridSize * GridSize}개 · 간격 {Spacing:F0} m");

            RunExperimentA();
            RunExperimentB();

            Console.WriteLine();
            Console.WriteLine(
                "A*는 목표까지의 직선거리(허용 가능 휴리스틱)로 탐색을 목표 쪽으로 당겨, 다익스트라와");
            Console.WriteLine(
                "같은 최단 경로를 더 적은 확장으로 찾는다. 정적 최단은 모두를 같은 길로 몰아 정체를");
            Console.WriteLine(
                "키우고, 정체를 엣지 비용에 실은 동적 라우팅은 부하를 분산해 평균 반송시간을 줄인다.");
        }

        private static void RunExperimentA()
        {
            RailGraph graph = BuildGrid();
            var random = new Random(BaseSeed);

            var dijkstraExpanded = new List<double>(PairSamples);
            var astarExpanded = new List<double>(PairSamples);
            int pathMatches = 0;

            // 계산 시간은 표본 전체를 합산해 쌍당 평균으로 환산한다(워밍업 1회 후 측정).
            WarmUp(graph);
            var dijkstraWatch = new Stopwatch();
            var astarWatch = new Stopwatch();

            for (int i = 0; i < PairSamples; i++)
            {
                int start = random.Next(graph.NodeCount);
                int goal = random.Next(graph.NodeCount);
                while (goal == start)
                {
                    goal = random.Next(graph.NodeCount);
                }

                dijkstraWatch.Start();
                List<int> pathD = graph.FindPath(start, goal, null, out int expD);
                dijkstraWatch.Stop();

                astarWatch.Start();
                List<int> pathA = graph.FindPathAStar(start, goal, graph.GetDistance, out int expA);
                astarWatch.Stop();

                dijkstraExpanded.Add(expD);
                astarExpanded.Add(expA);
                if (Math.Abs(graph.GetPathLength(pathD) - graph.GetPathLength(pathA)) < 1e-3f)
                {
                    pathMatches++;
                }
            }

            double dijkstraMicros = dijkstraWatch.Elapsed.TotalMilliseconds * 1000.0 / PairSamples;
            double astarMicros = astarWatch.Elapsed.TotalMilliseconds * 1000.0 / PairSamples;
            double matchPercent = 100.0 * pathMatches / PairSamples;

            Console.WriteLine();
            Console.WriteLine($"[다익스트라 vs A*] {GridSize}×{GridSize} 격자 · 표본 {PairSamples}쌍");
            Console.WriteLine("  알고리즘        평균 확장노드   평균 계산시간(µs)   경로길이 일치");
            Console.WriteLine("  " + new string('-', 58));
            Console.WriteLine(
                $"  다익스트라      {Mean(dijkstraExpanded),-14:F1}  {dijkstraMicros,-18:F2}  기준");
            Console.WriteLine(
                $"  A*(직선거리)    {Mean(astarExpanded),-14:F1}  {astarMicros,-18:F2}  {matchPercent:F0}%");
        }

        private static void RunExperimentB()
        {
            var staticTime = new List<double>(Replications);
            var dynamicTime = new List<double>(Replications);
            var staticMaxLoad = new List<double>(Replications);
            var dynamicMaxLoad = new List<double>(Replications);

            for (int r = 0; r < Replications; r++)
            {
                RailGraph graph = BuildGrid();
                var random = new Random(BaseSeed + r);
                (int start, int goal)[] demand = BuildDemand(graph, random);

                (double meanStatic, double maxStatic) = RouteDemand(graph, demand, congestionAware: false);
                (double meanDynamic, double maxDynamic) = RouteDemand(graph, demand, congestionAware: true);

                staticTime.Add(meanStatic);
                dynamicTime.Add(meanDynamic);
                staticMaxLoad.Add(maxStatic);
                dynamicMaxLoad.Add(maxDynamic);
            }

            Console.WriteLine();
            Console.WriteLine(
                $"[정적 vs 정체 인지 동적] 한쪽→반대쪽 수요 {VehiclesPerRep}대 · {Replications}회 반복");
            Console.WriteLine("  라우팅            평균 반송시간(s)     최대 구간 부하(대)");
            Console.WriteLine("  " + new string('-', 54));
            Console.WriteLine(
                $"  고정 최단경로     {ExperimentStats.Summarize(staticTime).Format("F1"),-20} " +
                $"{ExperimentStats.Summarize(staticMaxLoad).Format("F1")}");
            Console.WriteLine(
                $"  정체 인지 동적    {ExperimentStats.Summarize(dynamicTime).Format("F1"),-20} " +
                $"{ExperimentStats.Summarize(dynamicMaxLoad).Format("F1")}   ← 개선");
        }

        // 각 차량을 순서대로 라우팅한다. 정체 인지면 앞 차량들이 깔아 둔 부하를 엣지 비용에
        // 실어(FindPath(edgeCost)) 경로를 분산시킨다. 반환: (평균 혼잡 반송시간, 최대 구간 부하).
        private static (double meanTime, double maxLoad) RouteDemand(
            RailGraph graph, (int start, int goal)[] demand, bool congestionAware)
        {
            var load = new Dictionary<long, int>();
            var paths = new List<List<int>>(demand.Length);

            foreach ((int start, int goal) in demand)
            {
                List<int> path;
                if (congestionAware)
                {
                    path = graph.FindPath(start, goal, (a, b) =>
                        graph.GetDistance(a, b) * (1f + (float)(CongestionK * LoadOf(load, a, b))), out _);
                }
                else
                {
                    path = graph.FindPath(start, goal, null, out _);
                }

                paths.Add(path);
                AddLoad(load, path);
            }

            // 최종 부하로 각 차량의 혼잡 반송시간을 계산한다(공유 구간일수록 느려진다).
            int maxLoad = 0;
            foreach (int value in load.Values)
            {
                if (value > maxLoad)
                {
                    maxLoad = value;
                }
            }

            double totalTime = 0.0;
            foreach (List<int> path in paths)
            {
                double time = 0.0;
                for (int i = 1; i < path.Count; i++)
                {
                    double baseTime = graph.GetDistance(path[i - 1], path[i]) / Speed;
                    time += baseTime * (1.0 + CongestionK * LoadOf(load, path[i - 1], path[i]));
                }

                totalTime += time;
            }

            return (totalTime / demand.Length, maxLoad);
        }

        // 수요: 왼쪽 두 열에서 출발해 오른쪽 두 열로 향한다 → 정적 라우팅이 가운데로 몰린다.
        private static (int, int)[] BuildDemand(RailGraph graph, Random random)
        {
            var demand = new (int, int)[VehiclesPerRep];
            for (int i = 0; i < VehiclesPerRep; i++)
            {
                int startRow = random.Next(GridSize);
                int startCol = random.Next(2);                  // 0~1
                int goalRow = random.Next(GridSize);
                int goalCol = GridSize - 1 - random.Next(2);    // 4~5
                demand[i] = (startRow * GridSize + startCol, goalRow * GridSize + goalCol);
            }

            return demand;
        }

        private static void AddLoad(Dictionary<long, int> load, List<int> path)
        {
            for (int i = 1; i < path.Count; i++)
            {
                long key = EdgeKey(path[i - 1], path[i]);
                load[key] = (load.TryGetValue(key, out int value) ? value : 0) + 1;
            }
        }

        private static int LoadOf(Dictionary<long, int> load, int from, int to)
        {
            return load.TryGetValue(EdgeKey(from, to), out int value) ? value : 0;
        }

        // 무방향 부하 집계 — 양방향 엣지를 한 구간으로 본다.
        private static long EdgeKey(int a, int b)
        {
            int lo = Math.Min(a, b);
            int hi = Math.Max(a, b);
            return ((long)lo << 32) | (uint)hi;
        }

        private static void WarmUp(RailGraph graph)
        {
            graph.FindPath(0, graph.NodeCount - 1, null, out _);
            graph.FindPathAStar(0, graph.NodeCount - 1, graph.GetDistance, out _);
        }

        private static double Mean(IReadOnlyList<double> xs)
        {
            double sum = 0.0;
            foreach (double x in xs)
            {
                sum += x;
            }

            return sum / xs.Count;
        }

        // 6×6 격자 — node(r,c) = r*6+c, 위치 (c·간격, 0, r·간격), 4방향 양방향 엣지.
        private static RailGraph BuildGrid()
        {
            var graph = new RailGraph();
            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    graph.AddNode((float)(c * Spacing), 0f, (float)(r * Spacing));
                }
            }

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    int id = r * GridSize + c;
                    if (c + 1 < GridSize)
                    {
                        int right = id + 1;
                        graph.AddEdge(id, right);
                        graph.AddEdge(right, id);
                    }

                    if (r + 1 < GridSize)
                    {
                        int down = id + GridSize;
                        graph.AddEdge(id, down);
                        graph.AddEdge(down, id);
                    }
                }
            }

            return graph;
        }
    }
}
