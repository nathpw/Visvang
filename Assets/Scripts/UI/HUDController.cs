using UnityEngine;
using UnityEngine.UI;

using Visvang.Core;
using Visvang.Fishing;
using Visvang.Bait;
using Visvang.Progression;

namespace Visvang.UI
{
    /// <summary>
    /// Controls the in-game HUD during fishing.
    /// Shows tension bar, pap level, slime meter, cast power, and status info.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Tension Bar")]
        [SerializeField] private Slider tensionBar;
        [SerializeField] private Image tensionFill;
        [SerializeField] private Color tensionSafeColor = Color.green;
        [SerializeField] private Color tensionWarningColor = Color.yellow;
        [SerializeField] private Color tensionDangerColor = Color.red;

        [Header("Fight Progress")]
        [SerializeField] private Slider fightProgressBar;
        [SerializeField] private Text fishNameText;
        [SerializeField] private Text fishWeightText;

        [Header("Pap Bucket")]
        [SerializeField] private Slider papBar;
        [SerializeField] private Text papText;

        [Header("Slime Meter")]
        [SerializeField] private Slider slimeBar;
        [SerializeField] private GameObject slimePanel;

        [Header("Cast Power")]
        [SerializeField] private Slider castPowerBar;
        [SerializeField] private GameObject castPanel;

        [Header("Grip Strength")]
        [SerializeField] private Slider gripBar;
        [SerializeField] private GameObject gripPanel;

        [Header("Info")]
        [SerializeField] private Text timeText;
        [SerializeField] private Text weatherText;
        [SerializeField] private Text levelText;
        [SerializeField] private Slider xpBar;
        [SerializeField] private Text statusText;

        [Header("Disorientation Effect")]
        [SerializeField] private GameObject disorientOverlay;
        [SerializeField] private RectTransform hudRoot;

        private FightController fightController;
        private TensionSystem tensionSystem;
        private Vector3 originalHUDPosition;

        private void Start()
        {
            fightController = FindObjectOfType<FightController>();
            tensionSystem = FindObjectOfType<TensionSystem>();

            if (hudRoot != null)
                originalHUDPosition = hudRoot.localPosition;

            // Subscribe to events
            if (FishingController.Instance != null)
            {
                FishingController.Instance.OnStateChanged += HandleStateChanged;
                FishingController.Instance.OnSlimeChanged += UpdateSlimeMeter;
            }

            if (PapSystem.Instance != null)
                PapSystem.Instance.OnPapChanged += UpdatePapBar;

            if (tensionSystem != null)
                tensionSystem.OnTensionChanged += UpdateTensionBar;

            UpdateInfoDisplay();
        }

        private void OnDestroy()
        {
            if (FishingController.Instance != null)
            {
                FishingController.Instance.OnStateChanged -= HandleStateChanged;
                FishingController.Instance.OnSlimeChanged -= UpdateSlimeMeter;
            }

            if (PapSystem.Instance != null)
                PapSystem.Instance.OnPapChanged -= UpdatePapBar;

            if (tensionSystem != null)
                tensionSystem.OnTensionChanged -= UpdateTensionBar;
        }

        private void Update()
        {
            UpdateFightProgress();
            UpdateDisorientEffect();
            UpdateInfoDisplay();
        }

        private void HandleStateChanged(FishingState state)
        {
            bool isFighting = state == FishingState.Fighting || state == FishingState.Landing;
            bool isCasting = state == FishingState.Casting;

            if (tensionBar != null) tensionBar.gameObject.SetActive(isFighting);
            if (fightProgressBar != null) fightProgressBar.gameObject.SetActive(isFighting);
            if (castPanel != null) castPanel.SetActive(isCasting);
            if (gripPanel != null) gripPanel.SetActive(isFighting);

            if (state == FishingState.Fighting && FishingController.Instance != null)
            {
                var fish = FishingController.Instance.CurrentFish;
                if (fish != null)
                {
                    if (fishNameText != null) fishNameText.text = fish.fishName;
                    if (fishWeightText != null) fishWeightText.text = $"{FishingController.Instance.CurrentFishWeight:F1}kg";

                    // Show slime panel only for mudfish
                    if (slimePanel != null) slimePanel.SetActive(fish.causesSlime);
                }
            }

            UpdateStatus(state);
        }

        private void UpdateTensionBar(float normalizedTension)
        {
            if (tensionBar != null)
                tensionBar.value = normalizedTension;

            if (tensionFill != null)
            {
                if (normalizedTension < 0.5f)
                    tensionFill.color = tensionSafeColor;
                else if (normalizedTension < 0.8f)
                    tensionFill.color = tensionWarningColor;
                else
                    tensionFill.color = tensionDangerColor;
            }
        }

        private void UpdateFightProgress()
        {
            if (fightController == null || fightProgressBar == null) return;

            if (fightController.IsFighting)
                fightProgressBar.value = fightController.NormalizedProgress;
        }

        private void UpdatePapBar(float normalizedPap)
        {
            if (papBar != null)
                papBar.value = normalizedPap;

            if (papText != null)
                papText.text = $"Pap: {Mathf.RoundToInt(normalizedPap * 100)}%";
        }

        private void UpdateSlimeMeter(float slime)
        {
            if (slimeBar != null)
                slimeBar.value = slime / Constants.MAX_SLIME_METER;
        }

        public void UpdateCastPower(float normalizedPower)
        {
            if (castPowerBar != null)
                castPowerBar.value = normalizedPower;
        }

        public void UpdateGrip(float normalizedGrip)
        {
            if (gripBar != null)
                gripBar.value = normalizedGrip;
        }

        private void UpdateDisorientEffect()
        {
            if (fightController == null) return;

            bool disoriented = fightController.IsUIDisoriented;

            if (disorientOverlay != null)
                disorientOverlay.SetActive(disoriented);

            // Shake the HUD during disorientation
            if (disoriented && hudRoot != null)
            {
                float shake = Mathf.Sin(Time.time * 20f) * 10f;
                hudRoot.localPosition = originalHUDPosition + new Vector3(shake, shake * 0.5f, 0f);
            }
            else if (hudRoot != null)
            {
                hudRoot.localPosition = originalHUDPosition;
            }
        }

        private void UpdateInfoDisplay()
        {
            if (GameManager.Instance != null)
            {
                if (timeText != null)
                    timeText.text = GameManager.Instance.CurrentTimeOfDay.ToString();
                if (weatherText != null)
                    weatherText.text = GameManager.Instance.CurrentWeather.ToString();
            }

            if (PlayerProfile.Instance != null)
            {
                if (levelText != null)
                    levelText.text = $"Level {PlayerProfile.Instance.PlayerLevel}";
                if (xpBar != null)
                    xpBar.value = PlayerProfile.Instance.GetXPProgressToNextLevel();
            }
        }

        private void UpdateStatus(FishingState state)
        {
            if (statusText == null) return;

            switch (state)
            {
                case FishingState.Idle: statusText.text = "Ready to cast"; break;
                case FishingState.PreparingBait: statusText.text = "Mixing pap..."; break;
                case FishingState.Casting: statusText.text = "Hold to power up!"; break;
                case FishingState.Waiting: statusText.text = "Watching the rod tips..."; break;
                case FishingState.BiteDetected: statusText.text = "BITE! Strike now!"; break;
                case FishingState.Fighting: statusText.text = "FIGHT! Reel it in!"; break;
                case FishingState.Landing: statusText.text = "Almost there! Land it!"; break;
                case FishingState.RodPulledIn: statusText.text = "ROD IN WATER! Tap to dive!"; break;
                default: statusText.text = ""; break;
            }
        }
    }
}
