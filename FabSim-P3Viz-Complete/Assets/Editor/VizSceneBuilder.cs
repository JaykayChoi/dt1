using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using FabViz;

namespace FabViz.EditorTools
{
    /// <summary>
    /// "디지털 트윈 시각화" 완성본 씬을 배치 모드에서 자동 구축한다.
    /// docs/phase3/06(Shader Graph 상태·데이터 시각화)와 07(런타임 시각화)의 기법을
    /// 한 씬에 모은다: 상태 발광·선택 헤일로·히트맵·레일 라인/트레일·바닥 존·라벨·카메라.
    /// </summary>
    public static class VizSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/DtVizScene.unity";
        private const int MachineCount = 5;

        private static readonly MachineState[] States =
        {
            MachineState.Running,
            MachineState.Running,
            MachineState.Warning,
            MachineState.Idle,
            MachineState.Down,
        };

        /// <summary>
        /// 실행: Unity -batchmode -executeMethod FabViz.EditorTools.VizSceneBuilder.SetupAndBuild
        /// </summary>
        [MenuItem("FabViz/Build DT Viz Scene")]
        public static void SetupAndBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    "Assets/Settings/FabSimUrp.asset") == null)
            {
                Debug.LogWarning("[VizSceneBuilder] URP 에셋을 찾지 못함 — 렌더링이 기본 파이프라인일 수 있음.");
            }

            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEnvironment();
            Camera camera = CreateCameraAndVolume();
            CreateFloorAndWall();
            CreateMachines();
            CreateRailAndCarrier();
            CreateHeatmapStrip();
            CreateZone();
            CreateControllers(camera);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[VizSceneBuilder] Scene built: {ScenePath}");

            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("FABSIM_SHOT_PATH")))
            {
                CaptureScreenshot(camera);
            }
        }

        private static void CreateEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.18f, 0.22f);

            var sunGo = new GameObject("Directional Light");
            sunGo.transform.rotation = Quaternion.Euler(52f, -32f, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.97f, 0.9f);
            sun.intensity = 1.1f;
        }

        private static Camera CreateCameraAndVolume()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0f, 3.4f, -9.8f);
            cameraGo.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/FabVizPostProfile.asset");

            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.value = 0.3f;
            color.contrast.value = 10f;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.5f;
            bloom.threshold.value = 1.0f;

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.24f;

            foreach (VolumeComponent component in profile.components)
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }

            var volumeGo = new GameObject("Global Volume");
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            return camera;
        }

        private static void CreateFloorAndWall()
        {
            Material floorMat = LoadMaterial("MAT_Floor_Epoxy");
            Material wallMat = LoadMaterial("MAT_Wall_Paint");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(1.4f, 1f, 1.2f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall_Back";
            wall.transform.position = new Vector3(0f, 2.5f, 3f);
            wall.transform.localScale = new Vector3(13f, 5f, 0.15f);
            wall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        }

        private static void CreateMachines()
        {
            Material bodyMat = LoadMaterial("MAT_Panel_Painted");
            Material haloMat = CreateFresnelMaterial(
                "MAT_Viz_Halo", new Color(0.1f, 0.8f, 1f, 0.14f), new Color(0.4f, 0.98f, 1f, 1f), 1.8f);

            // 상태별 발광 머티리얼은 상태당 한 번만 만들어 공유한다(같은 경로 충돌 방지).
            var statusMaterials = new System.Collections.Generic.Dictionary<MachineState, Material>();
            foreach (MachineState value in System.Enum.GetValues(typeof(MachineState)))
            {
                statusMaterials[value] = CreateStatusMaterial(value);
            }

            for (int i = 0; i < MachineCount; i++)
            {
                MachineState state = States[i];
                float x = -4f + i * 2f;

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = $"Machine_EQP-{i + 1:00}";
                body.transform.position = new Vector3(x, 1.1f, 1.5f);
                body.transform.localScale = new Vector3(1.4f, 2.2f, 1.6f);
                body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicator.name = "Indicator";
                indicator.transform.SetParent(body.transform, false);
                indicator.transform.localScale = new Vector3(0.5f, 0.09f, 0.06f);
                indicator.transform.localPosition = new Vector3(0f, 0.28f, -0.53f);
                Object.DestroyImmediate(indicator.GetComponent<Collider>());
                indicator.GetComponent<MeshRenderer>().sharedMaterial = statusMaterials[state];

                MachineStatus status = body.AddComponent<MachineStatus>();
                SerializedObject so = new SerializedObject(status);
                so.FindProperty("state").enumValueIndex = (int)state;
                so.FindProperty("indicator").objectReferenceValue =
                    indicator.GetComponent<MeshRenderer>();
                so.ApplyModifiedPropertiesWithoutUndo();

                CreateLabel(body.transform, $"EQP-{i + 1:00}", StatusPalette.Label(state));

                GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                halo.name = "SelectionHalo";
                halo.transform.SetParent(body.transform, false);
                halo.transform.localScale = new Vector3(1.08f, 1.06f, 1.08f);
                Object.DestroyImmediate(halo.GetComponent<Collider>());
                halo.GetComponent<MeshRenderer>().sharedMaterial = haloMat;
                // 정지 상태(EQP-05) 하나는 선택된 예시로 켜 둔다.
                halo.SetActive(state == MachineState.Down);
            }
        }

        private static void CreateLabel(Transform parent, string id, string statusText)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            labelGo.transform.localScale = Vector3.one * 0.13f;

            TextMesh text = labelGo.AddComponent<TextMesh>();
            text.text = $"{id}\n{statusText}";
            text.anchor = TextAnchor.LowerCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = 0.1f;
            text.color = Color.white;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (font != null)
            {
                text.font = font;
                labelGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }

            labelGo.AddComponent<Billboard>();
            // 스틸 컷에서 바로 읽히도록 카메라를 향해 초기 회전을 맞춰 둔다.
            labelGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        private static void CreateRailAndCarrier()
        {
            Material railMat = CreateUnlitMaterial("MAT_Viz_RailLine", new Color(0.2f, 0.8f, 1f) * 1.6f);

            var railGo = new GameObject("OHT Rail");
            LineRenderer line = railGo.AddComponent<LineRenderer>();
            var points = new Vector3[]
            {
                new Vector3(-4.6f, 3.7f, 0.2f),
                new Vector3(4.6f, 3.7f, 0.2f),
                new Vector3(4.6f, 3.7f, -2.2f),
                new Vector3(-4.6f, 3.7f, -2.2f),
            };
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.loop = true;
            line.widthMultiplier = 0.07f;
            line.numCornerVertices = 2;
            line.material = railMat;
            line.useWorldSpace = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject carrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carrier.name = "Carrier";
            carrier.transform.position = points[0];
            carrier.transform.localScale = new Vector3(0.5f, 0.32f, 0.5f);
            Object.DestroyImmediate(carrier.GetComponent<Collider>());
            carrier.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial("MAT_FOUP_Poly");

            TrailRenderer trail = carrier.AddComponent<TrailRenderer>();
            trail.time = 2.2f;
            trail.startWidth = 0.28f;
            trail.endWidth = 0f;
            trail.material = railMat;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            CarrierMover mover = carrier.AddComponent<CarrierMover>();
            mover.SetPath(points);
        }

        private static void CreateHeatmapStrip()
        {
            Material heatMat = CreateShaderMaterial("MAT_Viz_Heatmap", "FabViz/Heatmap");
            if (heatMat == null)
            {
                return;
            }

            var root = new GameObject("Heatmap (혼잡도)");
            const int tiles = 9;
            for (int i = 0; i < tiles; i++)
            {
                float t = i / (float)(tiles - 1);
                float x = -4.2f + t * 8.4f;
                // 가운데가 붐비는 혼잡 프로파일.
                float d = (t - 0.5f) * 2f;
                float value = Mathf.Clamp01(1f - d * d * 0.9f);

                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"Cell_{i:00}";
                tile.transform.SetParent(root.transform, false);
                tile.transform.position = new Vector3(x, 0.02f, -2.4f);
                tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                tile.transform.localScale = new Vector3(0.9f, 1.6f, 1f);
                Object.DestroyImmediate(tile.GetComponent<Collider>());
                tile.GetComponent<MeshRenderer>().sharedMaterial = heatMat;

                HeatmapCell cell = tile.AddComponent<HeatmapCell>();
                cell.Value = value;
            }
        }

        private static void CreateZone()
        {
            Material zoneMat = CreateFresnelMaterial(
                "MAT_Viz_Zone", new Color(0.95f, 0.6f, 0.1f, 0.16f), new Color(1f, 0.8f, 0.3f, 1f), 2.6f);

            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Quad);
            zone.name = "Zone_통제구역";
            zone.transform.position = new Vector3(-3f, 0.04f, 0.1f);
            zone.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            zone.transform.localScale = new Vector3(3.2f, 2.6f, 1f);
            Object.DestroyImmediate(zone.GetComponent<Collider>());
            zone.GetComponent<MeshRenderer>().sharedMaterial = zoneMat;
        }

        private static void CreateControllers(Camera camera)
        {
            var rigGo = new GameObject("DT Viz Controller");
            rigGo.AddComponent<DtVizController>();
            rigGo.AddComponent<SelectionController>();

            CameraRig rig = rigGo.AddComponent<CameraRig>();
            SerializedObject so = new SerializedObject(rig);
            so.FindProperty("cam").objectReferenceValue = camera;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- 머티리얼 헬퍼 ----------

        private static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");
        }

        private static Material CreateStatusMaterial(MachineState state)
        {
            // URP Unlit + HDR BaseColor로 스스로 빛나는 상태등. 값 > 1 이라 블룸이 잡는다.
            string name = $"MAT_Status_{state}";
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", StatusPalette.Color(state) * 1.6f);
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static Material CreateUnlitMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static Material CreateShaderMaterial(string name, string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[VizSceneBuilder] Shader not found: {shaderName}");
                return null;
            }

            var material = new Material(shader);
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static Material CreateFresnelMaterial(
            string name, Color baseColor, Color rimColor, float rimPower)
        {
            Shader shader = Shader.Find("FabViz/FresnelRim");
            if (shader == null)
            {
                Debug.LogError("[VizSceneBuilder] Shader not found: FabViz/FresnelRim");
                return null;
            }

            var material = new Material(shader);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_RimColor", rimColor);
            material.SetFloat("_RimPower", rimPower);
            AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int slash = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
            }
        }

        /// <summary>완성본 씬을 카메라 시점으로 렌더링해 PNG로 저장한다 (문서·확인용).</summary>
        private static void CaptureScreenshot(Camera camera)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                Debug.LogError("[VizSceneBuilder] Main camera not found for screenshot.");
                return;
            }

            const int width = 1600;
            const int height = 900;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            texture.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;

            string outputPath = System.Environment.GetEnvironmentVariable("FABSIM_SHOT_PATH");
            System.IO.File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            Debug.Log($"[VizSceneBuilder] Screenshot saved: {outputPath}");
        }
    }
}
