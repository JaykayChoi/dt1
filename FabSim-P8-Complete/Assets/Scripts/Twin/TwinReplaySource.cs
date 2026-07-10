using System;
using System.Collections.Generic;
using System.Globalization;
using FabSim.Sim;

namespace FabSim.Twin
{
    /// <summary>
    /// NDJSON 이벤트 스트림을 소비해 비히클 상태를 재구성하는 트윈 소스(헤드리스, UnityEngine
    /// 무의존이라 테스트 가능). 이벤트를 플레이아웃 지연만큼 늦춰 적용하고, SNAPSHOT으로
    /// 재동기화한다. 뷰는 이 소스가 채운 VehicleAgent 필드를 손대지 않은 EvaluatePosition으로 그린다.
    /// </summary>
    public sealed class TwinReplaySource : IFleetSource
    {
        private readonly RailGraph graph;
        private readonly List<VehicleAgent> vehicles = new List<VehicleAgent>();
        private readonly Dictionary<int, VehicleAgent> byId = new Dictionary<int, VehicleAgent>();
        private readonly Dictionary<int, double> jobCreatedAt = new Dictionary<int, double>();
        private readonly List<(double t, string line)> events = new List<(double, string)>();
        private readonly double playoutDelay;

        private int cursor;
        private double now;
        private int completedJobs;
        private int pendingJobs;
        private double totalDeliveryTime;
        private int deliveredCount;

        /// <summary>로그 라인 목록으로 소스를 구성한다. 첫 줄 HELLO로 그래프·비히클을 세운다.</summary>
        public TwinReplaySource(IEnumerable<string> lines, double playoutDelay)
        {
            this.playoutDelay = playoutDelay;
            graph = FabLayout.Build(out int[] portNodes);

            var pending = new List<string>();
            int vehicleCount = -1;
            foreach (string line in lines)
            {
                if (vehicleCount < 0 && line.Contains("\"HELLO\""))
                {
                    vehicleCount = ExtractInt(line, "\"vehicleCount\":");
                    int nodeCount = ExtractInt(line, "\"nodeCount\":");
                    if (nodeCount != graph.NodeCount)
                    {
                        throw new InvalidOperationException(
                            $"HELLO nodeCount {nodeCount} != 로컬 그래프 {graph.NodeCount}");
                    }

                    continue;
                }

                pending.Add(line);
            }

            if (vehicleCount < 0)
            {
                vehicleCount = 4;
            }

            for (int i = 0; i < vehicleCount; i++)
            {
                var vehicle = new VehicleAgent
                {
                    Id = i,
                    Phase = VehiclePhase.Idle,
                    NodeId = i * graph.NodeCount / vehicleCount,
                };
                vehicles.Add(vehicle);
                byId[i] = vehicle;
            }

            foreach (string line in pending)
            {
                events.Add((ExtractDouble(line, "\"t\":"), line));
            }
        }

        /// <summary>라이브 수신 라인을 이벤트 버퍼에 덧붙인다(TwinFeedClient가 TCP에서 받아 호출).</summary>
        public void AppendLine(string line)
        {
            if (line.Contains("\"HELLO\""))
            {
                return;
            }

            events.Add((ExtractDouble(line, "\"t\":"), line));
        }

        public RailGraph Graph => graph;

        public IReadOnlyList<VehicleAgent> Vehicles => vehicles;

        public double Now => now;

        public int CompletedJobs => completedJobs;

        public int PendingJobCount => pendingJobs;

        public double GetThroughputPerHour(double clock)
        {
            return clock <= 0.0 ? 0.0 : completedJobs / (clock / 3600.0);
        }

        public double GetAverageDeliveryTime()
        {
            return deliveredCount == 0 ? 0.0 : totalDeliveryTime / deliveredCount;
        }

        public double GetFleetUtilization(double clock)
        {
            if (clock <= 0.0)
            {
                return 0.0;
            }

            double busy = 0.0;
            foreach (VehicleAgent vehicle in vehicles)
            {
                busy += vehicle.TotalBusyTime;
                if (vehicle.Phase != VehiclePhase.Idle)
                {
                    busy += clock - vehicle.BusyStartedAt;
                }
            }

            return busy / (clock * vehicles.Count);
        }

        /// <summary>플레이아웃 지연을 지난 이벤트를 모두 적용하며 시각을 dt만큼 전진시킨다.</summary>
        public void Tick(float deltaTime)
        {
            now += deltaTime;
            double horizon = now - playoutDelay;
            while (cursor < events.Count && events[cursor].t <= horizon)
            {
                Apply(events[cursor].line, events[cursor].t);
                cursor++;
            }
        }

        /// <summary>남은 이벤트를 즉시 모두 적용한다(테스트·시킹용).</summary>
        public void ApplyAll()
        {
            while (cursor < events.Count)
            {
                Apply(events[cursor].line, events[cursor].t);
                cursor++;
            }

            if (events.Count > 0)
            {
                now = events[events.Count - 1].t;
            }
        }

