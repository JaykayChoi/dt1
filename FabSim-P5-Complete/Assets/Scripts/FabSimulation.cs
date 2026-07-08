using FabSim.Sim;
using UnityEngine;

namespace FabSim
{
    /// <summary>
    /// 시뮬레이션 러너 — 순수 C# 모델(FabModel)을 소유하고, 매 프레임
    /// "실시간 × 배속"만큼 시뮬레이션 클록을 밀어 준다.
    /// Phase 2에서 예고한 Run(Now + deltaTime * speed) 브리지의 실물.
    /// </summary>
    public sealed class FabSimulation : MonoBehaviour
    {
        [Tooltip("비히클 대수.")]
        [SerializeField, Range(1, 12)]
        private int vehicleCount = 4;

        [Tooltip("비히클 주행 속도 [m/s].")]
        [SerializeField]
        private float vehicleSpeed = 2.5f;

        [Tooltip("반송 명령 평균 발생 간격 [초] (지수분포).")]
        [SerializeField]
        private float jobIntervalMean = 25f;

        [Tooltip("시뮬레이션 배속. 0이면 일시 정지. 키 0/1/2/3으로도 조절.")]
        [SerializeField]
        private float timeScale = 30f;

        [SerializeField]
        private int randomSeed = 7;

        /// <summary>시뮬레이션 모델. 뷰와 HUD가 읽는다.</summary>
        public FabModel Model { get; private set; }

        /// <summary>현재 배속.</summary>
        public float TimeScale => timeScale;

        private void Awake()
        {
            RailGraph graph = FabLayout.Build(out int[] portNodes);
            Model = new FabModel(
                graph, portNodes, vehicleCount, jobIntervalMean, vehicleSpeed, randomSeed);
            Model.Start();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                timeScale = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                timeScale = 1f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                timeScale = 30f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                timeScale = 120f;
            }

            // 시뮬레이션 시간과 벽시계 시간의 분리 — 엔진은 배속을 모른다.
            Model.Simulation.Run(Model.Simulation.Now + Time.deltaTime * timeScale);
        }
    }
}
