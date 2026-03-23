using UnityEngine;
using System;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Save;

namespace Visvang.Progression
{
    /// <summary>
    /// Persistent player profile. Tracks XP, level, catch history, records, and unlocks.
    /// All persistence handled by SaveManager/SaveBridge — no direct PlayerPrefs.
    /// </summary>
    public class PlayerProfile : MonoBehaviour
    {
        public static PlayerProfile Instance { get; private set; }

        [Header("Player Info")]
        [SerializeField] private string playerName = "Hengelaar";
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int totalXP;
        [SerializeField] private int currency;

        [Header("Statistics")]
        [SerializeField] private int totalFishCaught;
        [SerializeField] private int totalFishLost;
        [SerializeField] private int totalCarpCaught;
        [SerializeField] private int totalBarbelCaught;
        [SerializeField] private int totalMudfishCaught;
        [SerializeField] private float heaviestFishWeight;
        [SerializeField] private string heaviestFishName;
        [SerializeField] private int longestFightSeconds;
        [SerializeField] private int rodsLostToBarbers;
        [SerializeField] private int timesSlapped;

        [Header("Catch Log")]
        [SerializeField] private List<CatchRecord> catchHistory = new List<CatchRecord>();
        private HashSet<FishSpecies> caughtSpecies = new HashSet<FishSpecies>();

        public string PlayerName => playerName;
        public int PlayerLevel => playerLevel;
        public int TotalXP => totalXP;
        public int Currency => currency;
        public int TotalFishCaught => totalFishCaught;
        public int TotalFishLost => totalFishLost;
        public int TotalBarbelCaught => totalBarbelCaught;
        public int TotalMudfishCaught => totalMudfishCaught;
        public float HeaviestFishWeight => heaviestFishWeight;
        public string HeaviestFishName => heaviestFishName;
        public int RodsLostToBarbers => rodsLostToBarbers;
        public int TimesSlapped => timesSlapped;
        public List<CatchRecord> CatchHistory => catchHistory;

        public event Action<int> OnLevelUp;
        public event Action<CatchRecord> OnNewRecord;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddXP(int xp)
        {
            totalXP += xp;

            // Sync to save
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.player.totalXP = totalXP;

            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (playerLevel < Constants.MAX_PLAYER_LEVEL)
            {
                int xpNeeded = XPSystem.XPForLevel(playerLevel + 1);
                if (totalXP >= xpNeeded)
                {
                    playerLevel++;

                    // Sync to save
                    var save = SaveManager.Instance?.CurrentSave;
                    if (save != null)
                        save.player.playerLevel = playerLevel;

                    OnLevelUp?.Invoke(playerLevel);
                }
                else
                {
                    break;
                }
            }
        }

