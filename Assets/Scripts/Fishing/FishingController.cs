using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Bait;
using Visvang.Equipment;
using Visvang.Events;

namespace Visvang.Fishing
{
    /// <summary>
    /// Main fishing state machine. Controls the entire flow from casting to catching/losing fish.
    /// </summary>
    public class FishingController : MonoBehaviour
    {
        public static FishingController Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private FishingState currentState = FishingState.Idle;

        [Header("References")]
        [SerializeField] private FishSpawner fishSpawner;
        [SerializeField] private FightController fightController;
        [SerializeField] private CastingSystem castingSystem;
        [SerializeField] private TensionSystem tensionSystem;
        [SerializeField] private PapSystem papSystem;

        [Header("Active Session")]
        private FishData currentFish;
        private float currentFishWeight;
        private FishBehaviour activeFishBehaviour;
        private float hookSetTimer;
        private float slimeMeter;
        private bool rodInWater;

        public FishingState CurrentState => currentState;
        public FishData CurrentFish => currentFish;
        public float CurrentFishWeight => currentFishWeight;
        public float SlimeMeter => slimeMeter;
        public bool RodInWater => rodInWater;

        public event Action<FishingState> OnStateChanged;
        public event Action<FishData, float> OnFishCaught;
        public event Action<FishData> OnFishLost;
        public event Action OnRodPulledIn;
        public event Action<float> OnSlimeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (fishSpawner != null)
            {
                fishSpawner.OnFishBite += HandleFishBite;
                fishSpawner.OnMudfishStreak += HandleMudfishStreak;
            }
        }

        private void OnDestroy()
        {
            if (fishSpawner != null)
            {
                fishSpawner.OnFishBite -= HandleFishBite;
                fishSpawner.OnMudfishStreak -= HandleMudfishStreak;
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case FishingState.BiteDetected:
                    UpdateBiteDetected();
                    break;
                case FishingState.SettingHook:
                    UpdateSettingHook();
                    break;
                case FishingState.Fighting:
                    UpdateFighting();
                    break;
                case FishingState.Landing:
                    UpdateLanding();
                    break;
            }
        }

