#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildForItch
{
    const string ScenePath = "Assets/Scenes/GamePlay.unity";
    const string WindowsDir = "Builds/Windows";
    const string WebGLDir = "Builds/WebGL";
    const string WindowsZip = "Builds/BalloonGo-Windows.zip";
    const string WebGLZip = "Builds/BalloonGo-WebGL.zip";

    [MenuItem("Balloon Go/Build Windows")]
    public static void BuildWindows()
    {
        PrepareSettings();
        bool ok = BuildWindowsInternal();
        Finish(ok);
    }

    [MenuItem("Balloon Go/Build WebGL")]
    public static void BuildWebGL()
    {
        PrepareSettings();
        bool ok = BuildWebGLInternal();
        Finish(ok);
    }

    [MenuItem("Balloon Go/Build All")]
    public static void BuildAll()
    {
        PrepareSettings();
        bool windows = BuildWindowsInternal();
        bool webgl = BuildWebGLInternal();
        Finish(windows && webgl);
    }

    static bool BuildWindowsInternal()
    {
        CleanDirectory(WindowsDir);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = WindowsDir + "/BalloonGo.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log("BUILD Windows: " + report.summary.result + " size=" + report.summary.totalSize);
        if (ok)
        {
            ZipFolder(WindowsDir, WindowsZip);
            Debug.Log("BUILD Windows zip: " + Path.GetFullPath(WindowsZip));
        }
        return ok;
    }

    static bool BuildWebGLInternal()
    {
        CleanDirectory(WebGLDir);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = WebGLDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log("BUILD WebGL: " + report.summary.result + " size=" + report.summary.totalSize);
        if (ok)
        {
            ZipFolder(WebGLDir, WebGLZip);
            Debug.Log("BUILD WebGL zip: " + Path.GetFullPath(WebGLZip));
        }
        return ok;
    }

    static void PrepareSettings()
    {
        PlayerSettings.companyName = "Balloon Go Studio";
        PlayerSettings.productName = "Balloon Go";
        PlayerSettings.bundleVersion = "1.0.0";

        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.balloongo.game");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, "com.balloongo.game");

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        AssetDatabase.SaveAssets();
    }

    static void CleanDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }

    static void ZipFolder(string sourceDir, string zipPath)
    {
        if (File.Exists(zipPath)) File.Delete(zipPath);
        ZipFile.CreateFromDirectory(sourceDir, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
    }

    static void Finish(bool ok)
    {
        if (!Application.isBatchMode) return;
        EditorApplication.Exit(ok ? 0 : 1);
    }
}
#endif
