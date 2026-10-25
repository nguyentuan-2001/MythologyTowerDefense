using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using MythologyTowerDefense.Core;
using MythologyTowerDefense.Gameplay.Economy;
using MythologyTowerDefense.Gameplay.Enemies;
using MythologyTowerDefense.Gameplay.Path;
using MythologyTowerDefense.Gameplay.Towers;
using MythologyTowerDefense.Gameplay.Waves;
using MythologyTowerDefense.UI;

namespace MythologyTowerDefense.EditorTools
{
    /// <summary>
    /// Procedurally builds the "Celestial Gate Road" prototype battlefield into
    /// Assets/_Game/Scenes/Main.unity. Safe to run repeatedly: it clears and
    /// rebuilds only the TDRoot hierarchy it owns.
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string RootName = "TDRoot";
        private const string ScenePath = "Assets/_Game/Scenes/Main.unity";
        private const string MaterialsFolder = "Assets/_Game/Art/Materials";
        private const string EnemyPrefabFolder = "Assets/_Game/Prefabs/Enemies";
        private const string TowerPrefabFolder = "Assets/_Game/Prefabs/Towers";
        private const string ProjectilePrefabFolder = "Assets/_Game/Prefabs/Projectiles";

        // Jade / gold / cinnabar / deep-teal / stone / cream palette per the art bible.
        private static readonly Color JadeGreen = new Color(0.22f, 0.47f, 0.38f);
        private static readonly Color StoneGray = new Color(0.55f, 0.53f, 0.50f);
        private static readonly Color WarmGold = new Color(0.83f, 0.69f, 0.22f);
        private static readonly Color CinnabarRed = new Color(0.72f, 0.14f, 0.10f);
        private static readonly Color DeepTeal = new Color(0.11f, 0.30f, 0.28f);
        private static readonly Color CreamHighlight = new Color(0.96f, 0.93f, 0.85f);
        private static readonly Color DemonTeal = new Color(0.20f, 0.18f, 0.30f);

        private static readonly Vector3[] WaypointPositions =
        {
            new Vector3(-22f, 1f, -16f),
            new Vector3(-22f, 1f, 10f),
            new Vector3(-8f, 1f, 10f),
            new Vector3(-8f, 1f, -10f),
            new Vector3(8f, 1f, -10f),
            new Vector3(8f, 1f, 14f),
            new Vector3(22f, 1f, 14f),
        };

        private const float PathWidth = 4f;
        // Must stay comfortably inside every tower's range (shortest is 4.5) so a tower on the
        // pad can actually reach enemies walking the path centerline. 5.5 (the original value)
        // placed pads just outside tower range entirely, which is why towers never fired.
        private const float PadOffsetDistance = 3.8f;

        [MenuItem("Tools/Mythology TD/Build Prototype Scene (and Save)")]
        public static void BuildSceneAndSave()
        {
            EnsureSceneOpen();
            BuildScene();

            bool saved = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log(saved
                ? $"[PrototypeSceneBuilder] Scene saved to disk at '{ScenePath}'."
                : $"[PrototypeSceneBuilder] FAILED to save scene at '{ScenePath}'.");
        }

