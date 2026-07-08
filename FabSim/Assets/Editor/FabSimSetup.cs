using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FabSim.EditorTools
{
    /// <summary>
    /// 배치 모드에서 URP 파이프라인 에셋을 생성하고 프로젝트에 할당하는 셋업 도구.
    /// </summary>
    public static class FabSimSetup
    {
        private const string SettingsFolder = "Assets/Settings";

        /// <summary>
        /// URP 렌더러·파이프라인 에셋을 만들고 Graphics/Quality 설정에 할당한다.
        /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.FabSimSetup.ConfigureUrp
        /// </summary>
        public static void ConfigureUrp()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, SettingsFolder + "/FabSimRenderer.asset");

            UniversalRenderPipelineAsset pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipeline, SettingsFolder + "/FabSimUrp.asset");

            GraphicsSettings.defaultRenderPipeline = pipeline;

            // 모든 퀄리티 레벨에 같은 파이프라인을 할당해 레벨 전환 시 URP가 풀리지 않게 한다.
            int originalLevel = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }

            QualitySettings.SetQualityLevel(originalLevel, false);

            AssetDatabase.SaveAssets();
            Debug.Log($"[FabSimSetup] URP configured: {AssetDatabase.GetAssetPath(pipeline)}");
        }
    }
}
