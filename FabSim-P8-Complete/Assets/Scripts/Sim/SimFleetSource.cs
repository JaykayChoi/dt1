using System.Collections.Generic;

namespace FabSim.Sim
{
    /// <summary>
    /// 내장 시뮬레이션을 IFleetSource로 감싼 소스. Tick(dt)에서 DES 클록을 배속만큼 민다 —
    /// Phase 2에서 예고하고 Phase 5가 구현한 Run(Now + dt × 배속) 브리지가 여기로 옮겨 왔다.
    /// </summary>
    public sealed class SimFleetSource : IFleetSource
    {
        private readonly FabModel model;

        public SimFleetSource(FabModel model, float timeScale)
        {
            this.model = model;
            TimeScale = timeScale;
        }

        /// <summary>시뮬레이션 배속.</summary>
        public float TimeScale { get; set; }

        /// <summary>감싸고 있는 시뮬레이션 모델.</summary>
        public FabModel Model => model;

        public RailGraph Graph => model.Graph;

        public IReadOnlyList<VehicleAgent> Vehicles => model.Vehicles;

        public double Now => model.Simulation.Now;

        public int CompletedJobs => model.CompletedJobs;

        public int PendingJobCount => model.PendingJobCount;

        public double GetThroughputPerHour(double now)
        {
            return model.GetThroughputPerHour(now);
        }

        public double GetAverageDeliveryTime()
        {
            return model.GetAverageDeliveryTime();
        }

        public double GetFleetUtilization(double now)
        {
            return model.GetFleetUtilization(now);
        }

        public void Tick(float deltaTime)
        {
            model.Simulation.Run(model.Simulation.Now + deltaTime * TimeScale);
        }
    }
}
