using System;
using System.Collections.Generic;

namespace FleetRouting
{
    /// <summary>
    /// 레일 웨이포인트 그래프 — 노드(3D 좌표)와 방향성 엣지. UnityEngine 무의존
    /// 순수 C#이라 헤드리스로 돈다. FabSim.Sim.RailGraph와 동일 스펙의 의도적 복제
    /// (DesCore가 Unity로 복제된 것과 같은 방식).
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

        /// <summary>
        /// 다익스트라 최단 경로. start와 goal을 포함한 노드 시퀀스를 반환하고,
        /// 도달 불가능하면 null을 반환한다. start == goal이면 [start].
        /// </summary>
        public List<int> FindPath(int start, int goal)
        {
            return FindPath(start, goal, null, out _);
        }

        /// <summary>
        /// 엣지 비용을 외부에서 주입하는 다익스트라. edgeCost(from, to)가 null이면
        /// 유클리드 거리를 쓴다. 확장(확정)한 노드 수를 expandedCount로 돌려준다.
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
            expandedCount = 0;
            if (start == goal)
            {
                return new List<int> { start };
            }

            var gCost = new float[nodes.Count];
            var previous = new int[nodes.Count];
            var closed = new bool[nodes.Count];
            var open = new bool[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                gCost[i] = float.PositiveInfinity;
                previous[i] = -1;
            }

            gCost[start] = 0f;
            open[start] = true;

            while (true)
            {
                // f = g + h가 최소인 open 노드를 고른다(작은 그래프라 선형 탐색으로 충분).
                int current = -1;
                float bestF = float.PositiveInfinity;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (!open[i])
                    {
                        continue;
                    }

                    float f = gCost[i] + heuristic(i, goal);
                    if (f < bestF)
                    {
                        bestF = f;
                        current = i;
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

                open[current] = false;
                closed[current] = true;
                expandedCount++;
                foreach (int next in outgoing[current])
                {
                    if (closed[next])
                    {
                        continue;
                    }

                    float tentative = gCost[current] + GetDistance(current, next);
                    if (tentative < gCost[next])
                    {
                        gCost[next] = tentative;
                        previous[next] = current;
                        open[next] = true;
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
