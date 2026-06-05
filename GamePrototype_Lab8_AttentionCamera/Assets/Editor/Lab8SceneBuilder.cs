using System.IO;
using Lab8Attention;
using UnityEditor;
using UnityEditor.Events;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab8SceneBuilder
{
    private const string Root = "Assets";
    private const string MaterialFolder = Root + "/Materials";
    private const string ScenePath = Root + "/Scenes/NarrativeScene.unity";

    [MenuItem("Lab Builders/Build Lab8 Attention Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(Root + "/Scenes");

        EnsureTag("Player");

        Material floorMat = CreateMaterial("M_Narrative_Floor", new Color(0.025f, 0.03f, 0.045f), 0f, 0f);
        Material wallMat = CreateMaterial("M_Narrative_Wall", new Color(0.06f, 0.07f, 0.1f), 0f, 0.05f);
        Material cyanMat = CreateMaterial("M_Cyan_Guide", new Color(0.05f, 0.82f, 1f), 0.1f, 1.8f);
        Material magentaMat = CreateMaterial("M_Magenta_Memory", new Color(1f, 0.08f, 0.62f), 0.1f, 1.7f);
        Material amberMat = CreateMaterial("M_Amber_Focus", new Color(1f, 0.68f, 0.1f), 0.1f, 1.5f);
        Material triggerMat = CreateTransparentMaterial("M_Attention_Trigger", new Color(0.05f, 0.82f, 1f, 0.18f));
        Material darkGlassMat = CreateTransparentMaterial("M_Dark_Glass", new Color(0.04f, 0.08f, 0.12f, 0.55f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.012f, 0.015f, 0.025f);
        RenderSettings.fogDensity = 0.018f;

        CreateArchitecture(floorMat, wallMat, cyanMat, magentaMat, darkGlassMat);
        GameObject player = CreatePlayer();
        CameraController cameraController = CreateCameraRig(player.transform);
        CreateLighting(cyanMat, magentaMat, amberMat);
        CreateUI(cameraController);
        CreateNarrativeObjects(cameraController, cyanMat, magentaMat, amberMat, triggerMat);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab8 attention scene generated: " + ScenePath);
    }

    private static GameObject CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(-10.5f, 0.95f, -6.5f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = Vector3.up * 0.9f;

        GameObject pivot = new GameObject("View Pivot");
        pivot.transform.SetParent(player.transform);
        pivot.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        NarrativePlayerController playerController = player.AddComponent<NarrativePlayerController>();
        playerController.viewPivot = pivot.transform;
        return player;
    }

    private static CameraController CreateCameraRig(Transform player)
    {
        Transform followTarget = player.Find("View Pivot");

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = followTarget.position;
        cameraObject.transform.rotation = followTarget.rotation;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<AudioSource>();

        GameObject rig = new GameObject("Camera Rig");
        CameraController controller = rig.AddComponent<CameraController>();
        controller.playerCamera = camera;
        controller.followTarget = player;
        controller.followOffset = new Vector3(0f, 2.1f, -5.1f);
        controller.lookAtOffset = new Vector3(0f, 1.05f, 0f);

        NarrativePlayerController playerController = player.GetComponent<NarrativePlayerController>();
        playerController.cameraController = controller;
        return controller;
    }

    private static void CreateArchitecture(Material floorMat, Material wallMat, Material cyanMat, Material magentaMat, Material glassMat)
    {
        CreateCube("Archive Floor", Vector3.zero, new Vector3(30f, 0.15f, 22f), floorMat);

        for (int x = -14; x <= 14; x += 2)
        {
            CreateCube("Floor Guide X " + x, new Vector3(x, 0.1f, 0f), new Vector3(0.025f, 0.025f, 22f), x % 4 == 0 ? cyanMat : magentaMat);
        }

        for (int z = -10; z <= 10; z += 2)
        {
            CreateCube("Floor Guide Z " + z, new Vector3(0f, 0.11f, z), new Vector3(30f, 0.025f, 0.025f), z % 4 == 0 ? magentaMat : cyanMat);
        }

        CreateCube("North Wall", new Vector3(0f, 1.4f, 11f), new Vector3(30f, 2.8f, 0.35f), wallMat);
        CreateCube("South Wall", new Vector3(0f, 1.4f, -11f), new Vector3(30f, 2.8f, 0.35f), wallMat);
        CreateCube("West Wall", new Vector3(-15f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 22f), wallMat);
        CreateCube("East Wall", new Vector3(15f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 22f), wallMat);

        CreateCube("Entrance Divider Left", new Vector3(-6f, 1.4f, -5.5f), new Vector3(0.35f, 2.8f, 10.5f), wallMat);
        CreateCube("Entrance Divider Right", new Vector3(6f, 1.4f, -5.5f), new Vector3(0.35f, 2.8f, 10.5f), wallMat);
        CreateCube("Back Gallery Divider", new Vector3(0f, 1.4f, 4.2f), new Vector3(9f, 2.8f, 0.35f), wallMat);
        CreateCube("Glass Display Left", new Vector3(-9.5f, 0.95f, 4f), new Vector3(2f, 1.9f, 0.12f), glassMat);
        CreateCube("Glass Display Right", new Vector3(9.5f, 0.95f, 4f), new Vector3(2f, 1.9f, 0.12f), glassMat);
    }

    private static void CreateNarrativeObjects(CameraController cameraController, Material cyanMat, Material magentaMat, Material amberMat, Material triggerMat)
    {
        GameObject statueRoot = new GameObject("Memory Statue");
        statueRoot.transform.position = new Vector3(0f, 0f, 1f);

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Statue Pedestal";
        pedestal.transform.SetParent(statueRoot.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        pedestal.transform.localScale = new Vector3(1.35f, 0.35f, 1.35f);
        pedestal.GetComponent<Renderer>().sharedMaterial = cyanMat;

        GameObject statueCore = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        statueCore.name = "Statue Focus Core";
        statueCore.transform.SetParent(statueRoot.transform);
        statueCore.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        statueCore.transform.localScale = new Vector3(0.55f, 1.25f, 0.55f);
        statueCore.GetComponent<Renderer>().sharedMaterial = magentaMat;
        HighlightOnApproach highlight = statueCore.AddComponent<HighlightOnApproach>();
        highlight.radius = 4f;
        highlight.highlightMaterial = amberMat;

        SphereCollider interactCollider = statueRoot.AddComponent<SphereCollider>();
        interactCollider.isTrigger = true;
        interactCollider.radius = 2.2f;
        InteractableObject interactable = statueRoot.AddComponent<InteractableObject>();
        interactable.prompt = "Press E near the statue";
        interactable.completeMessage = "The statue opens a memory route.";

        GameObject hiddenChest = CreateChest("Hidden Memory Chest", new Vector3(-7.8f, 0.55f, 6.7f), amberMat, magentaMat);
        hiddenChest.SetActive(false);

        GameObject revealLightObject = new GameObject("Chest Reveal Light");
        revealLightObject.transform.position = new Vector3(-7.8f, 3.2f, 6.7f);
        Light revealLight = revealLightObject.AddComponent<Light>();
        revealLight.type = LightType.Point;
        revealLight.color = new Color(1f, 0.65f, 0.1f);
        revealLight.range = 7f;
        revealLight.intensity = 4f;
        revealLight.enabled = false;
        revealLightObject.AddComponent<PulseLight>();

        RevealObjectAction reveal = statueRoot.AddComponent<RevealObjectAction>();
        reveal.objectsToReveal = new[] { hiddenChest };
        reveal.revealLight = revealLight;
        reveal.revealHint = "A hidden chest is now visible in the left gallery.";
        UnityEventTools.AddPersistentListener(interactable.onInteract, reveal.Reveal);

        Transform statueCamera = CreateCameraPoint("Statue Camera Point", new Vector3(-3.8f, 2.4f, -2.8f), statueCore.transform.position + Vector3.up * 0.6f);
        Transform chestCamera = CreateCameraPoint("Chest Camera Point", new Vector3(-10.5f, 2.2f, 4.8f), hiddenChest.transform.position + Vector3.up * 0.5f);
        Transform doorCamera = CreateCameraPoint("Door Camera Point", new Vector3(6.8f, 2.4f, 4.8f), new Vector3(12.5f, 1.2f, 8.8f));

        CreateAttentionTrigger("Statue Attention Trigger", new Vector3(-2f, 1.1f, -5.6f), new Vector3(7.5f, 2.2f, 1.4f), statueCamera, cameraController, "Something in the central room is pulling the camera.", 3f, triggerMat);
        CreateAttentionTrigger("Chest Attention Trigger", new Vector3(-7.8f, 1.1f, 4.4f), new Vector3(3.4f, 2.2f, 1.4f), chestCamera, cameraController, "The side gallery hides a memory container.", 2.7f, triggerMat);
        CreateAttentionTrigger("Door Attention Trigger", new Vector3(8.4f, 1.1f, 6.8f), new Vector3(3.6f, 2.2f, 1.4f), doorCamera, cameraController, "The exit door is framed by the last light cue.", 2.7f, triggerMat);

        CreateDoor(cyanMat, magentaMat);
        CreateArrowPath(cyanMat, amberMat);
    }

    private static GameObject CreateChest(string name, Vector3 position, Material baseMat, Material glowMat)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Chest Body";
        box.transform.SetParent(root.transform);
        box.transform.localPosition = Vector3.zero;
        box.transform.localScale = new Vector3(1.4f, 0.75f, 0.9f);
        box.GetComponent<Renderer>().sharedMaterial = baseMat;

        GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stripe.name = "Chest Light Stripe";
        stripe.transform.SetParent(root.transform);
        stripe.transform.localPosition = new Vector3(0f, 0.46f, -0.48f);
        stripe.transform.localScale = new Vector3(1.55f, 0.08f, 0.08f);
        stripe.GetComponent<Renderer>().sharedMaterial = glowMat;

        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2f;
        InteractableObject interactable = root.AddComponent<InteractableObject>();
        interactable.prompt = "Press E to inspect the memory chest";
        interactable.completeMessage = "A stored image confirms the route to the exit.";
        return root;
    }

    private static void CreateDoor(Material cyanMat, Material magentaMat)
    {
        GameObject door = CreateCube("Exit Door", new Vector3(12.8f, 1.3f, 8.8f), new Vector3(2.5f, 2.6f, 0.28f), magentaMat);
        CreateCube("Door Left Rail", new Vector3(11.35f, 1.4f, 8.65f), new Vector3(0.12f, 2.9f, 0.18f), cyanMat);
        CreateCube("Door Right Rail", new Vector3(14.25f, 1.4f, 8.65f), new Vector3(0.12f, 2.9f, 0.18f), cyanMat);

        BoxCollider trigger = door.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.2f, 2.7f, 2.2f);
        InteractableObject interactable = door.AddComponent<InteractableObject>();
        interactable.prompt = "Press E at the exit door";
        interactable.completeMessage = "The camera route is complete.";
    }

    private static void CreateArrowPath(Material cyanMat, Material amberMat)
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject step = CreateCube("Guidance Step " + (i + 1), new Vector3(-9f + i * 2.1f, 0.18f, -7.5f + i * 1.05f), new Vector3(0.9f, 0.05f, 0.12f), i % 2 == 0 ? cyanMat : amberMat);
            step.transform.rotation = Quaternion.Euler(0f, 28f, 0f);
        }
    }

    private static void CreateAttentionTrigger(string name, Vector3 position, Vector3 scale, Transform cameraPoint, CameraController controller, string message, float duration, Material material)
    {
        GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trigger.name = name;
        trigger.transform.position = position;
        trigger.transform.localScale = scale;
        trigger.GetComponent<Renderer>().sharedMaterial = material;
        BoxCollider collider = trigger.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        AttentionTrigger attention = trigger.AddComponent<AttentionTrigger>();
        attention.cameraController = controller;
        attention.cameraPoint = cameraPoint;
        attention.message = message;
        attention.cameraDuration = duration;
    }

    private static Transform CreateCameraPoint(string name, Vector3 position, Vector3 lookTarget)
    {
        GameObject point = new GameObject(name);
        point.transform.position = position;
        point.transform.LookAt(lookTarget);
        return point.transform;
    }

    private static void CreateLighting(Material cyanMat, Material magentaMat, Material amberMat)
    {
        GameObject directional = new GameObject("Narrative Directional Light");
        Light light = directional.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.55f;
        light.color = new Color(0.65f, 0.78f, 1f);
        directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        CreatePointLight("Statue Key Light", new Vector3(-2f, 4f, -1f), new Color(1f, 0.1f, 0.62f), 4.2f, 9f, true);
        CreatePointLight("Path Cyan Light", new Vector3(-8f, 3.2f, -5f), new Color(0.05f, 0.82f, 1f), 3.3f, 8f, false);
        CreatePointLight("Exit Rim Light", new Vector3(11f, 3.4f, 8f), new Color(0.05f, 0.82f, 1f), 3.5f, 8f, true);

        CreateCube("Ceiling Cyan Strip", new Vector3(-7f, 2.85f, -6.5f), new Vector3(4f, 0.08f, 0.16f), cyanMat);
        CreateCube("Ceiling Magenta Strip", new Vector3(3f, 2.85f, 1.2f), new Vector3(4f, 0.08f, 0.16f), magentaMat);
        CreateCube("Ceiling Amber Strip", new Vector3(9f, 2.85f, 7f), new Vector3(4f, 0.08f, 0.16f), amberMat);
    }

    private static void CreateUI(CameraController cameraController)
    {
        GameObject canvasObject = new GameObject("Narrative HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject focusOverlayObject = CreateScreenRect("Focus Overlay", canvasObject.transform, Vector2.zero, new Vector2(0f, 0f), Vector2.zero, Vector2.one);
        Image focusOverlay = focusOverlayObject.AddComponent<Image>();
        focusOverlay.color = new Color(0f, 0f, 0f, 0f);
        cameraController.focusOverlay = focusOverlay;

        GameObject panel = CreateScreenRect("Hint Panel", canvasObject.transform, new Vector2(0f, 38f), new Vector2(740f, 74f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.02f, 0.035f, 0.88f);

        GameObject textObject = CreateScreenRect("Hint Text", panel.transform, Vector2.zero, new Vector2(690f, 42f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = "";
        text.fontSize = 19;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.82f, 0.94f, 1f);

        GameObject controlsObject = CreateScreenRect("Controls Hint", canvasObject.transform, new Vector2(-22f, 22f), new Vector2(460f, 40f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        Text controls = controlsObject.AddComponent<Text>();
        controls.font = font;
        controls.text = "WASD move | Mouse look | E interact";
        controls.fontSize = 15;
        controls.alignment = TextAnchor.LowerRight;
        controls.color = new Color(0.65f, 0.84f, 1f, 0.85f);

        GameObject manager = new GameObject("Hint Manager");
        HintManager hintManager = manager.AddComponent<HintManager>();
        hintManager.hintPanel = panel;
        hintManager.hintText = text;
    }

    private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range, bool pulse)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;

        if (pulse)
        {
            PulseLight pulseLight = go.AddComponent<PulseLight>();
            pulseLight.baseIntensity = intensity;
            pulseLight.pulseIntensity = 0.6f;
            pulseLight.pulseSpeed = 1.7f;
        }
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
        rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        if (anchorMin != anchorMax)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
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
        Material material = CreateMaterial(name, color, 0f, 0.45f);
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
