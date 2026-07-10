using System.Collections.Generic;
using NUnit.Framework;
using FabSim.Sim;

namespace FabSim.Tests.EditMode
{
    /// <summary>
    /// RailGraph 다익스트라 최단 경로의 정답·도달불가·자기자신 경우를 검증한다.
    /// 답을 손으로 계산할 수 있는 소형 그래프로 확인한다.
    /// </summary>
    public sealed class RailGraphTests
    {
        [Test]
        public void FindPathReturnsKnownShortestPath()
        {
            var graph = new RailGraph();
            int n0 = graph.AddNode(0f, 0f, 0f);
            int n1 = graph.AddNode(3f, 0f, 0f);
            int n2 = graph.AddNode(3f, 4f, 0f);
            int n3 = graph.AddNode(6f, 0f, 0f);

            // n0→n1→n3 = 3 + 3 = 6  <  n0→n2→n3 = 5 + 5 = 10
            graph.AddEdge(n0, n1);
            graph.AddEdge(n1, n3);
            graph.AddEdge(n0, n2);
            graph.AddEdge(n2, n3);

            List<int> path = graph.FindPath(n0, n3);
            Assert.That(path, Is.EqualTo(new List<int> { n0, n1, n3 }));
            Assert.That(graph.GetPathLength(path), Is.EqualTo(6f).Within(1e-3f));
        }

        [Test]
        public void FindPathReturnsNullWhenUnreachable()
        {
            var graph = new RailGraph();
            int n0 = graph.AddNode(0f, 0f, 0f);
            int n1 = graph.AddNode(3f, 0f, 0f);
            int n4 = graph.AddNode(9f, 0f, 0f);

            graph.AddEdge(n0, n1);
            // n4는 n0으로 들어오기만 하고, n0에서 n4로 가는 경로가 없다.
            graph.AddEdge(n4, n0);

            Assert.That(graph.FindPath(n0, n4), Is.Null);
        }

        [Test]
        public void FindPathStartEqualsGoalReturnsSingleNode()
        {
            var graph = new RailGraph();
            int n0 = graph.AddNode(0f, 0f, 0f);
            graph.AddNode(3f, 0f, 0f);

            Assert.That(graph.FindPath(n0, n0), Is.EqualTo(new List<int> { n0 }));
        }
    }
}
