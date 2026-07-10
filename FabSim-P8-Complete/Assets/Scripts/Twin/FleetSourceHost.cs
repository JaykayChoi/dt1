using FabSim.Sim;
using UnityEngine;

namespace FabSim.Twin
{
    /// <summary>비히클 소스의 종류 — 내장 시뮬레이션이냐 외부 트윈 피드냐.</summary>
    public enum FleetSourceKind
    {
        Sim,
        Twin,
    }

    /// <summary>
    /// 활성 IFleetSource를 소유하고 매 프레임 Tick하는 호스트. 뷰와 대시보드는 FabSimulation이
    /// 아니라 이 호스트의 Source를 참조해, 소스를 Sim ↔ Twin으로 갈아 끼워도 코드가 안 바뀐다.
    /// </summary>
    public sealed class FleetSourceHost : MonoBehaviour
    {
        [SerializeField]
        private FleetSourceKind kind = FleetSourceKind.Sim;

        [SerializeField, Range(1, 12)]
        private int vehicleCount = 4;

        [SerializeField]
        private float vehicleSpeed = 2.5f;

        [SerializeField]
        private float jobIntervalMean = 25f;

        [SerializeField]
        private float timeScale = 30f;

        [SerializeField]
        private int randomSeed = 7;

        [SerializeField]
        private TwinFeedClient twinClient;

        private SimFleetSource simSource;

        /// <summary>현재 활성 소스. 뷰·대시보드가 읽는다.</summary>
        public IFleetSource Source { get; private set; }

        /// <summary>소스 종류.</summary>
        public FleetSourceKind Kind => kind;

        private void Awake()
        {
            Source = BuildSource();
        }

        private void Update()
        {
            if (Source is SimFleetSource sim)
            {
                sim.TimeScale = timeScale;
            }

            Source?.Tick(Time.deltaTime);
        }

        /// <summary>런타임에 소스를 전환한다(대시보드 소스 토글에서 호출).</summary>
        public void SwitchTo(FleetSourceKind next)
        {
            kind = next;
            Source = BuildSource();
        }

        private IFleetSource BuildSource()
        {
            if (kind == FleetSourceKind.Twin && twinClient != null)
            {
                twinClient.Connect();
                return twinClient;
            }

            RailGraph graph = FabLayout.Build(out int[] portNodes);
            var model = new FabModel(
                graph, portNodes, vehicleCount, jobIntervalMean, vehicleSpeed, randomSeed);
            model.Start();
            simSource = new SimFleetSource(model, timeScale);
            return simSource;
        }
    }
}