        [MenuItem("Tools/Mythology TD/Build Prototype Scene")]
        public static void BuildScene()
        {
            EnsureSceneOpen();
            ClearGeneratedScene();

            GameObject root = new GameObject(RootName);

            Material groundMat = GetOrCreateMaterial("Mat_Ground", JadeGreen);
            Material pathMat = GetOrCreateMaterial("Mat_Path", StoneGray);
            Material padAvailableMat = GetOrCreateMaterial("Mat_PadAvailable", WarmGold);
            Material entranceMat = GetOrCreateMaterial("Mat_Entrance", DemonTeal);
            Material baseMat = GetOrCreateMaterial("Mat_Base", CreamHighlight);
            Material towerBaseMat = GetOrCreateMaterial("Mat_TowerBase", DeepTeal);
            Material towerHeadMat = GetOrCreateMaterial("Mat_TowerHead", WarmGold);
            Material panguBaseMat = GetOrCreateMaterial("Mat_PanguBase", StoneGray);
            Material panguHeadMat = GetOrCreateMaterial("Mat_PanguHead", CinnabarRed);
            Material enemyMat = GetOrCreateMaterial("Mat_Enemy", DemonTeal);
            Material healthBarMat = GetOrCreateMaterial("Mat_HealthBar", new Color(0.1f, 0.9f, 0.2f));
            Material healthBarBgMat = GetOrCreateMaterial("Mat_HealthBarBG", Color.black);
            Material projectileMat = GetOrCreateMaterial("Mat_Projectile", CreamHighlight);
            Material rangeIndicatorMat = GetOrCreateMaterial("Mat_RangeIndicator", new Color(0.83f, 0.69f, 0.22f));

            Bounds pathBounds = new Bounds(WaypointPositions[0], Vector3.zero);
            foreach (Vector3 p in WaypointPositions)
            {
                pathBounds.Encapsulate(p);
            }

            Vector3 groundCenter = new Vector3(pathBounds.center.x, 0f, pathBounds.center.z);
            float groundSizeX = pathBounds.size.x + 20f;
            float groundSizeZ = pathBounds.size.z + 20f;

            CreateGround(root.transform, groundCenter, groundSizeX, groundSizeZ, groundMat);

            Transform[] waypointTransforms = CreatePathWaypoints(root.transform);
            CreatePathVisuals(root.transform, pathMat);

            GameObject pathGO = new GameObject("Path");
            pathGO.transform.SetParent(root.transform);
            PathDefinition pathDefinition = pathGO.AddComponent<PathDefinition>();
            SerializedObject pathSO = new SerializedObject(pathDefinition);
            SerializedProperty waypointsProp = pathSO.FindProperty("waypoints");
            waypointsProp.arraySize = waypointTransforms.Length;
            for (int i = 0; i < waypointTransforms.Length; i++)
            {
                waypointsProp.GetArrayElementAtIndex(i).objectReferenceValue = waypointTransforms[i];
            }
            pathSO.ApplyModifiedProperties();

            CreateEntranceMarker(root.transform, entranceMat);
            CreateBaseMarker(root.transform, baseMat);

            GameObject enemyPrefab = BuildEnemyPrefab(enemyMat, healthBarMat, healthBarBgMat);
            GameObject projectilePrefab = BuildProjectilePrefab(projectileMat);

            GameObject archerPrefab = BuildTowerPrefab(
                "CelestialArcher", "Celestial Archer", towerBaseMat, towerHeadMat, rangeIndicatorMat, projectilePrefab,
                range: 4.5f, attackInterval: 0.8f, damage: 12, projectileSpeed: 8f, splashRadius: 0f,
                basePosition: new Vector3(0f, 0.6f, 0f), baseScale: new Vector3(1f, 0.6f, 1f),
                headPosition: new Vector3(0f, 1.5f, 0f), headScale: new Vector3(0.6f, 0.6f, 0.9f),
                firePointLocalPosition: new Vector3(0f, 0f, 0.6f));

            GameObject panguPrefab = BuildTowerPrefab(
                "PanguTitan", "Pangu Titan", panguBaseMat, panguHeadMat, rangeIndicatorMat, projectilePrefab,
                range: 4.5f, attackInterval: 1.6f, damage: 20, projectileSpeed: 6f, splashRadius: 2f,
                basePosition: new Vector3(0f, 0.9f, 0f), baseScale: new Vector3(1.6f, 0.9f, 1.6f),
                headPosition: new Vector3(0f, 2.1f, 0f), headScale: new Vector3(1.0f, 1.0f, 1.2f),
                firePointLocalPosition: new Vector3(0f, 0f, 0.7f));

            GameObject systems = new GameObject("Systems");
            systems.transform.SetParent(root.transform);

            GameObject currencyGO = new GameObject("CurrencyManager");
            currencyGO.transform.SetParent(systems.transform);
            CurrencyManager currencyManager = currencyGO.AddComponent<CurrencyManager>();

            GameObject baseHealthGO = new GameObject("BaseHealth");
            baseHealthGO.transform.SetParent(systems.transform);
            BaseHealth baseHealth = baseHealthGO.AddComponent<BaseHealth>();

            GameObject waveManagerGO = new GameObject("WaveManager");
            waveManagerGO.transform.SetParent(systems.transform);
            WaveManager waveManager = waveManagerGO.AddComponent<WaveManager>();
            SerializedObject waveSO = new SerializedObject(waveManager);
            waveSO.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
            waveSO.FindProperty("path").objectReferenceValue = pathDefinition;
            waveSO.FindProperty("currencyManager").objectReferenceValue = currencyManager;
            waveSO.FindProperty("baseHealth").objectReferenceValue = baseHealth;
            waveSO.ApplyModifiedProperties();

            GameObject gameManagerGO = new GameObject("GameManager");
            gameManagerGO.transform.SetParent(systems.transform);
            GameManager gameManager = gameManagerGO.AddComponent<GameManager>();
            SerializedObject gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("baseHealth").objectReferenceValue = baseHealth;
            gmSO.FindProperty("waveManager").objectReferenceValue = waveManager;
            gmSO.ApplyModifiedProperties();

            CreatePlacementPads(root.transform, padAvailableMat, currencyManager, waveManager);

            Camera camera = SetupIsometricCamera(groundCenter, groundSizeX, groundSizeZ);

            GameObject selectionGO = new GameObject("TowerSelectionController");
            selectionGO.transform.SetParent(systems.transform);
            TowerSelectionController selection = selectionGO.AddComponent<TowerSelectionController>();
            SerializedObject selSO = new SerializedObject(selection);
            selSO.FindProperty("targetCamera").objectReferenceValue = camera;
            selSO.ApplyModifiedProperties();

            EnsureEventSystem();
            BuildUI(root.transform, gameManager, currencyManager, baseHealth, waveManager, selection, archerPrefab, panguPrefab);

            RegisterSceneInBuildSettings();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            Debug.Log("[PrototypeSceneBuilder] Prototype scene built successfully. Remember to save the scene (Ctrl+S) if you did not use the (and Save) menu entry.");
        }

