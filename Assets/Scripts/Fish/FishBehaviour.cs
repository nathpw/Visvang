using UnityEngine;
using Visvang.Core;

namespace Visvang.Fish
{
    /// <summary>
    /// Represents a live fish instance during a fight.
    /// Handles species-specific AI behaviour during the reel-in phase.
    /// </summary>
    public class FishBehaviour : MonoBehaviour
    {
        [Header("Fish Instance")]
        [SerializeField] private FishData fishData;

        private float currentStamina;
        private float fightTimer;
        private bool isDeathRolling;
        private bool isResting;
        private float pullForce;
        private Vector2 swimDirection;
        private float staminaMax;

        // Barbel-specific
        private float deathRollTimer;
        private int deathRollCount;

        // Mudfish-specific
        private float slimeBuildup;
        private int tangleCount;

        public FishData Data => fishData;
        public float CurrentStamina => currentStamina;
        public float PullForce => pullForce;
        public Vector2 SwimDirection => swimDirection;
        public bool IsDeathRolling => isDeathRolling;
        public float SlimeBuildup => slimeBuildup;
        public bool IsExhausted => currentStamina <= 0f;

        public void Initialize(FishData data, float weight)
        {
            fishData = data;
            staminaMax = data.fightStrength * 100f * (weight / data.maxWeight);
            currentStamina = staminaMax;
            fightTimer = 0f;
            slimeBuildup = 0f;
            tangleCount = 0;
            deathRollCount = 0;
        }

        private void Update()
        {
            if (fishData == null) return;

            fightTimer += Time.deltaTime;

            if (fishData.IsBarbel())
                UpdateBarbelBehaviour();
            else if (fishData.IsMudfish())
                UpdateMudfishBehaviour();
            else
                UpdateDefaultBehaviour();

            // Stamina drain over time
            currentStamina -= Time.deltaTime * 2f;
            currentStamina = Mathf.Max(0f, currentStamina);
        }

        private void UpdateDefaultBehaviour()
        {
            // Standard fish: periodic pulls with rest phases
            float fightCycle = Mathf.Sin(fightTimer * 1.5f);

            if (fightCycle > 0.3f)
            {
                isResting = false;
                pullForce = fishData.fightStrength * fightCycle * (currentStamina / staminaMax);
                swimDirection = new Vector2(
                    Mathf.Cos(fightTimer * 0.8f),
                    Mathf.Sin(fightTimer * 1.2f)
                ).normalized;
            }
            else
            {
                isResting = true;
                pullForce = fishData.fightStrength * 0.1f;
            }
        }

        private void UpdateBarbelBehaviour()
        {
            // Barbel: violent, unpredictable, with death rolls
            float fightCycle = Mathf.Sin(fightTimer * 2.5f);

            // Check for death roll trigger
            if (!isDeathRolling && fishData.hasDeathRoll && currentStamina > staminaMax * 0.3f)
            {
                if (Random.value < 0.005f) // Per-frame chance
                    TriggerDeathRoll();
            }

            if (isDeathRolling)
            {
                deathRollTimer -= Time.deltaTime;
                pullForce = fishData.fightStrength * 2f;
                swimDirection = new Vector2(
                    Mathf.Cos(fightTimer * 8f),
                    Mathf.Sin(fightTimer * 8f)
                ).normalized;

                if (deathRollTimer <= 0f)
                    isDeathRolling = false;
            }
            else
            {
                // Violent surges
                pullForce = fishData.fightStrength * (1f + Mathf.Abs(fightCycle)) * (currentStamina / staminaMax);

                // Random direction changes
                if (Random.value < 0.02f)
                {
                    swimDirection = Random.insideUnitCircle.normalized;
                }
            }
        }

        private void UpdateMudfishBehaviour()
        {
            // Mudfish: weak but annoying, causes tangles and slime
            float fightCycle = Mathf.Sin(fightTimer * 3f);

            pullForce = fishData.fightStrength * 0.3f * (1f + Mathf.Abs(fightCycle));

            // Random tangles
            if (Random.value < 0.008f)
            {
                tangleCount++;
                pullForce = 0f; // Tangle stops pull but locks reel
            }

            // Build slime
            if (fishData.causesSlime)
            {
                slimeBuildup += Time.deltaTime * 5f;
                slimeBuildup = Mathf.Min(slimeBuildup, Constants.MAX_SLIME_METER);
            }

            // Quick random movements
            swimDirection = new Vector2(
                Mathf.Cos(fightTimer * 4f + Random.Range(-0.5f, 0.5f)),
                Mathf.Sin(fightTimer * 3f + Random.Range(-0.5f, 0.5f))
            ).normalized;
        }

        private void TriggerDeathRoll()
        {
            isDeathRolling = true;
            deathRollTimer = Constants.BARBEL_DEATH_ROLL_DURATION;
            deathRollCount++;
        }

        /// <summary>
        /// Apply reeling pressure to the fish, draining stamina faster.
        /// </summary>
        public void ApplyPressure(float amount)
        {
            currentStamina -= amount * Time.deltaTime;
        }

        /// <summary>
        /// Check if the fish shakes off the hook.
        /// </summary>
        public bool TryShakeHook()
        {
            float shakeChance = fishData.hookShakeChance;

            // Mudfish shake more
            if (fishData.IsMudfish())
                shakeChance *= 1.5f;

            // Exhausted fish shake less
            shakeChance *= (currentStamina / staminaMax);

            return Random.value < shakeChance * Time.deltaTime;
        }

        /// <summary>
        /// Returns tension spike from a barbel tail slap.
        /// </summary>
        public float GetTailSlapTension()
        {
            if (!fishData.IsBarbel()) return 0f;

            if (Random.value < 0.003f)
                return 30f;

            return 0f;
        }

        public int GetTangleCount()
        {
            return tangleCount;
        }
    }
}