        public void SetState(FishingState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        // --- IDLE & PREPARATION ---

        public void PrepareBait(DipData dip)
        {
            if (currentState != FishingState.Idle) return;

            SetState(FishingState.PreparingBait);

            if (papSystem != null)
                papSystem.ApplyDip(dip);

            SetState(FishingState.Idle);
        }

        // --- CASTING ---

        public void StartCast()
        {
            if (currentState != FishingState.Idle) return;

            if (papSystem != null && !papSystem.HasPap())
            {
                // No pap left!
                return;
            }

            SetState(FishingState.Casting);
        }

        public void ReleaseCast(float power, float angle)
        {
            if (currentState != FishingState.Casting) return;

            if (castingSystem != null)
                castingSystem.ExecuteCast(power, angle);

            if (papSystem != null)
                papSystem.UsePap(Constants.PAP_PER_CAST);

            SetState(FishingState.Waiting);

            // Start spawning fish bites
            DipData activeDip = papSystem != null ? papSystem.ActiveDip : null;
            if (fishSpawner != null)
                fishSpawner.StartSpawning(activeDip);
        }

        // --- BITE DETECTION ---

        private void HandleFishBite(FishData fish, float weight)
        {
            if (currentState != FishingState.Waiting) return;

            currentFish = fish;
            currentFishWeight = weight;
            hookSetTimer = Constants.HOOK_SET_WINDOW;

            // Barbel can pull rod into water
            if (fish.canPullRodIn && UnityEngine.Random.value < 0.15f)
            {
                HandleRodPulledIn();
                return;
            }

            SetState(FishingState.BiteDetected);
        }

        private void UpdateBiteDetected()
        {
            hookSetTimer -= Time.deltaTime;

            if (hookSetTimer <= 0f)
            {
                // Missed the bite
                currentFish = null;
                SetState(FishingState.Waiting);
            }
        }

        /// <summary>
        /// Player attempts to set the hook (strike). Call this on player input during BiteDetected.
        /// </summary>
        public void Strike()
        {
            if (currentState != FishingState.BiteDetected) return;

            SetState(FishingState.SettingHook);

            // Violent takers hook themselves easier
            float hookChance = currentFish.violentTaker ? 0.95f : 0.75f;

            if (UnityEngine.Random.value < hookChance)
            {
                StartFight();
            }
            else
            {
                // Missed hook set
                OnFishLost?.Invoke(currentFish);
                currentFish = null;
                SetState(FishingState.Waiting);
            }
        }

        private void UpdateSettingHook()
        {
            // Brief transition state, handled by Strike()
        }

        // --- FIGHTING ---

        private void StartFight()
        {
            SetState(FishingState.Fighting);

            fishSpawner?.StopSpawning();

            // Spawn fish behaviour
            var fishGo = new GameObject($"Fish_{currentFish.fishName}");
            activeFishBehaviour = fishGo.AddComponent<FishBehaviour>();
            activeFishBehaviour.Initialize(currentFish, currentFishWeight);

            // Initialize tension
            if (tensionSystem != null)
            {
                tensionSystem.ResetTension();
                tensionSystem.SetFish(activeFishBehaviour);
            }

            // Initialize fight controller
            if (fightController != null)
                fightController.StartFight(activeFishBehaviour, currentFish, currentFishWeight);

            // Barbel destroys pap
            if (currentFish.canDestroyPap && papSystem != null)
                papSystem.DestroyPap(Constants.PAP_BARBEL_DESTROY_AMOUNT);

            // Mudfish ruins pap
            if (currentFish.IsMudfish() && papSystem != null)
                papSystem.DestroyPap(Constants.PAP_MUDFISH_RUIN_AMOUNT);

            GameManager.Instance?.SetPhase(GamePhase.FishFight);
        }

        private void UpdateFighting()
        {
            if (activeFishBehaviour == null) return;

            // Check for line snap
            if (tensionSystem != null && tensionSystem.IsLineSnapped)
            {
                LoseFish("Line snapped!");
                return;
            }

            // Check for hook shake
            if (activeFishBehaviour.TryShakeHook())
            {
                LoseFish("Fish shook the hook!");
                return;
            }

            // Update slime from mudfish
            if (currentFish.causesSlime)
            {
                slimeMeter = activeFishBehaviour.SlimeBuildup;
                OnSlimeChanged?.Invoke(slimeMeter);
            }

            // Check if fish is exhausted
            if (activeFishBehaviour.IsExhausted)
            {
                StartLanding();
            }
        }

        // --- LANDING ---

        private void StartLanding()
        {
            SetState(FishingState.Landing);

            if (fightController != null)
                fightController.StartLanding();
        }

        private void UpdateLanding()
        {
            // Landing is handled by QTE system via FightController
            // This state waits for QTE result
        }

        public void CompleteLanding(bool success)
        {
            if (success)
                CatchFish();
            else
                LoseFish("Fish escaped during landing!");
        }

        // --- RESULTS ---

        private void CatchFish()
        {
            SetState(FishingState.Caught);

            OnFishCaught?.Invoke(currentFish, currentFishWeight);
            GameManager.Instance?.SetPhase(GamePhase.FishCaught);

            CleanupFight();
        }

        private void LoseFish(string reason)
        {
            SetState(FishingState.Lost);

            OnFishLost?.Invoke(currentFish);
            GameManager.Instance?.SetPhase(GamePhase.Fishing);

            CleanupFight();
        }

        private void HandleRodPulledIn()
        {
            rodInWater = true;
            SetState(FishingState.RodPulledIn);
            OnRodPulledIn?.Invoke();

            // Player must complete a QTE to retrieve the rod
        }

        public void RetrieveRod(bool success)
        {
            rodInWater = false;
            if (success)
            {
                // Rod saved, continue with the fish
                StartFight();
            }
            else
            {
                // Rod lost! (equipment penalty handled by EquipmentManager)
                currentFish = null;
                SetState(FishingState.Idle);
            }
        }

        private void HandleMudfishStreak(int count)
        {
            if (count >= Constants.MUDFISH_STREAK_PENALTY_THRESHOLD)
            {
                // Trigger Pap Bucket Penalty
                if (papSystem != null)
                    papSystem.DestroyPap(Constants.PAP_MUDFISH_RUIN_AMOUNT * 2);

                ChaosEventManager.Instance?.TriggerEvent(ChaosEventType.MudfishSwarm);
            }
        }

        private void CleanupFight()
        {
            if (activeFishBehaviour != null)
            {
                Destroy(activeFishBehaviour.gameObject);
                activeFishBehaviour = null;
            }

            slimeMeter = 0f;
            currentFish = null;
            currentFishWeight = 0f;

            tensionSystem?.ResetTension();
        }

        public void ReelIn()
        {
            fishSpawner?.StopSpawning();
            CleanupFight();
            SetState(FishingState.Idle);
        }

        /// <summary>
        /// Player input: reel during a fight.
        /// </summary>
        public void ReelFight(float reelInput)
        {
            if (currentState != FishingState.Fighting) return;
            if (fightController == null) return;

            // Slime reduces reel effectiveness
            float slimeMultiplier = 1f - (slimeMeter / Constants.MAX_SLIME_METER * Constants.SLIME_REEL_PENALTY);
            fightController.ApplyReel(reelInput * slimeMultiplier);
        }

        /// <summary>
        /// Player washes hands at bucket to reduce slime.
        /// </summary>
        public void WashHands()
        {
            if (slimeMeter <= 0f) return;

            slimeMeter = Mathf.Max(0f, slimeMeter - 50f);
            OnSlimeChanged?.Invoke(slimeMeter);
        }
    }
}
