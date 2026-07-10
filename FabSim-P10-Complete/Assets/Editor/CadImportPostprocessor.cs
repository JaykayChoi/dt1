using UnityEditor;
using UnityEngine;

namespace FabSim.EditorTools
{
    /// <summary>
    /// Assets/Vendor/ 하위 모델 임포트를 자동 교정한다 — 스케일·Read/Write·메시 압축·재질 통합·
    /// 콜라이더·정적 플래그. 벤더가 준 결함 FBX를 손 안 대도 배칭되는 상태로 길들인다.
    /// Unity가 리플렉션으로 이 훅들을 호출하므로 접근자는 private, 클래스는 sealed다.
    /// </summary>
    public sealed class CadImportPostprocessor : AssetPostprocessor
    {
        private const string VendorRoot = "Assets/Vendor/";
        private const float MillimeterToMeter = 0.01f;

        /// <summary>메시 생성 전 — ModelImporter 설정을 교정한다.</summary>
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(VendorRoot))
            {
                return;
            }

            var importer = assetImporter as ModelImporter;
            if (importer == null)
            {
                return;
            }

            // 단위 불일치 보정(mm → m), 런타임 메시 접근 차단(메모리 2배 방지),
            // 메시 압축·정점 용접·최적화로 가볍게, 노멀맵용 탄젠트 계산.
            importer.useFileScale = false;
            importer.globalScale = MillimeterToMeter;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.weldVertices = true;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
        }

        /// <summary>생성된 GameObject 트리 후처리 — 재질 통합·콜라이더·정적 플래그.</summary>
        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(VendorRoot))
            {
                return;
            }

            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                UnifyMaterials(renderer);
                AttachCollider(renderer.gameObject);
            }

            GameObjectUtility.SetStaticEditorFlags(
                root,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic
                    | StaticEditorFlags.OccludeeStatic);
        }

        // 서브메시들의 재질을 첫 재질로 통일해 드로우콜을 줄인다(같은 재질끼리 배칭 가능).
        // 복제(material)가 아니라 sharedMaterials로 다뤄 배칭을 유지한다.
        private static void UnifyMaterials(MeshRenderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials.Length <= 1)
            {
                return;
            }

            for (int i = 1; i < materials.Length; i++)
            {
                materials[i] = materials[0];
            }

            renderer.sharedMaterials = materials;
        }

        private static void AttachCollider(GameObject target)
        {
            if (target.GetComponent<MeshCollider>() == null)
            {
                target.AddComponent<MeshCollider>();
            }
        }
    }
}
