using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FabSim.Sim;

namespace TwinBridge.Recorder
{
    /// <summary>
    /// FabModel 이벤트를 NDJSON 라인으로 직렬화해 TextWriter에 기록한다. 한 줄에 완결된
    /// JSON 객체 하나(줄바꿈 프레이밍)를 쓰고, 주기적으로 전체 상태 SNAPSHOT을 흘려보낸다.
    /// 직렬화는 이 레코더에만 존재한다 — Sim 코드에는 손대지 않는다.
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
            line.Append(",\"jid\":").Append(job.Id)
                .Append(",\"from\":").Append(job.FromPort)
                .Append(",\"to\":").Append(job.ToPort);
            EndLine();
        }

        private void OnDispatched(VehicleAgent vehicle, TransportJob job)
        {
            BeginLine("DISPATCH");
            line.Append(",\"jid\":").Append(job.Id).Append(",\"vid\":").Append(vehicle.Id);
            EndLine();
        }

        private void OnEdgeDeparted(VehicleAgent vehicle)
        {
            BeginLine("EDGE_DEPART");
            line.Append(",\"vid\":").Append(vehicle.Id)
                .Append(",\"from\":").Append(vehicle.EdgeFromNode)
                .Append(",\"to\":").Append(vehicle.EdgeToNode)
                .Append(",\"eta\":").Append(Num(vehicle.EdgeArriveAt));
            EndLine();
        }

        private void OnEdgeArrived(VehicleAgent vehicle)
        {
            BeginLine("EDGE_ARRIVE");
            line.Append(",\"vid\":").Append(vehicle.Id).Append(",\"node\":").Append(vehicle.NodeId);
            EndLine();
        }

        private void OnPickedUp(VehicleAgent vehicle)
        {
            BeginLine("PICKUP");
            line.Append(",\"vid\":").Append(vehicle.Id)
                .Append(",\"jid\":").Append(vehicle.CurrentJob != null ? vehicle.CurrentJob.Id : -1);
            EndLine();
        }

        private void OnJobCompleted(VehicleAgent vehicle, TransportJob job)
        {
            BeginLine("JOB_COMPLETE");
            line.Append(",\"vid\":").Append(vehicle.Id).Append(",\"jid\":").Append(job.Id);
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
