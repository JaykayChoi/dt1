using System.Collections.Generic;
using FabSim.Sim;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FabSim.EditorTools
{
    /// <summary>
    /// Phase 5 미니 팹 시뮬레이터 씬을 배치 모드에서 자동 구축한다.
    /// FabLayout이 정의한 레일 그래프를 시각물로 옮기고, 시뮬레이션
    /// 컴포넌트(FabSimulation/VehicleFleetView/StatsHud)를 연결한다.
    /// </summary>
    public static class P5SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MiniFab.unity";
        private const float RoomHalfWidth = 8.5f;
        private const float RoomHalfLength = 18.5f;
        private const float CeilingHeight = 5.2f;

        private static readonly Dictionary<string, Material> Materials =
            new Dictionary<string, Material>();

        /// <summary>
        /// 스모크 테스트 → URP 구성(필요 시) → 씬 구축·베이크를 수행한다.
        /// 실행: Unity -batchmode -executeMethod FabSim.EditorTools.P5SceneBuilder.SetupAndBuild
        /// </summary>
        public static void SetupAndBuild()
        {
            SmokeTest();

            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    "Assets/Settings/FabSimUrp.asset") == null)
            {
                FabSimSetup.ConfigureUrp();
            }

            BuildScene();
        }

        /// <summary>
        /// 렌더링 없이 순수 모델만 1시간 굴려 로직을 검증한다.
        /// 완료 반송이 0이면 예외 — 배치 실행이 실패로 드러난다.
        /// </summary>
        public static void SmokeTest()
        {
            RailGraph graph = FabLayout.Build(out int[] portNodes);
            var model = new FabModel(graph, portNodes, 4, 25.0, 2.5, 7);
            model.Start();
            model.Simulation.Run(3600.0);

            string summary =
                $"jobs={model.CompletedJobs}, " +
                $"avgDelivery={model.GetAverageDeliveryTime():F1}s, " +
                $"utilization={model.GetFleetUtilization(3600.0):P0}, " +
                $"pending={model.PendingJobCount}, " +
                $"events={model.Simulation.ExecutedCount}";
            Debug.Log($"[P5SmokeTest] {summary}");

            if (model.CompletedJobs == 0)
            {
                throw new System.Exception($"Smoke test failed — no jobs completed. {summary}");
            }
        }

        private static void BuildScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMaterials();
            GameObject staticRoot = CreateRoomAndRails();
            CreateSimulationObjects();
            CreateCameraAndVolume();
            ConfigureEnvironment();
            MarkStaticRecursively(staticRoot);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);

            var lightingSettings = new LightingSettings
            {
                lightmapper = LightingSettings.Lightmapper.ProgressiveCPU,
                lightmapResolution = 12f,
                lightmapMaxSize = 1024,
                ao = true,
            };
            AssetDatabase.CreateAsset(lightingSettings, "Assets/Settings/MiniFabLighting.lighting");
            Lightmapping.lightingSettings = lightingSettings;
            bool isBaked = Lightmapping.Bake();
            Debug.Log($"[P5SceneBuilder] Lightmap bake finished: {(isBaked ? "OK" : "FAILED")}");

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[P5SceneBuilder] Scene built: {ScenePath}");

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
            CreateLitMaterial("MAT_FOUP_Poly", new Color(0.52f, 0.60f, 0.68f), 0f, 0.6f);
        }

        private static Material CreateLitMaterial(
            string name, Color baseColor, float metallic, float smoothness)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            Materials[name] = material;
            return material;
        }

        private static GameObject CreateRoomAndRails()
        {
            var root = new GameObject("Fab (Static)");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.localScale =
                new Vector3(RoomHalfWidth * 2f / 10f, 1f, RoomHalfLength * 2f / 10f);
            SetMaterial(floor, "MAT_Floor_Epoxy");

            CreateBox(root.transform, "Ceiling", "MAT_Wall_Paint",
                new Vector3(0f, CeilingHeight + 0.075f, 0f),
                new Vector3(RoomHalfWidth * 2f, 0.15f, RoomHalfLength * 2f));
            CreateBox(root.transform, "Wall_Left", "MAT_Wall_Paint",
                new Vector3(-RoomHalfWidth - 0.075f, CeilingHeight / 2f, 0f),
                new Vector3(0.15f, CeilingHeight, RoomHalfLength * 2f));
            CreateBox(root.transform, "Wall_Right", "MAT_Wall_Paint",
                new Vector3(RoomHalfWidth + 0.075f, CeilingHeight / 2f, 0f),
                new Vector3(0.15f, CeilingHeight, RoomHalfLength * 2f));
            CreateBox(root.transform, "Wall_Back", "MAT_Wall_Paint",
                new Vector3(0f, CeilingHeight / 2f, RoomHalfLength + 0.075f),
                new Vector3(RoomHalfWidth * 2f, CeilingHeight, 0.15f));

            // 레일 — FabLayout 그래프의 엣지를 그대로 시각물로 옮긴다.
            RailGraph graph = FabLayout.Build(out int[] portNodes);
            for (int from = 0; from < graph.NodeCount; from++)
            {
                RailGraph.NodePoint a = graph.GetNode(from);
                foreach (int to in graph.GetOutgoing(from))
                {
                    RailGraph.NodePoint b = graph.GetNode(to);
                    var center = new Vector3((a.X + b.X) / 2f, a.Y, (a.Z + b.Z) / 2f);
                    float length = graph.GetDistance(from, to);
                    bool isAlongZ = Mathf.Abs(a.Z - b.Z) > Mathf.Abs(a.X - b.X);
                    Vector3 scale = isAlongZ
                        ? new Vector3(0.25f, 0.12f, length + 0.25f)
                        : new Vector3(length + 0.25f, 0.12f, 0.25f);
                    CreateBox(root.transform, $"Rail_{from}_{to}", "MAT_Rail_Alu", center, scale);
                }
            }

            // 포트 노드 아래에 장비 박스 — 픽업/드롭이 일어나는 로드포트.
            foreach (int port in portNodes)
            {
                RailGraph.NodePoint node = graph.GetNode(port);
                CreateBox(root.transform, $"Equipment_Port{port}", "MAT_Panel_Painted",
                    new Vector3(node.X, 1.1f, node.Z), new Vector3(1.6f, 2.2f, 1.8f));
            }

            // 천장 조명 — 전부 Baked.
            for (int i = 0; i < 5; i++)
            {
                float z = -RoomHalfLength + (RoomHalfLength * 2f) / 5f * (i + 0.5f);
                var fixture = new GameObject($"CeilingLight_{i}");
                fixture.transform.SetParent(root.transform);
                fixture.transform.position = new Vector3(0f, CeilingHeight - 0.4f, z);
                Light light = fixture.AddComponent<Light>();
                light.type = LightType.Point;
                light.lightmapBakeType = LightmapBakeType.Baked;
                light.color = new Color(0.95f, 0.97f, 1f);
                light.intensity = 2.4f;
                light.range = 11f;
            }

            return root;
        }

        private static void CreateSimulationObjects()
        {
            var simGo = new GameObject("FabSimulation");
            FabSimulation simulation = simGo.AddComponent<FabSimulation>();

            VehicleFleetView view = simGo.AddComponent<VehicleFleetView>();
            view.Initialize(simulation, Materials["MAT_Vehicle"], Materials["MAT_FOUP_Poly"]);

            StatsHud hud = simGo.AddComponent<StatsHud>();
            hud.Initialize(simulation);
        }

        private static void CreateCameraAndVolume()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0f, 2.3f, -16.5f);
            cameraGo.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/MiniFabPostProfile.asset");
            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.35f;
            bloom.threshold.value = 1.1f;
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.2f;
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
            Vector3 position, Vector3 scale)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
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

        /// <summary>미니 팹 씬을 카메라 시점으로 렌더링해 PNG로 저장한다.</summary>
        public static void CaptureScreenshot()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("[P5SceneBuilder] Main camera not found.");
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
                outputPath = "MiniFabScreenshot.png";
            }

            System.IO.File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            Debug.Log($"[P5SceneBuilder] Screenshot saved: {outputPath}");
        }
    }
}