        public void RecordCatch(FishData fish, float weight, float fightDuration)
        {
            totalFishCaught++;
            bool isFirstCatch = !caughtSpecies.Contains(fish.species);
            caughtSpecies.Add(fish.species);

            // Species tracking
            if (fish.IsCarp()) totalCarpCaught++;
            if (fish.IsBarbel()) totalBarbelCaught++;
            if (fish.IsMudfish()) totalMudfishCaught++;

            // Records
            bool isNewRecord = false;
            if (weight > heaviestFishWeight)
            {
                heaviestFishWeight = weight;
                heaviestFishName = fish.fishName;
                isNewRecord = true;
            }
            if (fightDuration > longestFightSeconds)
            {
                longestFightSeconds = Mathf.RoundToInt(fightDuration);
                isNewRecord = true;
            }

            var record = new CatchRecord
            {
                fishName = fish.fishName,
                species = fish.species,
                weight = weight,
                isFirstCatch = isFirstCatch,
                isNewRecord = isNewRecord,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            catchHistory.Add(record);

            if (isNewRecord)
                OnNewRecord?.Invoke(record);

            // Award XP
            XPSystem.Instance?.AwardXP(fish, weight, isFirstCatch);

            // Check equipment unlocks
            Equipment.UpgradeSystem.Instance?.CheckUnlocks(fish.species, totalFishCaught, playerLevel);

            // Sync stats to save
            SyncStatisticsToSave();

            // Mark save dirty (will auto-save on interval or on session end)
            SaveManager.Instance?.MarkDirty();
        }

        public void RecordFishLost()
        {
            totalFishLost++;
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.statistics.totalFishLost = totalFishLost;
        }

        public void RecordRodLost()
        {
            rodsLostToBarbers++;
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.statistics.rodsLostToBarbers = rodsLostToBarbers;
        }

        public void RecordSlapped()
        {
            timesSlapped++;
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.statistics.timesSlapped = timesSlapped;
        }

        public bool HasCaughtSpecies(FishSpecies species)
        {
            return caughtSpecies.Contains(species);
        }

        public void MarkSpeciesCaught(FishSpecies species)
        {
            caughtSpecies.Add(species);
        }

        public int GetSpeciesCatchCount(FishSpecies species)
        {
            int count = 0;
            foreach (var record in catchHistory)
            {
                if (record.species == species) count++;
            }
            return count;
        }

        public int CaughtSpeciesCount => caughtSpecies.Count;

        public bool SpendCurrency(int amount)
        {
            if (currency < amount) return false;
            currency -= amount;
            SyncPlayerToSave();
            return true;
        }

        public void AddCurrency(int amount)
        {
            currency += amount;
            SyncPlayerToSave();
        }

        public void SetPlayerName(string name)
        {
            playerName = name;
            SyncPlayerToSave();
        }

        public float GetXPProgressToNextLevel()
        {
            if (playerLevel >= Constants.MAX_PLAYER_LEVEL) return 1f;

            int currentLevelXP = XPSystem.XPForLevel(playerLevel);
            int nextLevelXP = XPSystem.XPForLevel(playerLevel + 1);
            int diff = nextLevelXP - currentLevelXP;
            if (diff <= 0) return 1f;
            return (float)(totalXP - currentLevelXP) / diff;
        }

        // ===== Save/Restore methods (called by SaveBridge) =====

        public void RestoreFromSave(string name, int level, int xp, int cur)
        {
            playerName = name;
            playerLevel = level;
            totalXP = xp;
            currency = cur;
        }

        public void RestoreStatistics(int fishCaught, int fishLost, int carp, int barbel, int mudfish,
            float heaviestWeight, string heaviestName, int longestFight, int rodsLost, int slapped)
        {
            totalFishCaught = fishCaught;
            totalFishLost = fishLost;
            totalCarpCaught = carp;
            totalBarbelCaught = barbel;
            totalMudfishCaught = mudfish;
            heaviestFishWeight = heaviestWeight;
            heaviestFishName = heaviestName;
            longestFightSeconds = longestFight;
            rodsLostToBarbers = rodsLost;
            timesSlapped = slapped;
        }

        private void SyncPlayerToSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;
            save.player.playerName = playerName;
            save.player.playerLevel = playerLevel;
            save.player.totalXP = totalXP;
            save.player.currency = currency;
            SaveManager.Instance.MarkDirty();
        }

        private void SyncStatisticsToSave()
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;
            save.statistics.totalFishCaught = totalFishCaught;
            save.statistics.totalFishLost = totalFishLost;
            save.statistics.totalCarpCaught = totalCarpCaught;
            save.statistics.totalBarbelCaught = totalBarbelCaught;
            save.statistics.totalMudfishCaught = totalMudfishCaught;
            save.statistics.heaviestFishWeight = heaviestFishWeight;
            save.statistics.heaviestFishName = heaviestFishName ?? "";
            save.statistics.longestFightSeconds = longestFightSeconds;
            save.statistics.rodsLostToBarbers = rodsLostToBarbers;
            save.statistics.timesSlapped = timesSlapped;
        }

        public void ResetProfile()
        {
            playerLevel = 1;
            totalXP = 0;
            currency = 0;
            totalFishCaught = 0;
            totalFishLost = 0;
            totalBarbelCaught = 0;
            totalMudfishCaught = 0;
            totalCarpCaught = 0;
            heaviestFishWeight = 0f;
            heaviestFishName = "";
            longestFightSeconds = 0;
            rodsLostToBarbers = 0;
            timesSlapped = 0;
            catchHistory.Clear();
            caughtSpecies.Clear();

            SaveManager.Instance?.DeleteSave();
        }
    }

    [Serializable]
    public class CatchRecord
    {
        public string fishName;
        public FishSpecies species;
        public float weight;
        public bool isFirstCatch;
        public bool isNewRecord;
        public string timestamp;
    }
}
