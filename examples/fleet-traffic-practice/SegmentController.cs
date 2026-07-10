using System;
using System.Collections.Generic;

namespace FleetTraffic
{
    /// <summary>
    /// 실습용 SegmentController 스켈레톤. 핵심 메서드가 TODO 스텁이라 그대로 컴파일·실행되되
    /// 교통 제어가 사실상 꺼진 것처럼 굴러(점유 무시) 실험 A에서 처리량이 꺾이지 않고,
    /// 실험 B에서 데드락을 못 잡는다. TODO를 채우면 완성본 examples/fleet-traffic/과 같은
    /// 수치(처리량 꺾임·데드락 탐지)가 나온다. 정답지는 그 완성본이다.
    /// </summary>
    public sealed class SegmentController
    {
        private readonly Dictionary<int, int> holder = new Dictionary<int, int>();
        private readonly Dictionary<int, int> waitingEdge = new Dictionary<int, int>();

        /// <summary>구간이 비면 즉시, 점유 중이면 차례가 왔을 때 onGranted를 부른다.</summary>
        public void Acquire(int edgeId, int vehicleId, Action onGranted)
        {
            // TODO(실습 1): 구간(edgeId)이 비어 있으면(HolderOf == -1) 점유자로 기록하고 즉시
            //   onGranted()를 부른다. 이미 점유 중이면 대기 큐에 (vehicleId, onGranted)를 넣고
            //   waitingEdge[vehicleId] = edgeId로 "이 차량이 이 구간을 기다린다"를 기록한다.
            // 스텁 동작: 점유를 무시하고 무조건 즉시 승인 → 교통 제어 OFF처럼 굴러 처리량이 꺾이지 않는다.
            onGranted();
        }

        /// <summary>구간을 반납하고, 대기자가 있으면 그에게 즉시 넘긴다.</summary>
        public void Release(int edgeId, int vehicleId)
        {
            // TODO(실습 1): 점유자가 vehicleId면 반납한다. 대기 큐에 다음 차량이 있으면 점유를
            //   그에게 승계(holder 갱신 + waitingEdge 제거 + onGranted 호출)하고, 없으면 holder를 -1로.
            // 스텁 동작: 아무 것도 하지 않는다.
        }

        /// <summary>구간을 점유한 차량 id (없으면 -1).</summary>
        public int HolderOf(int edgeId)
        {
            // TODO(실습 1): holder에서 edgeId의 점유자를 반환(없으면 -1).
            return -1;
        }

        /// <summary>차량이 대기 중인 구간 id (없으면 -1).</summary>
        public int WaitingEdgeOf(int vehicleId)
        {
            // TODO(실습 1): waitingEdge에서 vehicleId가 기다리는 구간을 반환(없으면 -1).
            return -1;
        }

        /// <summary>
        /// wait-for 그래프를 DFS로 훑어 데드락 사이클에 낀 차량 id들을 반환한다.
        /// 사이클이 없으면 빈 목록. 간선: 대기 차량 → 그 구간을 점유한 차량.
        /// </summary>
        public IReadOnlyList<int> DetectDeadlockCycle()
        {
            // TODO(실습 2): 대기 중인 차량마다 "기다리는 구간의 점유자"로 향하는 간선을 만들고,
            //   그 방향 그래프를 DFS로 훑어 사이클에 낀 차량 id들을 정렬해 반환한다.
            // 스텁 동작: 항상 빈 목록 → 실험 B에서 데드락을 못 잡는다.
            return Array.Empty<int>();
        }
    }
}
