using UnityEngine;

namespace Visvang.Core
{
    /// <summary>
    /// Zero-setup auto-bootstrapper. Uses [RuntimeInitializeOnLoadMethod] to
    /// automatically create the GameBootstrap when the game starts — no scene setup,
    /// no manual component adding, no nothing. Just press Play.
    /// </summary>
    public static class AutoBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameStart()
        {
            // Check if GameBootstrap already exists (e.g., placed manually in scene)
            if (Object.FindObjectOfType<GameBootstrap>() != null)
                return;

            Debug.Log("[Visvang] AutoBoot: Creating GameBootstrap automatically...");

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();

            // Make sure it survives scene loads
            Object.DontDestroyOnLoad(bootstrapGo);
        }
    }
}
