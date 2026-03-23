using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Visvang.Core;
using Visvang.Progression;
using Visvang.Equipment;
using Visvang.Bait;
using Visvang.Fish;
using Visvang.Data;

namespace Visvang.Save
{
    /// <summary>
    /// Bridges SaveData ↔ runtime managers.
    /// Reads save data into all managers on load, and collects state from all managers on save.
    /// Equipment and dips are referenced by name and resolved via the runtime-loaded data factories.
    /// </summary>
    public static class SaveBridge
    {
        /// <summary>
        /// Push save data into all runtime managers. Call after RuntimeDataLoader.LoadAll().
        /// </summary>
        public static void RestoreFromSave(SaveData save)
        {
            if (save == null) return;

            RestorePlayer(save);
            RestoreEquipment(save);
            RestoreInventory(save);
            RestoreProgress(save);
            RestoreStatistics(save);

            Debug.Log("[SaveBridge] Game state restored from save.");
        }

        /// <summary>
        /// Collect current state from all managers into SaveData. Call before SaveManager.Save().
        /// </summary>
        public static void CollectToSave(SaveData save)
        {
            if (save == null) return;

            CollectPlayer(save);
            CollectEquipment(save);
            CollectInventory(save);
            CollectProgress(save);
            // Statistics are updated incrementally by SessionTracker, no batch collection needed
        }

        // ===== RESTORE (SaveData → Managers) =====

        private static void RestorePlayer(SaveData save)
        {
            var p = PlayerProfile.Instance;
            if (p == null) return;

            p.RestoreFromSave(
                save.player.playerName,
                save.player.playerLevel,
                save.player.totalXP,
                save.player.currency
            );
        }

        private static void RestoreEquipment(SaveData save)
        {
            var em = EquipmentManager.Instance;
            if (em == null) return;

            // Restore owned rods by name lookup
            foreach (string rodName in save.equipment.ownedRodNames)
            {
                var rod = FindRodByName(rodName);
                if (rod != null) em.AddRod(rod);
            }

            // Restore owned reels
            foreach (string reelName in save.equipment.ownedReelNames)
            {
                var reel = FindReelByName(reelName);
                if (reel != null) em.AddReel(reel);
            }

            // Equip saved loadout
            if (!string.IsNullOrEmpty(save.equipment.equippedRodName))
            {
                var rod = FindRodByName(save.equipment.equippedRodName);
                if (rod != null) em.EquipRod(rod);
            }
            if (!string.IsNullOrEmpty(save.equipment.equippedReelName))
            {
                var reel = FindReelByName(save.equipment.equippedReelName);
                if (reel != null) em.EquipReel(reel);
            }

            // Accessories
            if (save.equipment.hasSpecialGloves) em.UnlockAccessory("specialGloves");
            if (save.equipment.hasBarbelProofStand) em.UnlockAccessory("barbelProofStand");
            if (save.equipment.hasSlimeResistantGrip) em.UnlockAccessory("slimeResistantGrip");
            if (save.equipment.hasBetterPapBucket) em.UnlockAccessory("betterPapBucket");
        }

        private static void RestoreInventory(SaveData save)
        {
            var bm = BaitManager.Instance;
            if (bm == null) return;

            foreach (var dipEntry in save.inventory.dips)
            {
                var dip = FindDipByName(dipEntry.dipName);
                if (dip != null)
                    bm.PurchaseDip(dip, dipEntry.quantity);
            }

            foreach (var baitEntry in save.inventory.baits)
            {
                var bait = FindBaitByName(baitEntry.baitName);
                if (bait != null)
                    bm.PurchaseBait(bait, baitEntry.quantity);
            }
        }

        private static void RestoreProgress(SaveData save)
        {
            var us = UpgradeSystem.Instance;
            if (us != null)
            {
                us.RestoreUnlocks(save.progress.unlockedUpgradeIds);
            }

            // Caught species restored into PlayerProfile
            var pp = PlayerProfile.Instance;
            if (pp != null)
            {
                foreach (int speciesId in save.progress.caughtSpeciesIds)
                    pp.MarkSpeciesCaught((FishSpecies)speciesId);
            }
        }

