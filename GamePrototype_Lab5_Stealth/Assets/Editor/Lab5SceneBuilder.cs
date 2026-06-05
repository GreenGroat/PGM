using System.IO;
using Lab5Stealth;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab5SceneBuilder
{
    private const string Root = "Assets";
    private const string MaterialFolder = Root + "/Materials";
    private const string ScenePath = Root + "/Scenes/StealthLabScene.unity";

    [MenuItem("Lab Builders/Build Lab5 Stealth Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(Root + "/Scenes");

        EnsureTag("Player");
        EnsureTag("Enemy");
        EnsureTag("Cover");
        EnsureLayer(8, "Player");
        EnsureLayer(9, "Obstacle");

        Material floorMat = CreateMaterial("M_Floor_DarkGrid", new Color(0.025f, 0.03f, 0.045f), 0f, 0f);
        Material wallMat = CreateMaterial("M_Wall_Graphite", new Color(0.06f, 0.07f, 0.1f), 0f, 0f);
        Material cyanMat = CreateMaterial("M_Cyan_Emission", new Color(0.05f, 0.78f, 1f), 0.2f, 1.8f);
        Material magentaMat = CreateMaterial("M_Magenta_Emission", new Color(1f, 0.08f, 0.65f), 0.2f, 1.6f);
        Material amberMat = CreateMaterial("M_Amber_Emission", new Color(1f, 0.65f, 0.08f), 0.1f, 1.3f);
        Material redMat = CreateMaterial("M_Red_Alert", new Color(1f, 0.12f, 0.22f), 0.1f, 1.5f);
        Material coverMat = CreateMaterial("M_Cover_BlueBlack", new Color(0.03f, 0.18f, 0.26f), 0.15f, 0.7f);
        Material coneMat = CreateTransparentMaterial("M_VisionCone_Transparent", new Color(0.05f, 0.9f, 1f, 0.22f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.015f, 0.018f, 0.027f);
        RenderSettings.fogDensity = 0.018f;

        GameObject floor = CreateCube("Obsidian Floor", Vector3.zero, new Vector3(34f, 0.15f, 24f), floorMat);
        floor.layer = LayerMask.NameToLayer("Obstacle");

        CreateGridLines(cyanMat, magentaMat);
        CreateLevelGeometry(wallMat, coverMat, cyanMat);
        CreatePlayer(cyanMat);
        CreateEnemies(cyanMat, amberMat, redMat, coneMat);
        CreateLighting(cyanMat, magentaMat, amberMat);
        CreateUI();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab5 stealth scene generated: " + ScenePath);
    }

    private static void CreatePlayer(Material playerMat)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        SetLayerRecursively(player, LayerMask.NameToLayer("Player"));
        player.transform.position = new Vector3(-13.2f, 0.95f, 8.4f);
        player.GetComponent<Renderer>().sharedMaterial = playerMat;
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = Vector3.up * 0.9f;

        PlayerStealth stealth = player.AddComponent<PlayerStealth>();
        stealth.noiseRadiusMultiplier = 10f;

        GameObject pivot = new GameObject("Camera Pivot");
        pivot.transform.SetParent(player.transform);
        pivot.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(pivot.transform);
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        cameraObject.AddComponent<AudioListener>();

        FPSController controllerScript = player.AddComponent<FPSController>();
        controllerScript.playerCamera = camera;
        controllerScript.cameraPivot = pivot.transform;
    }

    private static void CreateEnemies(Material patrolMat, Material suspicionMat, Material alertMat, Material coneMat)
    {
        CreateEnemy("Enemy Sentinel A", new Vector3(-3f, 1f, -5f), new[]
        {
            new Vector3(-7f, 0f, -5f),
            new Vector3(2f, 0f, -5f),
            new Vector3(2f, 0f, 1f),
            new Vector3(-7f, 0f, 1f)
        }, patrolMat, suspicionMat, alertMat, coneMat);

        CreateEnemy("Enemy Sentinel B", new Vector3(8f, 1f, 4f), new[]
        {
            new Vector3(5f, 0f, 4f),
            new Vector3(12f, 0f, 4f),
            new Vector3(12f, 0f, -3f),
            new Vector3(5f, 0f, -3f)
        }, patrolMat, suspicionMat, alertMat, coneMat);
    }

    private static void CreateEnemy(string name, Vector3 position, Vector3[] waypointPositions, Material patrolMat, Material suspicionMat, Material alertMat, Material coneMat)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.tag = "Enemy";
        enemy.transform.position = position;
        enemy.GetComponent<Renderer>().sharedMaterial = patrolMat;

        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 2.2f;
        agent.angularSpeed = 540f;
        agent.acceleration = 16f;

        EnemyVision vision = enemy.AddComponent<EnemyVision>();
        vision.viewRadius = 9f;
        vision.viewAngle = 86f;
        vision.obstacleMask = LayerMask.GetMask("Obstacle");
        vision.playerMask = LayerMask.GetMask("Player");

        EnemyStateMachine stateMachine = enemy.AddComponent<EnemyStateMachine>();
        stateMachine.bodyRenderer = enemy.GetComponent<Renderer>();
        stateMachine.patrolMaterial = patrolMat;
        stateMachine.suspicionMaterial = suspicionMat;
        stateMachine.alertMaterial = alertMat;
        stateMachine.hearingRange = 8.5f;

        GameObject eye = new GameObject("Eye Point");
        eye.transform.SetParent(enemy.transform);
        eye.transform.localPosition = new Vector3(0f, 0.55f, 0.15f);
        vision.eyePoint = eye.transform;

        GameObject cone = new GameObject("Vision Cone Mesh");
        cone.transform.SetParent(enemy.transform);
        cone.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        cone.transform.localRotation = Quaternion.identity;
        cone.AddComponent<MeshFilter>();
        MeshRenderer coneRenderer = cone.AddComponent<MeshRenderer>();
        coneRenderer.sharedMaterial = coneMat;
        VisionConeMesh coneMesh = cone.AddComponent<VisionConeMesh>();
        coneMesh.vision = vision;

        Transform[] waypoints = new Transform[waypointPositions.Length];
        GameObject waypointRoot = new GameObject(name + " Waypoints");
        for (int i = 0; i < waypointPositions.Length; i++)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name + " Waypoint " + (i + 1);
            marker.transform.SetParent(waypointRoot.transform);
            marker.transform.position = waypointPositions[i] + Vector3.up * 0.03f;
            marker.transform.localScale = new Vector3(0.35f, 0.035f, 0.35f);
            marker.GetComponent<Renderer>().sharedMaterial = patrolMat;
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            waypoints[i] = marker.transform;
        }
        stateMachine.waypoints = waypoints;
    }

    private static void CreateLevelGeometry(Material wallMat, Material coverMat, Material cyanMat)
    {
        CreateWall("North Wall", new Vector3(0f, 1.25f, 12f), new Vector3(34f, 2.5f, 0.35f), wallMat);
        CreateWall("South Wall", new Vector3(0f, 1.25f, -12f), new Vector3(34f, 2.5f, 0.35f), wallMat);
        CreateWall("West Wall", new Vector3(-17f, 1.25f, 0f), new Vector3(0.35f, 2.5f, 24f), wallMat);
        CreateWall("East Wall", new Vector3(17f, 1.25f, 0f), new Vector3(0.35f, 2.5f, 24f), wallMat);

        CreateWall("Room Divider A", new Vector3(-6f, 1.25f, -1.5f), new Vector3(0.35f, 2.5f, 13f), wallMat);
        CreateWall("Room Divider B", new Vector3(6f, 1.25f, 1.5f), new Vector3(0.35f, 2.5f, 13f), wallMat);
        CreateWall("Short Corridor Wall A", new Vector3(0f, 1.25f, -6f), new Vector3(8f, 2.5f, 0.35f), wallMat);
        CreateWall("Short Corridor Wall B", new Vector3(0f, 1.25f, 6f), new Vector3(8f, 2.5f, 0.35f), wallMat);

        CreateCover("Cover Stack A", new Vector3(-8.5f, 0.55f, -3.5f), new Vector3(2.1f, 1.1f, 1.2f), coverMat);
        CreateCover("Cover Stack B", new Vector3(-1.5f, 0.55f, 2.5f), new Vector3(1.5f, 1.1f, 1.5f), coverMat);
        CreateCover("Cover Stack C", new Vector3(8.5f, 0.55f, -2.5f), new Vector3(2f, 1.1f, 1.2f), coverMat);
        CreateCover("Cover Stack D", new Vector3(11.8f, 0.55f, 6f), new Vector3(1.4f, 1.1f, 2f), coverMat);

        for (int i = 0; i < 5; i++)
        {
            GameObject strip = CreateCube("Route Light " + (i + 1), new Vector3(-13f + i * 3f, 0.08f, -9.5f), new Vector3(1.1f, 0.03f, 0.08f), cyanMat);
            strip.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
        }
    }

    private static void CreateGridLines(Material cyanMat, Material magentaMat)
    {
        for (int x = -16; x <= 16; x += 2)
        {
            Material mat = x % 4 == 0 ? cyanMat : magentaMat;
            CreateCube("Grid X " + x, new Vector3(x, 0.09f, 0f), new Vector3(0.025f, 0.025f, 24f), mat);
        }

        for (int z = -12; z <= 12; z += 2)
        {
            Material mat = z % 4 == 0 ? magentaMat : cyanMat;
            CreateCube("Grid Z " + z, new Vector3(0f, 0.1f, z), new Vector3(34f, 0.025f, 0.025f), mat);
        }
    }

    private static void CreateLighting(Material cyanMat, Material magentaMat, Material amberMat)
    {
        GameObject directional = new GameObject("Soft Directional Light");
        Light light = directional.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.55f;
        light.color = new Color(0.6f, 0.78f, 1f);
        directional.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        CreatePointLight("Cyan Patrol Light", new Vector3(-9f, 3.2f, -7f), new Color(0.1f, 0.8f, 1f), 3.5f, 10f);
        CreatePointLight("Magenta Archive Light", new Vector3(9f, 3.2f, 6f), new Color(1f, 0.1f, 0.65f), 3f, 9f);
        CreatePointLight("Amber Suspicion Light", new Vector3(0f, 3.2f, 0f), new Color(1f, 0.58f, 0.12f), 2f, 8f);

        CreateBeacon("Exit Beacon", new Vector3(14.5f, 1.1f, 9.5f), cyanMat);
        CreateBeacon("Security Core", new Vector3(1f, 1.1f, 8.2f), magentaMat);
        CreateBeacon("Noise Lure Marker", new Vector3(-10.8f, 1.1f, 5f), amberMat);
    }

    private static void CreateUI()
    {
        GameObject canvasObject = new GameObject("Stealth HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject panel = CreateUIRect("Indicator Panel", canvasObject.transform, new Vector2(22f, -22f), new Vector2(340f, 92f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.02f, 0.035f, 0.82f);

        GameObject labelObject = CreateUIRect("Status Text", panel.transform, new Vector2(18f, -16f), new Vector2(180f, 26f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Text statusText = labelObject.AddComponent<Text>();
        statusText.font = font;
        statusText.text = "HIDDEN";
        statusText.fontSize = 18;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleLeft;
        statusText.color = new Color(0.1f, 0.95f, 0.65f);

        GameObject barBack = CreateUIRect("Stealth Bar Back", panel.transform, new Vector2(18f, -54f), new Vector2(295f, 18f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image backImage = barBack.AddComponent<Image>();
        backImage.color = new Color(0.2f, 0.25f, 0.32f, 0.65f);

        GameObject barFill = CreateUIRect("Stealth Bar Fill", barBack.transform, Vector2.zero, new Vector2(295f, 18f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        Image fillImage = barFill.AddComponent<Image>();
        fillImage.color = new Color(0.1f, 0.95f, 0.65f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0f;

        GameObject helpObject = CreateUIRect("Controls Hint", canvasObject.transform, new Vector2(-22f, 22f), new Vector2(560f, 72f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        Text help = helpObject.AddComponent<Text>();
        help.font = font;
        help.text = "WASD move | Shift sprint makes noise | Ctrl/C crouch hides | cover blocks vision";
        help.fontSize = 15;
        help.alignment = TextAnchor.LowerRight;
        help.color = new Color(0.75f, 0.9f, 1f, 0.86f);

        GameObject manager = new GameObject("Stealth Indicator Manager");
        StealthIndicator indicator = manager.AddComponent<StealthIndicator>();
        indicator.fillImage = fillImage;
        indicator.statusText = statusText;
    }

    private static GameObject CreateUIRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
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

    private static void CreateBeacon(string name, Vector3 position, Material material)
    {
        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = name;
        beacon.transform.position = position;
        beacon.transform.localScale = new Vector3(0.35f, 1.1f, 0.35f);
        beacon.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = CreateCube(name, position, scale, material);
        wall.layer = LayerMask.NameToLayer("Obstacle");
    }

    private static void CreateCover(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cover = CreateCube(name, position, scale, material);
        cover.tag = "Cover";
        cover.layer = LayerMask.NameToLayer("Obstacle");
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
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
        material.SetFloat("_Glossiness", 0.55f);
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
        Material material = CreateMaterial(name, color, 0f, 0.6f);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
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
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureLayer(int index, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(layer.stringValue))
        {
            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
