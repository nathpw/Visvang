using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Visvang/Build Android APK")]
    public static void BuildAndroid()
    {
        // Switch to Android platform first
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Configure player settings
        PlayerSettings.companyName = "Visvang";
        PlayerSettings.productName = "Visvang";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.visvang.fishing");
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // Splash
        PlayerSettings.SplashScreen.showUnityLogo = false;

        // App icon — use Visvang.png
        var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Sprites/splash_screen.png");
        if (iconTex != null)
        {
            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Android);
            var sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
            if (icons.Length > 0)
            {
                for (int i = 0; i < icons.Length; i++)
                    icons[i] = iconTex;
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
            }
        }

        // Build output
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
        Directory.CreateDirectory(buildPath);
        string apkPath = Path.Combine(buildPath, "Visvang.apk");

        // Ensure we have at least one scene
        string[] scenes = GetBuildScenes();

        Debug.Log($"[BuildScript] Building APK to: {apkPath}");
        Debug.Log($"[BuildScript] Scenes: {string.Join(", ", scenes)}");

        var report = BuildPipeline.BuildPlayer(scenes, apkPath, BuildTarget.Android, BuildOptions.None);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] BUILD SUCCEEDED! APK: {apkPath} ({report.summary.totalSize / 1024 / 1024}MB)");
        }
        else
        {
            Debug.LogError($"[BuildScript] BUILD FAILED: {report.summary.totalErrors} errors");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError(msg.content);
                }
            }
        }
    }

    private static string[] GetBuildScenes()
    {
        // Check build settings first
        var editorScenes = EditorBuildSettings.scenes;
        if (editorScenes != null && editorScenes.Length > 0)
        {
            var paths = new System.Collections.Generic.List<string>();
            foreach (var s in editorScenes)
            {
                if (s.enabled && !string.IsNullOrEmpty(s.path))
                    paths.Add(s.path);
            }
            if (paths.Count > 0) return paths.ToArray();
        }

        // Find any existing scene
        var foundScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        if (foundScenes.Length > 0)
            return new string[] { AssetDatabase.GUIDToAssetPath(foundScenes[0]) };

        // Create a minimal empty scene — AutoBoot handles the rest
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);
        string scenePath = "Assets/Scenes/MainScene.unity";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", scenePath)));
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        // Also add to build settings
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

        return new string[] { scenePath };
    }
}
