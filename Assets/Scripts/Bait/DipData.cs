using UnityEngine;
using Visvang.Core;

namespace Visvang.Bait
{
    [CreateAssetMenu(fileName = "NewDip", menuName = "Visvang/Dip Data")]
    public class DipData : ScriptableObject
    {
        [Header("Identity")]
        public string dipName;
        public DipCategory category;
        [TextArea(2, 3)] public string description;
        public Sprite dipIcon;

        [Header("Cost & Unlock")]
        public int purchasePrice;
        public int playerLevelRequired;
        public bool isStarterDip;

        [Header("Attraction Factors")]
        [Tooltip("Positive = attracts barbel. Used by FishDatabase to weight barbel spawns.")]
        [Range(0f, 2f)] public float garlicFactor;
        [Range(0f, 2f)] public float sweetcornFactor;
        [Range(0f, 2f)] public float vanillaFactor;
        [Range(0f, 2f)] public float bloodwormFactor;
        [Range(0f, 2f)] public float blackMagicFactor;

        [Header("Species Modifiers")]
        [Range(-1f, 2f)] public float barbelAttractionBonus;
        [Range(-1f, 2f)] public float mudfishAttractionBonus;
        [Range(-1f, 2f)] public float carpAttractionBonus;

        [Header("Gameplay Effects")]
        [Range(0f, 0.5f)] public float biteFrequencyBonus;
        [Range(0f, 1f)] public float papDurabilityBonus;
        public float papConsumptionMultiplier = 1f;

        [Header("Flavour")]
        [TextArea(1, 2)] public string mixingMessage;
        public Color dipColor = Color.white;

        public bool AttractsBarbelStrongly()
        {
            return category == DipCategory.BarbelAttractor || barbelAttractionBonus > 0.5f;
        }

        public bool AttractsMudfishStrongly()
        {
            return category == DipCategory.MudfishAttractor || mudfishAttractionBonus > 0.5f;
        }

        public bool RepelsMudfish()
        {
            return category == DipCategory.MudfishRepellent || mudfishAttractionBonus < -0.3f;
        }
    }
}
