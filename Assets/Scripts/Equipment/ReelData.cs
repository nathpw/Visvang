using UnityEngine;
using Visvang.Core;

namespace Visvang.Equipment
{
    [CreateAssetMenu(fileName = "NewReel", menuName = "Visvang/Reel Data")]
    public class ReelData : ScriptableObject
    {
        [Header("Identity")]
        public string reelName;
        public RodTier tier;
        [TextArea(2, 3)] public string description;
        public Sprite reelIcon;

        [Header("Stats")]
        [Range(0f, 2f)] public float reelSpeed;
        [Range(0f, 1f)] public float dragStrength;
        [Range(0f, 1f)] public float lineCapacity;
        [Range(0f, 1f)] public float smoothness;

        [Header("Resistance")]
        [Range(0f, 1f)] public float slimeResistance;
        [Range(0f, 1f)] public float saltResistance;

        [Header("Economy")]
        public int purchasePrice;
        public int playerLevelRequired;
        public bool isStarterReel;

        [Header("Flavour")]
        [TextArea(1, 2)] public string equipMessage;
    }
}
