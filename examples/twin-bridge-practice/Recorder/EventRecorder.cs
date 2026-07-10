using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FabSim.Sim;

namespace TwinBridge.Recorder
{
    /// <summary>
    /// 실습용 EventRecorder 스켈레톤. HELLO 헤더·이벤트 구독·주기 SNAPSHOT은 채워져 있고,
    /// 각 델타 이벤트를 JSON 라인으로 직렬화하는 부분만 TODO 스텁이다. 스텁 상태로 실행하면
    /// type·n만 찍힌 "필드 없는 반쪽 로그"가 나와 미완성이 드러난다. 정답지는
    /// examples/twin-bridge/Recorder/EventRecorder.cs다.
    /// </summary>
    public sealed class EventRecorder
    {
        private readonly FabModel model;
        private readonly TextWriter writer;
        private readonly double snapshotInterval;
        private readonly StringBuilder line = new StringBuilder(256);
        private long sequence;
        private double lastSnapshot;

        public EventRecorder(FabModel model, TextWriter writer, double snapshotInterval)
        {
            this.model = model;
            this.writer = writer;
            this.snapshotInterval = snapshotInterval;
        }

        /// <summary>모델 이벤트를 구독하고 HELLO 헤더를 쓴다.</summary>
        public void Begin(int nodeCount, int[] portNodes, int vehicleCount)
        {
            line.Clear();
            line.Append("{\"n\":").Append(sequence++).Append(",\"t\":0,\"type\":\"HELLO\",\"v\":1,")
                .Append("\"layout\":\"loop-").Append(nodeCount).Append("\",\"nodeCount\":").Append(nodeCount)
                .Append(",\"portNodes\":[");
            for (int i = 0; i < portNodes.Length; i++)
            {
                if (i > 0)
                {
                    line.Append(',');
                }

                line.Append(portNodes[i]);
            }

            line.Append("],\"vehicleCount\":").Append(vehicleCount).Append('}');
            writer.Write(line.ToString());
            writer.Write('\n');

            model.JobCreated += OnJobCreated;
            model.Dispatched += OnDispatched;
            model.EdgeDeparted += OnEdgeDeparted;
            model.EdgeArrived += OnEdgeArrived;
            model.PickedUp += OnPickedUp;
            model.JobCompleted += OnJobCompleted;
        }

        /// <summary>현재 시각까지 필요한 주기 SNAPSHOT을 흘려보낸다(러너가 루프에서 호출).</summary>
        public void PumpSnapshots(double now)
        {
            while (now - lastSnapshot >= snapshotInterval)
            {
                lastSnapshot += snapshotInterval;
                WriteSnapshot(lastSnapshot);
            }
        }

        private void OnJobCreated(TransportJob job)
        {
            BeginLine("JOB_CREATE");
            // TODO(실습 1): jid=job.Id, from=job.FromPort, to=job.ToPort 필드를 라인에 추가한다.
            EndLine();
        }

        private void OnDispatched(VehicleAgent vehicle, TransportJob job)
        {
            BeginLine("DISPATCH");
            // TODO(실습 1): jid=job.Id, vid=vehicle.Id 필드를 추가한다.
            EndLine();
        }

        private void OnEdgeDeparted(VehicleAgent vehicle)
        {
            BeginLine("EDGE_DEPART");
            // TODO(실습 1): vid·from=EdgeFromNode·to=EdgeToNode·eta=EdgeArriveAt를 추가한다
            //   (이 네 필드가 View의 EvaluatePosition 보간 입력과 1:1이다).
            EndLine();
        }

        private void OnEdgeArrived(VehicleAgent vehicle)
        {
            BeginLine("EDGE_ARRIVE");
            // TODO(실습 1): vid·node=NodeId를 추가한다.
            EndLine();
        }

        private void OnPickedUp(VehicleAgent vehicle)
        {
            BeginLine("PICKUP");
            // TODO(실습 1): vid·jid를 추가한다.
            EndLine();
        }

        private void OnJobCompleted(VehicleAgent vehicle, TransportJob job)
        {
            BeginLine("JOB_COMPLETE");
            // TODO(실습 1): vid·jid를 추가한다.
            EndLine();
        }

        private void WriteSnapshot(double now)
        {
            line.Clear();
            line.Append("{\"n\":").Append(sequence++).Append(",\"t\":").Append(Num(now))
                .Append(",\"type\":\"SNAPSHOT\",\"v\":1,\"vehicles\":[");
            IReadOnlyList<VehicleAgent> vehicles = model.Vehicles;
            for (int i = 0; i < vehicles.Count; i++)
            {
                VehicleAgent vehicle = vehicles[i];
                if (i > 0)
                {
                    line.Append(',');
                }

                line.Append("{\"vid\":").Append(vehicle.Id)
                    .Append(",\"phase\":\"").Append(vehicle.Phase).Append('"')
                    .Append(",\"busy\":").Append(Num(vehicle.TotalBusyTime));
                if (vehicle.IsMoving)
                {
                    line.Append(",\"moving\":true,\"from\":").Append(vehicle.EdgeFromNode)
                        .Append(",\"to\":").Append(vehicle.EdgeToNode)
                        .Append(",\"depart\":").Append(Num(vehicle.EdgeDepartAt))
                        .Append(",\"eta\":").Append(Num(vehicle.EdgeArriveAt));
                }
                else
                {
                    line.Append(",\"moving\":false,\"node\":").Append(vehicle.NodeId);
                }

                if (vehicle.CurrentJob != null)
                {
                    line.Append(",\"jid\":").Append(vehicle.CurrentJob.Id);
                }

                line.Append('}');
            }

            line.Append("],\"stats\":{\"completed\":").Append(model.CompletedJobs)
                .Append(",\"pending\":").Append(model.PendingJobCount).Append("}}");
            writer.Write(line.ToString());
            writer.Write('\n');
        }

        private void BeginLine(string type)
        {
            line.Clear();
            line.Append("{\"n\":").Append(sequence++)
                .Append(",\"t\":").Append(Num(model.Simulation.Now))
                .Append(",\"type\":\"").Append(type).Append('"');
        }

        private void EndLine()
        {
            line.Append('}');
            writer.Write(line.ToString());
            writer.Write('\n');
        }

        private static string Num(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
