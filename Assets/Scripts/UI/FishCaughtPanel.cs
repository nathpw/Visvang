using UnityEngine;
using UnityEngine.UI;

using Visvang.Core;
using Visvang.Fish;
using Visvang.Fishing;
using Visvang.Progression;

namespace Visvang.UI
{
    /// <summary>
    /// Displays the fish caught screen with brag photo, stats, and XP gain.
    /// </summary>
    public class FishCaughtPanel : MonoBehaviour
    {
        [Header("Fish Display")]
        [SerializeField] private Image fishImage;
        [SerializeField] private Text fishNameText;
        [SerializeField] private Text speciesText;
        [SerializeField] private Text weightText;
        [SerializeField] private Text lengthText;
        [SerializeField] private Text rarityText;

        [Header("XP Display")]
        [SerializeField] private Text xpGainedText;
        [SerializeField] private Text totalXPText;
        [SerializeField] private Slider xpBar;

        [Header("Badges")]
        [SerializeField] private GameObject firstCatchBadge;
        [SerializeField] private GameObject newRecordBadge;
        [SerializeField] private GameObject legendaryBadge;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button shareButton;

        [Header("Brag Photo")]
        [SerializeField] private GameObject bragPhotoFrame;
        [SerializeField] private GameObject barbelPhotobombOverlay;

        private void Start()
        {
            if (FishingController.Instance != null)
                FishingController.Instance.OnFishCaught += ShowCatchScreen;

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinue);
        }

        private void OnDestroy()
        {
            if (FishingController.Instance != null)
                FishingController.Instance.OnFishCaught -= ShowCatchScreen;
        }

        public void ShowCatchScreen(FishData fish, float weight)
        {
            gameObject.SetActive(true);

            // Fish info
            if (fishImage != null && fish.fishPhoto != null)
                fishImage.sprite = fish.fishPhoto;
            if (fishNameText != null)
                fishNameText.text = fish.fishName;
            if (speciesText != null)
                speciesText.text = fish.species.ToString();
            if (weightText != null)
                weightText.text = $"{weight:F1} kg";
            if (rarityText != null)
            {
                rarityText.text = fish.rarity.ToString();
                rarityText.color = GetRarityColor(fish.rarity);
            }

            // Badges
            bool isFirst = PlayerProfile.Instance != null && !PlayerProfile.Instance.HasCaughtSpecies(fish.species);
            if (firstCatchBadge != null)
                firstCatchBadge.SetActive(isFirst);
            if (legendaryBadge != null)
                legendaryBadge.SetActive(fish.IsLegendary());
            if (newRecordBadge != null)
                newRecordBadge.SetActive(PlayerProfile.Instance != null && weight > PlayerProfile.Instance.HeaviestFishWeight);

            // XP
            int xp = XPSystem.Instance != null ? XPSystem.Instance.CalculateXP(fish, weight, isFirst) : 0;
            if (xpGainedText != null)
                xpGainedText.text = $"+{xp} XP";

            // Barbel photobomb chance
            if (barbelPhotobombOverlay != null)
                barbelPhotobombOverlay.SetActive(fish.IsCarp() && Random.value < 0.1f);

            // Show message
            MessageSystem.Instance?.ShowCatchMessage(fish, weight);
        }

        private Color GetRarityColor(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Common: return Color.white;
                case FishRarity.Uncommon: return Color.green;
                case FishRarity.Rare: return new Color(0.2f, 0.5f, 1f);
                case FishRarity.Epic: return new Color(0.6f, 0.2f, 0.9f);
                case FishRarity.Legendary: return new Color(1f, 0.85f, 0f);
                default: return Color.white;
            }
        }

        private void OnContinue()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.SetPhase(GamePhase.Fishing);
            FishingController.Instance?.SetState(FishingState.Idle);
        }
    }
}
