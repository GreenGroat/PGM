using System.IO;
using Lab7Territory;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab7SceneBuilder
{
    private const string Root = "Assets";
    private const string MaterialFolder = Root + "/Materials";
    private const string ScenePath = Root + "/Scenes/TerritoryScene.unity";

    [MenuItem("Lab Builders/Build Lab7 Territory Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(Root + "/Scenes");

        EnsureTag("Player");
        EnsureTag("Enemy");

        Material floorMat = CreateMaterial("M_Tactical_Floor", new Color(0.025f, 0.03f, 0.045f), 0f, 0f);
        Material lineCyan = CreateMaterial("M_Line_Cyan", new Color(0.05f, 0.85f, 1f), 0.1f, 1.8f);
        Material lineMagenta = CreateMaterial("M_Line_Magenta", new Color(1f, 0.08f, 0.62f), 0.1f, 1.5f);
        Material neutralMat = CreateTransparentMaterial("M_Zone_Neutral", new Color(0.55f, 0.58f, 0.64f, 0.42f));
        Material playerMat = CreateTransparentMaterial("M_Zone_Player", new Color(0.05f, 0.78f, 1f, 0.52f));
        Material enemyMat = CreateTransparentMaterial("M_Zone_Enemy", new Color(1f, 0.12f, 0.28f, 0.52f));
        Material contestedMat = CreateTransparentMaterial("M_Zone_Contested", new Color(1f, 0.72f, 0.1f, 0.55f));
        Material playerUnitMat = CreateMaterial("M_Player_Unit", new Color(0.08f, 0.85f, 1f), 0.3f, 1.6f);
        Material enemyUnitMat = CreateMaterial("M_Enemy_Unit", new Color(1f, 0.1f, 0.3f), 0.25f, 1.5f);
        Material wallMat = CreateMaterial("M_Tactical_Walls", new Color(0.07f, 0.08f, 0.11f), 0f, 0.1f);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.14f, 0.16f, 0.22f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.015f, 0.018f, 0.028f);
        RenderSettings.fogDensity = 0.012f;

        CreateArena(floorMat, lineCyan, lineMagenta, wallMat);
        GameObject player = CreatePlayer(playerUnitMat);
        CaptureZone[] zones = CreateZones(neutralMat, playerMat, enemyMat, contestedMat, lineCyan, lineMagenta);
        CreateEnemy(enemyUnitMat, zones);
        CreateLighting();
        CreateUI(zones);

        player.transform.position = new Vector3(-10f, 0.95f, -6.5f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab7 territory scene generated: " + ScenePath);
    }

    private static void CreateArena(Material floorMat, Material cyanMat, Material magentaMat, Material wallMat)
    {
        CreateCube("Command Grid Floor", Vector3.zero, new Vector3(32f, 0.15f, 22f), floorMat);

        for (int x = -16; x <= 16; x += 2)
        {
            CreateCube("Grid Column " + x, new Vector3(x, 0.11f, 0f), new Vector3(0.03f, 0.03f, 22f), x % 4 == 0 ? cyanMat : magentaMat);
        }

        for (int z = -10; z <= 10; z += 2)
        {
            CreateCube("Grid Row " + z, new Vector3(0f, 0.12f, z), new Vector3(32f, 0.03f, 0.03f), z % 4 == 0 ? magentaMat : cyanMat);
        }

        CreateCube("North Barrier", new Vector3(0f, 0.7f, 11f), new Vector3(32f, 1.4f, 0.4f), wallMat);
        CreateCube("South Barrier", new Vector3(0f, 0.7f, -11f), new Vector3(32f, 1.4f, 0.4f), wallMat);
        CreateCube("West Barrier", new Vector3(-16f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 22f), wallMat);
        CreateCube("East Barrier", new Vector3(16f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 22f), wallMat);

        CreateCube("Central Low Cover A", new Vector3(-2f, 0.35f, 0f), new Vector3(3.5f, 0.7f, 0.4f), wallMat);
        CreateCube("Central Low Cover B", new Vector3(2f, 0.35f, 0f), new Vector3(3.5f, 0.7f, 0.4f), wallMat);
        CreateCube("Diagonal Data Spine", new Vector3(0f, 0.25f, 5.5f), new Vector3(8f, 0.5f, 0.3f), wallMat).transform.rotation = Quaternion.Euler(0f, -20f, 0f);
    }

    private static GameObject CreatePlayer(Material material)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player Commander";
        player.tag = "Player";
        player.transform.position = new Vector3(-10f, 0.95f, -6.5f);
        player.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.center = Vector3.up * 0.9f;

        GameObject pivot = new GameObject("Camera Pivot");
        pivot.transform.SetParent(player.transform);
        pivot.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(pivot.transform);
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;
        Camera playerCamera = cameraObject.AddComponent<Camera>();
        playerCamera.fieldOfView = 68f;
        playerCamera.nearClipPlane = 0.05f;
        cameraObject.AddComponent<AudioListener>();

        TopDownPlayerController playerController = player.AddComponent<TopDownPlayerController>();
        playerController.playerCamera = playerCamera;
        playerController.cameraPivot = pivot.transform;

        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = "Player Selection Ring";
        beacon.transform.SetParent(player.transform);
        beacon.transform.localPosition = new Vector3(0f, -0.88f, 0f);
        beacon.transform.localScale = new Vector3(1.3f, 0.035f, 1.3f);
        beacon.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(beacon.GetComponent<Collider>());
        return player;
    }

    private static void CreateEnemy(Material material, CaptureZone[] zones)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy Raider";
        enemy.tag = "Enemy";
        enemy.transform.position = new Vector3(10.5f, 0.95f, 6f);
        enemy.GetComponent<Renderer>().sharedMaterial = material;
        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.speed = 4.2f;
        agent.angularSpeed = 540f;
        agent.acceleration = 18f;

        EnemyAI ai = enemy.AddComponent<EnemyAI>();
        ai.allZones = zones;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Enemy Selection Ring";
        marker.transform.SetParent(enemy.transform);
        marker.transform.localPosition = new Vector3(0f, -0.88f, 0f);
        marker.transform.localScale = new Vector3(1.25f, 0.035f, 1.25f);
        marker.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(marker.GetComponent<Collider>());
    }

    private static CaptureZone[] CreateZones(Material neutralMat, Material playerMat, Material enemyMat, Material contestedMat, Material cyanMat, Material magentaMat)
    {
        CaptureZone alpha = CreateZone("Zone Alpha Relay", new Vector3(-7f, 0.1f, 4.8f), 0.15f, neutralMat, playerMat, enemyMat, contestedMat, cyanMat);
        CaptureZone beta = CreateZone("Zone Beta Core", new Vector3(1.5f, 0.1f, -4.2f), 0f, neutralMat, playerMat, enemyMat, contestedMat, magentaMat);
        CaptureZone gamma = CreateZone("Zone Gamma Gate", new Vector3(8.5f, 0.1f, 3.8f), -0.15f, neutralMat, playerMat, enemyMat, contestedMat, cyanMat);
        return new[] { alpha, beta, gamma };
    }

    private static CaptureZone CreateZone(string name, Vector3 position, float startProgress, Material neutralMat, Material playerMat, Material enemyMat, Material contestedMat, Material beaconMat)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;

        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3f;

        GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disk.name = "Capture Disk";
        disk.transform.SetParent(root.transform);
        disk.transform.localPosition = Vector3.zero;
        disk.transform.localScale = new Vector3(3f, 0.06f, 3f);
        disk.GetComponent<Renderer>().sharedMaterial = neutralMat;
        Object.DestroyImmediate(disk.GetComponent<Collider>());

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        core.name = "Status Core";
        core.transform.SetParent(root.transform);
        core.transform.localPosition = new Vector3(0f, 1f, 0f);
        core.transform.localScale = new Vector3(0.4f, 1.4f, 0.4f);
        core.GetComponent<Renderer>().sharedMaterial = beaconMat;
        core.AddComponent<FloatingBeacon>();

        CreateZoneRing(root.transform, beaconMat);
        ZoneUI ui = CreateWorldZoneUI(root.transform, name.Replace("Zone ", ""));

        CaptureZone zone = root.AddComponent<CaptureZone>();
        zone.captureTime = 4.5f;
        zone.captureProgress = startProgress;
        zone.zoneRenderer = disk.GetComponent<Renderer>();
        zone.coreRenderer = core.GetComponent<Renderer>();
        zone.neutralMaterial = neutralMat;
        zone.playerMaterial = playerMat;
        zone.enemyMaterial = enemyMat;
        zone.contestedMaterial = contestedMat;
        ui.zone = zone;
        return zone;
    }

    private static void CreateZoneRing(Transform parent, Material material)
    {
        for (int i = 0; i < 20; i++)
        {
            float angle = i * Mathf.PI * 2f / 20f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 3.1f, 0.08f, Mathf.Sin(angle) * 3.1f);
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Ring Segment " + i;
            block.transform.SetParent(parent);
            block.transform.localPosition = pos;
            block.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
            block.transform.localScale = new Vector3(0.46f, 0.05f, 0.08f);
            block.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(block.GetComponent<Collider>());
        }
    }

    private static ZoneUI CreateWorldZoneUI(Transform parent, string label)
    {
        GameObject canvasObject = new GameObject("Zone Progress UI");
        canvasObject.transform.SetParent(parent);
        canvasObject.transform.localPosition = new Vector3(0f, 2.65f, 0f);
        canvasObject.transform.localScale = Vector3.one * 0.013f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObject.AddComponent<GraphicRaycaster>();
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(230f, 72f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject labelObject = CreateUIRect("Zone Label", canvasObject.transform, new Vector2(0f, 20f), new Vector2(220f, 26f));
        Text labelText = labelObject.AddComponent<Text>();
        labelText.font = font;
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = new Color(0.8f, 0.92f, 1f);

        GameObject sliderObject = CreateUIRect("Progress Slider", canvasObject.transform, new Vector2(0f, -12f), new Vector2(210f, 18f));
        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.transition = Selectable.Transition.None;

        GameObject background = CreateUIRect("Background", sliderObject.transform, Vector2.zero, new Vector2(210f, 18f));
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.02f, 0.03f, 0.05f, 0.78f);
        slider.targetGraphic = bgImage;

        GameObject fillArea = CreateUIRect("Fill Area", sliderObject.transform, Vector2.zero, new Vector2(210f, 18f));
        GameObject fill = CreateUIRect("Fill", fillArea.transform, Vector2.zero, new Vector2(210f, 18f));
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.05f, 0.78f, 1f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        ZoneUI ui = canvasObject.AddComponent<ZoneUI>();
        ui.progressSlider = slider;
        ui.fillImage = fillImage;
        ui.labelText = labelText;
        return ui;
    }

    private static void CreateUI(CaptureZone[] zones)
    {
        GameObject canvasObject = new GameObject("Tactical HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject scorePanel = CreateScreenRect("Score Panel", canvasObject.transform, new Vector2(22f, -22f), new Vector2(360f, 76f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image panelImage = scorePanel.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.02f, 0.035f, 0.84f);

        GameObject scoreTextObject = CreateScreenRect("Score Text", scorePanel.transform, new Vector2(16f, -12f), new Vector2(320f, 26f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Text scoreText = scoreTextObject.AddComponent<Text>();
        scoreText.font = font;
        scoreText.text = "Player 0  |  Enemy 0";
        scoreText.fontSize = 18;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.alignment = TextAnchor.MiddleLeft;
        scoreText.color = new Color(0.82f, 0.93f, 1f);

        GameObject helpObject = CreateScreenRect("Controls Text", scorePanel.transform, new Vector2(16f, -42f), new Vector2(320f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Text helpText = helpObject.AddComponent<Text>();
        helpText.font = font;
        helpText.text = "WASD: move | Mouse: look | enter zones to capture";
        helpText.fontSize = 14;
        helpText.alignment = TextAnchor.MiddleLeft;
        helpText.color = new Color(0.58f, 0.78f, 0.92f);

        GameObject winPanel = CreateScreenRect("Win Panel", canvasObject.transform, Vector2.zero, new Vector2(560f, 120f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Image winPanelImage = winPanel.AddComponent<Image>();
        winPanelImage.color = new Color(0.015f, 0.02f, 0.035f, 0.92f);

        GameObject winTextObject = CreateScreenRect("Win Text", winPanel.transform, Vector2.zero, new Vector2(520f, 80f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Text winText = winTextObject.AddComponent<Text>();
        winText.font = font;
        winText.text = "PLAYER CONTROLS THE GRID";
        winText.fontSize = 24;
        winText.fontStyle = FontStyle.Bold;
        winText.alignment = TextAnchor.MiddleCenter;
        winText.color = new Color(0.05f, 0.85f, 1f);
        winPanel.SetActive(false);

        GameObject managers = new GameObject("Game Managers");
        ResourceManager resources = managers.AddComponent<ResourceManager>();
        resources.scoreText = scoreText;

        GameManager gameManager = managers.AddComponent<GameManager>();
        gameManager.zones = zones;
        gameManager.resourceManager = resources;
        gameManager.winPanel = winPanel;
        gameManager.winText = winText;
        gameManager.targetPoints = 180;

        CreateTopZoneStatus(canvasObject.transform, zones, font);
    }

    private static void CreateTopZoneStatus(Transform canvasRoot, CaptureZone[] zones, Font font)
    {
        GameObject row = CreateScreenRect("Zone Status Row", canvasRoot, new Vector2(0f, -30f), new Vector2(360f, 84f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        ZoneStatusHUD hud = row.AddComponent<ZoneStatusHUD>();
        hud.zones = zones;
        hud.radialFills = new Image[zones.Length];
        hud.rings = new Image[zones.Length];
        hud.labels = new Text[zones.Length];

        for (int i = 0; i < zones.Length; i++)
        {
            GameObject circle = CreateScreenRect("Zone Circle " + (i + 1), row.transform, new Vector2(-112f + i * 112f, 0f), new Vector2(66f, 66f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Image ring = circle.AddComponent<Image>();
            ring.color = new Color(0.5f, 0.55f, 0.62f, 0.28f);

            GameObject fillObject = CreateScreenRect("Radial Fill", circle.transform, Vector2.zero, new Vector2(56f, 56f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Image fill = fillObject.AddComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = 2;
            fill.fillAmount = 0f;
            fill.color = new Color(0.05f, 0.78f, 1f);

            GameObject labelObject = CreateScreenRect("Zone Label", circle.transform, Vector2.zero, new Vector2(44f, 24f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Text label = labelObject.AddComponent<Text>();
            label.font = font;
            label.text = ((char)('A' + i)).ToString();
            label.fontSize = 18;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.82f, 0.93f, 1f);

            hud.rings[i] = ring;
            hud.radialFills[i] = fill;
            hud.labels[i] = label;
        }
    }

    private static void CreateLighting()
    {
        GameObject directional = new GameObject("Strategic Directional Light");
        Light light = directional.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.7f;
        light.color = new Color(0.68f, 0.82f, 1f);
        directional.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        CreatePointLight("Alpha Cyan Wash", new Vector3(-7f, 4f, 5f), new Color(0.05f, 0.78f, 1f), 3f, 9f);
        CreatePointLight("Beta Magenta Wash", new Vector3(1.5f, 4f, -4f), new Color(1f, 0.08f, 0.62f), 3f, 9f);
        CreatePointLight("Gamma Cyan Wash", new Vector3(8.5f, 4f, 4f), new Color(0.05f, 0.78f, 1f), 3f, 9f);
    }

    private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static GameObject CreateUIRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return go;
    }

    private static GameObject CreateScreenRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return go;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static Material CreateMaterial(string name, Color color, float metallic, float emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", 0.58f);
        if (emission > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
        return material;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material material = CreateMaterial(name, color, 0f, 0.7f);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        material.color = color;
        return material;
    }

    private static void EnsureTag(string tag)
    {
        foreach (string existingTag in InternalEditorUtility.tags)
        {
            if (existingTag == tag)
            {
                return;
            }
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
