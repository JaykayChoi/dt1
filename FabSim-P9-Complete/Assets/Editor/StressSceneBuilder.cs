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
    /// Phase 4 최적화 실험장 씬을 배치 모드에서 자동 구축한다.
    /// 장비 수백 대 + 순환 주행 비히클 무리 — Profiler/Frame Debugger로
    /// 드로우콜·배칭·GC를 관찰하는 무대다.
    /// </summary>
    public static class StressSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/StressField.unity";
        private const int BayCount = 6;
        private const float BayWidth = 8f;
        private const float BayLength = 44f;
        private const float CeilingHeight = 5f;
        private const int EquipmentPerRow = 16;
        private const int VehicleCount = 96;

        private static readonly Dictionary<string, Material> Materials =
            new Dictionary<string, Material>();

        /// <summary>
        /// URP 구성(필요 시)과 스트레스 씬 구축·베이크를 수행한다.
        /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.StressSceneBuilder.SetupAndBuild
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
            GameObject staticRoot = CreateBays();
            Transform[] fleet = CreateVehicles();
            CreateCameraAndHud(fleet);
            ConfigureEnvironment();
            MarkStaticRecursively(staticRoot);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);

            var lightingSettings = new LightingSettings
            {
                lightmapper = LightingSettings.Lightmapper.ProgressiveCPU,
                lightmapResolution = 8f,
                lightmapMaxSize = 1024,
                ao = true,
            };
            AssetDatabase.CreateAsset(lightingSettings, "Assets/Settings/StressLighting.lighting");
            Lightmapping.lightingSettings = lightingSettings;
            bool isBaked = Lightmapping.Bake();
            Debug.Log($"[StressSceneBuilder] Lightmap bake finished: {(isBaked ? "OK" : "FAILED")}");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();

            int rendererCount = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;
            Debug.Log($"[StressSceneBuilder] Scene built: {ScenePath} (renderers: {rendererCount})");

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
            CreateLitMaterial("MAT_Panel_Painted", new Color(0.66f, 0.67f, 0.68f), 0.1f, 0.45f);
            CreateLitMaterial("MAT_Rail_Alu", new Color(0.45f, 0.46f, 0.48f), 0.6f, 0.45f);
            CreateLitMaterial("MAT_Vehicle", new Color(0.85f, 0.55f, 0.15f), 0.2f, 0.5f);

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

            // GPU 인스턴싱 — 같은 메시+머티리얼을 드로우콜 하나로 묶는 열쇠.
            material.enableInstancing = true;

            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            Materials[name] = material;
            return material;
        }

        private static GameObject CreateBays()
        {
            var root = new GameObject("Fab (Static)");
            float totalWidth = BayCount * BayWidth;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.localScale = new Vector3(totalWidth / 10f, 1f, BayLength / 10f);
            SetMaterial(floor, "MAT_Floor_Epoxy");

            CreateBox(root.transform, "Ceiling", "MAT_Wall_Paint",
                new Vector3(0f, CeilingHeight + 0.075f, 0f),
                new Vector3(totalWidth, 0.15f, BayLength));

            for (int bay = 0; bay < BayCount; bay++)
            {
                float bayCenterX = -totalWidth / 2f + BayWidth * (bay + 0.5f);
                CreateBayContent(root.transform, bay, bayCenterX);
            }

            // 조명은 베이당 4개 — 전부 Baked라 런타임 비용은 0이다.
            for (int bay = 0; bay < BayCount; bay++)
            {
                float bayCenterX = -totalWidth / 2f + BayWidth * (bay + 0.5f);
                for (int i = 0; i < 4; i++)
                {
                    float z = -BayLength / 2f + BayLength / 4f * (i + 0.5f);
                    var fixture = new GameObject($"Light_B{bay}_{i}");
                    fixture.transform.SetParent(root.transform);
                    fixture.transform.position = new Vector3(bayCenterX, CeilingHeight - 0.4f, z);
                    Light light = fixture.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.lightmapBakeType = LightmapBakeType.Baked;
                    light.color = new Color(0.95f, 0.97f, 1f);
                    light.intensity = 2.2f;
                    light.range = 10f;
                }
            }

            return root;
        }

        private static void CreateBayContent(Transform parent, int bayIndex, float centerX)
        {
            float spacing = BayLength / EquipmentPerRow;
            for (int i = 0; i < EquipmentPerRow; i++)
            {
                float z = -BayLength / 2f + spacing * (i + 0.5f);
                foreach (float side in new[] { -1f, 1f })
                {
                    float x = centerX + side * 2.6f;
                    GameObject equipment = CreateBox(parent,
                        $"Equip_B{bayIndex}_{(side < 0 ? "L" : "R")}{i}", "MAT_Panel_Painted",
                        new Vector3(x, 1.1f, z), new Vector3(1.5f, 2.2f, 1.7f));

                    CreateBox(equipment.transform, "StatusLight", "MAT_StatusLight",
                        new Vector3(x - side * 0.81f, 1.9f, z),
                        new Vector3(0.1f, 0.1f, 0.1f), true);
                }
            }

            CreateBox(parent, $"Rail_B{bayIndex}", "MAT_Rail_Alu",
                new Vector3(centerX, 4f, 0f), new Vector3(0.25f, 0.12f, BayLength));
        }

        private static Transform[] CreateVehicles()
        {
            var parent = new GameObject("Vehicles (Dynamic)");
            var fleet = new Transform[VehicleCount];
            float halfWidth = BayCount * BayWidth / 2f - 1.5f;
            float halfLength = BayLength / 2f - 2f;
            float perimeter = 4f * (halfWidth + halfLength);

            for (int i = 0; i < VehicleCount; i++)
            {
                GameObject vehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vehicle.name = $"Vehicle_{i:D3}";
                vehicle.transform.SetParent(parent.transform);
                vehicle.transform.localScale = new Vector3(0.5f, 0.35f, 0.9f);

                // 초기 위치도 순환 트랙 위에 고르게 — 플레이 전 씬/스크린샷에서도
                // 무리가 보이도록 런타임과 같은 트랙 수식으로 배치한다.
                float distance = perimeter / VehicleCount * i;
                vehicle.transform.localPosition =
                    EvaluateTrack(distance, 3.6f, halfWidth, halfLength);
                SetMaterial(vehicle, "MAT_Vehicle");
                fleet[i] = vehicle.transform;
            }

            VehicleFleet manager = parent.AddComponent<VehicleFleet>();
            manager.Initialize(fleet, halfWidth, halfLength);
            return fleet;
        }

        private static Vector3 EvaluateTrack(
            float distance, float height, float halfWidth, float halfLength)
        {
            float w = halfWidth * 2f;
            float l = halfLength * 2f;
            if (distance < l)
            {
                return new Vector3(-halfWidth, height, -halfLength + distance);
            }

            distance -= l;
            if (distance < w)
            {
                return new Vector3(-halfWidth + distance, height, halfLength);
            }

            distance -= w;
            if (distance < l)
            {
                return new Vector3(halfWidth, height, halfLength - distance);
            }

            distance -= l;
            return new Vector3(halfWidth - distance, height, -halfLength);
        }

        private static void CreateCameraAndHud(Transform[] fleet)
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(-BayWidth * 0.5f, 2.6f, -BayLength / 2f + 3f);
            cameraGo.transform.rotation = Quaternion.Euler(6f, 8f, 0f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            cameraGo.AddComponent<FpsHud>();

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/StressPostProfile.asset");
            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.35f;
            bloom.threshold.value = 1.1f;
            foreach (VolumeComponent component in profile.components)
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            var volumeGo = new GameObject("Global Volume");
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
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

        private static void ConfigureEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.18f, 0.20f);
        }

        private static void MarkStaticRecursively(GameObject root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, (StaticEditorFlags)~0);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int slash = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
            }
        }

        /// <summary>스트레스 씬을 카메라 시점으로 렌더링해 PNG로 저장한다.</summary>
        public static void CaptureScreenshot()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("[StressSceneBuilder] Main camera not found.");
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
                outputPath = "StressFieldScreenshot.png";
            }

            System.IO.File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            Debug.Log($"[StressSceneBuilder] Screenshot saved: {outputPath}");
        }
    }
}
