#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectPolish
{
    const string ScenePath = "Assets/Scenes/GamePlay.unity";

    [MenuItem("Balloon Go/Polish/Remove Old Canvas")]
    public static void RemoveOldCanvas()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.Log("POLISH: old Canvas not found");
            return;
        }

        Debug.Log("POLISH: removing old Canvas children=" + canvas.transform.childCount);
        Object.DestroyImmediate(canvas);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("POLISH: scene saved");
    }

    [MenuItem("Balloon Go/Polish/Optimize Textures")]
    public static void OptimizeTextures()
    {
        OptimizeFolder("Assets/FREE Parallax Forest Background HQ/Parallax Forest Background", 1024);
        OptimizeFolder("Assets/Cute_Birds(4 in 1)Mini", 512);
        OptimizeFolder("Assets/Material", 512);
        AssetDatabase.Refresh();
        Debug.Log("POLISH: textures optimized");
    }

    public static void Cleanup()
    {
        RemoveOldCanvas();
        OptimizeTextures();
    }

    static void OptimizeFolder(string folder, int maxSize)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 50;
            importer.SaveAndReimport();
            Debug.Log("POLISH: texture " + path + " max=" + maxSize);
        }
    }
}
#endif
