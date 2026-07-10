using System;
using System.Collections.Generic;
using DesCore;

namespace FleetTraffic
{
    /// <summary>비히클의 작업 단계.</summary>
    public enum TrafficPhase
    {
        Idle,
        ToPickup,
        Carrying,
    }

    /// <summary>단일 방향 루프 위에서 도는 한 대의 비히클.</summary>
    public sealed class TrafficVehicle
    {
        public int Id;
        public int NodeId;
        public int HoldEdge = -1;     // 현재 점유 중인 엣지(구간) id
        public TrafficPhase Phase = TrafficPhase.Idle;
        public bool IsWaiting;        // 다음 구간이 막혀 대기 중
        public int FromPort;
        public int ToPort;
        public double JobCreatedAt;
    }

    /// <summary>
    /// 단일 방향 루프 위의 교통 시뮬레이션. 교통 제어를 켜면 각 엣지를 용량 1 구간으로
    /// 점유(hold-and-wait: 현재 구간을 쥔 채 다음 구간을 예약)하고, 끄면 차량이 서로를
    /// 통과한다. 처리량이 차량 수에 따라 어떻게 변하는지를 재는 것이 목적이다.
    /// </summary>
    public sealed class TrafficModel
    {
        private const double Dwell = 4.0;

        private readonly RailGraph graph;
        private readonly int loopLen;
        private readonly int[] ports;
        private readonly double speed;
        private readonly double jobIntervalMean;
        private readonly bool useControl;
        private readonly SegmentController segments;
        private readonly Random random;
        private readonly List<TrafficVehicle> vehicles = new List<TrafficVehicle>();
        private readonly Queue<int[]> pendingJobs = new Queue<int[]>();

        private int completed;
        private double totalDelivery;

        /// <summary>완료된 반송 수.</summary>
        public int Completed => completed;

        /// <summary>비히클 목록.</summary>
        public IReadOnlyList<TrafficVehicle> Vehicles => vehicles;

        /// <summary>DesCore 시뮬레이션 엔진.</summary>
        public Simulation Simulation { get; } = new Simulation();

        public TrafficModel(RailGraph graph, int loopLen, int[] ports, int vehicleCount,
            double speed, double jobIntervalMean, bool useControl, int seed)
        {
            this.graph = graph;
            this.loopLen = loopLen;
            this.ports = ports;
            this.speed = speed;
            this.jobIntervalMean = jobIntervalMean;
            this.useControl = useControl;
            random = new Random(seed);
            segments = useControl ? new SegmentController() : null;

            // 비히클을 루프에 고르게 흩어 배치하고, 도착 엣지를 점유시킨다.
            for (int i = 0; i < vehicleCount; i++)
            {
                int node = i * loopLen / vehicleCount;
                int holdEdge = (node - 1 + loopLen) % loopLen;
                var vehicle = new TrafficVehicle { Id = i, NodeId = node, HoldEdge = holdEdge };
                vehicles.Add(vehicle);
                if (useControl)
                {
                    segments.Acquire(holdEdge, i, () => { });
                }
            }
        }

        /// <summary>완료 기준 평균 반송 시간 [초].</summary>
        public double AverageDeliveryTime()
        {
            return completed == 0 ? 0.0 : totalDelivery / completed;
        }

        /// <summary>시간당 처리량 [건/h].</summary>
        public double ThroughputPerHour(double now)
        {
            return now <= 0.0 ? 0.0 : completed / (now / 3600.0);
        }

        /// <summary>첫 반송 명령 도착을 예약해 시동한다.</summary>
        public void Start()
        {
            ScheduleNextArrival();
        }

        private void ScheduleNextArrival()
        {
            Simulation.Schedule(random.NextExponential(jobIntervalMean), () =>
            {
                int from = ports[random.Next(ports.Length)];
                int to = from;
                while (to == from)
                {
                    to = ports[random.Next(ports.Length)];
                }

                pendingJobs.Enqueue(new[] { from, to });
                TryDispatch();
                ScheduleNextArrival();
            });
        }

        private void TryDispatch()
        {
            while (pendingJobs.Count > 0)
            {
                TrafficVehicle idle = null;
                foreach (TrafficVehicle vehicle in vehicles)
                {
                    if (vehicle.Phase == TrafficPhase.Idle)
                    {
                        idle = vehicle;
                        break;
                    }
                }

                if (idle == null)
                {
                    return;
                }

                int[] job = pendingJobs.Dequeue();
                Assign(idle, job[0], job[1]);
            }
        }

        private void Assign(TrafficVehicle vehicle, int from, int to)
        {
            vehicle.Phase = TrafficPhase.ToPickup;
            vehicle.FromPort = from;
            vehicle.ToPort = to;
            vehicle.JobCreatedAt = Simulation.Now;

            TraverseForward(vehicle, from, () =>
            {
                Simulation.Schedule(Dwell, () =>
                {
                    vehicle.Phase = TrafficPhase.Carrying;
                    TraverseForward(vehicle, to, () =>
                    {
                        Simulation.Schedule(Dwell, () => CompleteJob(vehicle));
                    });
                });
            });
        }

        // 단일 방향 루프이므로 목표 노드까지 앞으로만 한 엣지씩 전진한다.
        private void TraverseForward(TrafficVehicle vehicle, int target, Action onArrived)
        {
            if (vehicle.NodeId == target)
            {
                onArrived();
                return;
            }

            MoveOneEdge(vehicle, () => TraverseForward(vehicle, target, onArrived));
        }

        private void MoveOneEdge(TrafficVehicle vehicle, Action onDone)
        {
            int c = vehicle.NodeId;
            int d = (c + 1) % loopLen;
            int edge = c;                       // 엣지 id = 출발 노드 id
            double travel = graph.GetDistance(c, d) / speed;
            int previousHold = vehicle.HoldEdge;

            if (useControl)
            {
                vehicle.IsWaiting = segments.HolderOf(edge) != -1
                    && segments.HolderOf(edge) != vehicle.Id;
                segments.Acquire(edge, vehicle.Id, () =>
                {
                    vehicle.IsWaiting = false;
                    Simulation.Schedule(travel, () =>
                    {
                        segments.Release(previousHold, vehicle.Id);
                        vehicle.HoldEdge = edge;
                        vehicle.NodeId = d;
                        onDone();
                    });
                });
            }
            else
            {
                Simulation.Schedule(travel, () =>
                {
                    vehicle.NodeId = d;
                    onDone();
                });
            }
        }

        private void CompleteJob(TrafficVehicle vehicle)
        {
            completed++;
            totalDelivery += Simulation.Now - vehicle.JobCreatedAt;
            vehicle.Phase = TrafficPhase.Idle;
            TryDispatch();
        }
    }
}
