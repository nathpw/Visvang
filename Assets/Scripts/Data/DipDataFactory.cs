using UnityEngine;
using Visvang.Core;
using Visvang.Bait;

namespace Visvang.Data
{
    /// <summary>
    /// Creates all dip ScriptableObject data at runtime.
    /// Maps directly to the spec's dip categories.
    /// </summary>
    public static class DipDataFactory
    {
        // --- BARBEL ATTRACTORS ---

        public static DipData CreateGarlic()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Garlic";
            dip.category = DipCategory.BarbelAttractor;
            dip.description = "Strong garlic dip. Barbel can't resist it, but watch out...";
            dip.garlicFactor = 1.5f;
            dip.barbelAttractionBonus = 0.8f;
            dip.mudfishAttractionBonus = -0.2f;
            dip.carpAttractionBonus = 0.3f;
            dip.biteFrequencyBonus = 0.1f;
            dip.purchasePrice = 50;
            dip.mixingMessage = "The garlic smell fills the air. Something big is coming...";
            dip.dipColor = new Color(0.9f, 0.85f, 0.6f);
            return dip;
        }

        public static DipData CreateBloodworm()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Bloodworm";
            dip.category = DipCategory.BarbelAttractor;
            dip.description = "Potent bloodworm extract. The barbers love this stuff.";
            dip.bloodwormFactor = 1.8f;
            dip.barbelAttractionBonus = 1f;
            dip.mudfishAttractionBonus = 0.1f;
            dip.carpAttractionBonus = 0.2f;
            dip.biteFrequencyBonus = 0.15f;
            dip.purchasePrice = 80;
            dip.playerLevelRequired = 5;
            dip.mixingMessage = "Mixing bloodworm into your pap. It's getting primal out here.";
            dip.dipColor = new Color(0.6f, 0.1f, 0.1f);
            return dip;
        }

        public static DipData CreateDevilsFork()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Devil's Fork";
            dip.category = DipCategory.BarbelAttractor;
            dip.description = "The strongest barbel dip known to man. Use at your own risk.";
            dip.garlicFactor = 1f;
            dip.bloodwormFactor = 1.5f;
            dip.barbelAttractionBonus = 1.5f;
            dip.mudfishAttractionBonus = -0.5f;
            dip.carpAttractionBonus = 0.5f;
            dip.biteFrequencyBonus = 0.2f;
            dip.purchasePrice = 200;
            dip.playerLevelRequired = 15;
            dip.mixingMessage = "Devil's Fork mixed in. Whatever comes, you asked for it.";
            dip.dipColor = new Color(0.3f, 0f, 0f);
            return dip;
        }

        public static DipData CreateFX()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "FX";
            dip.category = DipCategory.AllRounder;
            dip.description = "All-purpose FX dip. Sometimes attracts barbel. Sometimes doesn't.";
            dip.garlicFactor = 0.5f;
            dip.sweetcornFactor = 0.5f;
            dip.barbelAttractionBonus = 0.3f;
            dip.mudfishAttractionBonus = 0.1f;
            dip.carpAttractionBonus = 0.5f;
            dip.biteFrequencyBonus = 0.1f;
            dip.purchasePrice = 60;
            dip.isStarterDip = true;
            dip.mixingMessage = "FX goes in. Let's see what the dam delivers.";
            dip.dipColor = new Color(0.4f, 0.6f, 0.2f);
            return dip;
        }

        // --- MUDFISH ATTRACTORS ---

        public static DipData CreateCheapSweetcorn()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Cheap Sweetcorn";
            dip.category = DipCategory.MudfishAttractor;
            dip.description = "Budget sweetcorn dip. Mudfish magnet. You've been warned.";
            dip.sweetcornFactor = 1.5f;
            dip.mudfishAttractionBonus = 0.8f;
            dip.carpAttractionBonus = 0.2f;
            dip.biteFrequencyBonus = 0.2f;
            dip.purchasePrice = 15;
            dip.isStarterDip = true;
            dip.mixingMessage = "Cheap sweetcorn in the pap. The mudfish are already circling.";
            dip.dipColor = new Color(1f, 0.9f, 0.3f);
            return dip;
        }

        public static DipData CreateVanilla()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Vanilla";
            dip.category = DipCategory.MudfishAttractor;
            dip.description = "Sweet vanilla dip. Good for beginners but attracts mudfish.";
            dip.vanillaFactor = 1.5f;
            dip.mudfishAttractionBonus = 0.6f;
            dip.carpAttractionBonus = 0.4f;
            dip.biteFrequencyBonus = 0.15f;
            dip.purchasePrice = 25;
            dip.isStarterDip = true;
            dip.mixingMessage = "Vanilla pap ready. Smells like a bakery at the dam.";
            dip.dipColor = new Color(1f, 0.95f, 0.8f);
            return dip;
        }

        public static DipData CreateBanjo()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Banjo";
            dip.category = DipCategory.MudfishAttractor;
            dip.description = "Classic Banjo flavour. Brings all the mudfish to the yard.";
            dip.sweetcornFactor = 0.8f;
            dip.vanillaFactor = 0.5f;
            dip.mudfishAttractionBonus = 0.7f;
            dip.carpAttractionBonus = 0.3f;
            dip.biteFrequencyBonus = 0.15f;
            dip.purchasePrice = 20;
            dip.mixingMessage = "Banjo in the mix. Authentic papgooi vibes.";
            dip.dipColor = new Color(0.9f, 0.7f, 0.3f);
            return dip;
        }

        public static DipData CreatePinkSweets()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Pink Sweets";
            dip.category = DipCategory.MudfishAttractor;
            dip.description = "Crushed pink sweets. The mudfish can't help themselves.";
            dip.sweetcornFactor = 0.3f;
            dip.vanillaFactor = 1f;
            dip.mudfishAttractionBonus = 0.9f;
            dip.carpAttractionBonus = 0.1f;
            dip.biteFrequencyBonus = 0.2f;
            dip.purchasePrice = 10;
            dip.mixingMessage = "Pink sweets mixed in. This is chaos bait, my bru.";
            dip.dipColor = new Color(1f, 0.5f, 0.7f);
            return dip;
        }

        // --- MUDFISH REPELLENTS ---

        public static DipData CreateBlackMagic()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Black Magic";
            dip.category = DipCategory.MudfishRepellent;
            dip.description = "Premium competition dip. Repels mudfish, attracts carp.";
            dip.blackMagicFactor = 2f;
            dip.mudfishAttractionBonus = -0.7f;
            dip.carpAttractionBonus = 0.8f;
            dip.biteFrequencyBonus = 0.05f;
            dip.papDurabilityBonus = 0.2f;
            dip.purchasePrice = 150;
            dip.playerLevelRequired = 10;
            dip.mixingMessage = "Black Magic applied. The mudfish will think twice now.";
            dip.dipColor = new Color(0.1f, 0.1f, 0.15f);
            return dip;
        }

        public static DipData CreateSyntheticAttractor()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Synthetic Attractor";
            dip.category = DipCategory.MudfishRepellent;
            dip.description = "Lab-made precision dip. Targets carp specifically.";
            dip.blackMagicFactor = 1f;
            dip.mudfishAttractionBonus = -0.5f;
            dip.carpAttractionBonus = 1f;
            dip.biteFrequencyBonus = 0.1f;
            dip.papDurabilityBonus = 0.15f;
            dip.purchasePrice = 120;
            dip.playerLevelRequired = 8;
            dip.mixingMessage = "Synthetic mix deployed. Science meets papgooi.";
            dip.dipColor = new Color(0.3f, 0.8f, 0.9f);
            return dip;
        }

        public static DipData CreateCompetitionPremium()
        {
            var dip = ScriptableObject.CreateInstance<DipData>();
            dip.dipName = "Competition Premium";
            dip.category = DipCategory.CarpSpecialist;
            dip.description = "European-grade competition dip. The best money can buy.";
            dip.garlicFactor = 0.5f;
            dip.blackMagicFactor = 1.5f;
            dip.mudfishAttractionBonus = -0.8f;
            dip.carpAttractionBonus = 1.2f;
            dip.barbelAttractionBonus = 0.2f;
            dip.biteFrequencyBonus = 0.15f;
            dip.papDurabilityBonus = 0.3f;
            dip.papConsumptionMultiplier = 0.8f;
            dip.purchasePrice = 300;
            dip.playerLevelRequired = 20;
            dip.mixingMessage = "Competition grade pap. This is tournament level.";
            dip.dipColor = new Color(0.8f, 0.6f, 0.1f);
            return dip;
        }
    }
}
