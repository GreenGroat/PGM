using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ProjectStartupScene
{
    private const string MainScenePath = "Assets/Scenes/NarrativeScene.unity";
    private const string SessionKey = "ProjectStartupScene.Opened";

    static ProjectStartupScene()
    {
        EditorApplication.delayCall += OpenMainSceneOnce;
    }

    private static void OpenMainSceneOnce()
    {
        if (Application.isBatchMode || SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.path))
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) != null)
        {
            EditorSceneManager.OpenScene(MainScenePath);
        }
    }
}
