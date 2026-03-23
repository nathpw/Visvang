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
            // 4. Load all game data definitions (fish, dips, rods, reels)
            RuntimeDataLoader.LoadAll();

            // 5. Load local save and restore progress
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

            // 6. Initialize cloud services (Firebase) — async, non-blocking
            CloudManager.Instance?.Initialize();
            CloudManager.Instance.OnCloudReady += OnCloudReady;

            // 7. Cancel any pending notifications (player is back!)
            NotificationManager.Instance?.CancelAll();

            // 8. Build the 3D environment
            DamEnvironmentBuilder.Build();

            // 9. Generate and wire all procedural art assets
            AssetWiring.WireAll();

            // 10. Override procedural art with real assets from Resources/ (if any downloaded)
            SpriteAssetLoader.LoadAndOverride();

            // 11. Build the entire UI
            var uiRefs = UIBuilder.Build();

            // 11. Create and initialize GameFlow (master controller)
            var flowGo = new GameObject("GameFlow");
            var gameFlow = flowGo.AddComponent<GameFlow>();
            gameFlow.Initialize(uiRefs);

            // 14. Auto-load audio from Resources/ folders (if downloaded)
            var audioLoader = AudioManager.Instance?.gameObject.AddComponent<AudioAssetLoader>();
            audioLoader?.AutoScanResources();

            // 15. Fill pap bucket
            PapSystem.Instance?.FillBucket();

            Debug.Log($"[Visvang] Game ready! Save: {SaveManager.Instance.SaveFilePath}");
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
