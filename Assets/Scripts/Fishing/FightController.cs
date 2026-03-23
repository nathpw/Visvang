using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Equipment;
using Visvang.QTE;

namespace Visvang.Fishing
{
    /// <summary>
    /// Controls the fish fight phase with species-specific mechanics.
    /// Manages tension, reeling progress, QTE triggers, and fight resolution.
    /// </summary>
    public class FightController : MonoBehaviour
    {
        [Header("Fight Progress")]
        [SerializeField] private float reelProgress;
        [SerializeField] private float targetDistance = 100f;

        [Header("Settings")]
        [SerializeField] private float baseReelSpeed = 10f;
        [SerializeField] private float fishPullBack = 5f;

        private FishBehaviour currentFish;
        private FishData fishData;
        private float fishWeight;
        private bool isFighting;
        private bool isLanding;
        private float fightTimer;

        // Barbel-specific
        private bool deathRollActive;
        private bool uiDisoriented;
        private float disorientTimer;
        private int tailSlapCount;

        // QTE tracking
        private bool awaitingQTE;

        public float ReelProgress => reelProgress;
        public float TargetDistance => targetDistance;
        public float NormalizedProgress => reelProgress / targetDistance;
        public bool IsFighting => isFighting;
        public bool IsUIDisoriented => uiDisoriented;
        public float DisorientTimer => disorientTimer;

        public event Action OnDeathRollStarted;
        public event Action OnDeathRollEnded;
        public event Action OnTailSlap;
        public event Action<QTEType> OnQTERequired;
        public event Action OnLandingStarted;
        public event Action<bool> OnLandingComplete;

        public void StartFight(FishBehaviour fish, FishData data, float weight)
        {
            currentFish = fish;
            fishData = data;
            fishWeight = weight;
            isFighting = true;
            isLanding = false;
            fightTimer = 0f;
            reelProgress = 0f;
            tailSlapCount = 0;
            deathRollActive = false;
            uiDisoriented = false;

            // Distance scales with fish weight and strength
            targetDistance = 50f + (weight / data.maxWeight) * 50f + data.fightStrength * 30f;

            // Barbel fights trigger immediate grip QTE
            if (data.IsBarbel())
                OnQTERequired?.Invoke(QTEType.GripStrength);
        }

        private void Update()
        {
            if (!isFighting || currentFish == null) return;

            fightTimer += Time.deltaTime;

            if (!isLanding)
                UpdateFightPhase();
            else
                UpdateLandingPhase();

            UpdateDisorient();
        }

        private void UpdateFightPhase()
        {
            // Fish pulls back
            float pullBack = currentFish.PullForce * fishPullBack * Time.deltaTime;
            reelProgress = Mathf.Max(0f, reelProgress - pullBack);

            // Barbel death roll check
            if (fishData.IsBarbel())
                UpdateBarbelSpecific();

            // Barbel tail slap
            float tailSlapTension = currentFish.GetTailSlapTension();
            if (tailSlapTension > 0f)
            {
                tailSlapCount++;
                uiDisoriented = true;
                disorientTimer = Constants.BARBEL_UI_DISORIENT_DURATION;
                OnTailSlap?.Invoke();
            }

            // Check fight complete
            if (reelProgress >= targetDistance)
            {
                StartLanding();
            }
        }

        private void UpdateBarbelSpecific()
        {
            bool wasDeathRolling = deathRollActive;
            deathRollActive = currentFish.IsDeathRolling;

            if (deathRollActive && !wasDeathRolling)
            {
                OnDeathRollStarted?.Invoke();
                OnQTERequired?.Invoke(QTEType.GripStrength);
            }
            else if (!deathRollActive && wasDeathRolling)
            {
                OnDeathRollEnded?.Invoke();
            }
        }

        private void UpdateDisorient()
        {
            if (uiDisoriented)
            {
                disorientTimer -= Time.deltaTime;
                if (disorientTimer <= 0f)
                    uiDisoriented = false;
            }
        }

        public void ApplyReel(float reelInput)
        {
            if (!isFighting || isLanding || awaitingQTE) return;

            // Can't reel during death roll without completing QTE
            if (deathRollActive) return;

            float reelAmount = reelInput * baseReelSpeed * Time.deltaTime;

            // Rod and reel quality affects speed
            if (EquipmentManager.Instance != null)
                reelAmount *= EquipmentManager.Instance.GetReelSpeedMultiplier();

            reelProgress = Mathf.Min(reelProgress + reelAmount, targetDistance);
        }

        public void StartLanding()
        {
            isLanding = true;
            OnLandingStarted?.Invoke();

            // Different landing QTE based on species
            if (fishData.IsBarbel())
                OnQTERequired?.Invoke(QTEType.GripStrength);
            else if (fishData.IsMudfish())
                OnQTERequired?.Invoke(QTEType.Tap); // Mudfish jumps out of net
            else
                OnQTERequired?.Invoke(QTEType.Hold);
        }

        private void UpdateLandingPhase()
        {
            // Mudfish has a chance to jump out each frame during landing
            if (fishData.IsMudfish() && UnityEngine.Random.value < 0.002f)
            {
                OnQTERequired?.Invoke(QTEType.Tap);
            }
        }

        public void OnQTEResult(bool success)
        {
            awaitingQTE = false;

            if (isLanding)
            {
                OnLandingComplete?.Invoke(success);
                if (success)
                    CompleteFight(true);
                else
                    CompleteFight(false);
            }
        }

        private void CompleteFight(bool caught)
        {
            isFighting = false;
            FishingController.Instance?.CompleteLanding(caught);
        }

        public void CancelFight()
        {
            isFighting = false;
            isLanding = false;
            currentFish = null;
        }
    }
}
