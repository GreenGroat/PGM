using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lab3VisualPolish
{
    private const string ScenePath = "Assets/Scenes/FPS_Arsenal_Prototype.unity";
    private const string MaterialsPath = "Assets/Materials";
    private const string DocumentationPath = "Assets/Documentation";

    public static void Apply()
    {
        EditorSceneManager.OpenScene(ScenePath);

        Material floor = CreateMaterial("Polish_RangeFloor", new Color(0.035f, 0.045f, 0.055f), new Color(0.0f, 0.04f, 0.07f), 0.2f);
        Material wall = CreateMaterial("Polish_RangeWall", new Color(0.08f, 0.09f, 0.13f), new Color(0.01f, 0.02f, 0.06f), 0.08f);
        Material cover = CreateMaterial("Polish_CoverGraphite", new Color(0.20f, 0.23f, 0.25f), new Color(0.02f, 0.03f, 0.04f), 0.05f);
        Material cyan = CreateMaterial("Polish_RangeCyan", new Color(0.0f, 0.82f, 1.0f), new Color(0.0f, 0.78f, 1.0f), 1.6f);
        Material amber = CreateMaterial("Polish_RangeAmber", new Color(1.0f, 0.62f, 0.16f), new Color(1.0f, 0.40f, 0.04f), 1.3f);
        Material red = CreateMaterial("Polish_TargetRed", new Color(1.0f, 0.10f, 0.18f), new Color(1.0f, 0.02f, 0.05f), 1.1f);
        Material magenta = CreateMaterial("Polish_MagentaSignal", new Color(1.0f, 0.16f, 0.70f), new Color(1.0f, 0.02f, 0.48f), 1.2f);
        Material weapon = CreateMaterial("Polish_WeaponMatte", new Color(0.08f, 0.10f, 0.13f), new Color(0.02f, 0.06f, 0.08f), 0.15f);
        Material rifle = CreateMaterial("Polish_RifleBlueSteel", new Color(0.12f, 0.20f, 0.26f), new Color(0.0f, 0.11f, 0.20f), 0.25f);

        Assign("Ground", floor);
        AssignContains("Wall", wall);
        AssignContains("Cover_Block", cover);
        AssignContains("Damage_Zone", magenta);
        AssignContains("EnemyTarget", red);
        AssignContains("Pistol", weapon);
        AssignContains("Rifle", rifle);
        AssignContains("AmmoPack", amber);
        AssignContains("HealthPack", magenta);

        GameObject old = GameObject.Find("VisualPolish_CyberRange");
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        GameObject root = new GameObject("VisualPolish_CyberRange");
        CreateRangeGrid(root.transform, cyan, magenta);
        CreateTargetFrames(root.transform, cyan, amber);
        CreateCeilingSignals(root.transform, cyan, magenta);
        CreateCoverStripes(root.transform, amber);

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.07f);
            camera.fieldOfView = 68f;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.11f, 0.15f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.035f, 0.04f, 0.055f);
        RenderSettings.fogDensity = 0.016f;

        AddOrUpdateLight("Polish_Range_Cyan_Key", new Vector3(-8f, 5.2f, -4f), new Color(0f, 0.82f, 1f), 1.6f, 18f).transform.SetParent(root.transform);
        AddOrUpdateLight("Polish_Range_Amber_Key", new Vector3(9f, 4.6f, 7f), new Color(1f, 0.46f, 0.10f), 1.2f, 16f).transform.SetParent(root.transform);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        CaptureGameView();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lab3 visual polish applied.");
    }

    public static void CaptureGameView()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        string assetPath = DocumentationPath + "/Lab3_GameView.png";
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

    private static void CreateRangeGrid(Transform root, Material cyan, Material magenta)
    {
        for (int i = -4; i <= 4; i++)
        {
            CreateCube($"Grid_Cyan_Z_{i}", root, new Vector3(i * 5f, 0.035f, 0f), new Vector3(0.055f, 0.05f, 40f), cyan);
        }

        for (int i = -4; i <= 4; i++)
        {
            CreateCube($"Grid_Magenta_X_{i}", root, new Vector3(0f, 0.04f, i * 5f), new Vector3(40f, 0.055f, 0.05f), magenta);
        }
    }

    private static void CreateTargetFrames(Transform root, Material cyan, Material amber)
    {
        foreach (GameObject target in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!target.name.StartsWith("EnemyTarget_"))
            {
                continue;
            }

            Vector3 p = target.transform.position;
            Transform frameRoot = new GameObject(target.name + "_Frame").transform;
            frameRoot.SetParent(root);
            frameRoot.position = p;
            frameRoot.rotation = target.transform.rotation;
            CreateCube("Frame_Left", frameRoot, p + target.transform.right * -0.95f + Vector3.up * 0.05f, new Vector3(0.08f, 2.9f, 0.08f), cyan);
            CreateCube("Frame_Right", frameRoot, p + target.transform.right * 0.95f + Vector3.up * 0.05f, new Vector3(0.08f, 2.9f, 0.08f), cyan);
            CreateCube("Frame_Top", frameRoot, p + Vector3.up * 1.45f, new Vector3(2.15f, 0.08f, 0.08f), amber);
            AddOrUpdateLight(target.name + "_Light", p + Vector3.up * 1.7f, new Color(1f, 0.12f, 0.18f), 0.8f, 5f).transform.SetParent(root);
        }
    }

    private static void CreateCeilingSignals(Transform root, Material cyan, Material magenta)
    {
        for (int i = -2; i <= 2; i++)
        {
            CreateCube($"Signal_Bar_Cyan_{i}", root, new Vector3(i * 6f, 4.2f, -10f), new Vector3(3.8f, 0.12f, 0.12f), cyan);
            CreateCube($"Signal_Bar_Magenta_{i}", root, new Vector3(i * 6f, 4.2f, 12f), new Vector3(3.8f, 0.12f, 0.12f), magenta);
        }
    }

    private static void CreateCoverStripes(Transform root, Material amber)
    {
        foreach (GameObject cover in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!cover.name.StartsWith("Cover_Block"))
            {
                continue;
            }

            Vector3 p = cover.transform.position + Vector3.up * (cover.transform.localScale.y * 0.5f + 0.05f);
            GameObject stripe = CreateCube(cover.name + "_Amber_Stripe", root, p, new Vector3(cover.transform.localScale.x * 0.85f, 0.08f, 0.08f), amber);
            stripe.transform.rotation = cover.transform.rotation;
        }
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