        [MenuItem("Tools/Mythology TD/Clear Generated Scene")]
        public static void ClearGeneratedScene()
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }

            GameObject existingEventSystem = GameObject.Find("EventSystem");
            if (existingEventSystem != null && existingEventSystem.GetComponent<EventSystem>() != null)
            {
                Object.DestroyImmediate(existingEventSystem);
            }
        }

        private static void EnsureSceneOpen()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes"))
            {
                CreateFolderRecursive("Assets/_Game/Scenes");
            }

            if (System.IO.File.Exists(ScenePath))
            {
                if (EditorSceneManager.GetActiveScene().path != ScenePath)
                {
                    EditorSceneManager.OpenScene(ScenePath);
                }
            }
            else
            {
                UnityEngine.SceneManagement.Scene newScene =
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(newScene, ScenePath);
            }
        }

        private static void CreateFolderRecursive(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static GameObject CreateGround(Transform parent, Vector3 center, float sizeX, float sizeZ, Material mat)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = center;
            ground.transform.localScale = new Vector3(sizeX / 10f, 1f, sizeZ / 10f);
            ground.GetComponent<Renderer>().sharedMaterial = mat;
            return ground;
        }

        private static Transform[] CreatePathWaypoints(Transform parent)
        {
            GameObject waypointsRoot = new GameObject("Waypoints");
            waypointsRoot.transform.SetParent(parent);

            Transform[] result = new Transform[WaypointPositions.Length];
            for (int i = 0; i < WaypointPositions.Length; i++)
            {
                GameObject wp = new GameObject($"Waypoint_{i:00}");
                wp.transform.SetParent(waypointsRoot.transform);
                wp.transform.position = WaypointPositions[i];
                result[i] = wp.transform;
            }

            return result;
        }

        private static void CreatePathVisuals(Transform parent, Material pathMat)
        {
            GameObject segmentsRoot = new GameObject("PathVisuals");
            segmentsRoot.transform.SetParent(parent);

            for (int i = 0; i < WaypointPositions.Length - 1; i++)
            {
                Vector3 a = new Vector3(WaypointPositions[i].x, 0f, WaypointPositions[i].z);
                Vector3 b = new Vector3(WaypointPositions[i + 1].x, 0f, WaypointPositions[i + 1].z);
                Vector3 mid = (a + b) * 0.5f;
                float length = Vector3.Distance(a, b);

                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"PathSegment_{i:00}";
                segment.transform.SetParent(segmentsRoot.transform);
                segment.transform.position = mid + Vector3.up * 0.05f;
                segment.transform.localScale = new Vector3(PathWidth, 0.1f, length + PathWidth * 0.5f);
                segment.transform.rotation = Quaternion.LookRotation((b - a).normalized, Vector3.up);
                segment.GetComponent<Renderer>().sharedMaterial = pathMat;
                Object.DestroyImmediate(segment.GetComponent<BoxCollider>());
            }
        }

        private static void CreateEntranceMarker(Transform parent, Material mat)
        {
            GameObject entrance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            entrance.name = "Entrance";
            entrance.transform.SetParent(parent);
            Vector3 start = WaypointPositions[0];
            entrance.transform.position = new Vector3(start.x, 1.5f, start.z);
            entrance.transform.localScale = new Vector3(3f, 3f, 0.6f);
            entrance.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateBaseMarker(Transform parent, Material mat)
        {
            GameObject baseGoal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            baseGoal.name = "BaseGoal";
            baseGoal.transform.SetParent(parent);
            Vector3 last = WaypointPositions[WaypointPositions.Length - 1];
            baseGoal.transform.position = new Vector3(last.x, 1.5f, last.z);
            baseGoal.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            baseGoal.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreatePlacementPads(Transform parent, Material padMat, CurrencyManager currencyManager, WaveManager waveManager)
        {
            GameObject padsRoot = new GameObject("TowerPads");
            padsRoot.transform.SetParent(parent);

            Vector3 fieldCenterApprox = Vector3.zero;

            for (int i = 0; i < WaypointPositions.Length - 1; i++)
            {
                Vector3 a = new Vector3(WaypointPositions[i].x, 0f, WaypointPositions[i].z);
                Vector3 b = new Vector3(WaypointPositions[i + 1].x, 0f, WaypointPositions[i + 1].z);
                Vector3 mid = (a + b) * 0.5f;
                Vector3 dir = (b - a).normalized;
                Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

                Vector3 candidateA = mid + perp * PadOffsetDistance;
                Vector3 candidateB = mid - perp * PadOffsetDistance;

                Vector3 chosen = (candidateA - fieldCenterApprox).sqrMagnitude >= (candidateB - fieldCenterApprox).sqrMagnitude
                    ? candidateA
                    : candidateB;

                CreatePad(padsRoot.transform, $"TowerPad_{i:00}", new Vector3(chosen.x, 0.1f, chosen.z), padMat, currencyManager, waveManager);
            }
        }

        private static void CreatePad(Transform parent, string name, Vector3 position, Material padMat, CurrencyManager currencyManager, WaveManager waveManager)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = name;
            pad.transform.SetParent(parent);
            pad.transform.position = position;
            pad.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
            pad.GetComponent<Renderer>().sharedMaterial = padMat;

            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(pad.transform);
            spawnPoint.transform.position = position + Vector3.up * 0.1f;

            TowerPad padComponent = pad.AddComponent<TowerPad>();
            SerializedObject so = new SerializedObject(padComponent);
            so.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
            so.FindProperty("padRenderer").objectReferenceValue = pad.GetComponent<Renderer>();
            so.FindProperty("currencyManager").objectReferenceValue = currencyManager;
            so.FindProperty("waveManager").objectReferenceValue = waveManager;
            so.ApplyModifiedProperties();
        }

        private static GameObject BuildEnemyPrefab(Material enemyMat, Material healthBarMat, Material healthBarBgMat)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "LesserYaoguai";
            enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;

            GameObject healthBarBG = GameObject.CreatePrimitive(PrimitiveType.Cube);
            healthBarBG.name = "HealthBarBG";
            healthBarBG.transform.SetParent(enemy.transform);
            healthBarBG.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            healthBarBG.transform.localScale = new Vector3(1f, 0.12f, 0.12f);
            healthBarBG.GetComponent<Renderer>().sharedMaterial = healthBarBgMat;
            Object.DestroyImmediate(healthBarBG.GetComponent<BoxCollider>());

            GameObject healthBarFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            healthBarFill.name = "HealthBarFill";
            healthBarFill.transform.SetParent(enemy.transform);
            healthBarFill.transform.localPosition = new Vector3(0f, 1.4f, -0.01f);
            healthBarFill.transform.localScale = new Vector3(1f, 0.12f, 0.12f);
            healthBarFill.GetComponent<Renderer>().sharedMaterial = healthBarMat;
            Object.DestroyImmediate(healthBarFill.GetComponent<BoxCollider>());

            Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());

            EnemyController enemyComponent = enemy.AddComponent<EnemyController>();
            SerializedObject so = new SerializedObject(enemyComponent);
            so.FindProperty("healthBarFill").objectReferenceValue = healthBarFill.transform;
            so.ApplyModifiedProperties();

            return SaveAsPrefab(enemy, EnemyPrefabFolder, "LesserYaoguai");
        }

        private static GameObject BuildTowerPrefab(
            string prefabName, string displayName,
            Material baseMat, Material headMat, Material rangeIndicatorMat, GameObject projectilePrefab,
            float range, float attackInterval, int damage, float projectileSpeed, float splashRadius,
            Vector3 basePosition, Vector3 baseScale,
            Vector3 headPosition, Vector3 headScale, Vector3 firePointLocalPosition)
        {
            GameObject tower = new GameObject(prefabName);

            GameObject baseCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCylinder.name = "Base";
            baseCylinder.transform.SetParent(tower.transform);
            baseCylinder.transform.localPosition = basePosition;
            baseCylinder.transform.localScale = baseScale;
            baseCylinder.GetComponent<Renderer>().sharedMaterial = baseMat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(tower.transform);
            head.transform.localPosition = headPosition;
            head.transform.localScale = headScale;
            head.GetComponent<Renderer>().sharedMaterial = headMat;
            Object.DestroyImmediate(head.GetComponent<BoxCollider>());

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(head.transform);
            firePoint.transform.localPosition = firePointLocalPosition;

            GameObject rangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rangeIndicator.name = "RangeIndicator";
            rangeIndicator.transform.SetParent(tower.transform);
            rangeIndicator.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            rangeIndicator.transform.localScale = new Vector3(range, 0.01f, range);
            rangeIndicator.GetComponent<Renderer>().sharedMaterial = rangeIndicatorMat;
            Object.DestroyImmediate(rangeIndicator.GetComponent<CapsuleCollider>());
            rangeIndicator.SetActive(false);

            TowerController towerComponent = tower.AddComponent<TowerController>();
            SerializedObject towerSO = new SerializedObject(towerComponent);
            towerSO.FindProperty("displayName").stringValue = displayName;
            towerSO.FindProperty("range").floatValue = range;
            towerSO.FindProperty("attackInterval").floatValue = attackInterval;
            towerSO.FindProperty("damage").intValue = damage;
            towerSO.FindProperty("projectileSpeed").floatValue = projectileSpeed;
            towerSO.FindProperty("splashRadius").floatValue = splashRadius;
            towerSO.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            towerSO.FindProperty("rotatingHead").objectReferenceValue = head.transform;
            towerSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            towerSO.FindProperty("rangeIndicator").objectReferenceValue = rangeIndicator;
            towerSO.ApplyModifiedProperties();

            return SaveAsPrefab(tower, TowerPrefabFolder, prefabName);
        }

        private static GameObject BuildProjectilePrefab(Material projectileMat)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Projectile";
            projectile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            projectile.GetComponent<Renderer>().sharedMaterial = projectileMat;
            Object.DestroyImmediate(projectile.GetComponent<SphereCollider>());
            projectile.AddComponent<ProjectileController>();

            return SaveAsPrefab(projectile, ProjectilePrefabFolder, "Projectile");
        }

        private static GameObject SaveAsPrefab(GameObject instance, string folder, string name)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                CreateFolderRecursive(folder);
            }

            string path = $"{folder}/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                CreateFolderRecursive(MaterialsFolder);
            }

            string path = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
            }

            return mat;
        }

        private static Camera SetupIsometricCamera(Vector3 groundCenter, float groundSizeX, float groundSizeZ)
        {
            GameObject camGO = GameObject.FindWithTag("MainCamera");
            if (camGO == null)
            {
                camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }

            Camera cam = camGO.GetComponent<Camera>();
            if (cam == null)
            {
                cam = camGO.AddComponent<Camera>();
            }

            Quaternion rotation = Quaternion.Euler(45f, 45f, 0f);
            float maxExtent = Mathf.Max(groundSizeX, groundSizeZ);
            float distance = maxExtent * 0.9f;

            camGO.transform.rotation = rotation;
            camGO.transform.position = groundCenter - rotation * Vector3.forward * distance;

            cam.orthographic = true;
            cam.orthographicSize = maxExtent * 0.3f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = Mathf.Max(200f, distance * 2.5f);

            return cam;
        }

        private static void EnsureEventSystem()
        {
            GameObject existing = Object.FindAnyObjectByType<EventSystem>()?.gameObject;
            if (existing != null)
            {
                return;
            }

            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        private static void BuildUI(Transform parent, GameManager gameManager, CurrencyManager currencyManager, BaseHealth baseHealth, WaveManager waveManager, TowerSelectionController selection, GameObject archerPrefab, GameObject panguPrefab)
        {
            GameObject canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(parent);
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform topBar = CreatePanel(canvasGO.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, 80f), DeepTeal);

            Text currencyText = CreateText("CurrencyText", topBar, builtinFont, "Gold: 0", 28,
                new Vector2(0f, 0f), new Vector2(0.33f, 1f), TextAnchor.MiddleLeft, WarmGold);

            Text baseHealthText = CreateText("BaseHealthText", topBar, builtinFont, "Base HP: 0/0", 28,
                new Vector2(0.33f, 0f), new Vector2(0.66f, 1f), TextAnchor.MiddleCenter, CreamHighlight);

            Text waveStatusText = CreateText("WaveStatusText", topBar, builtinFont, "Press Start Wave", 28,
                new Vector2(0.66f, 0f), new Vector2(1f, 1f), TextAnchor.MiddleRight, CreamHighlight);

            GameObject endPanelGO = new GameObject("EndPanel", typeof(RectTransform), typeof(Image));
            endPanelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform endRT = endPanelGO.GetComponent<RectTransform>();
            endRT.anchorMin = new Vector2(0.5f, 0.5f);
            endRT.anchorMax = new Vector2(0.5f, 0.5f);
            endRT.sizeDelta = new Vector2(900f, 200f);
            endPanelGO.GetComponent<Image>().color = new Color(DeepTeal.r, DeepTeal.g, DeepTeal.b, 0.92f);

            Text endPanelText = CreateText("EndPanelText", endPanelGO.transform, builtinFont, "GAME OVER", 72,
                Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, CinnabarRed);
            endPanelGO.SetActive(false);

            RectTransform bottomBar = CreatePanel(canvasGO.transform, "BottomBar", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 50f), new Vector2(500f, 80f), new Color(0f, 0f, 0f, 0f));

            Button startWaveButton = CreateButton("StartWaveButton", bottomBar, builtinFont, "Start Wave",
                new Vector2(0f, 0f), new Vector2(0.33f, 1f));

            Button pauseButton = CreateButton("PauseButton", bottomBar, builtinFont, "Pause",
                new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
            Text pauseLabel = pauseButton.GetComponentInChildren<Text>();

            Button restartButton = CreateButton("RestartButton", bottomBar, builtinFont, "Restart",
                new Vector2(0.66f, 0f), new Vector2(1f, 1f));

            RectTransform selectionPanel = CreatePanel(canvasGO.transform, "SelectionPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(170f, 170f), new Vector2(300f, 220f), DeepTeal);
            Text selectionText = CreateText("SelectionText", selectionPanel, builtinFont, "Tower Info", 22,
                new Vector2(0f, 0.3f), new Vector2(1f, 1f), TextAnchor.UpperLeft, CreamHighlight);

            Button upgradePlaceholder = CreateButton("UpgradeButtonPlaceholder", selectionPanel, builtinFont, "Upgrade (Soon)",
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.28f));
            upgradePlaceholder.interactable = false;

            selectionPanel.gameObject.SetActive(false);

            RectTransform buildPanel = CreatePanel(canvasGO.transform, "BuildChoicePanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(170f, 170f), new Vector2(300f, 220f), DeepTeal);
            Text buildPanelTitle = CreateText("BuildPanelTitle", buildPanel, builtinFont, "Build Tower", 22,
                new Vector2(0f, 0.75f), new Vector2(1f, 1f), TextAnchor.UpperLeft, CreamHighlight);

            Button archerOptionButton = CreateButton("ArcherOptionButton", buildPanel, builtinFont, string.Empty,
                new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.72f));
            Text archerOptionLabel = archerOptionButton.GetComponentInChildren<Text>();

            Button panguOptionButton = CreateButton("PanguOptionButton", buildPanel, builtinFont, string.Empty,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.37f));
            Text panguOptionLabel = panguOptionButton.GetComponentInChildren<Text>();

            buildPanel.gameObject.SetActive(false);

            GameObject buildMenuGO = new GameObject("TowerBuildMenuController");
            buildMenuGO.transform.SetParent(parent);
            TowerBuildMenuController buildMenu = buildMenuGO.AddComponent<TowerBuildMenuController>();
            SerializedObject buildMenuSO = new SerializedObject(buildMenu);
            buildMenuSO.FindProperty("selectionController").objectReferenceValue = selection;
            buildMenuSO.FindProperty("panel").objectReferenceValue = buildPanel.gameObject;

            SerializedProperty buttonsProp = buildMenuSO.FindProperty("optionButtons");
            buttonsProp.arraySize = 2;
            buttonsProp.GetArrayElementAtIndex(0).objectReferenceValue = archerOptionButton;
            buttonsProp.GetArrayElementAtIndex(1).objectReferenceValue = panguOptionButton;

            SerializedProperty labelsProp = buildMenuSO.FindProperty("optionLabels");
            labelsProp.arraySize = 2;
            labelsProp.GetArrayElementAtIndex(0).objectReferenceValue = archerOptionLabel;
            labelsProp.GetArrayElementAtIndex(1).objectReferenceValue = panguOptionLabel;

            SerializedProperty catalogProp = buildMenuSO.FindProperty("catalog");
            catalogProp.arraySize = 2;
            SerializedProperty archerEntry = catalogProp.GetArrayElementAtIndex(0);
            archerEntry.FindPropertyRelative("displayName").stringValue = "Celestial Archer";
            archerEntry.FindPropertyRelative("prefab").objectReferenceValue = archerPrefab;
            archerEntry.FindPropertyRelative("cost").intValue = 30;
            SerializedProperty panguEntry = catalogProp.GetArrayElementAtIndex(1);
            panguEntry.FindPropertyRelative("displayName").stringValue = "Pangu Titan";
            panguEntry.FindPropertyRelative("prefab").objectReferenceValue = panguPrefab;
            panguEntry.FindPropertyRelative("cost").intValue = 50;

            buildMenuSO.ApplyModifiedProperties();

            HUDController hud = canvasGO.AddComponent<HUDController>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("gameManager").objectReferenceValue = gameManager;
            so.FindProperty("currencyManager").objectReferenceValue = currencyManager;
            so.FindProperty("baseHealth").objectReferenceValue = baseHealth;
            so.FindProperty("waveManager").objectReferenceValue = waveManager;
            so.FindProperty("selectionController").objectReferenceValue = selection;
            so.FindProperty("currencyText").objectReferenceValue = currencyText;
            so.FindProperty("baseHealthText").objectReferenceValue = baseHealthText;
            so.FindProperty("waveStatusText").objectReferenceValue = waveStatusText;
            so.FindProperty("startWaveButton").objectReferenceValue = startWaveButton;
            so.FindProperty("pauseButton").objectReferenceValue = pauseButton;
            so.FindProperty("pauseButtonLabel").objectReferenceValue = pauseLabel;
            so.FindProperty("restartButton").objectReferenceValue = restartButton;
            so.FindProperty("endPanel").objectReferenceValue = endPanelGO;
            so.FindProperty("endPanelText").objectReferenceValue = endPanelText;
            so.FindProperty("selectionPanel").objectReferenceValue = selectionPanel.gameObject;
            so.FindProperty("selectionText").objectReferenceValue = selectionText;
            so.ApplyModifiedProperties();
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color backgroundColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            go.GetComponent<Image>().color = backgroundColor;
            return rt;
        }

        private static Text CreateText(string name, Transform parent, Font font, string content, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text text = go.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(4f, 4f);
            rt.offsetMax = new Vector2(-4f, -4f);

            Image image = go.GetComponent<Image>();
            image.color = DeepTeal;

            Button button = go.GetComponent<Button>();

            GameObject textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            Text text = textGO.GetComponent<Text>();
            text.font = font;
            text.text = label;
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = CreamHighlight;

            return button;
        }

        private static void RegisterSceneInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
