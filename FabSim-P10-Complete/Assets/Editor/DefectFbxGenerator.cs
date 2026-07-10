using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

namespace FabSim.EditorTools
{
    /// <summary>
    /// 외부 다운로드 없이 "결함 있는 벤더 FBX"를 만들어 내보낸다 — 과다 재질·100배 스케일·
    /// 난잡한 계층을 일부러 주입한다. 실습에서는 이 산출 FBX를 FabSim으로 복사해 길들인다.
    /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.DefectFbxGenerator.Generate
    /// </summary>
    public static class DefectFbxGenerator
    {
        private const string ExportFolder = "Assets/Vendor";
        private const string ExportPath = ExportFolder + "/oht_station_raw.fbx";
        private const float DefectScale = 100f;
        private const int MaterialCount = 10;

        /// <summary>프리미티브 조합 설비 모형을 만들고 결함을 주입해 FBX로 내보낸다.</summary>
        [MenuItem("FabSim/Generate Defect FBX")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(ExportFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Vendor");
            }

            GameObject root = BuildDefectHierarchy();
            try
            {
                ModelExporter.ExportObject(ExportPath, root);
                AssetDatabase.Refresh();
                Debug.Log($"결함 FBX 생성 완료 → {ExportPath} (재질 {MaterialCount}개·{DefectScale:F0}배 스케일)");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // 의미 없는 빈 부모 중첩 + 부품마다 개별 재질(공유 없음) + 100배 스케일 + 이격 피벗.
        private static GameObject BuildDefectHierarchy()
        {
            var root = new GameObject("oht_station");
            var wrapperA = new GameObject("export_group");
            var wrapperB = new GameObject("node_0");
            wrapperA.transform.SetParent(root.transform);
            wrapperB.transform.SetParent(wrapperA.transform);

            for (int i = 0; i < MaterialCount; i++)
            {
                GameObject part = GameObject.CreatePrimitive(
                    i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                part.name = $"part_{i:D2}";
                part.transform.SetParent(wrapperB.transform);
                // 피벗을 지오메트리 중심에서 이격시키고 부품을 흩어 놓는다.
                part.transform.localPosition = new Vector3(i * 1.5f, 0f, (i % 3) * 1.2f);
                part.GetComponent<MeshRenderer>().sharedMaterial = CreateUniqueMaterial(i);
                Object.DestroyImmediate(part.GetComponent<Collider>());
            }

            // 전체를 100배로 부풀려 단위 불일치를 흉내낸다.
            root.transform.localScale = Vector3.one * DefectScale;
            return root;
        }

        private static Material CreateUniqueMaterial(int index)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = $"vendor_mat_{index:D2}",
                color = Color.HSVToRGB(index / (float)MaterialCount, 0.6f, 0.8f),
            };
            return material;
        }
    }
}
