using UnityEngine;
using System;
using System.IO;

namespace Visvang.Save
{
    /// <summary>
    /// Handles reading/writing the full game save to a JSON file.
    /// Save file lives in Application.persistentDataPath/visvang_save.json.
    /// Auto-saves on catch, session end, and periodically.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILENAME = "visvang_save.json";
        private const string BACKUP_FILENAME = "visvang_save_backup.json";
        private const float AUTO_SAVE_INTERVAL = 60f;

        private SaveData currentSave;
        private float autoSaveTimer;
        private bool isDirty;

        public SaveData CurrentSave => currentSave;
        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);

        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentSave = new SaveData();
        }

        private void Update()
        {
            if (!isDirty) return;

            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                autoSaveTimer = 0f;
                WriteToDisk();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && isDirty)
                WriteToDisk();
        }

        private void OnApplicationQuit()
        {
            if (isDirty)
                WriteToDisk();
        }

        /// <summary>
        /// Mark save as dirty so it will be written on next auto-save cycle.
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// Immediately write save data to disk.
        /// </summary>
        public void Save()
        {
            currentSave.lastSaveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            WriteToDisk();
        }

        /// <summary>
        /// Load save data from disk. Returns true if a save file existed.
        /// </summary>
        public bool Load()
        {
            string path = SaveFilePath;

            if (!File.Exists(path))
            {
                Debug.Log("[SaveManager] No save file found. Starting fresh.");
                currentSave = new SaveData();
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                currentSave = JsonUtility.FromJson<SaveData>(json);

                if (currentSave == null)
                {
                    Debug.LogWarning("[SaveManager] Save file corrupted. Trying backup...");
                    return TryLoadBackup();
                }

                Debug.Log($"[SaveManager] Save loaded. Player: {currentSave.player.playerName} Lv{currentSave.player.playerLevel} | " +
                         $"Fish caught: {currentSave.statistics.totalFishCaught} | Sessions: {currentSave.statistics.totalSessionsPlayed}");

                isDirty = false;
                OnLoadCompleted?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load save: {e.Message}. Trying backup...");
                return TryLoadBackup();
            }
        }

        private bool TryLoadBackup()
        {
            string backupPath = Path.Combine(Application.persistentDataPath, BACKUP_FILENAME);
            if (!File.Exists(backupPath))
            {
                currentSave = new SaveData();
                return false;
            }

            try
            {
                string json = File.ReadAllText(backupPath);
                currentSave = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                Debug.Log("[SaveManager] Restored from backup.");
                isDirty = true;
                return true;
            }
            catch
            {
                currentSave = new SaveData();
                return false;
            }
        }

        private void WriteToDisk()
        {
            string path = SaveFilePath;

            try
            {
                // Backup existing save before overwriting
                if (File.Exists(path))
                {
                    string backupPath = Path.Combine(Application.persistentDataPath, BACKUP_FILENAME);
                    File.Copy(path, backupPath, true);
                }

                string json = JsonUtility.ToJson(currentSave, true);
                File.WriteAllText(path, json);

                isDirty = false;
                Debug.Log($"[SaveManager] Game saved to {path}");
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        /// <summary>
        /// Delete all save data and start fresh.
        /// </summary>
        public void DeleteSave()
        {
            string path = SaveFilePath;
            string backupPath = Path.Combine(Application.persistentDataPath, BACKUP_FILENAME);

            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(backupPath)) File.Delete(backupPath);

            currentSave = new SaveData();
            isDirty = false;

            Debug.Log("[SaveManager] Save data deleted.");
        }

        public bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }
    }
}
