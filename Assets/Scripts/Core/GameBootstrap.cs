using UnityEngine;
using Visvang.Fish;
using Visvang.Fishing;
using Visvang.Bait;
using Visvang.Equipment;
using Visvang.Events;
using Visvang.QTE;
using Visvang.Progression;
using Visvang.UI;
using Visvang.Audio;
using Visvang.Data;
using Visvang.Save;
using Visvang.Art;
using Visvang.Cloud;
using Visvang.Notifications;

namespace Visvang.Core
{
    /// <summary>
    /// Bootstraps the entire game. Attach this to a single empty GameObject.
    /// Creates all managers, loads save, loads data, restores progress,
    /// initializes cloud + notifications, builds UI + environment, starts GameFlow.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("[Visvang] Bootstrapping game...");

            // 1. Create save + cloud systems first
            EnsureManager<SaveManager>("SaveManager");
            EnsureManager<CloudManager>("CloudManager");
            EnsureManager<CloudSaveSync>("CloudSaveSync");
            EnsureManager<LeaderboardManager>("LeaderboardManager");
            EnsureManager<NotificationManager>("NotificationManager");

            // 2. Create all game manager singletons
            EnsureManager<GameManager>("GameManager");
            EnsureManager<FishDatabase>("FishDatabase");
            EnsureManager<FishSpawner>("FishSpawner");
            EnsureManager<BaitManager>("BaitManager");
            EnsureManager<PapSystem>("PapSystem");
            EnsureManager<EquipmentManager>("EquipmentManager");
            EnsureManager<UpgradeSystem>("UpgradeSystem");
            EnsureManager<ChaosEventManager>("ChaosEventManager");
            EnsureManager<QTEManager>("QTEManager");
            EnsureManager<XPSystem>("XPSystem");
            EnsureManager<PlayerProfile>("PlayerProfile");
            EnsureManager<AudioManager>("AudioManager");
            EnsureManager<SessionTracker>("SessionTracker");

            // 3. Create fishing systems with proper wiring
            CreateFishingController();
        }

        private void Start()
        {
            // 4. Show splash screen first, then load everything behind it
            var splashGo = new GameObject("SplashScreen");
            var splash = splashGo.AddComponent<UI.SplashScreen>();
            splash.OnSplashComplete += OnSplashComplete;
            splash.Show(0.5f, 2.5f, 0.8f);

            // 5. Load everything while splash is showing
            LoadGame();
        }

        private void LoadGame()
        {
            // 6. Load all game data definitions (fish, dips, rods, reels)
            RuntimeDataLoader.LoadAll();

            // 7. Load local save and restore progress
            bool hasSave = SaveManager.Instance.Load();
            if (hasSave)
            {
                SaveBridge.RestoreFromSave(SaveManager.Instance.CurrentSave);
                Debug.Log("[Visvang] Progress restored from local save.");
            }
            else
            {
                Debug.Log("[Visvang] No save found. Fresh start!");
            }

            // 8. Initialize cloud services (Firebase) — async, non-blocking
            if (CloudManager.Instance != null)
            {
                CloudManager.Instance.Initialize();
                CloudManager.Instance.OnCloudReady += OnCloudReady;
            }

            // 9. Cancel any pending notifications (player is back!)
            NotificationManager.Instance?.CancelAll();

            // 10. Build the 3D environment
            DamEnvironmentBuilder.Build();

            // 11. Generate and wire all procedural art assets
            AssetWiring.WireAll();

            // 12. Override procedural art with real assets from Resources/ (if any downloaded)
            SpriteAssetLoader.LoadAndOverride();

            // 13. Build the entire UI (hidden behind splash screen)
            var uiRefs = UIBuilder.Build();

            // 14. Create and initialize GameFlow (master controller)
            var flowGo = new GameObject("GameFlow");
            var gameFlow = flowGo.AddComponent<GameFlow>();
            gameFlow.Initialize(uiRefs);

            // 15. Auto-load audio from Resources/ folders
            var audioLoader = AudioManager.Instance?.gameObject.AddComponent<AudioAssetLoader>();
            audioLoader?.AutoScanResources();

            // 16. Fill pap bucket
            PapSystem.Instance?.FillBucket();

            Debug.Log($"[Visvang] Game loaded! Save: {SaveManager.Instance.SaveFilePath}");
        }

        private void OnSplashComplete()
        {
            Debug.Log("[Visvang] Splash complete. Welcome to the dam, bru!");
            // Main menu is already visible from UIBuilder — splash just fades away to reveal it
        }

        private void OnCloudReady()
        {
            Debug.Log("[Visvang] Cloud connected! Checking for cloud save...");

            // Try to restore from cloud if newer
            CloudSaveSync.Instance?.DownloadAndRestore();
        }

        private void CreateFishingController()
        {
            var fishingGo = new GameObject("FishingSystem");

            var controller = fishingGo.AddComponent<FishingController>();
            var fightCtrl = fishingGo.AddComponent<FightController>();
            var tensionSys = fishingGo.AddComponent<TensionSystem>();
            var castingSys = fishingGo.AddComponent<CastingSystem>();

            SetPrivateField(controller, "fishSpawner", FishSpawner.Instance ?? FindObjectOfType<FishSpawner>());
            SetPrivateField(controller, "fightController", fightCtrl);
            SetPrivateField(controller, "castingSystem", castingSys);
            SetPrivateField(controller, "tensionSystem", tensionSys);
            SetPrivateField(controller, "papSystem", PapSystem.Instance ?? FindObjectOfType<PapSystem>());
        }

        private void EnsureManager<T>(string name) where T : MonoBehaviour
        {
            if (FindObjectOfType<T>() == null)
            {
                var go = new GameObject(name);
                go.AddComponent<T>();
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null && value != null)
                field.SetValue(target, value);
            else if (field == null)
                Debug.LogWarning($"[Visvang] Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
