using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace FabSim.EditorTools
{
    /// <summary>
    /// 선택한 모델의 정점 수·서브메시 수·고유 재질 수·텍스처 메모리 추정을 요약하는 창.
    /// 재질 통합·자동화 전/후를 숫자로 대조하는 대시보드다(FabSim/Import Report 메뉴).
    /// </summary>
    public sealed class ImportReportWindow : EditorWindow
    {
        [MenuItem("FabSim/Import Report")]
        private static void Open()
        {
            GetWindow<ImportReportWindow>("Import Report");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("임포트 리포트", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "하이어라키/프로젝트에서 모델을 선택하면 지표를 요약합니다. 재질 통합 전/후를 대조하세요.",
                MessageType.Info);

            GameObject target = Selection.activeGameObject;
            if (target == null)
            {
                EditorGUILayout.LabelField("선택된 모델이 없습니다.");
                return;
            }

            int vertices = 0;
            int submeshes = 0;
            var materials = new HashSet<Material>();
            var textures = new HashSet<Texture>();

            foreach (MeshFilter filter in target.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh != null)
                {
                    vertices += mesh.vertexCount;
                    submeshes += mesh.subMeshCount;
                }
            }

            foreach (MeshRenderer renderer in target.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    materials.Add(material);
                    CollectTextures(material, textures);
                }
            }

            long textureBytes = 0;
            foreach (Texture texture in textures)
            {
                textureBytes += Profiler.GetRuntimeMemorySizeLong(texture);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("대상", target.name);
            EditorGUILayout.LabelField("총 정점 수", vertices.ToString("N0"));
            EditorGUILayout.LabelField("서브메시 수 (드로우콜 하한)", submeshes.ToString());
            EditorGUILayout.LabelField("고유 재질 수", materials.Count.ToString());
            EditorGUILayout.LabelField("텍스처 메모리(추정)", EditorUtility.FormatBytes(textureBytes));
        }

        private static void CollectTextures(Material material, HashSet<Texture> textures)
        {
            Shader shader = material.shader;
            int count = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < count; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    continue;
                }

                Texture texture = material.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                if (texture != null)
                {
                    textures.Add(texture);
                }
            }
        }
    }
}
