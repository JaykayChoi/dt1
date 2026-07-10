namespace FabSim.Sim
{
    /// <summary>
    /// 미니 팹의 레일 레이아웃 정의 — 일방통행 직사각 루프 + 포트 노드.
    /// 씬 빌더(시각물)와 런타임(그래프)이 같은 정의를 공유해 어긋남을 막는다.
    /// 일방통행 루프는 Phase 1에서 배운 데드락 구조적 회피이기도 하다.
    /// </summary>
    public static class FabLayout
    {
        /// <summary>레일 높이 [m].</summary>
        public const float RailHeight = 4f;

        /// <summary>루프의 X 반폭 [m].</summary>
        public const float HalfWidth = 6f;

        /// <summary>루프의 Z 반길이 [m].</summary>
        public const float HalfLength = 16f;

        /// <summary>
        /// 일방통행 루프 그래프를 만든다. 반환된 portNodes는 픽업/드롭이
        /// 일어나는 노드 id들(긴 변의 비코너 노드 6개)이다.
        /// </summary>
        public static RailGraph Build(out int[] portNodes)
        {
            var graph = new RailGraph();
            float y = RailHeight;

            // 반시계 방향 일방통행 루프 — 12개 노드.
            int n0 = graph.AddNode(-HalfWidth, y, -HalfLength);
            int n1 = graph.AddNode(-HalfWidth, y, -8f);
            int n2 = graph.AddNode(-HalfWidth, y, 0f);
            int n3 = graph.AddNode(-HalfWidth, y, 8f);
            int n4 = graph.AddNode(-HalfWidth, y, HalfLength);
            int n5 = graph.AddNode(0f, y, HalfLength);
            int n6 = graph.AddNode(HalfWidth, y, HalfLength);
            int n7 = graph.AddNode(HalfWidth, y, 8f);
            int n8 = graph.AddNode(HalfWidth, y, 0f);
            int n9 = graph.AddNode(HalfWidth, y, -8f);
            int n10 = graph.AddNode(HalfWidth, y, -HalfLength);
            int n11 = graph.AddNode(0f, y, -HalfLength);

            int[] loop = { n0, n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11 };
            for (int i = 0; i < loop.Length; i++)
            {
                graph.AddEdge(loop[i], loop[(i + 1) % loop.Length]);
            }

            // 긴 변의 중간 노드들이 장비 로드포트 위 정지 지점이다.
            portNodes = new[] { n1, n2, n3, n7, n8, n9 };
            return graph;
        }
    }
}
