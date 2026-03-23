using UnityEngine;
using System;
using System.Collections.Generic;
using Visvang.Core;

namespace Visvang.Equipment
{
    /// <summary>
    /// Manages player's equipment loadout and inventory.
    /// Singleton accessible throughout the game for stat calculations.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        [Header("Current Loadout")]
        [SerializeField] private RodData equippedRod;
        [SerializeField] private ReelData equippedReel;

        [Header("Accessories")]
        [SerializeField] private bool hasSpecialGloves;
        [SerializeField] private bool hasBarbelProofStand;
        [SerializeField] private bool hasSlimeResistantGrip;
        [SerializeField] private bool hasBetterPapBucket;

        [Header("Inventory")]
        [SerializeField] private List<RodData> ownedRods = new List<RodData>();
        [SerializeField] private List<ReelData> ownedReels = new List<ReelData>();

        public RodData EquippedRod => equippedRod;
        public ReelData EquippedReel => equippedReel;
        public bool HasSpecialGloves => hasSpecialGloves;
        public bool HasBarbelProofStand => hasBarbelProofStand;
        public bool HasSlimeResistantGrip => hasSlimeResistantGrip;
        public bool HasBetterPapBucket => hasBetterPapBucket;

        public event Action<RodData> OnRodEquipped;
        public event Action<ReelData> OnReelEquipped;
        public event Action OnRodBroken;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void EquipRod(RodData rod)
        {
            equippedRod = rod;
            OnRodEquipped?.Invoke(rod);
        }

        public void EquipReel(ReelData reel)
        {
            equippedReel = reel;
            OnReelEquipped?.Invoke(reel);
        }

        // --- Stat Accessors (used by fishing systems) ---

        public float GetLineStrength()
        {
            if (equippedRod == null) return 5f;
            return equippedRod.maxLineStrength;
        }

        public float GetReelSpeedMultiplier()
        {
            if (equippedReel == null) return 1f;
            return equippedReel.reelSpeed;
        }

        public float GetCastAccuracy()
        {
            if (equippedRod == null) return 0.5f;
            return equippedRod.castAccuracy;
        }

        public float GetCastPowerMultiplier()
        {
            if (equippedRod == null) return 1f;
            return equippedRod.castPower;
        }

        public float GetBarbelResistance()
        {
            float resistance = equippedRod != null ? equippedRod.barbelResistance : 0f;
            if (hasBarbelProofStand) resistance += 0.2f;
            if (hasSpecialGloves) resistance += 0.1f;
            return Mathf.Clamp01(resistance);
        }

        public float GetGripBonus()
        {
            float grip = equippedRod != null ? equippedRod.gripStrength : 0.3f;
            if (hasSpecialGloves) grip += 0.3f;
            if (hasSlimeResistantGrip) grip += 0.15f;
            return Mathf.Clamp01(grip);
        }

        public float GetSlimeResistance()
        {
            float resistance = 0f;
            if (equippedReel != null) resistance += equippedReel.slimeResistance;
            if (hasSlimeResistantGrip) resistance += 0.3f;
            return Mathf.Clamp01(resistance);
        }

        public float GetRodDropChance()
        {
            float dropChance = 0.15f;
            if (equippedRod != null)
                dropChance -= equippedRod.rodSecureChance * 0.1f;
            if (hasSpecialGloves)
                dropChance -= 0.05f;
            if (hasBarbelProofStand)
                dropChance -= 0.05f;
            return Mathf.Max(0.01f, dropChance);
        }

        /// <summary>
        /// Check if the rod snaps (budget rods snap on barbel).
        /// </summary>
        public bool CheckRodSnap(float fishStrength)
        {
            if (equippedRod == null) return true;

            float snapChance = equippedRod.snapChance * fishStrength;
            if (UnityEngine.Random.value < snapChance)
            {
                OnRodBroken?.Invoke();
                return true;
            }
            return false;
        }

        public float GetPapBucketBonus()
        {
            return hasBetterPapBucket ? 1.3f : 1f;
        }

        // --- Inventory ---

        public void AddRod(RodData rod)
        {
            if (!ownedRods.Contains(rod))
                ownedRods.Add(rod);
        }

        public void AddReel(ReelData reel)
        {
            if (!ownedReels.Contains(reel))
                ownedReels.Add(reel);
        }

        public void UnlockAccessory(string accessoryId)
        {
            switch (accessoryId)
            {
                case "specialGloves":
                    hasSpecialGloves = true;
                    break;
                case "barbelProofStand":
                    hasBarbelProofStand = true;
                    break;
                case "slimeResistantGrip":
                    hasSlimeResistantGrip = true;
                    break;
                case "betterPapBucket":
                    hasBetterPapBucket = true;
                    break;
            }
        }

        public List<RodData> GetOwnedRods() => new List<RodData>(ownedRods);
        public List<ReelData> GetOwnedReels() => new List<ReelData>(ownedReels);
    }
}
