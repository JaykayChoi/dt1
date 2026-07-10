using System;
using System.Globalization;
using System.IO;
using System.Text;
using FabSim.Sim;

namespace TwinBridge.Recorder
{
    /// <summary>
    /// 헤드리스 레코더 — FabModel을 Unity 없이 돌리며 상태 변화를 NDJSON 이벤트 스트림으로
    /// 파일에 굽는다. 같은 시드면 같은 로그(결정성). 이 로그를 ReplayServer가 재생하고,
    /// Unity의 TwinFeedClient가 받아 같은 뷰를 구동한다.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string outPath = ArgValue(args, "--out", "sample.ndjson");
            double until = double.Parse(ArgValue(args, "--until", "3600"), CultureInfo.InvariantCulture);
            int seed = int.Parse(ArgValue(args, "--seed", "7"));
            int vehicles = int.Parse(ArgValue(args, "--vehicles", "4"));
            double jobInterval = double.Parse(ArgValue(args, "--interval", "25"), CultureInfo.InvariantCulture);
            double snapshot = double.Parse(ArgValue(args, "--snapshot", "60"), CultureInfo.InvariantCulture);

            RailGraph graph = FabLayout.Build(out int[] portNodes);
            var model = new FabModel(graph, portNodes, vehicles, jobInterval, 2.5, seed);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath)));
            using (var writer = new StreamWriter(outPath, false, new UTF8Encoding(false)))
            {
                var recorder = new EventRecorder(model, writer, snapshot);
                recorder.Begin(graph.NodeCount, portNodes, vehicles);
                model.Start();

                double t = 0.0;
                while (t < until)
                {
                    t = Math.Min(t + snapshot, until);
                    model.Simulation.Run(t);
                    recorder.PumpSnapshots(t);
                }

                writer.Flush();
            }

            Console.WriteLine($"레코딩 완료 → {outPath}");
            Console.WriteLine(
                $"  파라미터: until={until:F0}s · seed={seed} · vehicles={vehicles} · " +
                $"jobInterval={jobInterval:F0}s · snapshot={snapshot:F0}s");
            Console.WriteLine(
                $"  완료 반송 {model.CompletedJobs}건 · 라인 {CountLines(outPath)}줄 · " +
                $"모든 차량 Idle: {AllIdle(model)}");
        }

        private static bool AllIdle(FabModel model)
        {
            foreach (VehicleAgent vehicle in model.Vehicles)
            {
                if (vehicle.Phase != VehiclePhase.Idle)
                {
                    return false;
                }
            }

            return true;
        }

        private static int CountLines(string path)
        {
            int count = 0;
            foreach (string _ in File.ReadLines(path))
            {
                count++;
            }

            return count;
        }

        private static string ArgValue(string[] args, string key, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == key)
                {
                    return args[i + 1];
                }
            }

            return fallback;
        }
    }
}
