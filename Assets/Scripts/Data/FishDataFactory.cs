using UnityEngine;
using Visvang.Core;
using Visvang.Fish;

namespace Visvang.Data
{
    /// <summary>
    /// Creates all fish ScriptableObject data at runtime if not loaded from assets.
    /// Use this as a fallback or for testing. In production, create these as .asset files.
    /// Call Initialize() from GameBootstrap or an editor script.
    /// </summary>
    public static class FishDataFactory
    {
        public static FishData CreateCommonCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Common Carp";
            fish.species = FishSpecies.CommonCarp;
            fish.rarity = FishRarity.Common;
            fish.description = "The bread and butter of SA papgooiing. A reliable, heavy taker.";
            fish.minWeight = 1f; fish.maxWeight = 12f;
            fish.minLength = 30f; fish.maxLength = 80f;
            fish.biteAggressiveness = 0.4f;
            fish.fightStrength = 0.5f;
            fish.hookShakeChance = 0.05f;
            fish.lineBreakMultiplier = 1f;
            fish.fightDurationBase = 15f;
            fish.tensionBuildRate = 4f;
            fish.biteFrequency = 0.15f;
            fish.biteWindow = 1.5f;
            fish.spawnChance = 0.3f;
            fish.baseXP = Constants.XP_CARP_BASE;
            fish.bonusXPFirstCatch = 50;
            fish.preferredTimes = new[] { TimeOfDay.Morning, TimeOfDay.EarlyMorning, TimeOfDay.Evening };
            fish.garlicAttraction = 0.3f;
            fish.sweetcornAttraction = 0.5f;
            fish.vanillaAttraction = 0.4f;
            return fish;
        }

