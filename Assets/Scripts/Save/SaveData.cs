using System;
using System.Collections.Generic;
using Visvang.Core;

namespace Visvang.Save
{
    /// <summary>
    /// Complete game save state, serialized to JSON.
    /// Every piece of player progress lives here.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string lastSaveTimestamp;

        public PlayerData player = new PlayerData();
        public EquipmentData equipment = new EquipmentData();
        public InventoryData inventory = new InventoryData();
        public ProgressData progress = new ProgressData();
        public StatisticsData statistics = new StatisticsData();
        public List<SessionRecord> sessionHistory = new List<SessionRecord>();
        public List<CatchEntry> catchLog = new List<CatchEntry>();
    }

    [Serializable]
    public class PlayerData
    {
        public string playerName = "Hengelaar";
        public int playerLevel = 1;
        public int totalXP;
        public int currency;
    }

    [Serializable]
    public class EquipmentData
    {
        public string equippedRodName;
        public string equippedReelName;
        public List<string> ownedRodNames = new List<string>();
        public List<string> ownedReelNames = new List<string>();
        public bool hasSpecialGloves;
        public bool hasBarbelProofStand;
        public bool hasSlimeResistantGrip;
        public bool hasBetterPapBucket;
    }

    [Serializable]
    public class InventoryData
    {
        public List<DipInventoryEntry> dips = new List<DipInventoryEntry>();
        public List<BaitInventoryEntry> baits = new List<BaitInventoryEntry>();
    }

    [Serializable]
    public class DipInventoryEntry
    {
        public string dipName;
        public int quantity;
    }

    [Serializable]
    public class BaitInventoryEntry
    {
        public string baitName;
        public int quantity;
    }

    [Serializable]
    public class ProgressData
    {
        public List<string> unlockedUpgradeIds = new List<string>();
        public List<int> caughtSpeciesIds = new List<int>();
    }

    [Serializable]
    public class StatisticsData
    {
        public int totalFishCaught;
        public int totalFishLost;
        public int totalCarpCaught;
        public int totalBarbelCaught;
        public int totalMudfishCaught;
        public int totalOtherFishCaught;
        public float heaviestFishWeight;
        public string heaviestFishName;
        public int longestFightSeconds;
        public int rodsLostToBarbers;
        public int timesSlapped;
        public int totalCastsThrown;
        public int totalLinesSnapped;
        public int totalChaosEvents;
        public int totalSessionsPlayed;
        public float totalPlayTimeSeconds;
        public int totalPapBucketsUsed;
    }

    [Serializable]
    public class CatchEntry
    {
        public string fishName;
        public int speciesId;
        public float weight;
        public bool wasFirstCatch;
        public bool wasRecord;
        public string dipUsed;
        public string rodUsed;
        public string timestamp;
        public int sessionIndex;
    }

    [Serializable]
    public class SessionRecord
    {
        public int sessionIndex;
        public string startTimestamp;
        public string endTimestamp;
        public float durationSeconds;
        public int fishCaught;
        public int fishLost;
        public float totalWeightCaught;
        public float heaviestCatchWeight;
        public string heaviestCatchName;
        public int xpEarned;
        public string dipUsed;
        public string rodUsed;
        public string reelUsed;
        public int chaosEventsTriggered;
        public int castsThrown;
        public int linesSnapped;
        public int mudfishStreak;
    }
}
