using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Visvang.Core;
using Visvang.Bait;

namespace Visvang.Fish
{
    public class FishDatabase : MonoBehaviour
    {
        public static FishDatabase Instance { get; private set; }

        [SerializeField] private List<FishData> allFish = new List<FishData>();

        public void RegisterFish(FishData fish)
        {
            if (!allFish.Contains(fish))
                allFish.Add(fish);
        }

        public int FishCount => allFish.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public FishData GetFishBySpecies(FishSpecies species)
        {
            return allFish.FirstOrDefault(f => f.species == species);
        }

        public List<FishData> GetFishByRarity(FishRarity rarity)
        {
            return allFish.Where(f => f.rarity == rarity).ToList();
        }

        public List<FishData> GetAllCarp()
        {
            return allFish.Where(f => f.IsCarp()).ToList();
        }

        public List<FishData> GetAllBarbel()
        {
            return allFish.Where(f => f.IsBarbel()).ToList();
        }

        /// <summary>
        /// Selects a fish to bite based on current conditions and bait.
        /// Considers time of day, weather, dip attraction, and spawn chances.
        /// </summary>
        public FishData RollFishBite(DipData currentDip, TimeOfDay time, Weather weather, float depth)
        {
            List<WeightedFish> candidates = new List<WeightedFish>();

            foreach (var fish in allFish)
            {
                float weight = fish.spawnChance;

                // Time preference bonus
                if (fish.preferredTimes != null && fish.preferredTimes.Contains(time))
                    weight *= 2f;

                // Weather preference bonus
                if (fish.preferredWeather != null && fish.preferredWeather.Contains(weather))
                    weight *= 1.5f;

                // Depth check
                if (depth < fish.minDepth || depth > fish.maxDepth)
                    weight *= 0.1f;

                // Dip attraction modifiers
                if (currentDip != null)
                    weight *= CalculateDipAttraction(fish, currentDip);

                // Night barbel bonus
                if (fish.IsBarbel() && time == TimeOfDay.Night)
                    weight *= 2f;

                if (weight > 0f)
                    candidates.Add(new WeightedFish { fish = fish, weight = weight });
            }

            return SelectWeightedRandom(candidates);
        }

        private float CalculateDipAttraction(FishData fish, DipData dip)
        {
            float attraction = 1f;

            attraction += fish.garlicAttraction * dip.garlicFactor;
            attraction += fish.sweetcornAttraction * dip.sweetcornFactor;
            attraction += fish.vanillaAttraction * dip.vanillaFactor;
            attraction += fish.bloodwormAttraction * dip.bloodwormFactor;
            attraction += fish.blackMagicAttraction * dip.blackMagicFactor;

            return Mathf.Max(0.01f, attraction);
        }

        private FishData SelectWeightedRandom(List<WeightedFish> candidates)
        {
            if (candidates.Count == 0) return null;

            float totalWeight = candidates.Sum(c => c.weight);
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var candidate in candidates)
            {
                cumulative += candidate.weight;
                if (roll <= cumulative)
                    return candidate.fish;
            }

            return candidates.Last().fish;
        }

        private struct WeightedFish
        {
            public FishData fish;
            public float weight;
        }
    }
}