        public static FishData CreateMirrorCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Mirror Carp";
            fish.species = FishSpecies.MirrorCarp;
            fish.rarity = FishRarity.Uncommon;
            fish.description = "Distinctive scaled beauty. Fights a bit harder than common carp.";
            fish.minWeight = 2f; fish.maxWeight = 15f;
            fish.minLength = 35f; fish.maxLength = 90f;
            fish.biteAggressiveness = 0.45f;
            fish.fightStrength = 0.55f;
            fish.hookShakeChance = 0.06f;
            fish.fightDurationBase = 18f;
            fish.tensionBuildRate = 5f;
            fish.biteFrequency = 0.1f;
            fish.spawnChance = 0.2f;
            fish.baseXP = 65;
            fish.bonusXPFirstCatch = 75;
            fish.preferredTimes = new[] { TimeOfDay.Morning, TimeOfDay.Afternoon };
            fish.garlicAttraction = 0.4f;
            fish.sweetcornAttraction = 0.3f;
            return fish;
        }

        public static FishData CreateLeatherCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Leather Carp";
            fish.species = FishSpecies.LeatherCarp;
            fish.rarity = FishRarity.Rare;
            fish.description = "Smooth-skinned and rare. A prized catch among SA anglers.";
            fish.minWeight = 3f; fish.maxWeight = 18f;
            fish.minLength = 40f; fish.maxLength = 95f;
            fish.biteAggressiveness = 0.35f;
            fish.fightStrength = 0.6f;
            fish.hookShakeChance = 0.04f;
            fish.fightDurationBase = 20f;
            fish.tensionBuildRate = 5f;
            fish.biteFrequency = 0.07f;
            fish.spawnChance = 0.1f;
            fish.baseXP = 80;
            fish.bonusXPFirstCatch = 100;
            fish.preferredTimes = new[] { TimeOfDay.EarlyMorning, TimeOfDay.Evening };
            fish.blackMagicAttraction = 0.5f;
            return fish;
        }

        public static FishData CreateGhostCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Ghost Carp";
            fish.species = FishSpecies.GhostCarp;
            fish.rarity = FishRarity.Rare;
            fish.description = "Pale and elusive. You'll feel lucky to land one.";
            fish.minWeight = 2f; fish.maxWeight = 14f;
            fish.minLength = 35f; fish.maxLength = 85f;
            fish.biteAggressiveness = 0.3f;
            fish.fightStrength = 0.5f;
            fish.hookShakeChance = 0.08f;
            fish.fightDurationBase = 16f;
            fish.tensionBuildRate = 4f;
            fish.biteFrequency = 0.06f;
            fish.spawnChance = 0.08f;
            fish.baseXP = 90;
            fish.bonusXPFirstCatch = 120;
            fish.preferredTimes = new[] { TimeOfDay.Night, TimeOfDay.EarlyMorning };
            fish.blackMagicAttraction = 0.6f;
            return fish;
        }

        public static FishData CreateWildSmallCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Wild Small Carp";
            fish.species = FishSpecies.WildSmallCarp;
            fish.rarity = FishRarity.Common;
            fish.description = "Small but feisty. Common in most SA dams.";
            fish.minWeight = 0.3f; fish.maxWeight = 2f;
            fish.minLength = 15f; fish.maxLength = 35f;
            fish.biteAggressiveness = 0.6f;
            fish.fightStrength = 0.2f;
            fish.hookShakeChance = 0.1f;
            fish.fightDurationBase = 5f;
            fish.tensionBuildRate = 2f;
            fish.biteFrequency = 0.25f;
            fish.spawnChance = 0.35f;
            fish.baseXP = 20;
            fish.bonusXPFirstCatch = 25;
            fish.sweetcornAttraction = 0.7f;
            fish.vanillaAttraction = 0.5f;
            return fish;
        }

        public static FishData CreateBoknesGoldenCarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Boknes Golden Carp";
            fish.species = FishSpecies.BoknesGoldenCarp;
            fish.rarity = FishRarity.Legendary;
            fish.description = "The legendary golden carp of Boknes. Many have tried. Few have succeeded.";
            fish.minWeight = 10f; fish.maxWeight = 30f;
            fish.minLength = 70f; fish.maxLength = 120f;
            fish.biteAggressiveness = 0.2f;
            fish.fightStrength = 0.9f;
            fish.hookShakeChance = 0.02f;
            fish.lineBreakMultiplier = 1.5f;
            fish.fightDurationBase = 45f;
            fish.tensionBuildRate = 8f;
            fish.biteFrequency = 0.01f;
            fish.biteWindow = 0.8f;
            fish.spawnChance = 0.005f;
            fish.baseXP = 500;
            fish.bonusXPFirstCatch = 1000;
            fish.preferredTimes = new[] { TimeOfDay.EarlyMorning };
            fish.preferredWeather = new[] { Weather.Cloudy };
            fish.blackMagicAttraction = 1f;
            fish.garlicAttraction = 0.5f;
            return fish;
        }

        public static FishData CreateBarbel()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Barbel";
            fish.species = FishSpecies.Barbel;
            fish.rarity = FishRarity.Uncommon;
            fish.description = "Massive, powerful, unpredictable. The barber gives everyone a hiding.";
            fish.minWeight = 3f; fish.maxWeight = 25f;
            fish.minLength = 40f; fish.maxLength = 120f;
            fish.biteAggressiveness = 0.8f;
            fish.fightStrength = 0.85f;
            fish.hookShakeChance = 0.03f;
            fish.lineBreakMultiplier = 1.8f;
            fish.fightDurationBase = 30f;
            fish.tensionBuildRate = 10f;
            fish.biteFrequency = 0.08f;
            fish.biteWindow = 0.5f;
            fish.violentTaker = true;
            fish.canPullRodIn = true;
            fish.canDestroyPap = true;
            fish.hasDeathRoll = true;
            fish.spawnChance = 0.12f;
            fish.baseXP = Constants.XP_BARBEL_BASE;
            fish.bonusXPFirstCatch = 300;
            fish.preferredTimes = new[] { TimeOfDay.Night, TimeOfDay.Evening };
            fish.garlicAttraction = 0.8f;
            fish.bloodwormAttraction = 0.9f;
            fish.sweetcornAttraction = -0.3f;
            return fish;
        }

        public static FishData CreateFlatNoseRiverBarber()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Flat-nose River Barber";
            fish.species = FishSpecies.FlatNoseRiverBarber;
            fish.rarity = FishRarity.Legendary;
            fish.description = "The legendary flat-nose. They said it was a myth.";
            fish.minWeight = 15f; fish.maxWeight = 50f;
            fish.minLength = 80f; fish.maxLength = 150f;
            fish.biteAggressiveness = 0.95f;
            fish.fightStrength = 0.98f;
            fish.hookShakeChance = 0.01f;
            fish.lineBreakMultiplier = 2.5f;
            fish.fightDurationBase = 60f;
            fish.tensionBuildRate = 15f;
            fish.biteFrequency = 0.005f;
            fish.biteWindow = 0.3f;
            fish.violentTaker = true;
            fish.canPullRodIn = true;
            fish.canDestroyPap = true;
            fish.hasDeathRoll = true;
            fish.spawnChance = 0.003f;
            fish.baseXP = 1000;
            fish.bonusXPFirstCatch = 2000;
            fish.preferredTimes = new[] { TimeOfDay.Night };
            fish.preferredWeather = new[] { Weather.Stormy };
            fish.garlicAttraction = 1f;
            fish.bloodwormAttraction = 1f;
            return fish;
        }

        public static FishData CreateMudfish()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Mudfish";
            fish.species = FishSpecies.Mudfish;
            fish.rarity = FishRarity.Common;
            fish.description = "Slimy nuisance. Extremely South African.";
            fish.minWeight = 0.2f; fish.maxWeight = 1.5f;
            fish.minLength = 10f; fish.maxLength = 30f;
            fish.biteAggressiveness = 0.9f;
            fish.fightStrength = 0.15f;
            fish.hookShakeChance = 0.2f;
            fish.fightDurationBase = 4f;
            fish.tensionBuildRate = 1f;
            fish.biteFrequency = 0.35f;
            fish.biteWindow = 2f;
            fish.causesSlime = true;
            fish.spawnChance = 0.3f;
            fish.baseXP = Constants.XP_MUDFISH_BASE;
            fish.bonusXPFirstCatch = 15;
            fish.sweetcornAttraction = 0.8f;
            fish.vanillaAttraction = 0.7f;
            fish.blackMagicAttraction = -0.5f;
            return fish;
        }

        public static FishData CreateKurper()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Kurper";
            fish.species = FishSpecies.Kurper;
            fish.rarity = FishRarity.Common;
            fish.description = "Small panfish. Quick bites, easy fights.";
            fish.minWeight = 0.1f; fish.maxWeight = 1f;
            fish.minLength = 8f; fish.maxLength = 25f;
            fish.biteAggressiveness = 0.7f;
            fish.fightStrength = 0.1f;
            fish.hookShakeChance = 0.15f;
            fish.fightDurationBase = 3f;
            fish.biteFrequency = 0.3f;
            fish.spawnChance = 0.25f;
            fish.baseXP = Constants.XP_KURPER_BASE;
            return fish;
        }

        public static FishData CreateTilapia()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Tilapia";
            fish.species = FishSpecies.Tilapia;
            fish.rarity = FishRarity.Common;
            fish.description = "Abundant and reliable. Puts up a decent scrap.";
            fish.minWeight = 0.3f; fish.maxWeight = 3f;
            fish.minLength = 15f; fish.maxLength = 40f;
            fish.biteAggressiveness = 0.5f;
            fish.fightStrength = 0.25f;
            fish.fightDurationBase = 6f;
            fish.biteFrequency = 0.2f;
            fish.spawnChance = 0.2f;
            fish.baseXP = Constants.XP_TILAPIA_BASE;
            return fish;
        }

        public static FishData CreateBass()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Bass";
            fish.species = FishSpecies.Bass;
            fish.rarity = FishRarity.Uncommon;
            fish.description = "Aggressive fighter with spectacular jumps.";
            fish.minWeight = 0.5f; fish.maxWeight = 5f;
            fish.minLength = 20f; fish.maxLength = 55f;
            fish.biteAggressiveness = 0.7f;
            fish.fightStrength = 0.55f;
            fish.hookShakeChance = 0.12f;
            fish.fightDurationBase = 12f;
            fish.biteFrequency = 0.12f;
            fish.spawnChance = 0.15f;
            fish.baseXP = Constants.XP_BASS_BASE;
            return fish;
        }

        public static FishData CreateGraskarp()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Graskarp";
            fish.species = FishSpecies.Graskarp;
            fish.rarity = FishRarity.Uncommon;
            fish.description = "Grass carp. Big, slow, and powerful.";
            fish.minWeight = 2f; fish.maxWeight = 20f;
            fish.minLength = 40f; fish.maxLength = 100f;
            fish.biteAggressiveness = 0.25f;
            fish.fightStrength = 0.65f;
            fish.fightDurationBase = 25f;
            fish.tensionBuildRate = 6f;
            fish.biteFrequency = 0.05f;
            fish.spawnChance = 0.08f;
            fish.baseXP = Constants.XP_GRASKARP_BASE;
            return fish;
        }

        public static FishData CreateYellowfish()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Yellowfish";
            fish.species = FishSpecies.Yellowfish;
            fish.rarity = FishRarity.Rare;
            fish.description = "SA's golden fighter. Fast, strong, and beautiful.";
            fish.minWeight = 1f; fish.maxWeight = 8f;
            fish.minLength = 25f; fish.maxLength = 70f;
            fish.biteAggressiveness = 0.6f;
            fish.fightStrength = 0.7f;
            fish.hookShakeChance = 0.08f;
            fish.fightDurationBase = 18f;
            fish.tensionBuildRate = 7f;
            fish.biteFrequency = 0.06f;
            fish.spawnChance = 0.06f;
            fish.baseXP = Constants.XP_YELLOWFISH_BASE;
            fish.preferredTimes = new[] { TimeOfDay.Morning, TimeOfDay.Afternoon };
            return fish;
        }

        public static FishData CreateEel()
        {
            var fish = ScriptableObject.CreateInstance<FishData>();
            fish.fishName = "Eel";
            fish.species = FishSpecies.Eel;
            fish.rarity = FishRarity.Rare;
            fish.description = "Rare, annoying, and slimy. Nobody asked for this.";
            fish.minWeight = 0.5f; fish.maxWeight = 3f;
            fish.minLength = 30f; fish.maxLength = 80f;
            fish.biteAggressiveness = 0.3f;
            fish.fightStrength = 0.3f;
            fish.hookShakeChance = 0.25f;
            fish.fightDurationBase = 8f;
            fish.causesSlime = true;
            fish.biteFrequency = 0.03f;
            fish.spawnChance = 0.04f;
            fish.baseXP = Constants.XP_EEL_BASE;
            fish.preferredTimes = new[] { TimeOfDay.Night };
            return fish;
        }
    }
}
