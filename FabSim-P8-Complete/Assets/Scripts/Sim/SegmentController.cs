using System;
using System.Collections.Generic;

namespace FabSim.Sim
{
    /// <summary>
    /// 레일 엣지를 용량 1의 점유 구간으로 관리하는 교통 제어기. SimResource의
    /// Request/Release 콜백 패턴을 엣지 단위로 적용하되, 데드락 탐지를 위해 점유자·대기자
    /// 관계를 겉으로 드러낸다. UnityEngine 무의존이라 헤드리스로 테스트된다.
    /// examples/fleet-traffic의 SegmentController와 동일 스펙이다.
    /// </summary>
    public sealed class SegmentController
    {
        private readonly struct Waiter
        {
            public readonly int VehicleId;
            public readonly Action OnGranted;

            public Waiter(int vehicleId, Action onGranted)
            {
                VehicleId = vehicleId;
                OnGranted = onGranted;
            }
        }

        private readonly Dictionary<int, int> holder = new Dictionary<int, int>();
        private readonly Dictionary<int, Queue<Waiter>> waiters = new Dictionary<int, Queue<Waiter>>();
        private readonly Dictionary<int, int> waitingEdge = new Dictionary<int, int>();

        /// <summary>구간이 비면 즉시, 점유 중이면 차례가 왔을 때 onGranted를 부른다.</summary>
        public void Acquire(int edgeId, int vehicleId, Action onGranted)
        {
            if (!holder.TryGetValue(edgeId, out int current) || current == -1)
            {
                holder[edgeId] = vehicleId;
                waitingEdge.Remove(vehicleId);
                onGranted();
                return;
            }

            if (!waiters.TryGetValue(edgeId, out Queue<Waiter> queue))
            {
                queue = new Queue<Waiter>();
                waiters[edgeId] = queue;
            }

            queue.Enqueue(new Waiter(vehicleId, onGranted));
            waitingEdge[vehicleId] = edgeId;
        }

        /// <summary>구간을 반납하고, 대기자가 있으면 그에게 즉시 넘긴다.</summary>
        public void Release(int edgeId, int vehicleId)
        {
            if (!holder.TryGetValue(edgeId, out int current) || current != vehicleId)
            {
                return;
            }

            if (waiters.TryGetValue(edgeId, out Queue<Waiter> queue) && queue.Count > 0)
            {
                Waiter next = queue.Dequeue();
                holder[edgeId] = next.VehicleId;
                waitingEdge.Remove(next.VehicleId);
                next.OnGranted();
            }
            else
            {
                holder[edgeId] = -1;
            }
        }

        /// <summary>구간을 점유한 차량 id (없으면 -1).</summary>
        public int HolderOf(int edgeId)
        {
            return holder.TryGetValue(edgeId, out int current) ? current : -1;
        }

        /// <summary>차량이 대기 중인 구간 id (없으면 -1).</summary>
        public int WaitingEdgeOf(int vehicleId)
        {
            return waitingEdge.TryGetValue(vehicleId, out int edge) ? edge : -1;
        }

        /// <summary>
        /// wait-for 그래프를 DFS로 훑어 데드락 사이클에 낀 차량 id들을 정렬해 반환한다.
        /// 사이클이 없으면 빈 목록. 간선: 대기 차량 → 그 구간을 점유한 차량.
        /// </summary>
        public IReadOnlyList<int> DetectDeadlockCycle()
        {
            var next = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> pair in waitingEdge)
            {
                int vehicle = pair.Key;
                int owner = HolderOf(pair.Value);
                if (owner != -1 && owner != vehicle)
                {
                    next[vehicle] = owner;
                }
            }

            var state = new Dictionary<int, int>();
            foreach (int start in next.Keys)
            {
                if (state.TryGetValue(start, out int s) && s != 0)
                {
                    continue;
                }

                var stack = new List<int>();
                int node = start;
                while (true)
                {
                    if (!next.ContainsKey(node))
                    {
                        foreach (int v in stack)
                        {
                            state[v] = 2;
                        }

                        state[node] = 2;
                        break;
                    }

                    if (state.TryGetValue(node, out int ns) && ns == 1)
                    {
                        int idx = stack.IndexOf(node);
                        var cycle = new List<int>();
                        for (int i = idx; i < stack.Count; i++)
                        {
                            cycle.Add(stack[i]);
                        }

                        cycle.Sort();
                        return cycle;
                    }

                    if (state.TryGetValue(node, out int ns2) && ns2 == 2)
                    {
                        break;
                    }

                    state[node] = 1;
                    stack.Add(node);
                    node = next[node];
                }
            }

            return Array.Empty<int>();
        }
    }
}
