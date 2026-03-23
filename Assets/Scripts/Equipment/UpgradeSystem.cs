using UnityEngine;
using System;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Progression;

namespace Visvang.Equipment
{
    /// <summary>
    /// Handles equipment upgrades, unlocks from catches, and gear progression.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        [Header("Available Upgrades")]
        [SerializeField] private List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();

        private HashSet<string> unlockedUpgrades = new HashSet<string>();

        public event Action<UpgradeDefinition> OnUpgradeUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Check unlock conditions after catching a fish.
        /// Called by the progression system.
        /// </summary>
        public void CheckUnlocks(FishSpecies caughtSpecies, int totalCatches, int playerLevel)
        {
            foreach (var upgrade in allUpgrades)
            {
                if (unlockedUpgrades.Contains(upgrade.upgradeId)) continue;

                bool unlocked = false;

                switch (upgrade.unlockCondition)
                {
                    case UnlockCondition.BarbelCatch:
                        unlocked = caughtSpecies == FishSpecies.Barbel || caughtSpecies == FishSpecies.FlatNoseRiverBarber;
                        break;
                    case UnlockCondition.MudfishCatch:
                        unlocked = caughtSpecies == FishSpecies.Mudfish;
                        break;
                    case UnlockCondition.TotalCatches:
                        unlocked = totalCatches >= upgrade.unlockThreshold;
                        break;
                    case UnlockCondition.PlayerLevel:
                        unlocked = playerLevel >= upgrade.unlockThreshold;
                        break;
                    case UnlockCondition.SpecificFishCount:
                        // Requires separate tracking per species
                        break;
                }

                if (unlocked)
                {
                    unlockedUpgrades.Add(upgrade.upgradeId);
                    ApplyUpgrade(upgrade);
                    OnUpgradeUnlocked?.Invoke(upgrade);
                }
            }
        }

        private void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            if (EquipmentManager.Instance == null) return;

            switch (upgrade.upgradeType)
            {
                case UpgradeType.Accessory:
                    EquipmentManager.Instance.UnlockAccessory(upgrade.upgradeId);
                    break;
                case UpgradeType.Rod:
                    if (upgrade.rodReward != null)
                        EquipmentManager.Instance.AddRod(upgrade.rodReward);
                    break;
                case UpgradeType.Reel:
                    if (upgrade.reelReward != null)
                        EquipmentManager.Instance.AddReel(upgrade.reelReward);
                    break;
            }
        }

        public bool IsUnlocked(string upgradeId)
        {
            return unlockedUpgrades.Contains(upgradeId);
        }

        public List<string> GetUnlockedIds()
        {
            return new List<string>(unlockedUpgrades);
        }

        public void RestoreUnlocks(List<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids)
                unlockedUpgrades.Add(id);
        }
    }

    [System.Serializable]
    public class UpgradeDefinition
    {
        public string upgradeId;
        public string upgradeName;
        [TextArea(1, 3)] public string description;
        public UpgradeType upgradeType;
        public UnlockCondition unlockCondition;
        public int unlockThreshold;
        public Sprite icon;

        // Rewards
        public RodData rodReward;
        public ReelData reelReward;
    }

    public enum UpgradeType
    {
        Accessory,
        Rod,
        Reel,
        Line,
        Cosmetic
    }

    public enum UnlockCondition
    {
        BarbelCatch,
        MudfishCatch,
        TotalCatches,
        PlayerLevel,
        SpecificFishCount
    }
}
