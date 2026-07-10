using NUnit.Framework;
using FabSim.Sim;
using FabSim.Twin;

namespace FabSim.Tests.EditMode
{
    /// <summary>
    /// TwinReplaySource가 NDJSON 이벤트 스트림에서 비히클 상태·KPI를 정확히 재구성하는지
    /// 검증한다. 뷰는 소스를 모르므로, 소스가 채운 VehicleAgent 필드가 맞으면 같은 뷰가 그대로 돈다.
    /// </summary>
    public sealed class TwinReplaySourceTests
    {
        [Test]
        public void ReconstructsFleetFromDeltaStream()
        {
            string[] lines =
            {
                "{\"n\":0,\"t\":0,\"type\":\"HELLO\",\"v\":1,\"layout\":\"loop-12\",\"nodeCount\":12,\"portNodes\":[1,2,3,7,8,9],\"vehicleCount\":2}",
                "{\"n\":1,\"t\":10,\"type\":\"JOB_CREATE\",\"jid\":0,\"from\":2,\"to\":8}",
                "{\"n\":2,\"t\":10,\"type\":\"DISPATCH\",\"jid\":0,\"vid\":0}",
                "{\"n\":3,\"t\":10,\"type\":\"EDGE_DEPART\",\"vid\":0,\"from\":0,\"to\":1,\"eta\":14}",
                "{\"n\":4,\"t\":14,\"type\":\"EDGE_ARRIVE\",\"vid\":0,\"node\":1}",
                "{\"n\":5,\"t\":18,\"type\":\"PICKUP\",\"vid\":0,\"jid\":0}",
                "{\"n\":6,\"t\":30,\"type\":\"JOB_COMPLETE\",\"vid\":0,\"jid\":0}",
            };

            var source = new TwinReplaySource(lines, 0.0);
            source.ApplyAll();

            Assert.That(source.Vehicles.Count, Is.EqualTo(2));
            Assert.That(source.CompletedJobs, Is.EqualTo(1));
            Assert.That(source.PendingJobCount, Is.EqualTo(0));

            VehicleAgent vehicle = source.Vehicles[0];
            Assert.That(vehicle.Phase, Is.EqualTo(VehiclePhase.Idle));
            Assert.That(vehicle.IsMoving, Is.False);
            Assert.That(vehicle.NodeId, Is.EqualTo(1));
            // BusyStartedAt=10(DISPATCH) → JOB_COMPLETE(30) → 누적 20.
            Assert.That(vehicle.TotalBusyTime, Is.EqualTo(20.0).Within(1e-9));
            // 반송 시간 = 완료(30) − 생성(10) = 20.
            Assert.That(source.GetAverageDeliveryTime(), Is.EqualTo(20.0).Within(1e-9));
        }

        [Test]
        public void SnapshotOverwritesEntireState()
        {
            string[] lines =
            {
                "{\"n\":0,\"t\":0,\"type\":\"HELLO\",\"v\":1,\"nodeCount\":12,\"portNodes\":[1,2,3,7,8,9],\"vehicleCount\":2}",
                "{\"n\":1,\"t\":60,\"type\":\"SNAPSHOT\",\"v\":1,\"vehicles\":[" +
                "{\"vid\":0,\"phase\":\"Carrying\",\"busy\":15,\"moving\":true,\"from\":5,\"to\":6,\"depart\":58,\"eta\":62,\"jid\":3}," +
                "{\"vid\":1,\"phase\":\"Idle\",\"busy\":0,\"moving\":false,\"node\":8}]," +
                "\"stats\":{\"completed\":5,\"pending\":2}}",
            };

            var source = new TwinReplaySource(lines, 0.0);
            source.ApplyAll();

            Assert.That(source.CompletedJobs, Is.EqualTo(5));
            Assert.That(source.PendingJobCount, Is.EqualTo(2));

            VehicleAgent moving = source.Vehicles[0];
            Assert.That(moving.Phase, Is.EqualTo(VehiclePhase.Carrying));
            Assert.That(moving.IsMoving, Is.True);
            Assert.That(moving.EdgeFromNode, Is.EqualTo(5));
            Assert.That(moving.EdgeToNode, Is.EqualTo(6));
            Assert.That(moving.EdgeArriveAt, Is.EqualTo(62.0).Within(1e-9));

            VehicleAgent idle = source.Vehicles[1];
            Assert.That(idle.Phase, Is.EqualTo(VehiclePhase.Idle));
            Assert.That(idle.IsMoving, Is.False);
            Assert.That(idle.NodeId, Is.EqualTo(8));
        }

        [Test]
        public void PlayoutDelayHoldsEventsUntilHorizon()
        {
            string[] lines =
            {
                "{\"n\":0,\"t\":0,\"type\":\"HELLO\",\"v\":1,\"nodeCount\":12,\"portNodes\":[1,2,3,7,8,9],\"vehicleCount\":1}",
                "{\"n\":1,\"t\":1.0,\"type\":\"JOB_CREATE\",\"jid\":0,\"from\":2,\"to\":8}",
                "{\"n\":2,\"t\":1.0,\"type\":\"DISPATCH\",\"jid\":0,\"vid\":0}",
                "{\"n\":3,\"t\":1.0,\"type\":\"JOB_COMPLETE\",\"vid\":0,\"jid\":0}",
            };

            var source = new TwinReplaySource(lines, 0.5);

            // now=0.4 → horizon=-0.1 → 아직 아무 것도 적용 안 됨(플레이아웃 지연).
            source.Tick(0.4f);
            Assert.That(source.CompletedJobs, Is.EqualTo(0));

            // now=1.6 → horizon=1.1 ≥ 1.0 → 이벤트 적용됨.
            source.Tick(1.2f);
            Assert.That(source.CompletedJobs, Is.EqualTo(1));
        }
    }
}
