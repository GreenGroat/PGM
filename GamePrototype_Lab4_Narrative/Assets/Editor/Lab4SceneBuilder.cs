using System.IO;
using Lab4Narrative;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab4SceneBuilder
{
    private const string RootPath = "Assets";
    private const string ScenePath = RootPath + "/Scenes/NarrativeArchive_Prototype.unity";
    private const string MaterialsPath = RootPath + "/Materials";
    private const string PrefabsPath = RootPath + "/Prefabs";

    public static void Build()
    {
        EnsureFolders();
        EnsureTag("Player");

        Material floorMat = CreateMaterial("Obsidian_Floor", new Color(0.05f, 0.07f, 0.09f), new Color(0.02f, 0.08f, 0.12f), 0.15f);
        Material wallMat = CreateMaterial("Archive_Wall", new Color(0.13f, 0.15f, 0.19f), Color.black, 0f);
        Material trimMat = CreateMaterial("Brass_Trim", new Color(0.66f, 0.48f, 0.24f), new Color(0.08f, 0.05f, 0.01f), 0.05f);
        Material cyanMat = CreateMaterial("Cyan_Rune_Glow", new Color(0.05f, 0.95f, 0.95f), new Color(0.0f, 0.85f, 0.95f), 1.8f);
        Material violetMat = CreateMaterial("Violet_Narrative_Glow", new Color(0.55f, 0.24f, 0.95f), new Color(0.40f, 0.10f, 0.90f), 1.6f);
        Material amberMat = CreateMaterial("Amber_Note_Glow", new Color(1.0f, 0.66f, 0.20f), new Color(1.0f, 0.42f, 0.08f), 1.4f);
        Material playerMat = CreateMaterial("Player_Cloak", new Color(0.08f, 0.20f, 0.34f), new Color(0.00f, 0.08f, 0.12f), 0.1f);
        Material npcMat = CreateMaterial("Keeper_Body", new Color(0.30f, 0.27f, 0.46f), new Color(0.18f, 0.08f, 0.35f), 0.4f);
        Material doorMat = CreateMaterial("Sealed_Door", new Color(0.08f, 0.10f, 0.16f), new Color(0.20f, 0.02f, 0.05f), 0.35f);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "NarrativeArchive_Prototype";

        SetupAtmosphere();
        CreateArchiveSpace(floorMat, wallMat, trimMat, cyanMat, violetMat);
        GameObject player = CreatePlayer(playerMat);
        CreateUI(out Text questText, out GameObject questCompletePanel, out NoteUI noteUI, out Inventory inventory, out DialogueUI dialogueUI);
        ExitDoor exitDoor = CreateExitDoor(doorMat, cyanMat);
        CreateNotes(amberMat, cyanMat);
        CreateKeeperNPC(npcMat, violetMat);
        CreateQuestManager(questText, questCompletePanel, exitDoor);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab4 narrative scene generated successfully.");
    }

    public static void CaptureGameView()
    {
        if (!File.Exists(ScenePath))
        {
            Build();
        }

        EditorSceneManager.OpenScene(ScenePath);
        Camera camera = GameObject.Find("PlayerCamera")?.GetComponent<Camera>();
        if (camera == null)
        {
            throw new System.InvalidOperationException("PlayerCamera not found.");
        }

        string assetRelativePath = RootPath + "/Documentation/Lab4_GameView.png";
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        RenderTexture renderTexture = new RenderTexture(1280, 720, 24);
        Texture2D screenshot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;

        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        camera.Render();
        screenshot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        screenshot.Apply();

        File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(screenshot);

        AssetDatabase.ImportAsset(assetRelativePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Lab4 Game View screenshot captured.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(RootPath + "/Scenes");
        Directory.CreateDirectory(RootPath + "/Scripts");
        Directory.CreateDirectory(RootPath + "/Prefabs");
        Directory.CreateDirectory(RootPath + "/Materials");
        Directory.CreateDirectory(RootPath + "/Documentation");
    }

    private static void SetupAtmosphere()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.16f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.08f);
        RenderSettings.fogDensity = 0.025f;

        GameObject moon = new GameObject("High_Blue_Key_Light");
        Light key = moon.AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(0.42f, 0.68f, 1f);
        key.intensity = 0.85f;
        moon.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
    }

    private static void CreateArchiveSpace(Material floorMat, Material wallMat, Material trimMat, Material cyanMat, Material violetMat)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Mirror_Dark_Floor";
        floor.transform.localScale = new Vector3(4.2f, 1f, 4.2f);
        AssignMaterial(floor, floorMat);

        CreateCube("Back_Wall", new Vector3(0f, 2.5f, 20f), new Vector3(42f, 5f, 0.6f), wallMat);
        CreateCube("Front_Wall", new Vector3(0f, 2.5f, -20f), new Vector3(42f, 5f, 0.6f), wallMat);
        CreateCube("Left_Wall", new Vector3(-21f, 2.5f, 0f), new Vector3(0.6f, 5f, 40f), wallMat);
        CreateCube("Right_Wall", new Vector3(21f, 2.5f, 0f), new Vector3(0.6f, 5f, 40f), wallMat);

        for (int i = -3; i <= 3; i++)
        {
            CreateCube($"Cyan_Floor_Line_Z_{i}", new Vector3(i * 4f, 0.035f, 0f), new Vector3(0.055f, 0.05f, 34f), cyanMat);
        }

        for (int i = -4; i <= 4; i++)
        {
            CreateCube($"Violet_Floor_Line_X_{i}", new Vector3(0f, 0.04f, i * 3.5f), new Vector3(34f, 0.055f, 0.05f), violetMat);
        }

        CreateCube("Central_Archive_Table", new Vector3(0f, 0.65f, 3f), new Vector3(7f, 1.3f, 2.2f), trimMat);
        CreateCube("Table_Cyan_Core", new Vector3(0f, 1.34f, 3f), new Vector3(6.5f, 0.08f, 1.7f), cyanMat);

        Vector3[] pillarPositions =
        {
            new Vector3(-13f, 2f, -9f), new Vector3(13f, 2f, -9f),
            new Vector3(-13f, 2f, 9f), new Vector3(13f, 2f, 9f),
            new Vector3(-7f, 2f, 15f), new Vector3(7f, 2f, 15f)
        };

        for (int i = 0; i < pillarPositions.Length; i++)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"Archive_Pillar_{i + 1:00}";
            pillar.transform.position = pillarPositions[i];
            pillar.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            AssignMaterial(pillar, wallMat);
            CreateCube($"Pillar_Rune_{i + 1:00}", pillarPositions[i] + new Vector3(0f, 1.4f, -0.82f), new Vector3(0.12f, 0.7f, 0.08f), i % 2 == 0 ? cyanMat : violetMat);
        }

        CreatePointLight("Cyan_Ambience_Left", new Vector3(-10f, 3.8f, -6f), new Color(0.0f, 0.85f, 1f), 1.6f, 13f);
        CreatePointLight("Violet_Ambience_Right", new Vector3(10f, 3.8f, 6f), new Color(0.72f, 0.22f, 1f), 1.5f, 13f);
        CreatePointLight("Warm_Table_Light", new Vector3(0f, 2.2f, 3f), new Color(1f, 0.55f, 0.20f), 1.2f, 9f);
    }

    private static GameObject CreatePlayer(Material playerMat)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, -15f);
        AssignMaterial(player, playerMat);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = Vector3.zero;

        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        cameraObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 66f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 120f;
        camera.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
        cameraObject.AddComponent<AudioListener>();

        NarrativePlayerController fps = player.AddComponent<NarrativePlayerController>();
        SerializedObject serialized = new SerializedObject(fps);
        serialized.FindProperty("playerCamera").objectReferenceValue = cameraObject.transform;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static void CreateNotes(Material noteMat, Material runeMat)
    {
        CreateNote(
            "Note_MemoryShard_01",
            "Записка I: Нулевой зал",
            "В архиве нет дверей, только решения. Первые хранители закрыли выход, когда город начал забывать собственные имена.",
            new Vector3(-9f, 1.15f, -6f),
            Quaternion.Euler(0f, 35f, 0f),
            noteMat,
            runeMat);

        CreateNote(
            "Note_MemoryShard_02",
            "Записка II: Синий контур",
            "Подсвеченные предметы отвечают только тем, кто подходит достаточно близко. Нажми E, когда видишь знак.",
            new Vector3(8f, 1.15f, -4f),
            Quaternion.Euler(0f, -25f, 0f),
            noteMat,
            runeMat);

        CreateNote(
            "Note_MemoryShard_03",
            "Записка III: Голос хранителя",
            "Хранитель не открывает путь силой. Он проверяет, понял ли исследователь связь между памятью, выбором и целью.",
            new Vector3(-11.5f, 1.15f, 9f),
            Quaternion.Euler(0f, 80f, 0f),
            noteMat,
            runeMat);

        CreateNote(
            "Note_MemoryShard_04",
            "Записка IV: Последний ключ",
            "Когда все фрагменты будут собраны, поговори с тем, кто стоит у светового стола. После этого дверь примет тебя.",
            new Vector3(10.5f, 1.15f, 11f),
            Quaternion.Euler(0f, -120f, 0f),
            noteMat,
            runeMat);
    }

    private static void CreateNote(string name, string title, string content, Vector3 position, Quaternion rotation, Material noteMat, Material runeMat)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        root.transform.rotation = rotation;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2f, 2f, 2f);

        GameObject tablet = CreateChildCube("Tablet", root.transform, new Vector3(0f, 0f, 0f), new Vector3(0.9f, 1.25f, 0.12f), noteMat);
        tablet.AddComponent<FloatAndSpin>();
        CreateChildCube("Tablet_Rune_Vertical", tablet.transform, new Vector3(0f, 0f, -0.58f), new Vector3(0.08f, 0.72f, 0.08f), runeMat);
        CreateChildCube("Tablet_Rune_Horizontal", tablet.transform, new Vector3(0f, 0.18f, -0.58f), new Vector3(0.54f, 0.08f, 0.08f), runeMat);

        Light light = CreatePointLight(name + "_Interaction_Light", position + Vector3.up * 1.2f, new Color(1f, 0.58f, 0.20f), 2.8f, 4f);
        light.enabled = false;

        Note note = root.AddComponent<Note>();
        SerializedObject serialized = new SerializedObject(note);
        serialized.FindProperty("noteTitle").stringValue = title;
        serialized.FindProperty("noteContent").stringValue = content;
        serialized.FindProperty("highlightColor").colorValue = new Color(1f, 0.84f, 0.32f);
        serialized.FindProperty("highlightLight").objectReferenceValue = light;
        serialized.FindProperty("prompt").stringValue = "E - read memory shard";
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateKeeperNPC(Material npcMat, Material glowMat)
    {
        GameObject npc = new GameObject("NPC_Archive_Keeper");
        npc.transform.position = new Vector3(0f, 1f, 7.2f);
        npc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        SphereCollider trigger = npc.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2.4f;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Keeper_Body";
        body.transform.SetParent(npc.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.85f, 1.15f, 0.85f);
        AssignMaterial(body, npcMat);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        halo.name = "Keeper_Halo";
        halo.transform.SetParent(npc.transform);
        halo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        halo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        halo.transform.localScale = new Vector3(1.35f, 0.05f, 1.35f);
        AssignMaterial(halo, glowMat);
        Object.DestroyImmediate(halo.GetComponent<Collider>());

        CreateChildCube("Keeper_Visor", npc.transform, new Vector3(0f, 0.7f, -0.42f), new Vector3(0.75f, 0.12f, 0.08f), glowMat);
        Light light = CreatePointLight("Keeper_Interaction_Light", npc.transform.position + Vector3.up * 1.6f, new Color(0.62f, 0.22f, 1f), 3f, 6f);
        light.enabled = false;

        NPC dialogue = npc.AddComponent<NPC>();
        SerializedObject serialized = new SerializedObject(dialogue);
        serialized.FindProperty("npcName").stringValue = "Хранитель архива";
        serialized.FindProperty("openingLine").stringValue = "Ты собрал достаточно тишины, чтобы услышать архив. Что ищешь?";
        SerializedProperty answers = serialized.FindProperty("answers");
        answers.arraySize = 2;
        answers.GetArrayElementAtIndex(0).stringValue = "Как открыть дверь?";
        answers.GetArrayElementAtIndex(1).stringValue = "Что случилось с городом?";
        SerializedProperty responses = serialized.FindProperty("responses");
        responses.arraySize = 2;
        responses.GetArrayElementAtIndex(0).stringValue = "Дверь принимает не ключ, а завершённую историю: собери все фрагменты и вернись ко мне.";
        responses.GetArrayElementAtIndex(1).stringValue = "Город не погиб. Он выжил слишком долго и научился забывать боль быстрее, чем имена.";
        serialized.FindProperty("highlightColor").colorValue = new Color(0.85f, 0.42f, 1f);
        serialized.FindProperty("highlightLight").objectReferenceValue = light;
        serialized.FindProperty("prompt").stringValue = "E - speak with the keeper";
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static ExitDoor CreateExitDoor(Material doorMat, Material glowMat)
    {
        GameObject root = new GameObject("ExitDoor_Root");
        root.transform.position = new Vector3(0f, 0f, 18.8f);

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(6f, 4f, 2f);
        trigger.center = new Vector3(0f, 1.6f, 0f);

        GameObject door = CreateChildCube("Sealed_Door_Visual", root.transform, new Vector3(0f, 2f, 0f), new Vector3(5f, 4f, 0.45f), doorMat);
        CreateChildCube("Door_Center_Rune", root.transform, new Vector3(0f, 2f, -0.28f), new Vector3(0.18f, 2.8f, 0.08f), glowMat);
        CreateChildCube("Door_Lintel_Glow", root.transform, new Vector3(0f, 4.2f, -0.28f), new Vector3(5.8f, 0.18f, 0.08f), glowMat);

        Light unlocked = CreatePointLight("Exit_Unlocked_Light", root.transform.position + new Vector3(0f, 2.8f, -1.3f), new Color(0f, 1f, 0.82f), 2.5f, 8f);
        unlocked.enabled = false;

        ExitDoor exitDoor = root.AddComponent<ExitDoor>();
        SerializedObject serialized = new SerializedObject(exitDoor);
        serialized.FindProperty("doorVisual").objectReferenceValue = door.transform;
        serialized.FindProperty("openedOffset").vector3Value = new Vector3(0f, 4.4f, 0f);
        serialized.FindProperty("unlockedLight").objectReferenceValue = unlocked;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return exitDoor;
    }

    private static void CreateQuestManager(Text questText, GameObject questCompletePanel, ExitDoor exitDoor)
    {
        GameObject manager = new GameObject("QuestManager");
        QuestManager questManager = manager.AddComponent<QuestManager>();
        SerializedObject serialized = new SerializedObject(questManager);
        serialized.FindProperty("totalNotes").intValue = 4;
        serialized.FindProperty("needTalkToNPC").boolValue = true;
        serialized.FindProperty("questStatusText").objectReferenceValue = questText;
        serialized.FindProperty("questCompletePanel").objectReferenceValue = questCompletePanel;
        serialized.FindProperty("exitDoor").objectReferenceValue = exitDoor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateUI(out Text questText, out GameObject questCompletePanel, out NoteUI noteUI, out Inventory inventory, out DialogueUI dialogueUI)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObject.AddComponent<GraphicRaycaster>();

        questText = CreateText("Quest_Status_Text", canvasObject.transform, new Vector2(24f, -18f), new Vector2(380f, 110f), 20, TextAnchor.UpperLeft);
        Text controlsText = CreateText("Controls_Text", canvasObject.transform, new Vector2(24f, -132f), new Vector2(620f, 42f), 16, TextAnchor.UpperLeft);
        controlsText.text = "E: interact  |  I: inventory  |  WASD: move  |  Shift: run";

        Text promptText = CreateText("Interaction_Prompt_Text", canvasObject.transform, new Vector2(0f, 96f), new Vector2(620f, 44f), 24, TextAnchor.MiddleCenter);
        RectTransform promptRect = promptText.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        InteractionPrompt prompt = canvasObject.AddComponent<InteractionPrompt>();
        SerializedObject promptSerialized = new SerializedObject(prompt);
        promptSerialized.FindProperty("promptText").objectReferenceValue = promptText;
        promptSerialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject notePanel = CreatePanel("NotePanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(700f, 440f), new Color(0.04f, 0.05f, 0.08f, 0.92f));
        Text noteTitle = CreatePanelText("Note_Title", notePanel.transform, new Vector2(24f, -22f), new Vector2(650f, 46f), 28, TextAnchor.UpperLeft);
        Text noteContent = CreatePanelText("Note_Content", notePanel.transform, new Vector2(24f, -84f), new Vector2(650f, 260f), 20, TextAnchor.UpperLeft);
        Button noteClose = CreateButton("Note_Close_Button", notePanel.transform, "Close", new Vector2(0f, 34f), new Vector2(150f, 40f));
        noteClose.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        noteClose.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
        notePanel.SetActive(false);
        noteUI = canvasObject.AddComponent<NoteUI>();
        SerializedObject noteSerialized = new SerializedObject(noteUI);
        noteSerialized.FindProperty("notePanel").objectReferenceValue = notePanel;
        noteSerialized.FindProperty("titleText").objectReferenceValue = noteTitle;
        noteSerialized.FindProperty("contentText").objectReferenceValue = noteContent;
        noteSerialized.FindProperty("closeButton").objectReferenceValue = noteClose;
        noteSerialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject inventoryPanel = CreatePanel("InventoryPanel", canvasObject.transform, new Vector2(1f, 0.5f), new Vector2(360f, 520f), new Color(0.03f, 0.04f, 0.07f, 0.92f));
        RectTransform invRect = inventoryPanel.GetComponent<RectTransform>();
        invRect.anchorMin = new Vector2(1f, 0.5f);
        invRect.anchorMax = new Vector2(1f, 0.5f);
        invRect.pivot = new Vector2(1f, 0.5f);
        invRect.anchoredPosition = new Vector2(-26f, 0f);
        CreatePanelText("Inventory_Title", inventoryPanel.transform, new Vector2(20f, -18f), new Vector2(310f, 38f), 24, TextAnchor.UpperLeft).text = "Collected Notes";
        Text emptyText = CreatePanelText("Inventory_Empty_Text", inventoryPanel.transform, new Vector2(20f, -68f), new Vector2(310f, 34f), 18, TextAnchor.UpperLeft);
        emptyText.text = "No notes yet.";
        GameObject notesContainer = new GameObject("Notes_Container");
        notesContainer.transform.SetParent(inventoryPanel.transform, false);
        RectTransform notesRect = notesContainer.AddComponent<RectTransform>();
        notesRect.anchorMin = new Vector2(0f, 1f);
        notesRect.anchorMax = new Vector2(1f, 1f);
        notesRect.pivot = new Vector2(0.5f, 1f);
        notesRect.anchoredPosition = new Vector2(0f, -112f);
        notesRect.sizeDelta = new Vector2(-40f, 360f);
        VerticalLayoutGroup layout = notesContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        Button noteButtonPrefab = CreateButton("Note_Button_Template", notesContainer.transform, "Note", Vector2.zero, new Vector2(300f, 38f));
        noteButtonPrefab.gameObject.SetActive(false);
        inventoryPanel.SetActive(false);
        inventory = canvasObject.AddComponent<Inventory>();
        SerializedObject inventorySerialized = new SerializedObject(inventory);
        inventorySerialized.FindProperty("inventoryPanel").objectReferenceValue = inventoryPanel;
        inventorySerialized.FindProperty("notesContainer").objectReferenceValue = notesContainer.transform;
        inventorySerialized.FindProperty("noteButtonPrefab").objectReferenceValue = noteButtonPrefab;
        inventorySerialized.FindProperty("emptyText").objectReferenceValue = emptyText;
        inventorySerialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject dialoguePanel = CreatePanel("DialoguePanel", canvasObject.transform, new Vector2(0.5f, 0f), new Vector2(850f, 250f), new Color(0.04f, 0.03f, 0.08f, 0.94f));
        RectTransform diaRect = dialoguePanel.GetComponent<RectTransform>();
        diaRect.anchorMin = new Vector2(0.5f, 0f);
        diaRect.anchorMax = new Vector2(0.5f, 0f);
        diaRect.pivot = new Vector2(0.5f, 0f);
        diaRect.anchoredPosition = new Vector2(0f, 28f);
        Text npcName = CreatePanelText("Dialogue_Name", dialoguePanel.transform, new Vector2(22f, -18f), new Vector2(800f, 34f), 22, TextAnchor.UpperLeft);
        Text npcLine = CreatePanelText("Dialogue_Line", dialoguePanel.transform, new Vector2(22f, -58f), new Vector2(800f, 82f), 19, TextAnchor.UpperLeft);
        Button answerA = CreateButton("Answer_A", dialoguePanel.transform, "Answer A", new Vector2(-210f, 62f), new Vector2(330f, 40f));
        Button answerB = CreateButton("Answer_B", dialoguePanel.transform, "Answer B", new Vector2(150f, 62f), new Vector2(330f, 40f));
        Button dialogueClose = CreateButton("Dialogue_Close", dialoguePanel.transform, "Close", new Vector2(0f, 18f), new Vector2(150f, 36f));
        dialogueClose.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        dialogueClose.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
        dialoguePanel.SetActive(false);
        dialogueUI = canvasObject.AddComponent<DialogueUI>();
        SerializedObject dialogueSerialized = new SerializedObject(dialogueUI);
        dialogueSerialized.FindProperty("dialoguePanel").objectReferenceValue = dialoguePanel;
        dialogueSerialized.FindProperty("nameText").objectReferenceValue = npcName;
        dialogueSerialized.FindProperty("dialogueText").objectReferenceValue = npcLine;
        SerializedProperty answerButtons = dialogueSerialized.FindProperty("answerButtons");
        answerButtons.arraySize = 2;
        answerButtons.GetArrayElementAtIndex(0).objectReferenceValue = answerA;
        answerButtons.GetArrayElementAtIndex(1).objectReferenceValue = answerB;
        dialogueSerialized.FindProperty("closeButton").objectReferenceValue = dialogueClose;
        dialogueSerialized.ApplyModifiedPropertiesWithoutUndo();

        questCompletePanel = CreatePanel("QuestCompletePanel", canvasObject.transform, new Vector2(0.5f, 0.15f), new Vector2(560f, 94f), new Color(0.0f, 0.22f, 0.19f, 0.9f));
        CreatePanelText("QuestComplete_Text", questCompletePanel.transform, new Vector2(0f, 0f), new Vector2(520f, 70f), 24, TextAnchor.MiddleCenter).text = "Archive complete. Exit unlocked.";
        questCompletePanel.SetActive(false);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static Text CreatePanelText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
    {
        Text text = CreateText(name, parent, anchoredPosition, size, fontSize, alignment);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.10f, 0.18f, 0.24f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(name + "_Text", buttonObject.transform, Vector2.zero, size, 18, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.text = label;
        return button;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
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
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        return text;
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

    private static GameObject CreateChildCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = localScale;
        AssignMaterial(cube, material);
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        return cube;
    }

    private static Light CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    private static Material CreateMaterial(string name, Color baseColor, Color emissionColor, float emissionStrength)
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
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }

        Color emission = emissionColor * emissionStrength;
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            if (emissionStrength > 0f)
            {
                material.EnableKeyword("_EMISSION");
            }
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
}
