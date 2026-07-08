using System;
using System.Collections.Generic;
using DesCore;

namespace FabSim.Sim
{
    /// <summary>
    /// 미니 팹 시뮬레이션 모델 — Phase 2의 DesCore 엔진 위에서 도는
    /// 간단한 MCS: 반송 명령 생성 → 최근접 유휴 비히클 배차 →
    /// 픽업 → 이동 → 드롭 → 통계. UnityEngine 무의존.
    /// </summary>
    public sealed class FabModel
    {
        private const double PickupDwell = 4.0;
        private const double DropDwell = 4.0;

        private readonly RailGraph graph;
        private readonly int[] portNodes;
        private readonly double jobIntervalMean;
        private readonly double vehicleSpeed;
        private readonly Random random;
        private readonly List<VehicleAgent> vehicles = new List<VehicleAgent>();
        private readonly Queue<TransportJob> pendingJobs = new Queue<TransportJob>();
        private int nextJobId;

        /// <summary>DesCore 시뮬레이션 엔진 (클록 + 사건 루프).</summary>
        public Simulation Simulation { get; } = new Simulation();

        /// <summary>비히클 목록 (뷰가 읽는다).</summary>
        public IReadOnlyList<VehicleAgent> Vehicles => vehicles;

        /// <summary>레일 그래프 (뷰가 노드 좌표를 읽는다).</summary>
        public RailGraph Graph => graph;

        /// <summary>완료된 반송 수.</summary>
        public int CompletedJobs { get; private set; }

        /// <summary>배차를 기다리는 반송 명령 수.</summary>
        public int PendingJobCount => pendingJobs.Count;

        private double totalDeliveryTime;

        public FabModel(RailGraph graph, int[] portNodes, int vehicleCount,
            double jobIntervalMean, double vehicleSpeed, int seed)
        {
            if (vehicleCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(vehicleCount));
            }

            this.graph = graph;
            this.portNodes = portNodes;
            this.jobIntervalMean = jobIntervalMean;
            this.vehicleSpeed = vehicleSpeed;
            random = new Random(seed);

            // 비히클을 루프 위에 고르게 흩어 배치한다.
            for (int i = 0; i < vehicleCount; i++)
            {
                vehicles.Add(new VehicleAgent
                {
                    Id = i,
                    Phase = VehiclePhase.Idle,
                    NodeId = i * graph.NodeCount / vehicleCount,
                });
            }
        }

        /// <summary>첫 반송 명령 도착을 예약해 시뮬레이션을 시동한다.</summary>
        public void Start()
        {
            ScheduleNextJobArrival();
        }

        /// <summary>now 기준 시간당 처리량 [건/시간].</summary>
        public double GetThroughputPerHour(double now)
        {
            if (now <= 0.0)
            {
                return 0.0;
            }

            return CompletedJobs / (now / 3600.0);
        }

        /// <summary>완료 기준 평균 반송 시간 [초] (명령 생성 → 드롭 완료).</summary>
        public double GetAverageDeliveryTime()
        {
            if (CompletedJobs == 0)
            {
                return 0.0;
            }

            return totalDeliveryTime / CompletedJobs;
        }

        /// <summary>now 기준 함대 평균 가동률 (0~1).</summary>
        public double GetFleetUtilization(double now)
        {
            if (now <= 0.0)
            {
                return 0.0;
            }

            double busy = 0.0;
            foreach (VehicleAgent vehicle in vehicles)
            {
                busy += vehicle.TotalBusyTime;
                if (vehicle.Phase != VehiclePhase.Idle)
                {
                    busy += now - vehicle.BusyStartedAt;
                }
            }

            return busy / (now * vehicles.Count);
        }

        private void ScheduleNextJobArrival()
        {
            Simulation.Schedule(random.NextExponential(jobIntervalMean), () =>
            {
                OnJobArrived();
                ScheduleNextJobArrival();
            });
        }

        private void OnJobArrived()
        {
            int from = portNodes[random.Next(portNodes.Length)];
            int to = from;
            while (to == from)
            {
                to = portNodes[random.Next(portNodes.Length)];
            }

            pendingJobs.Enqueue(new TransportJob
            {
                Id = nextJobId++,
                FromPort = from,
                ToPort = to,
                CreatedAt = Simulation.Now,
            });
            TryDispatch();
        }

        // 배차 규칙: 대기 명령 선입선출 × 최근접 유휴 비히클 (Phase 1 Station 04).
        private void TryDispatch()
        {
            while (pendingJobs.Count > 0)
            {
                TransportJob job = pendingJobs.Peek();
                VehicleAgent best = null;
                float bestDistance = float.PositiveInfinity;
                foreach (VehicleAgent vehicle in vehicles)
                {
                    if (vehicle.Phase != VehiclePhase.Idle)
                    {
                        continue;
                    }

                    List<int> path = graph.FindPath(vehicle.NodeId, job.FromPort);
                    if (path == null)
                    {
                        continue;
                    }

                    float distance = graph.GetPathLength(path);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = vehicle;
                    }
                }

                if (best == null)
                {
                    return;
                }

                pendingJobs.Dequeue();
                Assign(best, job);
            }
        }

        private void Assign(VehicleAgent vehicle, TransportJob job)
        {
            vehicle.Phase = VehiclePhase.ToPickup;
            vehicle.CurrentJob = job;
            vehicle.BusyStartedAt = Simulation.Now;
            job.AssignedAt = Simulation.Now;

            List<int> toPickup = graph.FindPath(vehicle.NodeId, job.FromPort);
            TraversePath(vehicle, toPickup, 1, () =>
            {
                // 픽업 핸드오프 (Phase 1의 E84 자리) — 고정 dwell로 근사.
                Simulation.Schedule(PickupDwell, () =>
                {
                    vehicle.Phase = VehiclePhase.Carrying;
                    List<int> toDrop = graph.FindPath(job.FromPort, job.ToPort);
                    TraversePath(vehicle, toDrop, 1, () =>
                    {
                        Simulation.Schedule(DropDwell, () => CompleteJob(vehicle));
                    });
                });
            });
        }

        // 경로를 엣지 단위 사건의 연쇄로 굴린다 — "사건이 사건을 낳는" Phase 2 패턴.
        private void TraversePath(VehicleAgent vehicle, List<int> path, int nextIndex,
            Action onArrived)
        {
            if (nextIndex >= path.Count)
            {
                vehicle.IsMoving = false;
                vehicle.NodeId = path[path.Count - 1];
                onArrived();
                return;
            }

            int from = path[nextIndex - 1];
            int to = path[nextIndex];
            double travel = graph.GetDistance(from, to) / vehicleSpeed;
            vehicle.IsMoving = true;
            vehicle.EdgeFromNode = from;
            vehicle.EdgeToNode = to;
            vehicle.EdgeDepartAt = Simulation.Now;
            vehicle.EdgeArriveAt = Simulation.Now + travel;

            Simulation.Schedule(travel, () =>
            {
                vehicle.NodeId = to;
                TraversePath(vehicle, path, nextIndex + 1, onArrived);
            });
        }

        private void CompleteJob(VehicleAgent vehicle)
        {
            TransportJob job = vehicle.CurrentJob;
            job.CompletedAt = Simulation.Now;
            CompletedJobs++;
            totalDeliveryTime += job.CompletedAt - job.CreatedAt;

            vehicle.TotalBusyTime += Simulation.Now - vehicle.BusyStartedAt;
            vehicle.Phase = VehiclePhase.Idle;
            vehicle.CurrentJob = null;

            TryDispatch();
        }
    }
}
