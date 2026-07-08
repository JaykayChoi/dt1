using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FabSim.EditorTools
{
    /// <summary>
    /// Phase 3 완성본 씬을 배치 모드에서 자동 구축한다.
    /// docs/phase3/04-fabsim-practice.html의 실습 체크리스트를 그대로 수행:
    /// 화이트박싱 → 머티리얼 6종 → Baked 조명 → 프로브 → 포스트 프로세싱 → 베이크.
    /// </summary>
    public static class FabSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FabBay.unity";
        private const float BayWidth = 8f;
        private const float BayLength = 20f;
        private const float CeilingHeight = 5f;
        private const float RailHeight = 4f;

        private static readonly Dictionary<string, Material> Materials =
            new Dictionary<string, Material>();

        /// <summary>
        /// URP 구성(필요 시)과 완성본 씬 구축·베이크를 한 번에 수행한다.
        /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.FabSceneBuilder.SetupAndBuild
        /// </summary>
        public static void SetupAndBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    "Assets/Settings/FabSimUrp.asset") == null)
            {
                FabSimSetup.ConfigureUrp();
            }

            BuildScene();
        }

        private static void BuildScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMaterials();
            GameObject staticRoot = CreateGeometry();
            CreateLights(staticRoot.transform);
            CreateProbes();
            CreateCameraAndVolume();
            TryAddScreenSpaceAmbientOcclusion();
            ConfigureEnvironment();
            MarkStaticRecursively(staticRoot);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);

            LightingSettings lightingSettings = CreateLightingSettings();
            Lightmapping.lightingSettings = lightingSettings;
            bool isBaked = Lightmapping.Bake();
            Debug.Log($"[FabSceneBuilder] Lightmap bake finished: {(isBaked ? "OK" : "FAILED")}");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FabSceneBuilder] Scene built: {ScenePath}");

            if (!string.IsNullOrEmpty(
                    System.Environment.GetEnvironmentVariable("FABSIM_SHOT_PATH")))
            {
                CaptureScreenshot();
            }
        }

        private static void CreateMaterials()
        {
            Materials.Clear();
            CreateLitMaterial("MAT_Floor_Epoxy", new Color(0.68f, 0.74f, 0.70f), 0f, 0.7f);
            CreateLitMaterial("MAT_Wall_Paint", new Color(0.72f, 0.73f, 0.74f), 0f, 0.3f);

            // 실제 팹 장비 외장은 도장 강판(비금속)이다. 순수 금속(Metallic 1)은
            // 닫힌 어두운 공간에서 맞은편의 어둠만 비춰 검게 무너진다.
            AssetDatabase.DeleteAsset("Assets/Materials/MAT_Panel_Stainless.mat");
            CreateLitMaterial("MAT_Panel_Painted", new Color(0.66f, 0.67f, 0.68f), 0.1f, 0.45f);
            CreateLitMaterial("MAT_Rail_Alu", new Color(0.45f, 0.46f, 0.48f), 0.6f, 0.45f);
            CreateLitMaterial("MAT_FOUP_Poly", new Color(0.52f, 0.60f, 0.68f), 0f, 0.6f);

            Material statusLight = CreateLitMaterial(
                "MAT_StatusLight", new Color(0.02f, 0.05f, 0.03f), 0f, 0.4f);
            statusLight.EnableKeyword("_EMISSION");
            statusLight.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            statusLight.SetColor("_EmissionColor", new Color(0.12f, 1f, 0.28f) * 2.5f);
        }

        private static Material CreateLitMaterial(
            string name, Color baseColor, float metallic, float smoothness)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            Materials[name] = material;
            return material;
        }

        private static GameObject CreateGeometry()
        {
            var root = new GameObject("Bay (Static)");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.localScale = new Vector3(BayWidth / 10f, 1f, BayLength / 10f);
            SetMaterial(floor, "MAT_Floor_Epoxy");

            CreateBox(root.transform, "Ceiling", "MAT_Wall_Paint",
                new Vector3(0f, CeilingHeight + 0.075f, 0f),
                new Vector3(BayWidth, 0.15f, BayLength));
            CreateBox(root.transform, "Wall_Left", "MAT_Wall_Paint",
                new Vector3(-BayWidth / 2f - 0.075f, CeilingHeight / 2f, 0f),
                new Vector3(0.15f, CeilingHeight, BayLength));
            CreateBox(root.transform, "Wall_Right", "MAT_Wall_Paint",
                new Vector3(BayWidth / 2f + 0.075f, CeilingHeight / 2f, 0f),
                new Vector3(0.15f, CeilingHeight, BayLength));
            CreateBox(root.transform, "Wall_Back", "MAT_Wall_Paint",
                new Vector3(0f, CeilingHeight / 2f, BayLength / 2f + 0.075f),
                new Vector3(BayWidth, CeilingHeight, 0.15f));

            // 통로 양쪽 장비 2열 — Phase 1에서 본 베이 배치.
            float[] equipmentRows = { -6.75f, -2.25f, 2.25f, 6.75f };
            foreach (float rowZ in equipmentRows)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    GameObject equipment = CreateBox(root.transform,
                        $"Equipment_{(side < 0 ? "L" : "R")}_{rowZ:0}", "MAT_Panel_Painted",
                        new Vector3(side * 2.9f, 1.1f, rowZ),
                        new Vector3(1.6f, 2.2f, 1.8f));

                    CreateBox(equipment.transform, "StatusLight", "MAT_StatusLight",
                        new Vector3(side * 2.06f, 1.9f, rowZ),
                        new Vector3(0.12f, 0.12f, 0.12f), true);
                }
            }

            CreateBox(root.transform, "Rail_Left", "MAT_Rail_Alu",
                new Vector3(-1.2f, RailHeight, 0f), new Vector3(0.25f, 0.12f, BayLength));
            CreateBox(root.transform, "Rail_Right", "MAT_Rail_Alu",
                new Vector3(1.2f, RailHeight, 0f), new Vector3(0.25f, 0.12f, BayLength));

            // 장비 위에 대기 중인 정적 FOUP들.
            CreateBox(root.transform, "FOUP_Static_A", "MAT_FOUP_Poly",
                new Vector3(-2.9f, 2.4f, -2.25f), new Vector3(0.4f, 0.4f, 0.4f));
            CreateBox(root.transform, "FOUP_Static_B", "MAT_FOUP_Poly",
                new Vector3(2.9f, 2.4f, 2.25f), new Vector3(0.4f, 0.4f, 0.4f));
            CreateBox(root.transform, "FOUP_Static_C", "MAT_FOUP_Poly",
                new Vector3(2.9f, 2.4f, -6.75f), new Vector3(0.4f, 0.4f, 0.4f));

            // 프로브 테스트용 동적 FOUP — 의도적으로 Static이 아니다.
            GameObject dynamicFoup = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dynamicFoup.name = "FOUP_Dynamic (드래그해서 프로브 조명 확인)";
            dynamicFoup.transform.position = new Vector3(1.2f, RailHeight - 0.35f, -4f);
            dynamicFoup.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            SetMaterial(dynamicFoup, "MAT_FOUP_Poly");

            return root;
        }

        private static GameObject CreateBox(Transform parent, string name, string materialName,
            Vector3 position, Vector3 scale, bool useWorldPosition = false)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, useWorldPosition);
            box.transform.position = position;
            box.transform.localScale = scale;
            SetMaterial(box, materialName);
            return box;
        }

        private static void SetMaterial(GameObject target, string materialName)
        {
            target.GetComponent<MeshRenderer>().sharedMaterial = Materials[materialName];
        }

        private static void CreateLights(Transform parent)
        {
            float[] positionsZ = { -8f, -4f, 0f, 4f, 8f };
            foreach (float z in positionsZ)
            {
                var fixture = new GameObject($"CeilingLight_{z:0}");
                fixture.transform.SetParent(parent);
                fixture.transform.position = new Vector3(0f, CeilingHeight - 0.4f, z);

                Light light = fixture.AddComponent<Light>();
                light.type = LightType.Point;
                light.lightmapBakeType = LightmapBakeType.Baked;
                light.color = new Color(0.95f, 0.97f, 1f);
                light.intensity = 2.4f;
                light.range = 9f;
            }
        }

        private static void CreateProbes()
        {
            var probeGroup = new GameObject("Light Probe Group");
            LightProbeGroup group = probeGroup.AddComponent<LightProbeGroup>();
            var positions = new List<Vector3>();
            float[] xs = { -1.2f, 0f, 1.2f };
            float[] ys = { 0.5f, 2.5f, 4.3f };
            for (float z = -9f; z <= 9f; z += 3f)
            {
                foreach (float x in xs)
                {
                    foreach (float y in ys)
                    {
                        positions.Add(new Vector3(x, y, z));
                    }
                }
            }

            group.probePositions = positions.ToArray();

            var reflectionGo = new GameObject("Reflection Probe");
            reflectionGo.transform.position = new Vector3(0f, CeilingHeight / 2f, 0f);
            ReflectionProbe reflection = reflectionGo.AddComponent<ReflectionProbe>();
            reflection.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
            reflection.size = new Vector3(BayWidth + 0.4f, CeilingHeight + 0.4f, BayLength + 0.4f);
            reflection.boxProjection = true;
        }

        private static void CreateCameraAndVolume()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0f, 1.8f, -9.2f);
            cameraGo.transform.rotation = Quaternion.Euler(4f, 0f, 0f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/FabSimPostProfile.asset");

            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.value = 0.4f;
            color.contrast.value = 8f;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.4f;
            bloom.threshold.value = 1.1f;

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.22f;

            foreach (VolumeComponent component in profile.components)
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            var volumeGo = new GameObject("Global Volume");
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        private static void TryAddScreenSpaceAmbientOcclusion()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(
                "Assets/Settings/FabSimRenderer.asset");
            if (rendererData == null)
            {
                Debug.LogWarning("[FabSceneBuilder] Renderer data not found — SSAO skipped.");
                return;
            }

            // SSAO 렌더러 피처 타입은 URP 어셈블리 내부에 있어 리플렉션으로 찾는다.
            System.Type ssaoType = typeof(UniversalRendererData).Assembly.GetType(
                "UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion");
            if (ssaoType == null)
            {
                Debug.LogWarning("[FabSceneBuilder] SSAO feature type not found — skipped.");
                return;
            }

            if (rendererData.rendererFeatures.Exists(
                    feature => feature != null && feature.GetType() == ssaoType))
            {
                return;
            }

            var feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(ssaoType);
            feature.name = "ScreenSpaceAmbientOcclusion";
            rendererData.rendererFeatures.Add(feature);
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            EditorUtility.SetDirty(rendererData);
            Debug.Log("[FabSceneBuilder] SSAO renderer feature added.");
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.18f, 0.20f);
        }

        private static void MarkStaticRecursively(GameObject root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, (StaticEditorFlags)~0);
            }
        }

        private static LightingSettings CreateLightingSettings()
        {
            var settings = new LightingSettings
            {
                lightmapper = LightingSettings.Lightmapper.ProgressiveCPU,
                lightmapResolution = 12f,
                lightmapMaxSize = 1024,
                ao = true,
            };
            AssetDatabase.CreateAsset(settings, "Assets/Settings/FabSimLighting.lighting");
            return settings;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int slash = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
            }
        }

        /// <summary>
        /// 완성본 씬을 카메라 시점으로 렌더링해 PNG로 저장한다 (문서·확인용).
        /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.FabSceneBuilder.CaptureScreenshot
        /// </summary>
        public static void CaptureScreenshot()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("[FabSceneBuilder] Main camera not found.");
                return;
            }

            const int Width = 1600;
            const int Height = 900;
            var renderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            texture.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;

            string outputPath = System.Environment.GetEnvironmentVariable("FABSIM_SHOT_PATH");
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = "FabBayScreenshot.png";
            }

            System.IO.File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            Debug.Log($"[FabSceneBuilder] Screenshot saved: {outputPath}");
        }
    }
}
