using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab1SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Level_Prototype.unity";
    private const string MaterialsPath = "Assets/Materials";
    private const string PrefabsPath = "Assets/Prefabs";
    private const string DocumentationPath = "Assets/Documentation";

    public static void Build()
    {
        EnsureFolders();
        EnsureTag("Player");
        int groundLayer = EnsureLayer("Ground");

        Material groundMaterial = CreateMaterial("Ground_Mat", new Color(0.36f, 0.58f, 0.40f));
        Material playerMaterial = CreateMaterial("Player_Mat", new Color(0.16f, 0.35f, 0.95f));
        Material obstacleMaterial = CreateMaterial("Obstacle_Mat", new Color(0.90f, 0.25f, 0.20f));
        Material coinMaterial = CreateMaterial("Coin_Mat", new Color(1.00f, 0.82f, 0.16f));
        Material boostMaterial = CreateMaterial("SpeedBoost_Mat", new Color(0.10f, 0.85f, 0.95f));
        Material finishMaterial = CreateMaterial("Finish_Mat", new Color(0.15f, 0.85f, 0.25f));
        Material wallMaterial = CreateMaterial("Wall_Mat", new Color(0.38f, 0.40f, 0.46f));
        Material platformMaterial = CreateMaterial("Platform_Mat", new Color(0.62f, 0.53f, 0.42f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_Prototype";

        SetupLighting();

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.layer = groundLayer;
        AssignMaterial(ground, groundMaterial);

        CreateWall("North_Wall", new Vector3(0f, 1f, 15.2f), new Vector3(30f, 2f, 0.4f), wallMaterial);
        CreateWall("South_Wall", new Vector3(0f, 1f, -15.2f), new Vector3(30f, 2f, 0.4f), wallMaterial);
        CreateWall("East_Wall", new Vector3(15.2f, 1f, 0f), new Vector3(0.4f, 2f, 30f), wallMaterial);
        CreateWall("West_Wall", new Vector3(-15.2f, 1f, 0f), new Vector3(0.4f, 2f, 30f), wallMaterial);

        GameObject raisedPlatform = CreateCube("Raised_Platform", new Vector3(6f, 0.35f, 1.5f), new Vector3(4f, 0.7f, 4f), platformMaterial);
        raisedPlatform.layer = groundLayer;

        GameObject ramp = CreateCube("Simple_Ramp", new Vector3(-5f, 0.35f, 2f), new Vector3(4f, 0.7f, 3f), platformMaterial);
        ramp.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
        ramp.layer = groundLayer;

        GameObject player = CreatePlayer(groundLayer, playerMaterial);
        Camera mainCamera = CreateCamera(player.transform);
        UIManager uiManager = CreateUI(player.transform);

        GameObject obstaclePrefab = CreateObstaclePrefab(obstacleMaterial);
        InstantiatePrefab(obstaclePrefab, "Low_Wall_01", new Vector3(0f, 0.5f, -4f), Quaternion.identity, new Vector3(5f, 1f, 0.75f));
        InstantiatePrefab(obstaclePrefab, "Low_Wall_02", new Vector3(-4f, 0.5f, 4.5f), Quaternion.Euler(0f, 30f, 0f), new Vector3(4f, 1f, 0.75f));
        InstantiatePrefab(obstaclePrefab, "Low_Wall_03", new Vector3(4.5f, 0.5f, 7.5f), Quaternion.Euler(0f, -25f, 0f), new Vector3(4f, 1f, 0.75f));

        GameObject coinPrefab = CreateCoinPrefab(coinMaterial);
        Vector3[] coinPositions =
        {
            new Vector3(-6f, 0.55f, -6f),
            new Vector3(4f, 0.55f, -7f),
            new Vector3(-7f, 0.55f, 2.5f),
            new Vector3(6f, 1.15f, 1.5f),
            new Vector3(1f, 0.55f, 8f)
        };

        for (int i = 0; i < coinPositions.Length; i++)
        {
            InstantiatePrefab(coinPrefab, $"Coin_{i + 1:00}", coinPositions[i], Quaternion.identity, Vector3.one);
        }

        GameObject speedBoostPrefab = CreateSpeedBoostPrefab(boostMaterial);
        InstantiatePrefab(speedBoostPrefab, "SpeedBoost_01", new Vector3(-5f, 0.65f, -1f), Quaternion.identity, Vector3.one);

        CreateFinish(uiManager, finishMaterial);
        CreateStartMarker(platformMaterial);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        if (!Application.isBatchMode)
        {
            RenderGameViewScreenshot(mainCamera, Path.Combine(DocumentationPath, "Lab1_GameView.png"));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab1 scene generated successfully.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Scripts");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Documentation");
    }

    private static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.58f, 0.61f, 0.66f);
        RenderSettings.fog = false;

        GameObject lightObject = new GameObject("Directional_Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
    }

    private static GameObject CreatePlayer(int groundLayer, Material material)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -10f);
        player.transform.localScale = new Vector3(1f, 1.5f, 1f);
        AssignMaterial(player, material);

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1.25f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        player.AddComponent<PlayerInput>();
        Mover mover = player.AddComponent<Mover>();

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.86f, 0f);

        SerializedObject moverObject = new SerializedObject(mover);
        moverObject.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        moverObject.FindProperty("groundLayer").intValue = 1 << groundLayer;
        moverObject.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static Camera CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 7f, -13f);
        cameraObject.transform.rotation = Quaternion.Euler(34f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.56f, 0.72f, 0.86f);
        camera.fieldOfView = 55f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        SerializedObject followObject = new SerializedObject(follow);
        followObject.FindProperty("target").objectReferenceValue = target;
        followObject.FindProperty("offset").vector3Value = new Vector3(0f, 7f, -9f);
        followObject.ApplyModifiedPropertiesWithoutUndo();

        return camera;
    }

    private static UIManager CreateUI(Transform player)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Text scoreText = CreateText("Score_Text", canvasObject.transform, new Vector2(20f, -20f), new Vector2(520f, 38f), 24);
        Text positionText = CreateText("Position_Text", canvasObject.transform, new Vector2(20f, -60f), new Vector2(620f, 34f), 20);
        Text statusText = CreateText("Status_Text", canvasObject.transform, new Vector2(20f, -100f), new Vector2(760f, 34f), 20);

        GameObject uiObject = new GameObject("UIManager");
        UIManager uiManager = uiObject.AddComponent<UIManager>();
        SerializedObject ui = new SerializedObject(uiManager);
        ui.FindProperty("scoreText").objectReferenceValue = scoreText;
        ui.FindProperty("positionText").objectReferenceValue = positionText;
        ui.FindProperty("statusText").objectReferenceValue = statusText;
        ui.FindProperty("player").objectReferenceValue = player;
        ui.FindProperty("targetScore").intValue = 5;
        ui.ApplyModifiedPropertiesWithoutUndo();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        return uiManager;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    private static GameObject CreateObstaclePrefab(Material material)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "ObstacleBlock";
        AssignMaterial(obstacle, material);
        obstacle.AddComponent<ObstacleReporter>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obstacle, $"{PrefabsPath}/ObstacleBlock.prefab");
        Object.DestroyImmediate(obstacle);
        return prefab;
    }

    private static GameObject CreateCoinPrefab(Material material)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Coin";
        coin.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        AssignMaterial(coin, material);

        SphereCollider collider = coin.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        Collectible collectible = coin.AddComponent<Collectible>();

        SerializedObject serialized = new SerializedObject(collectible);
        serialized.FindProperty("kind").enumValueIndex = (int)CollectibleKind.Coin;
        serialized.FindProperty("scoreValue").intValue = 1;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, $"{PrefabsPath}/Coin.prefab");
        Object.DestroyImmediate(coin);
        return prefab;
    }

    private static GameObject CreateSpeedBoostPrefab(Material material)
    {
        GameObject boost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boost.name = "SpeedBoost";
        boost.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        AssignMaterial(boost, material);

        CapsuleCollider collider = boost.GetComponent<CapsuleCollider>();
        collider.isTrigger = true;
        Collectible collectible = boost.AddComponent<Collectible>();

        SerializedObject serialized = new SerializedObject(collectible);
        serialized.FindProperty("kind").enumValueIndex = (int)CollectibleKind.SpeedBoost;
        serialized.FindProperty("boostMultiplier").floatValue = 1.65f;
        serialized.FindProperty("boostDuration").floatValue = 4.5f;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(boost, $"{PrefabsPath}/SpeedBoost.prefab");
        Object.DestroyImmediate(boost);
        return prefab;
    }

    private static void CreateFinish(UIManager uiManager, Material material)
    {
        GameObject finish = CreateCube("Finish_Zone", new Vector3(0f, 0.05f, 12f), new Vector3(5f, 0.1f, 2.25f), material);
        BoxCollider finishCollider = finish.GetComponent<BoxCollider>();
        finishCollider.isTrigger = true;
        GoalZone goalZone = finish.AddComponent<GoalZone>();

        SerializedObject serialized = new SerializedObject(goalZone);
        serialized.FindProperty("uiManager").objectReferenceValue = uiManager;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateCube("Finish_Left_Post", new Vector3(-2.55f, 1.25f, 12f), new Vector3(0.25f, 2.5f, 0.25f), material);
        CreateCube("Finish_Right_Post", new Vector3(2.55f, 1.25f, 12f), new Vector3(0.25f, 2.5f, 0.25f), material);
        CreateCube("Finish_Top_Bar", new Vector3(0f, 2.55f, 12f), new Vector3(5.4f, 0.25f, 0.25f), material);
    }

    private static void CreateStartMarker(Material material)
    {
        GameObject marker = CreateCube("Start_Marker", new Vector3(0f, 0.03f, -10f), new Vector3(3f, 0.06f, 2f), material);
        Object.DestroyImmediate(marker.GetComponent<BoxCollider>());
    }

    private static GameObject InstantiatePrefab(GameObject prefab, string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        return instance;
    }

    private static GameObject CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = CreateCube(name, position, scale, material);
        wall.AddComponent<ObstacleReporter>();
        return wall;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        AssignMaterial(cube, material);
        return cube;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string assetPath = $"{MaterialsPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private static void AssignMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void RenderGameViewScreenshot(Camera camera, string assetRelativePath)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        RenderTexture renderTexture = new RenderTexture(1280, 720, 24);
        Texture2D screenshot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousCameraTarget = camera.targetTexture;

        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        camera.Render();
        screenshot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        screenshot.Apply();

        File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());

        camera.targetTexture = previousCameraTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(screenshot);

        AssetDatabase.ImportAsset(assetRelativePath);
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            if (tagsProperty.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }

        tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
        tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static int EnsureLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProperty = tagManager.FindProperty("layers");

        for (int i = 0; i < layersProperty.arraySize; i++)
        {
            if (layersProperty.GetArrayElementAtIndex(i).stringValue == layerName)
            {
                return i;
            }
        }

        for (int i = 8; i < layersProperty.arraySize; i++)
        {
            SerializedProperty layer = layersProperty.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
        }

        throw new System.InvalidOperationException($"No free layer slot for {layerName}.");
    }
}
