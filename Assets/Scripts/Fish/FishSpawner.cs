using UnityEngine;
using System.Collections;
using Visvang.Core;
using Visvang.Bait;

namespace Visvang.Fish
{
    /// <summary>
    /// Manages fish spawning and bite timing at the current fishing spot.
    /// </summary>
    public class FishSpawner : MonoBehaviour
    {
        public static FishSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float baseBiteInterval = 15f;
        [SerializeField] private float biteIntervalVariance = 10f;
        [SerializeField] private float currentDepth = 5f;

        private Coroutine biteCoroutine;
        private bool isActive;
        private DipData activeDip;
        private int consecutiveMudfishCount;

        public int ConsecutiveMudfishCount => consecutiveMudfishCount;

        public event System.Action<FishData, float> OnFishBite;
        public event System.Action<int> OnMudfishStreak;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartSpawning(DipData dip)
        {
            activeDip = dip;
            isActive = true;
            consecutiveMudfishCount = 0;

            if (biteCoroutine != null)
                StopCoroutine(biteCoroutine);
            biteCoroutine = StartCoroutine(BiteLoop());
        }

        public void StopSpawning()
        {
            isActive = false;
            if (biteCoroutine != null)
            {
                StopCoroutine(biteCoroutine);
                biteCoroutine = null;
            }
        }

        public void SetDepth(float depth)
        {
            currentDepth = depth;
        }

        public void SetDip(DipData dip)
        {
            activeDip = dip;
        }

        private IEnumerator BiteLoop()
        {
            while (isActive)
            {
                float interval = baseBiteInterval + Random.Range(-biteIntervalVariance, biteIntervalVariance);

                // Dip affects bite frequency
                if (activeDip != null)
                    interval *= (1f - activeDip.biteFrequencyBonus);

                // Night fishing = slower bites but bigger fish
                if (GameManager.Instance != null && GameManager.Instance.IsNightTime())
                    interval *= 1.5f;

                interval = Mathf.Max(3f, interval);

                yield return new WaitForSeconds(interval);

                if (!isActive) yield break;

                TriggerBite();
            }
        }

        private void TriggerBite()
        {
            if (FishDatabase.Instance == null) return;

            var gm = GameManager.Instance;
            TimeOfDay time = gm != null ? gm.CurrentTimeOfDay : TimeOfDay.Morning;
            Weather weather = gm != null ? gm.CurrentWeather : Weather.Clear;

            FishData fish = FishDatabase.Instance.RollFishBite(activeDip, time, weather, currentDepth);

            if (fish == null) return;

            float weight = fish.RollWeight();

            // Track mudfish streak
            if (fish.IsMudfish())
            {
                consecutiveMudfishCount++;
                if (consecutiveMudfishCount >= Constants.MUDFISH_STREAK_PENALTY_THRESHOLD)
                    OnMudfishStreak?.Invoke(consecutiveMudfishCount);
            }
            else
            {
                consecutiveMudfishCount = 0;
            }

            OnFishBite?.Invoke(fish, weight);
        }
    }
}
