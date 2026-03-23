using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Fish;

namespace Visvang.Fishing
{
    /// <summary>
    /// Manages line tension during fish fights.
    /// Too much tension = line snaps. Too little = fish escapes.
    /// </summary>
    public class TensionSystem : MonoBehaviour
    {
        [Header("Tension")]
        [SerializeField] private float currentTension;
        [SerializeField] private float lineStrength = 100f;

        [Header("Settings")]
        [SerializeField] private float tensionDecayRate = 3f;
        [SerializeField] private float reelTensionRate = 8f;

        private FishBehaviour currentFish;
        private bool lineSnapped;

        public float CurrentTension => currentTension;
        public float NormalizedTension => currentTension / lineStrength;
        public bool IsLineSnapped => lineSnapped;
        public float LineStrength => lineStrength;

        public event Action OnLineSnapped;
        public event Action<float> OnTensionChanged;
        public event Action OnTensionDanger; // Fires when tension > 80%

        public void SetFish(FishBehaviour fish)
        {
            currentFish = fish;
        }

        public void SetLineStrength(float strength)
        {
            lineStrength = strength;
        }

        public void ResetTension()
        {
            currentTension = 0f;
            lineSnapped = false;
        }

        private void Update()
        {
            if (currentFish == null || lineSnapped) return;

            // Fish pull adds tension
            float fishTension = currentFish.PullForce * currentFish.Data.tensionBuildRate * Time.deltaTime;
            currentTension += fishTension;

            // Barbel tail slap tension spike
            float tailSlap = currentFish.GetTailSlapTension();
            if (tailSlap > 0f)
                currentTension += tailSlap;

            // Death roll doubles tension buildup
            if (currentFish.IsDeathRolling)
                currentTension += fishTension;

            // Natural tension decay when not reeling
            currentTension -= tensionDecayRate * Time.deltaTime;

            // Line break multiplier from fish data
            float breakThreshold = lineStrength / currentFish.Data.lineBreakMultiplier;

            currentTension = Mathf.Clamp(currentTension, Constants.MIN_LINE_TENSION, lineStrength * 1.2f);

            OnTensionChanged?.Invoke(NormalizedTension);

            // Danger warning
            if (NormalizedTension > 0.8f)
                OnTensionDanger?.Invoke();

            // Line snap check
            if (currentTension >= breakThreshold)
            {
                SnapLine();
            }
        }

        public void AddReelTension(float reelInput)
        {
            currentTension += reelInput * reelTensionRate * Time.deltaTime;
        }

        public void ReleaseTension(float amount)
        {
            currentTension = Mathf.Max(0f, currentTension - amount);
            OnTensionChanged?.Invoke(NormalizedTension);
        }

        private void SnapLine()
        {
            lineSnapped = true;
            OnLineSnapped?.Invoke();
        }

        /// <summary>
        /// Get the tension zone for UI display:
        /// 0 = safe (green), 1 = moderate (yellow), 2 = danger (red)
        /// </summary>
        public int GetTensionZone()
        {
            float normalized = NormalizedTension;
            if (normalized < 0.5f) return 0;
            if (normalized < 0.8f) return 1;
            return 2;
        }
    }
}
