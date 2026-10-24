using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class PrototypeSceneBuilder
{
    private const string RootName = "TDRoot";
    private const string MaterialsFolder = "Assets/Materials";
    private const string PrefabsFolder = "Assets/Prefabs";

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
    private const float PadOffsetDistance = 5.5f;
    private const int PadCost = 50;

    [MenuItem("Tower Defense/Build Prototype Scene")]
    public static void BuildScene()
    {
        ClearGeneratedScene();

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Prototype Scene");

        Material groundMat = GetOrCreateMaterial("Mat_Ground", new Color(0.33f, 0.55f, 0.28f));
        Material pathMat = GetOrCreateMaterial("Mat_Path", new Color(0.55f, 0.42f, 0.28f));
        Material padAvailableMat = GetOrCreateMaterial("Mat_PadAvailable", new Color(0.85f, 0.7f, 0.3f));
        Material baseMat = GetOrCreateMaterial("Mat_Base", new Color(0.2f, 0.35f, 0.8f));
        Material towerBaseMat = GetOrCreateMaterial("Mat_TowerBase", new Color(0.5f, 0.5f, 0.55f));
        Material towerHeadMat = GetOrCreateMaterial("Mat_TowerHead", new Color(0.4f, 0.28f, 0.15f));
        Material enemyMat = GetOrCreateMaterial("Mat_Enemy", new Color(0.8f, 0.15f, 0.15f));
        Material healthBarMat = GetOrCreateMaterial("Mat_HealthBar", new Color(0.1f, 0.9f, 0.2f));
        Material projectileMat = GetOrCreateMaterial("Mat_Projectile", new Color(0.05f, 0.05f, 0.05f));

        Bounds pathBounds = new Bounds(WaypointPositions[0], Vector3.zero);
        foreach (Vector3 p in WaypointPositions)
        {
            pathBounds.Encapsulate(p);
        }

        Vector3 groundCenter = new Vector3(pathBounds.center.x, 0f, pathBounds.center.z);
        float groundSizeX = pathBounds.size.x + 20f;
        float groundSizeZ = pathBounds.size.z + 20f;

        GameObject ground = CreateGround(root.transform, groundCenter, groundSizeX, groundSizeZ, groundMat);

        Transform[] waypointTransforms = CreatePathWaypoints(root.transform);
        CreatePathVisuals(root.transform, pathMat);

        GameObject pathGO = new GameObject("Path");
        pathGO.transform.SetParent(root.transform);
        WaypointPath waypointPath = pathGO.AddComponent<WaypointPath>();
        SerializedObject pathSO = new SerializedObject(waypointPath);
        SerializedProperty waypointsProp = pathSO.FindProperty("waypoints");
        waypointsProp.arraySize = waypointTransforms.Length;
        for (int i = 0; i < waypointTransforms.Length; i++)
        {
            waypointsProp.GetArrayElementAtIndex(i).objectReferenceValue = waypointTransforms[i];
        }
        pathSO.ApplyModifiedProperties();

        GameObject baseGoal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGoal.name = "BaseGoal";
        baseGoal.transform.SetParent(root.transform);
        Vector3 lastPoint = WaypointPositions[WaypointPositions.Length - 1];
        baseGoal.transform.position = new Vector3(lastPoint.x, 1f, lastPoint.z);
        baseGoal.transform.localScale = new Vector3(2f, 2f, 2f);
        baseGoal.GetComponent<Renderer>().sharedMaterial = baseMat;

        GameObject enemyPrefab = BuildEnemyPrefab(enemyMat, healthBarMat);
        GameObject projectilePrefab = BuildProjectilePrefab(projectileMat);
        GameObject towerPrefab = BuildTowerPrefab(towerBaseMat, towerHeadMat, projectilePrefab);

        CreatePlacementPads(root.transform, padAvailableMat, towerPrefab);

        GameObject systems = new GameObject("Systems");
        systems.transform.SetParent(root.transform);

        GameObject gameManagerGO = new GameObject("GameManager");
        gameManagerGO.transform.SetParent(systems.transform);
        gameManagerGO.AddComponent<GameManager>();

        GameObject spawnPointGO = new GameObject("EnemySpawnPoint");
        spawnPointGO.transform.SetParent(systems.transform);
        spawnPointGO.transform.position = WaypointPositions[0];

        GameObject waveSpawnerGO = new GameObject("WaveSpawner");
        waveSpawnerGO.transform.SetParent(systems.transform);
        WaveSpawner waveSpawner = waveSpawnerGO.AddComponent<WaveSpawner>();
        SerializedObject waveSO = new SerializedObject(waveSpawner);
        waveSO.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        waveSO.FindProperty("spawnPoint").objectReferenceValue = spawnPointGO.transform;
        waveSO.ApplyModifiedProperties();

        Camera camera = SetupIsometricCamera(groundCenter, groundSizeX, groundSizeZ);

        GameObject placementGO = new GameObject("PlacementInputController");
        placementGO.transform.SetParent(systems.transform);
        PlacementInputController placement = placementGO.AddComponent<PlacementInputController>();
        SerializedObject placementSO = new SerializedObject(placement);
        placementSO.FindProperty("targetCamera").objectReferenceValue = camera;
        placementSO.ApplyModifiedProperties();

        EnsureEventSystem();
        BuildUI(root.transform, gameManagerGO.GetComponent<GameManager>(), waveSpawner);

        RegisterActiveSceneInBuildSettings();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root;

        Debug.Log("[PrototypeSceneBuilder] Prototype scene built successfully. Remember to save the scene (Ctrl+S).");
    }

    [MenuItem("Tower Defense/Build Prototype Scene (and Save)")]
    public static void BuildSceneAndSave()
    {
        const string targetScenePath = "Assets/Scenes/SampleScene.unity";

        UnityEngine.SceneManagement.Scene active = EditorSceneManager.GetActiveScene();
        if (active.path != targetScenePath)
        {
            active = EditorSceneManager.OpenScene(targetScenePath);
        }

        BuildScene();

        bool saved = EditorSceneManager.SaveScene(active);
        Debug.Log(saved
            ? $"[PrototypeSceneBuilder] Scene saved to disk at '{active.path}'."
            : $"[PrototypeSceneBuilder] FAILED to save scene at '{active.path}'.");
    }

    [MenuItem("Tower Defense/Clear Generated Scene")]
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

    private static void CreatePlacementPads(Transform parent, Material padMat, GameObject towerPrefab)
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

            CreatePad(padsRoot.transform, $"TowerPad_{i:00}", new Vector3(chosen.x, 0.1f, chosen.z), padMat, towerPrefab);
        }
    }

    private static void CreatePad(Transform parent, string name, Vector3 position, Material padMat, GameObject towerPrefab)
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

        TowerPlacementPad padComponent = pad.AddComponent<TowerPlacementPad>();
        SerializedObject so = new SerializedObject(padComponent);
        so.FindProperty("towerPrefab").objectReferenceValue = towerPrefab;
        so.FindProperty("cost").intValue = PadCost;
        so.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        so.FindProperty("padRenderer").objectReferenceValue = pad.GetComponent<Renderer>();
        so.ApplyModifiedProperties();
    }

    private static GameObject BuildEnemyPrefab(Material enemyMat, Material healthBarMat)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;

        GameObject healthBarBG = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarBG.name = "HealthBarBG";
        healthBarBG.transform.SetParent(enemy.transform);
        healthBarBG.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        healthBarBG.transform.localScale = new Vector3(1f, 0.12f, 0.12f);
        healthBarBG.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("Mat_HealthBarBG", Color.black);
        Object.DestroyImmediate(healthBarBG.GetComponent<CapsuleCollider>());
        Object.DestroyImmediate(healthBarBG.GetComponent<BoxCollider>());

        GameObject healthBarFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBarFill.name = "HealthBarFill";
        healthBarFill.transform.SetParent(enemy.transform);
        healthBarFill.transform.localPosition = new Vector3(0f, 1.4f, -0.01f);
        healthBarFill.transform.localScale = new Vector3(1f, 0.12f, 0.12f);
        healthBarFill.GetComponent<Renderer>().sharedMaterial = healthBarMat;
        Object.DestroyImmediate(healthBarFill.GetComponent<BoxCollider>());

        Enemy enemyComponent = enemy.AddComponent<Enemy>();
        SerializedObject so = new SerializedObject(enemyComponent);
        so.FindProperty("healthBarFill").objectReferenceValue = healthBarFill.transform;
        so.ApplyModifiedProperties();

        return SaveAsPrefab(enemy, "Enemy");
    }

    private static GameObject BuildTowerPrefab(Material baseMat, Material headMat, GameObject projectilePrefab)
    {
        GameObject tower = new GameObject("ArcherTower");

        GameObject baseCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseCylinder.name = "Base";
        baseCylinder.transform.SetParent(tower.transform);
        baseCylinder.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        baseCylinder.transform.localScale = new Vector3(1f, 0.6f, 1f);
        baseCylinder.GetComponent<Renderer>().sharedMaterial = baseMat;

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(tower.transform);
        head.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        head.transform.localScale = new Vector3(0.6f, 0.6f, 0.9f);
        head.GetComponent<Renderer>().sharedMaterial = headMat;
        Object.DestroyImmediate(head.GetComponent<BoxCollider>());

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(head.transform);
        firePoint.transform.localPosition = new Vector3(0f, 0f, 0.6f);

        Tower towerComponent = tower.AddComponent<Tower>();
        SerializedObject so = new SerializedObject(towerComponent);
        so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        so.FindProperty("rotatingHead").objectReferenceValue = head.transform;
        so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        so.ApplyModifiedProperties();

        return SaveAsPrefab(tower, "ArcherTower");
    }

    private static GameObject BuildProjectilePrefab(Material projectileMat)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        projectile.GetComponent<Renderer>().sharedMaterial = projectileMat;
        Object.DestroyImmediate(projectile.GetComponent<SphereCollider>());
        projectile.AddComponent<Projectile>();

        return SaveAsPrefab(projectile, "Projectile");
    }

    private static GameObject SaveAsPrefab(GameObject instance, string name)
    {
        if (!AssetDatabase.IsValidFolder(PrefabsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string path = $"{PrefabsFolder}/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
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

    private static void BuildUI(Transform parent, GameManager gameManager, WaveSpawner waveSpawner)
    {
        GameObject canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(parent);
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform topBar = CreatePanel(canvasGO.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, 80f));

        Text currencyText = CreateText("CurrencyText", topBar, builtinFont, "Gold: 0", 28,
            new Vector2(0f, 0f), new Vector2(0.33f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);

        Text baseHealthText = CreateText("BaseHealthText", topBar, builtinFont, "Base HP: 0/0", 28,
            new Vector2(0.33f, 0f), new Vector2(0.66f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        Text waveStatusText = CreateText("WaveStatusText", topBar, builtinFont, "Press Start Wave", 28,
            new Vector2(0.66f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleRight);

        Text gameOverText = CreateText("GameOverText", canvasGO.transform, builtinFont, "GAME OVER", 72,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 150f), TextAnchor.MiddleCenter);
        gameOverText.color = Color.red;
        gameOverText.gameObject.SetActive(false);

        RectTransform bottomBar = CreatePanel(canvasGO.transform, "BottomBar", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 50f), new Vector2(500f, 80f));

        Button startWaveButton = CreateButton("StartWaveButton", bottomBar, builtinFont, "Start Wave",
            new Vector2(0f, 0f), new Vector2(0.33f, 1f));

        Button pauseButton = CreateButton("PauseButton", bottomBar, builtinFont, "Pause",
            new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
        Text pauseLabel = pauseButton.GetComponentInChildren<Text>();

        Button restartButton = CreateButton("RestartButton", bottomBar, builtinFont, "Restart",
            new Vector2(0.66f, 0f), new Vector2(1f, 1f));

        UIManager uiManager = canvasGO.AddComponent<UIManager>();
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("gameManager").objectReferenceValue = gameManager;
        so.FindProperty("waveSpawner").objectReferenceValue = waveSpawner;
        so.FindProperty("currencyText").objectReferenceValue = currencyText;
        so.FindProperty("baseHealthText").objectReferenceValue = baseHealthText;
        so.FindProperty("waveStatusText").objectReferenceValue = waveStatusText;
        so.FindProperty("gameOverText").objectReferenceValue = gameOverText;
        so.FindProperty("startWaveButton").objectReferenceValue = startWaveButton;
        so.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        so.FindProperty("pauseButtonLabel").objectReferenceValue = pauseLabel;
        so.FindProperty("restartButton").objectReferenceValue = restartButton;
        so.ApplyModifiedProperties();
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static Text CreateText(string name, Transform parent, Font font, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
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
        image.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

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
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return button;
    }

    private static void RegisterActiveSceneInBuildSettings()
    {
        string scenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(scenePath))
        {
            return;
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in scenes)
        {
            if (s.path == scenePath)
            {
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