        private void Apply(string line, double eventT)
        {
            string type = ExtractString(line, "\"type\":\"");
            switch (type)
            {
                case "JOB_CREATE":
                    jobCreatedAt[ExtractInt(line, "\"jid\":")] = eventT;
                    pendingJobs++;
                    break;
                case "DISPATCH":
                {
                    VehicleAgent vehicle = byId[ExtractInt(line, "\"vid\":")];
                    vehicle.Phase = VehiclePhase.ToPickup;
                    vehicle.BusyStartedAt = eventT;
                    if (pendingJobs > 0)
                    {
                        pendingJobs--;
                    }

                    break;
                }
                case "EDGE_DEPART":
                {
                    VehicleAgent vehicle = byId[ExtractInt(line, "\"vid\":")];
                    vehicle.IsMoving = true;
                    vehicle.EdgeFromNode = ExtractInt(line, "\"from\":");
                    vehicle.EdgeToNode = ExtractInt(line, "\"to\":");
                    vehicle.EdgeDepartAt = eventT;
                    vehicle.EdgeArriveAt = ExtractDouble(line, "\"eta\":");
                    break;
                }
                case "EDGE_ARRIVE":
                {
                    VehicleAgent vehicle = byId[ExtractInt(line, "\"vid\":")];
                    vehicle.IsMoving = false;
                    vehicle.NodeId = ExtractInt(line, "\"node\":");
                    break;
                }
                case "PICKUP":
                    byId[ExtractInt(line, "\"vid\":")].Phase = VehiclePhase.Carrying;
                    break;
                case "JOB_COMPLETE":
                {
                    VehicleAgent vehicle = byId[ExtractInt(line, "\"vid\":")];
                    vehicle.TotalBusyTime += eventT - vehicle.BusyStartedAt;
                    vehicle.Phase = VehiclePhase.Idle;
                    vehicle.IsMoving = false;
                    completedJobs++;
                    deliveredCount++;
                    int jid = ExtractInt(line, "\"jid\":");
                    if (jobCreatedAt.TryGetValue(jid, out double created))
                    {
                        totalDeliveryTime += eventT - created;
                    }

                    break;
                }
                case "SNAPSHOT":
                    ApplySnapshot(line);
                    break;
                default:
                    break;
            }
        }

        // SNAPSHOT은 전체 상태를 덮어써 재동기화한다 — 재접속·유실 복구의 공통 프리미티브.
        private void ApplySnapshot(string line)
        {
            completedJobs = ExtractInt(line, "\"completed\":");
            pendingJobs = ExtractInt(line, "\"pending\":");

            int scan = 0;
            while (true)
            {
                int vidKey = line.IndexOf("\"vid\":", scan, StringComparison.Ordinal);
                if (vidKey < 0)
                {
                    break;
                }

                int objEnd = line.IndexOf('}', vidKey);
                if (objEnd < 0)
                {
                    break;
                }

                string obj = line.Substring(vidKey, objEnd - vidKey);
                int vid = ExtractInt(obj, "\"vid\":");
                if (byId.TryGetValue(vid, out VehicleAgent vehicle))
                {
                    vehicle.TotalBusyTime = ExtractDouble(obj, "\"busy\":");
                    vehicle.Phase = ParsePhase(ExtractString(obj, "\"phase\":\""));
                    if (obj.Contains("\"moving\":true"))
                    {
                        vehicle.IsMoving = true;
                        vehicle.EdgeFromNode = ExtractInt(obj, "\"from\":");
                        vehicle.EdgeToNode = ExtractInt(obj, "\"to\":");
                        vehicle.EdgeDepartAt = ExtractDouble(obj, "\"depart\":");
                        vehicle.EdgeArriveAt = ExtractDouble(obj, "\"eta\":");
                    }
                    else
                    {
                        vehicle.IsMoving = false;
                        vehicle.NodeId = ExtractInt(obj, "\"node\":");
                    }
                }

                scan = objEnd + 1;
            }
        }

        private static VehiclePhase ParsePhase(string phase)
        {
            switch (phase)
            {
                case "ToPickup":
                    return VehiclePhase.ToPickup;
                case "Carrying":
                    return VehiclePhase.Carrying;
                default:
                    return VehiclePhase.Idle;
            }
        }

        private static int ExtractInt(string line, string key)
        {
            double value = ExtractDouble(line, key);
            return (int)Math.Round(value);
        }

        private static double ExtractDouble(string line, string key)
        {
            int at = line.IndexOf(key, StringComparison.Ordinal);
            if (at < 0)
            {
                return 0.0;
            }

            int start = at + key.Length;
            int end = start;
            while (end < line.Length && (char.IsDigit(line[end]) || line[end] == '.' || line[end] == '-'))
            {
                end++;
            }

            return double.TryParse(line.Substring(start, end - start),
                NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                ? value
                : 0.0;
        }

        private static string ExtractString(string line, string key)
        {
            int at = line.IndexOf(key, StringComparison.Ordinal);
            if (at < 0)
            {
                return string.Empty;
            }

            int start = at + key.Length;
            int end = line.IndexOf('"', start);
            return end < 0 ? string.Empty : line.Substring(start, end - start);
        }
    }
}
