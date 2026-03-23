using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Equipment;

namespace Visvang.Fishing
{
    /// <summary>
    /// Handles cast power and direction mechanics.
    /// Player holds to build power, releases to cast. Angle affects where bait lands.
    /// </summary>
    public class CastingSystem : MonoBehaviour
    {
        [Header("Cast Settings")]
        [SerializeField] private float powerBuildRate = 50f;
        [SerializeField] private float maxCastPower = 100f;

        [Header("State")]
        private float currentPower;
        private float currentAngle;
        private bool isCharging;
        private Vector3 landingPosition;

        public float CurrentPower => currentPower;
        public float NormalizedPower => currentPower / maxCastPower;
        public float CurrentAngle => currentAngle;
        public Vector3 LandingPosition => landingPosition;
        public bool IsCharging => isCharging;

        public event Action<float> OnPowerChanged;
        public event Action<Vector3> OnCastLanded;

        public void StartCharging()
        {
            isCharging = true;
            currentPower = 0f;
        }

        private void Update()
        {
            if (!isCharging) return;

            currentPower += powerBuildRate * Time.deltaTime;

            // Oscillate if over max (adds skill element)
            if (currentPower > maxCastPower)
            {
                currentPower = maxCastPower - (currentPower - maxCastPower);
                powerBuildRate = -Mathf.Abs(powerBuildRate);
            }
            else if (currentPower < 0f)
            {
                currentPower = -currentPower;
                powerBuildRate = Mathf.Abs(powerBuildRate);
            }

            OnPowerChanged?.Invoke(NormalizedPower);
        }

        public void SetAngle(float angle)
        {
            currentAngle = Mathf.Clamp(angle, -45f, 45f);
        }

        public void ExecuteCast(float power, float angle)
        {
            isCharging = false;

            float normalizedPower = Mathf.Clamp01(power / maxCastPower);
            float distance = normalizedPower * Constants.MAX_CAST_DISTANCE;

            // Calculate landing position
            float radAngle = angle * Mathf.Deg2Rad;
            landingPosition = new Vector3(
                Mathf.Sin(radAngle) * distance,
                0f,
                Mathf.Cos(radAngle) * distance
            );

            // Rod quality affects accuracy
            if (EquipmentManager.Instance != null)
            {
                float accuracy = EquipmentManager.Instance.GetCastAccuracy();
                float scatter = (1f - accuracy) * 5f;
                landingPosition += new Vector3(
                    Random.Range(-scatter, scatter),
                    0f,
                    Random.Range(-scatter, scatter)
                );
            }

            OnCastLanded?.Invoke(landingPosition);
        }

        /// <summary>
        /// Estimate water depth at the landing position.
        /// Deeper water = further from bank.
        /// </summary>
        public float EstimateDepth()
        {
            float distance = landingPosition.magnitude;
            return Mathf.Lerp(1f, 15f, distance / Constants.MAX_CAST_DISTANCE);
        }
    }
}
