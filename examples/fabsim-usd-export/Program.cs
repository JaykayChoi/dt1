using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FabSim.Sim;

namespace FabSim.UsdExport
{
    /// <summary>
    /// FabSim의 도메인 모델(RailGraph·FabModel.Vehicles)을 순회해 .usda 파일로 익스포트한다.
    /// </summary>
    /// <remarks>
    /// twin-bridge Recorder와 같은 패턴 — Sim/*.cs를 링크해 Unity 없이 헤드리스로 돈다.
    /// 커리큘럼 관통선: FabSim(C# DES) → USD(.usda) → Omniverse(Phase 12). USD SDK는
    /// 쓰지 않는다. .usda는 텍스트라 문자열로 직렬화하면 되고, 검증은 usd-core(Phase 11)로 한다.
    /// </remarks>
    internal static class Program
    {
        private const float VehicleColorR = 0.55f;
        private const float VehicleColorG = 0.57f;
        private const float VehicleColorB = 0.60f;

        private static int Main(string[] args)
        {
            int vehicleCount = ReadInt(args, "--vehicles", 8);
            int seed = ReadInt(args, "--seed", 7);
            string outPath = ReadString(args, "--out", "fab.usda");

            RailGraph graph = FabLayout.Build(out int[] portNodes);
            var model = new FabModel(graph, portNodes, vehicleCount, 20.0, 2.5, seed);

            string usda = BuildUsda(graph, portNodes, model);
            File.WriteAllText(outPath, usda);

            Console.WriteLine(
                $"wrote {outPath}: rail nodes {graph.NodeCount}, ports {portNodes.Length}, " +
                $"vehicles {model.Vehicles.Count} (OHT_00..)");
            Console.WriteLine("검증: python -c \"from pxr import Usd; Usd.Stage.Open('" + outPath + "')\"  또는  usdchecker");
            return 0;
        }

        private static string BuildUsda(RailGraph graph, int[] portNodes, FabModel model)
        {
            var w = new UsdaWriter();
            w.Header("World");

            w.BeginPrim("Xform", "World");
            w.BeginPrim("Xform", "Fab");

            WriteRail(w, graph);
            WritePorts(w, graph, portNodes);
            WriteVehicles(w, graph, model);

            w.EndPrim();
            w.EndPrim();
            return w.ToString();
        }

        private static void WriteRail(UsdaWriter w, RailGraph graph)
        {
            // 일방통행 루프 — 노드를 순서대로 잇고 첫 노드로 닫아 폐곡선을 만든다.
            var points = new List<(float X, float Y, float Z)>(graph.NodeCount + 1);
            for (int i = 0; i < graph.NodeCount; i++)
            {
                RailGraph.NodePoint node = graph.GetNode(i);
                points.Add((node.X, node.Y, node.Z));
            }

            RailGraph.NodePoint first = graph.GetNode(0);
            points.Add((first.X, first.Y, first.Z));

            w.BeginPrim("BasisCurves", "Rail");
            w.CurveVertexCounts(points.Count);
            w.Points(points);
            w.Raw("uniform token type = \"linear\"");
            w.Raw("float[] widths = [0.15] (interpolation = \"constant\")");
            w.DisplayColor(0.20f, 0.22f, 0.25f);
            w.EndPrim();
        }

        private static void WritePorts(UsdaWriter w, RailGraph graph, int[] portNodes)
        {
            w.BeginPrim("Scope", "Ports");
            for (int i = 0; i < portNodes.Length; i++)
            {
                RailGraph.NodePoint node = graph.GetNode(portNodes[i]);
                w.BeginPrim("Cube", $"Port_{i:00}");
                w.Raw("double size = 0.6");
                w.Translate(node.X, node.Y, node.Z);
                w.DisplayColor(0.90f, 0.70f, 0.20f);
                w.EndPrim();
            }

            w.EndPrim();
        }

        private static void WriteVehicles(UsdaWriter w, RailGraph graph, FabModel model)
        {
            // 각 비히클을 이름 붙은 Xform(OHT_xx)으로 심고, 드로어블 Cube "Body"를 자식으로 둔다.
            // Phase 12 Kit 확장이 이름으로 스캔하고 Body의 displayColor를 상태색으로 칠한다.
            foreach (VehicleAgent vehicle in model.Vehicles)
            {
                RailGraph.NodePoint node = graph.GetNode(vehicle.NodeId);
                w.BeginPrim("Xform", $"OHT_{vehicle.Id:00}");
                w.Translate(node.X, node.Y, node.Z);
                w.BeginPrim("Cube", "Body");
                w.Raw("double size = 1");
                w.DisplayColor(VehicleColorR, VehicleColorG, VehicleColorB);
                w.EndPrim();
                w.EndPrim();
            }
        }

        private static int ReadInt(string[] args, string flag, int fallback)
        {
            string value = ReadString(args, flag, null);
            if (value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            return fallback;
        }

        private static string ReadString(string[] args, string flag, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag)
                {
                    return args[i + 1];
                }
            }

            return fallback;
        }
    }
}
