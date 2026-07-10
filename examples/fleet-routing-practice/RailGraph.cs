using System;
using System.Collections.Generic;

namespace FleetRouting
{
    /// <summary>
    /// 실습용 RailGraph 스켈레톤. 다익스트라(FindPath)와 비용 주입 다익스트라는 완성 상태이고,
    /// A*(FindPathAStar)만 TODO 스텁이다. 스텁은 다익스트라 결과를 그대로 돌려주되 확장 노드를
    /// 전체 노드 수로 둬, 실험 A에서 "A* 확장노드 = 전체(개선 없음)"로 나와 미완성이 드러난다.
    /// TODO를 채우면 완성본 examples/fleet-routing/과 같은 수치가 나온다.
    /// </summary>
    public sealed class RailGraph
    {
        /// <summary>그래프 노드의 3D 좌표.</summary>
        public readonly struct NodePoint
        {
            public readonly float X;
            public readonly float Y;
            public readonly float Z;

            public NodePoint(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        private readonly List<NodePoint> nodes = new List<NodePoint>();
        private readonly List<List<int>> outgoing = new List<List<int>>();

        /// <summary>노드 수.</summary>
        public int NodeCount => nodes.Count;

        /// <summary>노드를 추가하고 id를 반환한다.</summary>
        public int AddNode(float x, float y, float z)
        {
            nodes.Add(new NodePoint(x, y, z));
            outgoing.Add(new List<int>());
            return nodes.Count - 1;
        }

        /// <summary>from → to 방향성 엣지를 추가한다.</summary>
        public void AddEdge(int from, int to)
        {
            outgoing[from].Add(to);
        }

        /// <summary>노드 좌표를 반환한다.</summary>
        public NodePoint GetNode(int id)
        {
            return nodes[id];
        }

        /// <summary>노드에서 나가는 엣지들의 도착 노드 목록.</summary>
        public IReadOnlyList<int> GetOutgoing(int node)
        {
            return outgoing[node];
        }

        /// <summary>두 노드 사이의 유클리드 거리.</summary>
        public float GetDistance(int from, int to)
        {
            NodePoint a = nodes[from];
            NodePoint b = nodes[to];
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>다익스트라 최단 경로. 도달 불가면 null, start == goal이면 [start].</summary>
        public List<int> FindPath(int start, int goal)
        {
            return FindPath(start, goal, null, out _);
        }

        /// <summary>
        /// 엣지 비용을 외부에서 주입하는 다익스트라. edgeCost가 null이면 유클리드 거리를 쓴다.
        /// 확장(확정)한 노드 수를 expandedCount로 돌려준다.
        /// </summary>
        public List<int> FindPath(int start, int goal, Func<int, int, float> edgeCost, out int expandedCount)
        {
            expandedCount = 0;
            if (start == goal)
            {
                return new List<int> { start };
            }

            var bestCost = new float[nodes.Count];
            var previous = new int[nodes.Count];
            var visited = new bool[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                bestCost[i] = float.PositiveInfinity;
                previous[i] = -1;
            }

            bestCost[start] = 0f;

            while (true)
            {
                int current = -1;
                float currentCost = float.PositiveInfinity;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (!visited[i] && bestCost[i] < currentCost)
                    {
                        current = i;
                        currentCost = bestCost[i];
                    }
                }

                if (current == -1)
                {
                    return null;
                }

                if (current == goal)
                {
                    break;
                }

                visited[current] = true;
                expandedCount++;
                foreach (int next in outgoing[current])
                {
                    float step = edgeCost != null ? edgeCost(current, next) : GetDistance(current, next);
                    float cost = currentCost + step;
                    if (cost < bestCost[next])
                    {
                        bestCost[next] = cost;
                        previous[next] = current;
                    }
                }
            }

            var path = new List<int>();
            for (int node = goal; node != -1; node = previous[node])
            {
                path.Add(node);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// A* 최단 경로. heuristic(node, goal)이 허용 가능(실제 비용을 과대평가하지 않음)해야
        /// 최단성이 보장된다. 확장(확정)한 노드 수를 expandedCount로 돌려준다.
        /// </summary>
        public List<int> FindPathAStar(
            int start, int goal, Func<int, int, float> heuristic, out int expandedCount)
        {
            // TODO(실습 3): f = g + h(=heuristic)로 open 노드 중 최소 f를 골라 확장하는 A*를
            //   구현한다. g는 출발점부터의 실비용, closed 처리한 노드 수를 expandedCount로 센다.
            //   허용 가능 heuristic(직선거리)이면 다익스트라와 같은 경로를 더 적은 확장으로 찾는다.
            // 스텁 동작: 다익스트라 결과를 그대로 반환하고 확장 노드를 전체 노드 수로 둔다
            //   → 실험 A에서 "A* 확장노드 = 전체(개선 없음)"로 나와 미완성이 드러난다.
            List<int> path = FindPath(start, goal, null, out _);
            expandedCount = nodes.Count;
            return path;
        }

        /// <summary>경로의 총 길이.</summary>
        public float GetPathLength(List<int> path)
        {
            float length = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                length += GetDistance(path[i - 1], path[i]);
            }

            return length;
        }
    }
}
