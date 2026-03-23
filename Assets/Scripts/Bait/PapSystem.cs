using UnityEngine;
using System;
using Visvang.Core;

namespace Visvang.Bait
{
    /// <summary>
    /// Manages the pap bucket - the core bait system.
    /// Pap is consumed per cast, destroyed by barbel/mudfish, and enhanced by dips.
    /// </summary>
    public class PapSystem : MonoBehaviour
    {
        public static PapSystem Instance { get; private set; }

        [Header("Pap State")]
        [SerializeField] private float currentPap;
        [SerializeField] private float maxPap = Constants.PAP_BUCKET_MAX;

        [Header("Active Dip")]
        [SerializeField] private DipData activeDip;

        private float dipEffectTimer;
        private float papQuality = 1f;

        public float CurrentPap => currentPap;
        public float MaxPap => maxPap;
        public float NormalizedPap => currentPap / maxPap;
        public DipData ActiveDip => activeDip;
        public float PapQuality => papQuality;

        public event Action<float> OnPapChanged;
        public event Action OnPapEmpty;
        public event Action<DipData> OnDipApplied;
        public event Action OnPapDestroyed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void FillBucket()
        {
            currentPap = maxPap;
            papQuality = 1f;
            OnPapChanged?.Invoke(NormalizedPap);
        }

        public void ApplyDip(DipData dip)
        {
            activeDip = dip;
            OnDipApplied?.Invoke(dip);

            // Dip affects pap quality
            if (dip != null)
                papQuality = 1f + dip.papDurabilityBonus;
        }

        public bool HasPap()
        {
            return currentPap > 0f;
        }

        public void UsePap(float amount)
        {
            float adjustedAmount = amount;

            // Dip can affect consumption
            if (activeDip != null)
                adjustedAmount *= activeDip.papConsumptionMultiplier;

            currentPap = Mathf.Max(0f, currentPap - adjustedAmount);
            OnPapChanged?.Invoke(NormalizedPap);

            if (currentPap <= 0f)
                OnPapEmpty?.Invoke();
        }

        /// <summary>
        /// Barbel/mudfish/chaos event destroys pap from the bucket.
        /// </summary>
        public void DestroyPap(float amount)
        {
            currentPap = Mathf.Max(0f, currentPap - amount);
            papQuality = Mathf.Max(0.3f, papQuality - 0.1f);

            OnPapChanged?.Invoke(NormalizedPap);
            OnPapDestroyed?.Invoke();

            if (currentPap <= 0f)
                OnPapEmpty?.Invoke();
        }

        /// <summary>
        /// Mudfish ruins pap quality without consuming it.
        /// </summary>
        public void DegradePapQuality(float degradation)
        {
            papQuality = Mathf.Max(0.2f, papQuality - degradation);
        }

        public void AddPap(float amount)
        {
            currentPap = Mathf.Min(maxPap, currentPap + amount);
            OnPapChanged?.Invoke(NormalizedPap);
        }
    }
}