        private static void RestoreStatistics(SaveData save)
        {
            var p = PlayerProfile.Instance;
            if (p == null) return;

            p.RestoreStatistics(
                save.statistics.totalFishCaught,
                save.statistics.totalFishLost,
                save.statistics.totalCarpCaught,
                save.statistics.totalBarbelCaught,
                save.statistics.totalMudfishCaught,
                save.statistics.heaviestFishWeight,
                save.statistics.heaviestFishName ?? "",
                save.statistics.longestFightSeconds,
                save.statistics.rodsLostToBarbers,
                save.statistics.timesSlapped
            );
        }

        // ===== COLLECT (Managers → SaveData) =====

        private static void CollectPlayer(SaveData save)
        {
            var p = PlayerProfile.Instance;
            if (p == null) return;

            save.player.playerName = p.PlayerName;
            save.player.playerLevel = p.PlayerLevel;
            save.player.totalXP = p.TotalXP;
            save.player.currency = p.Currency;
        }

        private static void CollectEquipment(SaveData save)
        {
            var em = EquipmentManager.Instance;
            if (em == null) return;

            save.equipment.equippedRodName = em.EquippedRod != null ? em.EquippedRod.rodName : "";
            save.equipment.equippedReelName = em.EquippedReel != null ? em.EquippedReel.reelName : "";

            save.equipment.ownedRodNames.Clear();
            foreach (var rod in em.GetOwnedRods())
                save.equipment.ownedRodNames.Add(rod.rodName);

            save.equipment.ownedReelNames.Clear();
            foreach (var reel in em.GetOwnedReels())
                save.equipment.ownedReelNames.Add(reel.reelName);

            save.equipment.hasSpecialGloves = em.HasSpecialGloves;
            save.equipment.hasBarbelProofStand = em.HasBarbelProofStand;
            save.equipment.hasSlimeResistantGrip = em.HasSlimeResistantGrip;
            save.equipment.hasBetterPapBucket = em.HasBetterPapBucket;
        }

        private static void CollectInventory(SaveData save)
        {
            var bm = BaitManager.Instance;
            if (bm == null) return;

            save.inventory.dips.Clear();
            foreach (var item in bm.DipInventory)
            {
                save.inventory.dips.Add(new DipInventoryEntry
                {
                    dipName = item.dip.dipName,
                    quantity = item.quantity
                });
            }

            save.inventory.baits.Clear();
            foreach (var item in bm.BaitInventory)
            {
                save.inventory.baits.Add(new BaitInventoryEntry
                {
                    baitName = item.bait.baitName,
                    quantity = item.quantity
                });
            }
        }

        private static void CollectProgress(SaveData save)
        {
            var us = UpgradeSystem.Instance;
            if (us != null)
                save.progress.unlockedUpgradeIds = us.GetUnlockedIds();

            // Caught species collected incrementally by SessionTracker.LogCatch
        }

        // ===== NAME LOOKUPS =====
        // Resolves saved equipment/dip names to runtime ScriptableObject instances

        private static RodData FindRodByName(string name)
        {
            var em = EquipmentManager.Instance;
            if (em == null) return null;
            return em.GetOwnedRods().FirstOrDefault(r => r.rodName == name);
        }

        private static ReelData FindReelByName(string name)
        {
            var em = EquipmentManager.Instance;
            if (em == null) return null;
            return em.GetOwnedReels().FirstOrDefault(r => r.reelName == name);
        }

        private static DipData FindDipByName(string name)
        {
            var bm = BaitManager.Instance;
            if (bm == null) return null;
            return bm.AllDips.FirstOrDefault(d => d.dipName == name);
        }

        private static BaitData FindBaitByName(string name)
        {
            // Baits are not yet loaded in data factory; return null for now
            return null;
        }
    }
}
