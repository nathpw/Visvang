using UnityEngine;
using Visvang.Core;

namespace Visvang.Fish
{
    [CreateAssetMenu(fileName = "NewFish", menuName = "Visvang/Fish Data")]
    public class FishData : ScriptableObject
    {
        [Header("Identity")]
        public string fishName;
        public FishSpecies species;
        public FishRarity rarity;
        [TextArea(2, 4)] public string description;
        public Sprite fishSprite;
        public Sprite fishPhoto;

        [Header("Weight & Size")]
        public float minWeight = 0.5f;
        public float maxWeight = 5f;
        public float minLength = 20f;
        public float maxLength = 60f;

        [Header("Behaviour")]
        [Range(0f, 1f)] public float biteAggressiveness = 0.5f;
        [Range(0f, 1f)] public float fightStrength = 0.5f;
        [Range(0f, 1f)] public float hookShakeChance = 0.1f;
        [Range(0f, 1f)] public float lineBreakMultiplier = 1f;
        public float fightDurationBase = 10f;
        public float tensionBuildRate = 5f;

        [Header("Bite Patterns")]
        public float biteFrequency = 0.1f;
        public float biteWindow = 1.5f;
        public bool violentTaker;
        public bool canPullRodIn;
        public bool canDestroyPap;
        public bool hasDeathRoll;
        public bool causesSlime;

        [Header("Spawn Conditions")]
        public TimeOfDay[] preferredTimes;
        public Weather[] preferredWeather;
        [Range(0f, 1f)] public float spawnChance = 0.3f;
        public float minDepth = 1f;
        public float maxDepth = 10f;

        [Header("XP & Rewards")]
        public int baseXP = 50;
        public int bonusXPFirstCatch = 100;

        [Header("Dip Attraction")]
        [Range(-1f, 1f)] public float garlicAttraction;
        [Range(-1f, 1f)] public float sweetcornAttraction;
        [Range(-1f, 1f)] public float vanillaAttraction;
        [Range(-1f, 1f)] public float bloodwormAttraction;
        [Range(-1f, 1f)] public float blackMagicAttraction;

        [Header("Sound")]
        public AudioClip biteSound;
        public AudioClip fightSound;
        public AudioClip catchSound;

        public float RollWeight()
        {
            return Random.Range(minWeight, maxWeight);
        }

        public float RollLength()
        {
            return Random.Range(minLength, maxLength);
        }

        public bool IsLegendary()
        {
            return rarity == FishRarity.Legendary;
        }

        public bool IsCarp()
        {
            return species == FishSpecies.CommonCarp ||
                   species == FishSpecies.MirrorCarp ||
                   species == FishSpecies.LeatherCarp ||
                   species == FishSpecies.GhostCarp ||
                   species == FishSpecies.WildSmallCarp ||
                   species == FishSpecies.BoknesGoldenCarp;
        }

        public bool IsBarbel()
        {
            return species == FishSpecies.Barbel ||
                   species == FishSpecies.FlatNoseRiverBarber;
        }

        public bool IsMudfish()
        {
            return species == FishSpecies.Mudfish;
        }
    }
}
