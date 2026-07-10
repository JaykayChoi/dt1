using NUnit.Framework;
using DesCore;
using FabSim.Sim;

namespace FabSim.Tests.EditMode
{
    /// <summary>
    /// "가장 가까운 유휴 비히클이 배차된다"는 규칙을 코드 변경 없이 end-to-end
    /// 불변식으로 검증한다. FabModel.TryDispatch가 private이므로, 관측 가능한
    /// 상태(배차 직후 비히클의 NodeId·CurrentJob)로 확인한다.
    /// </summary>
    public sealed class DispatchSelectionTests
    {
        [Test]
        public void FirstDispatchPicksNearestIdleVehicle()
        {
            RailGraph graph = FabLayout.Build(out int[] portNodes);
            var model = new FabModel(
                graph, portNodes, vehicleCount: 2, jobIntervalMean: 5.0, vehicleSpeed: 2.5, seed: 1);
            model.Start();
            Simulation sim = model.Simulation;

            // 첫 배차가 일어날 때까지 소량씩 진행한다. 스텝(0.5)이 최소 엣지 주행 시간
            // (최소 엣지 6m / 2.5 = 2.4초)보다 작아, 배차 직후·주행 사건 실행 전 상태를 포착한다.
            VehicleAgent dispatched = null;
            while (sim.Now < 600.0)
            {
                sim.Run(sim.Now + 0.5);
                dispatched = FindDispatched(model);
                if (dispatched != null)
                {
                    break;
                }
            }

            Assert.That(dispatched, Is.Not.Null, "600초 안에 첫 배차가 발생해야 한다.");
            Assert.That(dispatched.CurrentJob, Is.Not.Null);

            // 배차 직후엔 주행 사건이 아직 실행되지 않아 NodeId가 출발 노드 그대로다.
            int fromPort = dispatched.CurrentJob.FromPort;
            VehicleAgent other = null;
            foreach (VehicleAgent vehicle in model.Vehicles)
            {
                if (vehicle != dispatched && vehicle.Phase == VehiclePhase.Idle)
                {
                    other = vehicle;
                    break;
                }
            }

            Assert.That(other, Is.Not.Null, "다른 유휴 비히클이 하나 있어야 한다.");

            float dispatchedDistance = graph.GetPathLength(graph.FindPath(dispatched.NodeId, fromPort));
            float otherDistance = graph.GetPathLength(graph.FindPath(other.NodeId, fromPort));

            // 배차된 비히클이 다른 유휴 비히클보다 픽업 지점에 가깝거나 같아야 한다.
            Assert.That(dispatchedDistance, Is.LessThanOrEqualTo(otherDistance + 1e-4f));
        }

        private static VehicleAgent FindDispatched(FabModel model)
        {
            foreach (VehicleAgent vehicle in model.Vehicles)
            {
                if (vehicle.Phase != VehiclePhase.Idle && vehicle.CurrentJob != null)
                {
                    return vehicle;
                }
            }

            return null;
        }
    }
}
