using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildWindows
{
    [MenuItem("Visvang/Build Windows (Debug)")]
    public static void Build()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
        Directory.CreateDirectory(buildPath);
        string exePath = Path.Combine(buildPath, "Visvang_Win", "Visvang.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(exePath));

        // Ensure we have a scene
        string[] scenes = BuildScript.GetBuildScenes();

        var report = BuildPipeline.BuildPlayer(scenes, exePath, BuildTarget.StandaloneWindows64, BuildOptions.Development | BuildOptions.AllowDebugging);

        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"[BuildWindows] SUCCESS: {exePath}");
        else
            Debug.LogError($"[BuildWindows] FAILED: {report.summary.totalErrors} errors");
    }
}
