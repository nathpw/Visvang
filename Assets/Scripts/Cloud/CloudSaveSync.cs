using UnityEngine;
using System;
using System.Collections.Generic;
using Visvang.Save;

namespace Visvang.Cloud
{
    /// <summary>
    /// Syncs local SaveData to/from Firebase Firestore.
    /// Uploads on session end, downloads on first boot (cloud restore).
    /// Conflict resolution: most recent save timestamp wins.
    /// </summary>
    public class CloudSaveSync : MonoBehaviour
    {
        public static CloudSaveSync Instance { get; private set; }

        private float syncTimer;
        private bool syncPending;

        public event Action OnCloudSaveComplete;
        public event Action OnCloudRestoreComplete;
        public event Action<string> OnSyncError;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!syncPending) return;

            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable) return;

            var config = cloud.GetConfig();
            if (config == null || !config.cloudSaveEnabled) return;

            syncTimer += Time.unscaledDeltaTime;
            if (syncTimer >= config.cloudSyncInterval)
            {
                syncTimer = 0f;
                UploadSave();
            }
        }

        /// <summary>
        /// Mark that a sync should happen on the next interval.
        /// </summary>
        public void MarkSyncPending()
        {
            syncPending = true;
        }

        /// <summary>
        /// Immediately upload current save to Firestore.
        /// </summary>
        public void UploadSave()
        {
#if FIREBASE_ENABLED
            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable) return;

            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            // Convert SaveData to a dictionary for Firestore
            var data = new Dictionary<string, object>
            {
                { "saveVersion", save.saveVersion },
                { "lastSaveTimestamp", save.lastSaveTimestamp ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "playerName", save.player.playerName },
                { "playerLevel", save.player.playerLevel },
                { "totalXP", save.player.totalXP },
                { "currency", save.player.currency },
                { "totalFishCaught", save.statistics.totalFishCaught },
                { "totalBarbelCaught", save.statistics.totalBarbelCaught },
                { "totalMudfishCaught", save.statistics.totalMudfishCaught },
                { "heaviestFishWeight", save.statistics.heaviestFishWeight },
                { "heaviestFishName", save.statistics.heaviestFishName ?? "" },
                { "totalSessionsPlayed", save.statistics.totalSessionsPlayed },
                { "totalPlayTimeSeconds", save.statistics.totalPlayTimeSeconds },
                { "caughtSpeciesCount", save.progress.caughtSpeciesIds.Count },
                // Store full save JSON for complete restore
                { "fullSaveJson", JsonUtility.ToJson(save) }
            };

            cloud.GetUserDoc().SetAsync(data).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[CloudSave] Upload failed: {task.Exception}");
                    OnSyncError?.Invoke("Cloud save failed");
                }
                else
                {
                    syncPending = false;
                    Debug.Log("[CloudSave] Save uploaded to cloud.");
                    OnCloudSaveComplete?.Invoke();
                }
            });
#else
            Debug.Log("[CloudSave] Offline mode — upload skipped.");
#endif
        }

        /// <summary>
        /// Download save from Firestore and restore if newer than local.
        /// </summary>
        public void DownloadAndRestore()
        {
#if FIREBASE_ENABLED
            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable) return;

            cloud.GetUserDoc().GetSnapshotAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[CloudSave] Download failed: {task.Exception}");
                    OnSyncError?.Invoke("Cloud restore failed");
                    return;
                }

                var snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.Log("[CloudSave] No cloud save found.");
                    return;
                }

                string cloudTimestamp = snapshot.GetValue<string>("lastSaveTimestamp");
                string localTimestamp = SaveManager.Instance?.CurrentSave?.lastSaveTimestamp;

                // Compare timestamps — most recent wins
                bool cloudIsNewer = string.Compare(cloudTimestamp, localTimestamp, StringComparison.Ordinal) > 0;

                if (cloudIsNewer && snapshot.ContainsField("fullSaveJson"))
                {
                    string json = snapshot.GetValue<string>("fullSaveJson");
                    var cloudSave = JsonUtility.FromJson<SaveData>(json);

                    if (cloudSave != null)
                    {
                        // Replace local save with cloud save
                        var saveManager = SaveManager.Instance;
                        if (saveManager != null)
                        {
                            // We need to do this on the main thread
                            UnityMainThread.Execute(() =>
                            {
                                var field = typeof(SaveManager).GetField("currentSave",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                field?.SetValue(saveManager, cloudSave);
                                saveManager.Save();

                                SaveBridge.RestoreFromSave(cloudSave);
                                Debug.Log("[CloudSave] Cloud save restored (was newer).");
                                OnCloudRestoreComplete?.Invoke();
                            });
                        }
                    }
                }
                else
                {
                    Debug.Log("[CloudSave] Local save is newer or equal. Keeping local.");
                }
            });
#else
            Debug.Log("[CloudSave] Offline mode — restore skipped.");
#endif
        }

        /// <summary>
        /// Upload a single catch record for global leaderboard tracking.
        /// </summary>
        public void UploadCatchRecord(string fishName, float weight, string species)
        {
#if FIREBASE_ENABLED
            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable) return;

            var config = cloud.GetConfig();
            if (config == null || !config.syncOnEveryCatch) return;

            var data = new Dictionary<string, object>
            {
                { "userId", cloud.UserId },
                { "playerName", cloud.DisplayName },
                { "fishName", fishName },
                { "weight", weight },
                { "species", species },
                { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            cloud.GetFirestore().Collection(config.catchLogCollection).AddAsync(data);
#endif
        }
    }

    /// <summary>
    /// Helper to execute actions on the Unity main thread (Firebase callbacks are on background threads).
    /// </summary>
    public class UnityMainThread : MonoBehaviour
    {
        private static UnityMainThread instance;
        private static readonly Queue<Action> queue = new Queue<Action>();

        public static void Execute(Action action)
        {
            if (instance == null)
            {
                var go = new GameObject("UnityMainThread");
                instance = go.AddComponent<UnityMainThread>();
                DontDestroyOnLoad(go);
            }

            lock (queue)
            {
                queue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                    queue.Dequeue()?.Invoke();
            }
        }
    }
}
