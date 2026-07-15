using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FabSim.UsdExport
{
    /// <summary>
    /// Minimal .usda (USD ASCII) text writer that emits valid USD with strings alone.
    /// </summary>
    /// <remarks>
    /// USD SDK 없이 문자열만으로 유효한 .usda를 쓴다 — "USD는 쓰기(write)는 그냥 텍스트,
    /// 읽기·합성(read/compose)만 엔진이 필요하다"는 사실을 코드로 보여 준다. Phase 11의
    /// usd-core 랩이 파이썬 API로 하던 일을, 여기서는 도메인 모델(FabSim RailGraph·Vehicles)을
    /// 순회하며 손으로 직렬화한다.
    /// </remarks>
    public sealed class UsdaWriter
    {
        private readonly StringBuilder sb = new StringBuilder();
        private int indent;

        public void Raw(string line)
        {
            Line(line);
        }

        public void Header(string defaultPrim, string upAxis = "Y", double metersPerUnit = 1)
        {
            Line("#usda 1.0");
            Line("(");
            indent++;
            Line($"defaultPrim = \"{defaultPrim}\"");
            Line($"upAxis = \"{upAxis}\"");
            Line($"metersPerUnit = {Num(metersPerUnit)}");
            indent--;
            Line(")");
            sb.Append('\n');
        }

        public void BeginPrim(string type, string name)
        {
            Line($"def {type} \"{name}\"");
            Line("{");
            indent++;
        }

        public void EndPrim()
        {
            indent--;
            Line("}");
        }

        public void Translate(float x, float y, float z)
        {
            Line($"double3 xformOp:translate = ({Num(x)}, {Num(y)}, {Num(z)})");
            Line("uniform token[] xformOpOrder = [\"xformOp:translate\"]");
        }

        public void DisplayColor(float r, float g, float b)
        {
            Line($"color3f[] primvars:displayColor = [({Num(r)}, {Num(g)}, {Num(b)})]");
        }

        public void CurveVertexCounts(int count)
        {
            Line($"int[] curveVertexCounts = [{count}]");
        }

        public void Points(IReadOnlyList<(float X, float Y, float Z)> points)
        {
            var parts = new List<string>(points.Count);
            foreach ((float X, float Y, float Z) point in points)
            {
                parts.Add($"({Num(point.X)}, {Num(point.Y)}, {Num(point.Z)})");
            }

            Line($"point3f[] points = [{string.Join(", ", parts)}]");
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        private static string Num(float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static string Num(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private void Line(string text)
        {
            sb.Append(' ', indent * 4).Append(text).Append('\n');
        }
    }
}
