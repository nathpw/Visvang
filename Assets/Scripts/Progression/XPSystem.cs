using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Fish;

namespace Visvang.Progression
{
    /// <summary>
    /// Calculates and awards XP from fish catches with species-specific multipliers.
    /// </summary>
    public class XPSystem : MonoBehaviour
    {
        public static XPSystem Instance { get; private set; }

        [Header("Multipliers")]
        [SerializeField] private float weightBonusMultiplier = 0.5f;
        [SerializeField] private float nightBonusMultiplier = 1.3f;
        [SerializeField] private float stormBonusMultiplier = 1.2f;

        public event Action<int, int> OnXPAwarded; // xpGained, totalXP

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int CalculateXP(FishData fish, float weight, bool isFirstCatch)
        {
            int xp = fish.baseXP;

            // Weight bonus (heavier = more XP)
            float weightRatio = weight / fish.maxWeight;
            xp += Mathf.RoundToInt(xp * weightRatio * weightBonusMultiplier);

            // First catch bonus
            if (isFirstCatch)
                xp += fish.bonusXPFirstCatch;

            // Legendary multiplier
            if (fish.IsLegendary())
                xp *= Constants.XP_LEGENDARY_MULTIPLIER;

            // Time of day bonus
            if (GameManager.Instance != null && GameManager.Instance.IsNightTime())
                xp = Mathf.RoundToInt(xp * nightBonusMultiplier);

            // Weather bonus
            if (GameManager.Instance != null && GameManager.Instance.CurrentWeather == Weather.Stormy)
                xp = Mathf.RoundToInt(xp * stormBonusMultiplier);

            return xp;
        }

        public void AwardXP(FishData fish, float weight, bool isFirstCatch)
        {
            int xp = CalculateXP(fish, weight, isFirstCatch);

            if (PlayerProfile.Instance != null)
            {
                PlayerProfile.Instance.AddXP(xp);
                OnXPAwarded?.Invoke(xp, PlayerProfile.Instance.TotalXP);
            }
        }

        /// <summary>
        /// XP required for a given level.
        /// </summary>
        public static int XPForLevel(int level)
        {
            return Mathf.RoundToInt(Constants.BASE_XP_PER_LEVEL * Mathf.Pow(level, Constants.XP_LEVEL_SCALING));
        }
    }
}
