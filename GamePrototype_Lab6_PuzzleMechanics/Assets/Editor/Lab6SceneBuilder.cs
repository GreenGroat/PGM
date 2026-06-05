using System.IO;
using System.Reflection;
using Lab6Puzzle;
using UnityEditor;
using UnityEditor.Events;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab6SceneBuilder
{
    private const string Root = "Assets";
    private const string MaterialFolder = Root + "/Materials";
    private const string ScenePath = Root + "/Scenes/PuzzleScene.unity";

    [MenuItem("Lab Builders/Build Lab6 Puzzle Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(Root + "/Scenes");
        Directory.CreateDirectory(Root + "/Prefabs");

        EnsureTag("Key");
        EnsureTag("Gem");

        Material floorMat = CreateMaterial("M_Puzzle_Floor", new Color(0.025f, 0.03f, 0.045f), 0f, 0f);
        Material wallMat = CreateMaterial("M_Puzzle_Wall", new Color(0.06f, 0.07f, 0.1f), 0f, 0.05f);
        Material cyanMat = CreateMaterial("M_Key_Cyan", new Color(0.05f, 0.82f, 1f), 0.15f, 1.8f);
        Material magentaMat = CreateMaterial("M_Gem_Magenta", new Color(1f, 0.08f, 0.62f), 0.15f, 1.7f);
        Material amberMat = CreateMaterial("M_Hover_Amber", new Color(1f, 0.68f, 0.1f), 0.1f, 1.6f);
        Material greenMat = CreateMaterial("M_Solved_Green", new Color(0.18f, 1f, 0.55f), 0.1f, 1.4f);
        Material slotIdleMat = CreateTransparentMaterial("M_Slot_Idle", new Color(0.3f, 0.38f, 0.48f, 0.22f));
        Material slotCyanMat = CreateTransparentMaterial("M_Slot_Cyan", new Color(0.05f, 0.82f, 1f, 0.38f));
        Material slotMagentaMat = CreateTransparentMaterial("M_Slot_Magenta", new Color(1f, 0.08f, 0.62f, 0.38f));
        Material doorMat = CreateMaterial("M_Door_Graphite", new Color(0.12f, 0.13f, 0.17f), 0.05f, 0.15f);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.13f, 0.15f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.015f, 0.018f, 0.028f);
        RenderSettings.fogDensity = 0.012f;

        CreateRoom(floorMat, wallMat, cyanMat, magentaMat);
        CreateCamera();
        CreateLighting();

        Door innerDoor = CreateDoor("Inner Door", new Vector3(0f, 1.25f, 0.5f), new Vector3(0f, 4.2f, 0.5f), doorMat, cyanMat);
        Door exitDoor = CreateDoor("Exit Door", new Vector3(8.6f, 1.25f, 2.7f), new Vector3(8.6f, 4.2f, 2.7f), doorMat, magentaMat);

        Slot keySlot = CreateSlot("Key Slot", new Vector3(-2.8f, 0.18f, -2.5f), new[] { "Key" }, slotIdleMat, slotCyanMat, greenMat);
        Slot gemSlot = CreateSlot("Gem Slot", new Vector3(5.1f, 0.18f, -2.5f), new[] { "Gem" }, slotIdleMat, slotMagentaMat, greenMat);

        GameObject key = CreateItem("MovableItem_Key", "Key", PrimitiveType.Cube, new Vector3(-7f, 0.78f, -4.2f), cyanMat, amberMat);
        GameObject gem = CreateItem("MovableItem_Gem", "Gem", PrimitiveType.Sphere, new Vector3(3.8f, 0.78f, 1.7f), magentaMat, amberMat);
        CreateItemBeacon(gem, magentaMat);

        PuzzleManager manager = CreateUI();
        manager.gemSlot = gemSlot;
        manager.gemItem = gem;
        manager.innerDoor = innerDoor;
        manager.exitDoor = exitDoor;

        UnityEventTools.AddPersistentListener(keySlot.OnItemPlaced, innerDoor.Open);
        UnityEventTools.AddPersistentListener(keySlot.OnItemPlaced, manager.EnableGemPhase);
        UnityEventTools.AddPersistentListener(gemSlot.OnItemPlaced, exitDoor.Open);
        UnityEventTools.AddPersistentListener(gemSlot.OnItemPlaced, manager.CompletePuzzle);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab6 puzzle scene generated: " + ScenePath);
    }

    [MenuItem("Lab Builders/Validate Lab6 Puzzle Scene")]
    public static void Validate()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        PuzzleManager manager = Require(UnityEngine.Object.FindFirstObjectByType<PuzzleManager>(), "Puzzle Manager");
        Door innerDoor = Require(GameObject.Find("Inner Door")?.GetComponent<Door>(), "Inner Door");
        Door exitDoor = Require(GameObject.Find("Exit Door")?.GetComponent<Door>(), "Exit Door");
        Slot keySlot = Require(GameObject.Find("Key Slot")?.GetComponent<Slot>(), "Key Slot");
        Slot gemSlot = Require(GameObject.Find("Gem Slot")?.GetComponent<Slot>(), "Gem Slot");
        GameObject key = Require(GameObject.Find("MovableItem_Key"), "MovableItem_Key");
        GameObject gem = Require(GameObject.Find("MovableItem_Gem"), "MovableItem_Gem");

        InvokePrivate(manager, "Awake");
        InvokePrivate(manager, "Start");

        keySlot.PlaceItem(key);
        if (!innerDoor.IsOpen || manager.gemItem == null || !manager.gemItem.activeSelf || !gemSlot.IsAvailable)
        {
            throw new System.Exception("Lab6 validation failed after key placement.");
        }

        gemSlot.PlaceItem(gem);
        if (!exitDoor.IsOpen || manager.winPanel == null || !manager.winPanel.activeSelf)
        {
            throw new System.Exception("Lab6 validation failed after gem placement.");
        }

        Debug.Log("Lab6 validation passed: key placement unlocks gem phase, gem placement completes puzzle.");
    }

    private static void CreateRoom(Material floorMat, Material wallMat, Material cyanMat, Material magentaMat)
    {
        CreateCube("Puzzle Floor", Vector3.zero, new Vector3(22f, 0.15f, 16f), floorMat);
        CreateCube("North Wall", new Vector3(0f, 1.4f, 8f), new Vector3(22f, 2.8f, 0.35f), wallMat);
        CreateCube("South Wall", new Vector3(0f, 1.4f, -8f), new Vector3(22f, 2.8f, 0.35f), wallMat);
        CreateCube("West Wall", new Vector3(-11f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 16f), wallMat);
        CreateCube("East Wall", new Vector3(11f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 16f), wallMat);
        CreateCube("Inner Divider Left", new Vector3(0f, 1.4f, -4.7f), new Vector3(0.35f, 2.8f, 6.1f), wallMat);
        CreateCube("Inner Divider Right", new Vector3(0f, 1.4f, 5.7f), new Vector3(0.35f, 2.8f, 4.7f), wallMat);

        for (int x = -10; x <= 10; x += 2)
        {
            CreateCube("Grid X " + x, new Vector3(x, 0.1f, 0f), new Vector3(0.025f, 0.025f, 16f), x % 4 == 0 ? cyanMat : magentaMat);
        }

        for (int z = -8; z <= 8; z += 2)
        {
            CreateCube("Grid Z " + z, new Vector3(0f, 0.11f, z), new Vector3(22f, 0.025f, 0.025f), z % 4 == 0 ? magentaMat : cyanMat);
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 12.5f, -11.5f);
        cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 48f;
        cameraObject.AddComponent<AudioListener>();
    }

    private static Slot CreateSlot(string name, Vector3 position, string[] acceptedTags, Material idle, Material active, Material solved)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.8f, 0.25f, 1.8f);
        collider.center = Vector3.zero;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Slot Visual";
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.95f, 0.05f, 0.95f);
        visual.GetComponent<Renderer>().sharedMaterial = active;
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        GameObject snap = new GameObject("Snap Point");
        snap.transform.SetParent(root.transform);
        snap.transform.localPosition = new Vector3(0f, 0.62f, 0f);

        Slot slot = root.AddComponent<Slot>();
        slot.acceptedTags = acceptedTags;
        slot.snapPoint = snap.transform;
        slot.visualRenderer = visual.GetComponent<Renderer>();
        slot.idleMaterial = idle;
        slot.activeMaterial = active;
        slot.solvedMaterial = solved;
        return slot;
    }

    private static GameObject CreateItem(string name, string tag, PrimitiveType primitive, Vector3 position, Material material, Material hoverMaterial)
    {
        GameObject item = GameObject.CreatePrimitive(primitive);
        item.name = name;
        item.tag = tag;
        item.transform.position = position;
        item.GetComponent<Renderer>().sharedMaterial = material;
        DraggableObject draggable = item.AddComponent<DraggableObject>();
        draggable.hoverMaterial = hoverMaterial;
        return item;
    }

    private static void CreateItemBeacon(GameObject item, Material material)
    {
        GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        halo.name = "Active Item Halo";
        halo.transform.SetParent(item.transform, false);
        halo.transform.localPosition = new Vector3(0f, -0.52f, 0f);
        halo.transform.localScale = new Vector3(1.35f, 0.025f, 1.35f);
        halo.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(halo.GetComponent<Collider>());

        Light light = item.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.08f, 0.62f);
        light.intensity = 2.4f;
        light.range = 4.5f;
    }

    private static Door CreateDoor(string name, Vector3 closedPosition, Vector3 openPosition, Material doorMat, Material stripMat)
    {
        GameObject root = CreateCube(name, closedPosition, new Vector3(2.2f, 2.5f, 0.32f), doorMat);
        Door door = root.AddComponent<Door>();
        door.openPosition = openPosition;
        door.speed = 3.2f;

        GameObject strip = CreateCube(name + " Light Strip", closedPosition + new Vector3(0f, 0f, -0.22f), new Vector3(2.35f, 0.1f, 0.08f), stripMat);
        strip.transform.SetParent(root.transform);
        return door;
    }

    private static PuzzleManager CreateUI()
    {
        GameObject canvasObject = new GameObject("Puzzle HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject panel = CreateScreenRect("Objective Panel", canvasObject.transform, new Vector2(22f, -22f), new Vector2(560f, 76f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.02f, 0.035f, 0.86f);

        GameObject textObject = CreateScreenRect("Objective Text", panel.transform, new Vector2(18f, -14f), new Vector2(515f, 42f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        Text objectiveText = textObject.AddComponent<Text>();
        objectiveText.font = font;
        objectiveText.text = "";
        objectiveText.fontSize = 18;
        objectiveText.alignment = TextAnchor.MiddleLeft;
        objectiveText.color = new Color(0.82f, 0.94f, 1f);

        GameObject winPanel = CreateScreenRect("Win Panel", canvasObject.transform, Vector2.zero, new Vector2(540f, 112f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Image winImage = winPanel.AddComponent<Image>();
        winImage.color = new Color(0.015f, 0.02f, 0.035f, 0.92f);

        GameObject winTextObject = CreateScreenRect("Win Text", winPanel.transform, Vector2.zero, new Vector2(500f, 72f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Text winText = winTextObject.AddComponent<Text>();
        winText.font = font;
        winText.text = "PUZZLE SOLVED";
        winText.fontSize = 28;
        winText.fontStyle = FontStyle.Bold;
        winText.alignment = TextAnchor.MiddleCenter;
        winText.color = new Color(0.18f, 1f, 0.55f);
        winPanel.SetActive(false);

        GameObject managerObject = new GameObject("Puzzle Manager");
        PuzzleManager manager = managerObject.AddComponent<PuzzleManager>();
        manager.objectiveText = objectiveText;
        manager.winPanel = winPanel;
        return manager;
    }

    private static void CreateLighting()
    {
        GameObject directional = new GameObject("Puzzle Directional Light");
        Light light = directional.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.65f;
        light.color = new Color(0.66f, 0.8f, 1f);
        directional.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        CreatePointLight("Key Slot Light", new Vector3(-2.8f, 3.2f, -2.5f), new Color(0.05f, 0.82f, 1f), 3.5f, 7f);
        CreatePointLight("Gem Slot Light", new Vector3(5.1f, 3.2f, -2.5f), new Color(1f, 0.08f, 0.62f), 3.2f, 7f);
        CreatePointLight("Exit Light", new Vector3(8.5f, 3.3f, 2.8f), new Color(0.18f, 1f, 0.55f), 2.6f, 6f);
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

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
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
        Material material = CreateMaterial(name, color, 0f, 0.55f);
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

    private static T Require<T>(T value, string label) where T : class
    {
        if (value == null)
        {
            throw new System.Exception("Missing required Lab6 object: " + label);
        }

        return value;
    }

    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(target, null);
    }
}
