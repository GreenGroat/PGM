using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lab1VisualPolish
{
    private const string ScenePath = "Assets/Scenes/Level_Prototype.unity";
    private const string MaterialsPath = "Assets/Materials";
    private const string DocumentationPath = "Assets/Documentation";

    public static void Apply()
    {
        EditorSceneManager.OpenScene(ScenePath);

        Material floor = CreateMaterial("Polish_MidnightFloor", new Color(0.04f, 0.08f, 0.11f), new Color(0.00f, 0.07f, 0.12f), 0.25f);
        Material wall = CreateMaterial("Polish_AbyssWall", new Color(0.09f, 0.10f, 0.16f), new Color(0.02f, 0.02f, 0.08f), 0.1f);
        Material neonCyan = CreateMaterial("Polish_CyanGlow", new Color(0.0f, 0.92f, 1.0f), new Color(0.0f, 0.85f, 1.0f), 1.7f);
        Material neonPink = CreateMaterial("Polish_PinkGlow", new Color(1.0f, 0.18f, 0.62f), new Color(1.0f, 0.04f, 0.48f), 1.6f);
        Material hazard = CreateMaterial("Polish_HazardCoral", new Color(1.0f, 0.22f, 0.14f), new Color(1.0f, 0.08f, 0.02f), 0.75f);
        Material platform = CreateMaterial("Polish_BrushedStone", new Color(0.24f, 0.27f, 0.28f), new Color(0.04f, 0.05f, 0.06f), 0.1f);
        Material player = CreateMaterial("Polish_PlayerBlue", new Color(0.14f, 0.40f, 1.0f), new Color(0.02f, 0.14f, 0.55f), 0.35f);
        Material finish = CreateMaterial("Polish_GreenGate", new Color(0.12f, 1.0f, 0.58f), new Color(0.02f, 1.0f, 0.42f), 1.5f);

        Assign("Ground", floor);
        Assign("Player", player);
        AssignContains("Wall", wall);
        AssignContains("Low_Wall", hazard);
        AssignContains("Platform", platform);
        AssignContains("Ramp", platform);
        AssignContains("Finish", finish);
        AssignContains("Start_Marker", neonPink);
        AssignContains("Coin_", neonPink);

        GameObject old = GameObject.Find("VisualPolish_SignalGarden");
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        GameObject root = new GameObject("VisualPolish_SignalGarden");
        CreateFloorGrid(root.transform, neonCyan, neonPink);
        CreateCoinHalos(root.transform, neonPink, neonCyan);
        CreateBeacon("Beacon_Left", root.transform, new Vector3(-12f, 1.2f, -10f), neonPink);
        CreateBeacon("Beacon_Right", root.transform, new Vector3(12f, 1.2f, -10f), neonCyan);
        CreateBeacon("Beacon_Back_Left", root.transform, new Vector3(-12f, 1.2f, 11f), neonCyan);
        CreateBeacon("Beacon_Back_Right", root.transform, new Vector3(12f, 1.2f, 11f), neonPink);
        CreateGoalCrown(root.transform, finish, neonCyan);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.11f, 0.13f, 0.18f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.09f);
        RenderSettings.fogDensity = 0.012f;

        AddOrUpdateLight("Polish_Cyan_Key", new Vector3(-7f, 6f, -6f), new Color(0f, 0.85f, 1f), 1.5f, 20f);
        AddOrUpdateLight("Polish_Pink_Key", new Vector3(8f, 5f, 2f), new Color(1f, 0.18f, 0.65f), 1.2f, 18f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        CaptureGameView();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab1 visual polish applied.");
    }

    public static void CaptureGameView()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        string assetPath = DocumentationPath + "/Lab1_GameView.png";
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        RenderTexture rt = new RenderTexture(1280, 720, 24);
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = camera.targetTexture;

        camera.targetTexture = rt;
        RenderTexture.active = rt;
        camera.Render();
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        image.Apply();
        File.WriteAllBytes(fullPath, image.EncodeToPNG());

        camera.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(image);
        AssetDatabase.ImportAsset(assetPath);
    }

    private static void CreateFloorGrid(Transform root, Material cyan, Material pink)
    {
        for (int i = -3; i <= 3; i++)
        {
            CreateCube($"Grid_Cyan_Z_{i}", root, new Vector3(i * 4f, 0.035f, 0f), new Vector3(0.055f, 0.05f, 28f), cyan);
        }

        for (int i = -3; i <= 3; i++)
        {
            CreateCube($"Grid_Pink_X_{i}", root, new Vector3(0f, 0.04f, i * 4f), new Vector3(28f, 0.055f, 0.05f), pink);
        }
    }

    private static void CreateCoinHalos(Transform root, Material pink, Material cyan)
    {
        foreach (GameObject coin in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!coin.name.StartsWith("Coin_"))
            {
                continue;
            }

            AssignRenderer(coin, pink);
            Light light = AddOrUpdateLight(coin.name + "_Glow", coin.transform.position + Vector3.up * 0.65f, new Color(1f, 0.22f, 0.65f), 1.3f, 4f);
            light.transform.SetParent(root);
            GameObject halo = CreateCube(coin.name + "_Halo", root, coin.transform.position + new Vector3(0f, -0.45f, 0f), new Vector3(1.3f, 0.04f, 1.3f), cyan);
            halo.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        }
    }

    private static void CreateBeacon(string name, Transform root, Vector3 position, Material material)
    {
        CreateCube(name + "_Base", root, position, new Vector3(0.55f, 2.4f, 0.55f), material);
        CreateCube(name + "_Cross", root, position + Vector3.up * 1.45f, new Vector3(1.6f, 0.12f, 0.12f), material);
        AddOrUpdateLight(name + "_Light", position + Vector3.up * 2f, material.color, 1.4f, 8f).transform.SetParent(root);
    }

    private static void CreateGoalCrown(Transform root, Material finish, Material accent)
    {
        CreateCube("Finish_Crown_Left", root, new Vector3(-3.3f, 3.2f, 12f), new Vector3(0.18f, 1.5f, 0.18f), accent);
        CreateCube("Finish_Crown_Right", root, new Vector3(3.3f, 3.2f, 12f), new Vector3(0.18f, 1.5f, 0.18f), accent);
        CreateCube("Finish_Crown_Top", root, new Vector3(0f, 4.0f, 12f), new Vector3(6.8f, 0.16f, 0.16f), finish);
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        AssignRenderer(cube, material);
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        return cube;
    }

    private static Light AddOrUpdateLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject existing = GameObject.Find(name);
        GameObject lightObject = existing != null ? existing : new GameObject(name);
        lightObject.transform.position = position;
        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    private static void Assign(string name, Material material)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            AssignRenderer(obj, material);
        }
    }

    private static void AssignContains(string token, Material material)
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.name.Contains(token))
            {
                AssignRenderer(obj, material);
            }
        }
    }

    private static void AssignRenderer(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
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

        material.color = baseColor;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emissionColor * emissionStrength);
            if (emissionStrength > 0f)
            {
                material.EnableKeyword("_EMISSION");
            }
        }

        return material;
    }
}
