using UnityEngine;
using Visvang.Core;
using Visvang.Equipment;

namespace Visvang.Data
{
    /// <summary>
    /// Creates all rod and reel ScriptableObject data at runtime.
    /// Follows the tier system from the spec: Entry, Mid, High, Legendary.
    /// </summary>
    public static class RodDataFactory
    {
        // --- ENTRY LEVEL RODS ---

        public static RodData CreateMakroPlasticSpecial()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Makro Plastic Special";
            rod.tier = RodTier.Entry;
            rod.description = "R49.99 from the shelf. It works... mostly.";
            rod.castPower = 0.3f;
            rod.castAccuracy = 0.3f;
            rod.sensitivity = 0.2f;
            rod.barbelResistance = 0.05f;
            rod.durability = 0.3f;
            rod.maxLineStrength = 5f;
            rod.gripStrength = 0.2f;
            rod.rodSecureChance = 0.1f;
            rod.snapChance = 0.3f;
            rod.purchasePrice = 50;
            rod.isStarterRod = true;
            rod.equipMessage = "It's not about the rod... it's about the pap. Right?";
            return rod;
        }

        public static RodData CreateBentSteelRod()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Bent Steel Rod";
            rod.tier = RodTier.Entry;
            rod.description = "Found in your oom's garage. Already bent. Still works.";
            rod.castPower = 0.35f;
            rod.castAccuracy = 0.25f;
            rod.sensitivity = 0.3f;
            rod.barbelResistance = 0.15f;
            rod.durability = 0.5f;
            rod.maxLineStrength = 7f;
            rod.gripStrength = 0.3f;
            rod.rodSecureChance = 0.15f;
            rod.snapChance = 0.15f;
            rod.purchasePrice = 80;
            rod.equipMessage = "Oom swears this rod caught a 20kg carp in '94.";
            return rod;
        }

        public static RodData CreateBudgetFibreglass()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Budget Fibreglass";
            rod.tier = RodTier.Entry;
            rod.description = "Snaps easily with barbel. Fine for carp and smaller fish.";
            rod.castPower = 0.4f;
            rod.castAccuracy = 0.35f;
            rod.sensitivity = 0.35f;
            rod.barbelResistance = 0.1f;
            rod.durability = 0.25f;
            rod.maxLineStrength = 6f;
            rod.gripStrength = 0.25f;
            rod.rodSecureChance = 0.1f;
            rod.snapChance = 0.4f; // Snaps on barbel!
            rod.purchasePrice = 100;
            rod.equipMessage = "Budget fibreglass. Pray you don't hook a barbel.";
            return rod;
        }

        // --- MID TIER RODS ---

        public static RodData CreateSensationCarpRod()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Sensation Carp Rod";
            rod.tier = RodTier.Mid;
            rod.description = "The SA angler's reliable choice. Good all-rounder.";
            rod.castPower = 0.6f;
            rod.castAccuracy = 0.6f;
            rod.sensitivity = 0.6f;
            rod.barbelResistance = 0.4f;
            rod.durability = 0.7f;
            rod.maxLineStrength = 12f;
            rod.gripStrength = 0.5f;
            rod.rodSecureChance = 0.4f;
            rod.snapChance = 0.05f;
            rod.purchasePrice = 500;
            rod.playerLevelRequired = 5;
            rod.equipMessage = "Sensation equipped. Now we're talking, bru.";
            return rod;
        }

        public static RodData CreateOkumaTournament()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Okuma Tournament";
            rod.tier = RodTier.Mid;
            rod.description = "Tournament-ready. Can handle most things the dam throws at you.";
            rod.castPower = 0.65f;
            rod.castAccuracy = 0.7f;
            rod.sensitivity = 0.65f;
            rod.barbelResistance = 0.5f;
            rod.durability = 0.75f;
            rod.maxLineStrength = 15f;
            rod.gripStrength = 0.55f;
            rod.rodSecureChance = 0.5f;
            rod.snapChance = 0.03f;
            rod.purchasePrice = 700;
            rod.playerLevelRequired = 8;
            rod.equipMessage = "Okuma Tournament. Competition-grade, dam-approved.";
            return rod;
        }

        public static RodData CreateDaiwaLongCast()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Daiwa Long Cast";
            rod.tier = RodTier.Mid;
            rod.description = "Exceptional casting range. Reach spots nobody else can.";
            rod.castPower = 0.8f;
            rod.castAccuracy = 0.65f;
            rod.sensitivity = 0.55f;
            rod.barbelResistance = 0.35f;
            rod.durability = 0.7f;
            rod.maxLineStrength = 12f;
            rod.gripStrength = 0.5f;
            rod.rodSecureChance = 0.4f;
            rod.snapChance = 0.05f;
            rod.purchasePrice = 650;
            rod.playerLevelRequired = 7;
            rod.equipMessage = "Daiwa Long Cast. Your pap is going places.";
            return rod;
        }

        public static RodData CreateShimanoAerlex()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Shimano Aerlex";
            rod.tier = RodTier.Mid;
            rod.description = "Premium Shimano. Smooth, reliable, and tough.";
            rod.castPower = 0.7f;
            rod.castAccuracy = 0.75f;
            rod.sensitivity = 0.7f;
            rod.barbelResistance = 0.55f;
            rod.durability = 0.8f;
            rod.maxLineStrength = 16f;
            rod.gripStrength = 0.6f;
            rod.rodSecureChance = 0.55f;
            rod.snapChance = 0.02f;
            rod.purchasePrice = 900;
            rod.playerLevelRequired = 10;
            rod.equipMessage = "Shimano Aerlex. Japanese engineering meets SA dams.";
            return rod;
        }

        // --- HIGH TIER RODS ---

        public static RodData CreateBarbelSpecialist()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Barbel Specialist Power Rod";
            rod.tier = RodTier.High;
            rod.description = "Built specifically for barbel. This rod fights back.";
            rod.castPower = 0.75f;
            rod.castAccuracy = 0.7f;
            rod.sensitivity = 0.8f;
            rod.barbelResistance = 0.9f;
            rod.durability = 0.9f;
            rod.maxLineStrength = 25f;
            rod.lineBreakResistance = 1.5f;
            rod.gripStrength = 0.8f;
            rod.rodSecureChance = 0.8f;
            rod.isBarbelSpecialist = true;
            rod.snapChance = 0.005f;
            rod.purchasePrice = 2000;
            rod.playerLevelRequired = 20;
            rod.equipMessage = "Barbel Specialist. Bring it on, barbers.";
            return rod;
        }

        public static RodData CreateCompetitionEuropean()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Competition European Carp Gear";
            rod.tier = RodTier.High;
            rod.description = "Imported European competition setup. The real deal.";
            rod.castPower = 0.85f;
            rod.castAccuracy = 0.9f;
            rod.sensitivity = 0.85f;
            rod.barbelResistance = 0.6f;
            rod.durability = 0.85f;
            rod.maxLineStrength = 20f;
            rod.gripStrength = 0.7f;
            rod.rodSecureChance = 0.7f;
            rod.snapChance = 0.01f;
            rod.purchasePrice = 3000;
            rod.playerLevelRequired = 25;
            rod.equipMessage = "European gear. Your boytjies won't believe this setup.";
            return rod;
        }

        // --- LEGENDARY ROD ---

        public static RodData CreateOomFriksRod()
        {
            var rod = ScriptableObject.CreateInstance<RodData>();
            rod.rodName = "Oom Frik's Handcrafted Monster Rod";
            rod.tier = RodTier.Legendary;
            rod.description = "Handcrafted by the legendary Oom Frik himself. There's nothing this rod can't handle.";
            rod.castPower = 0.95f;
            rod.castAccuracy = 0.95f;
            rod.sensitivity = 0.95f;
            rod.barbelResistance = 0.98f;
            rod.durability = 0.99f;
            rod.maxLineStrength = 40f;
            rod.lineBreakResistance = 2f;
            rod.gripStrength = 0.95f;
            rod.rodSecureChance = 0.95f;
            rod.isBarbelSpecialist = true;
            rod.hasAntiSlimeGrip = true;
            rod.snapChance = 0.001f;
            rod.purchasePrice = 10000;
            rod.playerLevelRequired = 40;
            rod.equipMessage = "Oom Frik's Monster Rod. The dam trembles.";
            return rod;
        }

        // --- REELS ---

        public static ReelData CreateBudgetReel()
        {
            var reel = ScriptableObject.CreateInstance<ReelData>();
            reel.reelName = "Budget Plastic Reel";
            reel.tier = RodTier.Entry;
            reel.description = "Gets the job done. Slowly.";
            reel.reelSpeed = 0.7f;
            reel.dragStrength = 0.3f;
            reel.lineCapacity = 0.5f;
            reel.smoothness = 0.3f;
            reel.purchasePrice = 40;
            reel.isStarterReel = true;
            return reel;
        }

        public static ReelData CreateSensationReel()
        {
            var reel = ScriptableObject.CreateInstance<ReelData>();
            reel.reelName = "Sensation Tournament Reel";
            reel.tier = RodTier.Mid;
            reel.description = "Smooth drag, reliable performance.";
            reel.reelSpeed = 1.2f;
            reel.dragStrength = 0.6f;
            reel.lineCapacity = 0.7f;
            reel.smoothness = 0.7f;
            reel.slimeResistance = 0.3f;
            reel.purchasePrice = 400;
            reel.playerLevelRequired = 5;
            return reel;
        }

        public static ReelData CreateShimanoReel()
        {
            var reel = ScriptableObject.CreateInstance<ReelData>();
            reel.reelName = "Shimano Baitrunner";
            reel.tier = RodTier.High;
            reel.description = "Premium baitrunner. Handles anything.";
            reel.reelSpeed = 1.5f;
            reel.dragStrength = 0.85f;
            reel.lineCapacity = 0.9f;
            reel.smoothness = 0.9f;
            reel.slimeResistance = 0.6f;
            reel.purchasePrice = 1500;
            reel.playerLevelRequired = 15;
            return reel;
        }
    }
}
