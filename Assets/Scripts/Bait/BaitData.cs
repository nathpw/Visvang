using UnityEngine;
using Visvang.Core;

namespace Visvang.Bait
{
    [CreateAssetMenu(fileName = "NewBait", menuName = "Visvang/Bait Data")]
    public class BaitData : ScriptableObject
    {
        [Header("Identity")]
        public string baitName;
        [TextArea(2, 3)] public string description;
        public Sprite baitIcon;

        [Header("Type")]
        public BaitType baitType;

        [Header("Stats")]
        [Range(0f, 1f)] public float hookHoldStrength;
        [Range(0f, 1f)] public float attractionRange;
        public float castWeightBonus;
        public int maxCastsPerBait = 3;

        [Header("Cost")]
        public int purchasePrice;
        public int playerLevelRequired;
        public bool isStarterBait;
    }

    public enum BaitType
    {
        Pap,
        Mielie,
        Doughball,
        Worm,
        Sweetcorn,
        Bread,
        Boilie,
        Artificial
    }
}
