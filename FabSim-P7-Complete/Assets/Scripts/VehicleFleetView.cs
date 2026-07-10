using System.Collections.Generic;
using FabSim.Sim;
using UnityEngine;

namespace FabSim
{
    /// <summary>
    /// 비히클 시각화 — 매니저 하나가 전체를 보간한다 (Phase 4의 교훈).
    /// 시뮬레이션은 "엣지 출발/도착 시각"만 말해 주고, 렌더 위치는
    /// 매 프레임 그 시각들 사이를 선형 보간해 얻는다. Phase 7에서는 구간이 막혀
    /// 대기 중인 차량을 호박색, 데드락에 낀 차량을 적색으로 물들인다
    /// (MaterialPropertyBlock으로 GC 없이, 상태가 바뀔 때만).
    /// </summary>
    public sealed class VehicleFleetView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color WaitingColor = new Color(1f, 0.72f, 0.15f);
        private static readonly Color DeadlockColor = new Color(0.92f, 0.16f, 0.16f);
        private const double DeadlockCheckInterval = 0.4;

        [SerializeField]
        private FabSimulation simulation;

        [SerializeField]
        private Material vehicleMaterial;

        [SerializeField]
        private Material foupMaterial;

        private readonly List<Transform> bodies = new List<Transform>();
        private readonly List<MeshRenderer> renderers = new List<MeshRenderer>();
        private readonly List<GameObject> foups = new List<GameObject>();
        private readonly List<bool> foupVisible = new List<bool>();
        private readonly List<int> colorState = new List<int>();
        private readonly HashSet<int> deadlockVehicles = new HashSet<int>();

        private MaterialPropertyBlock propertyBlock;
        private Color baseColor = Color.white;
        private double nextDeadlockCheck;

        /// <summary>씬 빌더가 참조를 주입한다.</summary>
        public void Initialize(FabSimulation owner, Material vehicle, Material foup)
        {
            simulation = owner;
            vehicleMaterial = vehicle;
            foupMaterial = foup;
        }

        private void Start()
        {
            propertyBlock = new MaterialPropertyBlock();
            if (vehicleMaterial != null && vehicleMaterial.HasProperty(BaseColorId))
            {
                baseColor = vehicleMaterial.GetColor(BaseColorId);
            }

            foreach (VehicleAgent agent in simulation.Model.Vehicles)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = $"OHT_{agent.Id:D2}";
                body.transform.SetParent(transform);
                body.transform.localScale = new Vector3(0.55f, 0.35f, 0.95f);
                MeshRenderer bodyRenderer = body.GetComponent<MeshRenderer>();
                bodyRenderer.sharedMaterial = vehicleMaterial;

                GameObject foup = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foup.name = "FOUP";
                foup.transform.SetParent(body.transform);
                foup.transform.localScale = new Vector3(0.7f, 1.1f, 0.42f);
                foup.transform.localPosition = new Vector3(0f, -0.75f, 0f);
                foup.GetComponent<MeshRenderer>().sharedMaterial = foupMaterial;
                foup.SetActive(false);

                bodies.Add(body.transform);
                renderers.Add(bodyRenderer);
                foups.Add(foup);
                foupVisible.Add(false);
                colorState.Add(-1);
            }
        }

        private void LateUpdate()
        {
            double now = simulation.Model.Simulation.Now;
            RailGraph graph = simulation.Model.Graph;
            IReadOnlyList<VehicleAgent> agents = simulation.Model.Vehicles;

            RefreshDeadlockSet(now);

            for (int i = 0; i < agents.Count; i++)
            {
                VehicleAgent agent = agents[i];
                bodies[i].position = EvaluatePosition(agent, graph, now);

                bool isCarrying = agent.Phase == VehiclePhase.Carrying;
                if (isCarrying != foupVisible[i])
                {
                    foups[i].SetActive(isCarrying);
                    foupVisible[i] = isCarrying;
                }

                int state = deadlockVehicles.Contains(agent.Id) ? 2 : agent.IsWaiting ? 1 : 0;
                if (state != colorState[i])
                {
                    ApplyColor(i, state);
                    colorState[i] = state;
                }
            }
        }

        // 데드락 탐지는 매 프레임 대신 주기적으로 훑어 GC·비용을 줄인다.
        private void RefreshDeadlockSet(double now)
        {
            if (now < nextDeadlockCheck)
            {
                return;
            }

            nextDeadlockCheck = now + DeadlockCheckInterval;
            deadlockVehicles.Clear();
            IReadOnlyList<int> cycle = simulation.Model.Segments.DetectDeadlockCycle();
            for (int i = 0; i < cycle.Count; i++)
            {
                deadlockVehicles.Add(cycle[i]);
            }
        }

        private void ApplyColor(int index, int state)
        {
            Color color = state == 2 ? DeadlockColor : state == 1 ? WaitingColor : baseColor;
            propertyBlock.SetColor(BaseColorId, color);
            renderers[index].SetPropertyBlock(propertyBlock);
        }

        private static Vector3 EvaluatePosition(VehicleAgent agent, RailGraph graph, double now)
        {
            if (!agent.IsMoving)
            {
                return ToVector(graph.GetNode(agent.NodeId));
            }

            double span = agent.EdgeArriveAt - agent.EdgeDepartAt;
            float t = span <= 0.0
                ? 1f
                : Mathf.Clamp01((float)((now - agent.EdgeDepartAt) / span));
            Vector3 from = ToVector(graph.GetNode(agent.EdgeFromNode));
            Vector3 to = ToVector(graph.GetNode(agent.EdgeToNode));
            return Vector3.Lerp(from, to, t);
        }

        private static Vector3 ToVector(RailGraph.NodePoint node)
        {
            return new Vector3(node.X, node.Y, node.Z);
        }
    }
}
