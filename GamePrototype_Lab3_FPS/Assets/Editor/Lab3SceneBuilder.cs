using System.IO;
using Lab3FPS;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab3SceneBuilder
{
    private const string RootPath = "Assets";
    private const string ScenePath = RootPath + "/Scenes/FPS_Arsenal_Prototype.unity";
    private const string MaterialsPath = RootPath + "/Materials";
    private const string PrefabsPath = RootPath + "/Prefabs";

    public static void Build()
    {
        EnsureFolders();
        EnsureTag("Player");
        int enemyLayer = EnsureLayer("Enemy");

        Material groundMat = CreateMaterial("Ground_Mat", new Color(0.32f, 0.38f, 0.34f));
        Material wallMat = CreateMaterial("Wall_Mat", new Color(0.46f, 0.48f, 0.52f));
        Material coverMat = CreateMaterial("Cover_Mat", new Color(0.40f, 0.33f, 0.25f));
        Material playerMat = CreateMaterial("Player_Mat", new Color(0.12f, 0.26f, 0.65f));
        Material pistolMat = CreateMaterial("Pistol_Mat", new Color(0.12f, 0.13f, 0.15f));
        Material rifleMat = CreateMaterial("Rifle_Mat", new Color(0.18f, 0.22f, 0.24f));
        Material enemyMat = CreateMaterial("Enemy_Mat", new Color(0.85f, 0.18f, 0.16f));
        Material healthMat = CreateMaterial("HealthPack_Mat", new Color(0.93f, 0.08f, 0.10f));
        Material ammoMat = CreateMaterial("AmmoPack_Mat", new Color(0.95f, 0.76f, 0.16f));
        Material hazardMat = CreateMaterial("Hazard_Mat", new Color(0.75f, 0.05f, 0.05f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "FPS_Arsenal_Prototype";

        SetupLighting();
        CreateArena(groundMat, wallMat, coverMat, hazardMat);

        GameObject player = CreatePlayer(playerMat, out Camera camera, out WeaponSwitcher switcher, out Text ammoText, out Text weaponText, out Text healthText, out Text statusText);
        CreateWeapons(camera, player, switcher, ammoText, weaponText, statusText, enemyLayer, pistolMat, rifleMat);
        CreateTargets(enemyLayer, enemyMat);
        CreatePickups(healthMat, ammoMat);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab3 FPS scene generated successfully.");
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

        string assetRelativePath = RootPath + "/Documentation/Lab3_GameView.png";
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
        UnityEngine.Object.DestroyImmediate(renderTexture);
        UnityEngine.Object.DestroyImmediate(screenshot);

        AssetDatabase.ImportAsset(assetRelativePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Lab3 Game View screenshot captured.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(RootPath + "/Scenes");
        Directory.CreateDirectory(RootPath + "/Scripts");
        Directory.CreateDirectory(RootPath + "/Prefabs");
        Directory.CreateDirectory(RootPath + "/Materials");
        Directory.CreateDirectory(RootPath + "/Documentation");
    }

    private static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.52f, 0.56f, 0.62f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.52f, 0.57f, 0.63f);
        RenderSettings.fogDensity = 0.012f;

        GameObject lightObject = new GameObject("Directional_Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
    }

    private static void CreateArena(Material groundMat, Material wallMat, Material coverMat, Material hazardMat)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4.5f, 1f, 4.5f);
        AssignMaterial(ground, groundMat);

        CreateCube("North_Wall", new Vector3(0f, 2f, 22f), new Vector3(44f, 4f, 0.6f), wallMat);
        CreateCube("South_Wall", new Vector3(0f, 2f, -22f), new Vector3(44f, 4f, 0.6f), wallMat);
        CreateCube("East_Wall", new Vector3(22f, 2f, 0f), new Vector3(0.6f, 4f, 44f), wallMat);
        CreateCube("West_Wall", new Vector3(-22f, 2f, 0f), new Vector3(0.6f, 4f, 44f), wallMat);

        CreateCube("Cover_Block_01", new Vector3(-7f, 1f, -2f), new Vector3(4f, 2f, 2f), coverMat);
        CreateCube("Cover_Block_02", new Vector3(4f, 1f, 2.5f), new Vector3(5f, 2f, 2f), coverMat);
        CreateCube("Cover_Block_03", new Vector3(10f, 1.5f, 8f), new Vector3(2f, 3f, 5f), coverMat);
        CreateCube("Cover_Block_04", new Vector3(-11f, 1.5f, 9f), new Vector3(2f, 3f, 6f), coverMat);

        GameObject hazard = CreateCube("Damage_Zone", new Vector3(0f, 0.04f, 12f), new Vector3(8f, 0.08f, 5f), hazardMat);
        BoxCollider hazardCollider = hazard.GetComponent<BoxCollider>();
        hazardCollider.isTrigger = true;
        hazard.AddComponent<DamageZone>();
    }

    private static GameObject CreatePlayer(
        Material playerMat,
        out Camera camera,
        out WeaponSwitcher switcher,
        out Text ammoText,
        out Text weaponText,
        out Text healthText,
        out Text statusText)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, -14f);
        AssignMaterial(player, playerMat);
        UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = Vector3.zero;

        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        cameraObject.transform.localRotation = Quaternion.identity;
        camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 65f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 120f;
        camera.backgroundColor = new Color(0.55f, 0.62f, 0.70f);
        cameraObject.AddComponent<AudioListener>();

        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(cameraObject.transform);
        weaponHolder.transform.localPosition = Vector3.zero;
        weaponHolder.transform.localRotation = Quaternion.identity;

        CreateUI(out ammoText, out weaponText, out healthText, out statusText, out GameObject deathScreen);

        FPSController fpsController = player.AddComponent<FPSController>();
        SerializedObject fps = new SerializedObject(fpsController);
        fps.FindProperty("playerCamera").objectReferenceValue = cameraObject.transform;
        fps.ApplyModifiedPropertiesWithoutUndo();

        PlayerHealth health = player.AddComponent<PlayerHealth>();
        SerializedObject healthSerialized = new SerializedObject(health);
        healthSerialized.FindProperty("healthText").objectReferenceValue = healthText;
        healthSerialized.FindProperty("statusText").objectReferenceValue = statusText;
        healthSerialized.FindProperty("deathScreen").objectReferenceValue = deathScreen;
        healthSerialized.FindProperty("fpsController").objectReferenceValue = fpsController;
        healthSerialized.ApplyModifiedPropertiesWithoutUndo();

        switcher = player.AddComponent<WeaponSwitcher>();
        SerializedObject switcherSerialized = new SerializedObject(switcher);
        switcherSerialized.FindProperty("statusText").objectReferenceValue = statusText;
        switcherSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject healthLink = new SerializedObject(health);
        healthLink.FindProperty("weaponSwitcher").objectReferenceValue = switcher;
        healthLink.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static void CreateWeapons(
        Camera camera,
        GameObject player,
        WeaponSwitcher switcher,
        Text ammoText,
        Text weaponText,
        Text statusText,
        int enemyLayer,
        Material pistolMat,
        Material rifleMat)
    {
        Transform holder = camera.transform.Find("WeaponHolder");

        GameObject pistol = CreateWeaponRoot("Pistol", holder, new Vector3(0.36f, -0.32f, 0.72f), Quaternion.Euler(0f, -2f, 0f));
        CreateWeaponPart("Pistol_Body", pistol.transform, new Vector3(0f, 0f, 0.12f), new Vector3(0.22f, 0.18f, 0.42f), pistolMat);
        CreateWeaponPart("Pistol_Barrel", pistol.transform, new Vector3(0f, 0.04f, 0.42f), new Vector3(0.12f, 0.10f, 0.42f), pistolMat);
        CreateWeaponPart("Pistol_Grip", pistol.transform, new Vector3(0f, -0.18f, 0f), new Vector3(0.16f, 0.34f, 0.16f), pistolMat);
        AddMuzzleFlash(pistol.transform, new Vector3(0f, 0.05f, 0.68f));
        ConfigureGun(pistol, "Pistol", 25f, 0.32f, 80f, 12, 48, 1.1f, false, 1.4f, 0.010f, camera, player, ammoText, weaponText, statusText, enemyLayer);

        GameObject rifle = CreateWeaponRoot("Rifle", holder, new Vector3(0.42f, -0.34f, 0.82f), Quaternion.Euler(0f, -3f, 0f));
        CreateWeaponPart("Rifle_Body", rifle.transform, new Vector3(0f, 0f, 0.18f), new Vector3(0.24f, 0.20f, 0.72f), rifleMat);
        CreateWeaponPart("Rifle_Barrel", rifle.transform, new Vector3(0f, 0.04f, 0.72f), new Vector3(0.10f, 0.10f, 0.78f), rifleMat);
        CreateWeaponPart("Rifle_Stock", rifle.transform, new Vector3(0f, -0.02f, -0.33f), new Vector3(0.28f, 0.22f, 0.34f), rifleMat);
        CreateWeaponPart("Rifle_Magazine", rifle.transform, new Vector3(0f, -0.22f, 0.12f), new Vector3(0.16f, 0.38f, 0.22f), rifleMat);
        AddMuzzleFlash(rifle.transform, new Vector3(0f, 0.05f, 1.13f));
        ConfigureGun(rifle, "Rifle", 35f, 0.12f, 100f, 30, 90, 1.8f, true, 0.9f, 0.018f, camera, player, ammoText, weaponText, statusText, enemyLayer);

        SerializedObject switcherSerialized = new SerializedObject(switcher);
        SerializedProperty weapons = switcherSerialized.FindProperty("weapons");
        weapons.arraySize = 2;
        weapons.GetArrayElementAtIndex(0).objectReferenceValue = pistol;
        weapons.GetArrayElementAtIndex(1).objectReferenceValue = rifle;
        switcherSerialized.ApplyModifiedPropertiesWithoutUndo();

        pistol.SetActive(true);
        rifle.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(pistol, PrefabsPath + "/Pistol.prefab");
        PrefabUtility.SaveAsPrefabAsset(rifle, PrefabsPath + "/Rifle.prefab");
    }

    private static GameObject CreateWeaponRoot(string name, Transform parent, Vector3 localPosition, Quaternion localRotation)
    {
        GameObject weapon = new GameObject(name);
        weapon.transform.SetParent(parent);
        weapon.transform.localPosition = localPosition;
        weapon.transform.localRotation = localRotation;
        return weapon;
    }

    private static void CreateWeaponPart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;
        AssignMaterial(part, material);
        UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
    }

    private static void AddMuzzleFlash(Transform weapon, Vector3 localPosition)
    {
        GameObject flashObject = new GameObject("MuzzleFlash");
        flashObject.transform.SetParent(weapon);
        flashObject.transform.localPosition = localPosition;
        flashObject.transform.localRotation = Quaternion.identity;

        ParticleSystem flash = flashObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = flash.main;
        main.startLifetime = 0.06f;
        main.startSpeed = 0.35f;
        main.startSize = 0.10f;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 12;

        ParticleSystem.EmissionModule emission = flash.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });
    }

    private static void ConfigureGun(
        GameObject weapon,
        string weaponName,
        float damage,
        float fireDelay,
        float range,
        int magazineSize,
        int reserveAmmo,
        float reloadTime,
        bool automatic,
        float recoilAmount,
        float spread,
        Camera camera,
        GameObject player,
        Text ammoText,
        Text weaponText,
        Text statusText,
        int enemyLayer)
    {
        Gun gun = weapon.AddComponent<Gun>();
        SerializedObject serialized = new SerializedObject(gun);
        serialized.FindProperty("weaponName").stringValue = weaponName;
        serialized.FindProperty("damage").floatValue = damage;
        serialized.FindProperty("fireDelay").floatValue = fireDelay;
        serialized.FindProperty("range").floatValue = range;
        serialized.FindProperty("magazineSize").intValue = magazineSize;
        serialized.FindProperty("reserveAmmo").intValue = reserveAmmo;
        serialized.FindProperty("reloadTime").floatValue = reloadTime;
        serialized.FindProperty("automatic").boolValue = automatic;
        serialized.FindProperty("recoilAmount").floatValue = recoilAmount;
        serialized.FindProperty("spread").floatValue = spread;
        serialized.FindProperty("hitMask").intValue = (1 << enemyLayer) | (1 << 0);
        serialized.FindProperty("playerCamera").objectReferenceValue = camera;
        serialized.FindProperty("fpsController").objectReferenceValue = player.GetComponent<FPSController>();
        serialized.FindProperty("muzzlePoint").objectReferenceValue = weapon.transform.Find("MuzzleFlash");
        serialized.FindProperty("muzzleFlash").objectReferenceValue = weapon.transform.Find("MuzzleFlash").GetComponent<ParticleSystem>();
        serialized.FindProperty("ammoText").objectReferenceValue = ammoText;
        serialized.FindProperty("weaponText").objectReferenceValue = weaponText;
        serialized.FindProperty("statusText").objectReferenceValue = statusText;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateTargets(int enemyLayer, Material enemyMat)
    {
        GameObject targetPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetPrefab.name = "EnemyTarget";
        targetPrefab.transform.localScale = new Vector3(1.2f, 2.4f, 0.55f);
        targetPrefab.layer = enemyLayer;
        AssignMaterial(targetPrefab, enemyMat);
        targetPrefab.AddComponent<EnemyTarget>();
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(targetPrefab, PrefabsPath + "/EnemyTarget.prefab");
        UnityEngine.Object.DestroyImmediate(targetPrefab);

        Vector3[] positions =
        {
            new Vector3(-8f, 1.2f, 6f),
            new Vector3(0f, 1.2f, 8f),
            new Vector3(8f, 1.2f, 6f),
            new Vector3(-14f, 1.2f, 15f),
            new Vector3(14f, 1.2f, 15f),
            new Vector3(0f, 1.2f, 18f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject target = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            target.name = $"EnemyTarget_{i + 1:00}";
            target.transform.position = positions[i];
            target.transform.LookAt(new Vector3(0f, target.transform.position.y, -14f));
        }
    }

    private static void CreatePickups(Material healthMat, Material ammoMat)
    {
        GameObject healthPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthPrefab.name = "HealthPack";
        healthPrefab.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        AssignMaterial(healthPrefab, healthMat);
        healthPrefab.GetComponent<BoxCollider>().isTrigger = true;
        healthPrefab.AddComponent<HealthPack>();
        GameObject healthAsset = PrefabUtility.SaveAsPrefabAsset(healthPrefab, PrefabsPath + "/HealthPack.prefab");
        UnityEngine.Object.DestroyImmediate(healthPrefab);

        GameObject ammoPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ammoPrefab.name = "AmmoPack";
        ammoPrefab.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        AssignMaterial(ammoPrefab, ammoMat);
        ammoPrefab.GetComponent<BoxCollider>().isTrigger = true;
        ammoPrefab.AddComponent<AmmoPack>();
        GameObject ammoAsset = PrefabUtility.SaveAsPrefabAsset(ammoPrefab, PrefabsPath + "/AmmoPack.prefab");
        UnityEngine.Object.DestroyImmediate(ammoPrefab);

        InstantiatePrefab(healthAsset, "HealthPack_01", new Vector3(-5f, 0.6f, -7f), Quaternion.identity, Vector3.one);
        InstantiatePrefab(healthAsset, "HealthPack_02", new Vector3(12f, 0.6f, -2f), Quaternion.identity, Vector3.one);
        InstantiatePrefab(ammoAsset, "AmmoPack_01", new Vector3(5f, 0.6f, -6f), Quaternion.identity, Vector3.one);
        InstantiatePrefab(ammoAsset, "AmmoPack_02", new Vector3(-13f, 0.6f, 3f), Quaternion.identity, Vector3.one);
        InstantiatePrefab(ammoAsset, "AmmoPack_03", new Vector3(13f, 0.6f, 11f), Quaternion.identity, Vector3.one);
    }

    private static void CreateUI(out Text ammoText, out Text weaponText, out Text healthText, out Text statusText, out GameObject deathScreen)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        weaponText = CreateText("Weapon_Text", canvasObject.transform, new Vector2(24f, -20f), new Vector2(520f, 34f), 22, TextAnchor.UpperLeft);
        ammoText = CreateText("Ammo_Text", canvasObject.transform, new Vector2(24f, -56f), new Vector2(640f, 34f), 20, TextAnchor.UpperLeft);
        healthText = CreateText("Health_Text", canvasObject.transform, new Vector2(24f, -92f), new Vector2(360f, 34f), 20, TextAnchor.UpperLeft);
        statusText = CreateText("Status_Text", canvasObject.transform, new Vector2(24f, -128f), new Vector2(760f, 34f), 18, TextAnchor.UpperLeft);

        Text crosshair = CreateText("Crosshair", canvasObject.transform, Vector2.zero, new Vector2(80f, 80f), 34, TextAnchor.MiddleCenter);
        RectTransform crosshairRect = crosshair.GetComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.pivot = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        crosshair.text = "+";

        deathScreen = new GameObject("DeathScreen");
        deathScreen.transform.SetParent(canvasObject.transform, false);
        RectTransform deathRect = deathScreen.AddComponent<RectTransform>();
        deathRect.anchorMin = Vector2.zero;
        deathRect.anchorMax = Vector2.one;
        deathRect.offsetMin = Vector2.zero;
        deathRect.offsetMax = Vector2.zero;
        Image deathImage = deathScreen.AddComponent<Image>();
        deathImage.color = new Color(0f, 0f, 0f, 0.65f);
        Text deathText = CreateText("Death_Text", deathScreen.transform, Vector2.zero, new Vector2(900f, 120f), 38, TextAnchor.MiddleCenter);
        RectTransform deathTextRect = deathText.GetComponent<RectTransform>();
        deathTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        deathTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        deathTextRect.pivot = new Vector2(0.5f, 0.5f);
        deathTextRect.anchoredPosition = Vector2.zero;
        deathText.text = "PLAYER DIED";
        deathScreen.SetActive(false);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
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
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
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
