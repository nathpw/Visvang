using UnityEngine;
using Visvang.Core;

namespace Visvang.Equipment
{
    [CreateAssetMenu(fileName = "NewRod", menuName = "Visvang/Rod Data")]
    public class RodData : ScriptableObject
    {
        [Header("Identity")]
        public string rodName;
        public RodTier tier;
        [TextArea(2, 3)] public string description;
        public Sprite rodIcon;

        [Header("Stats")]
        [Range(0f, 1f)] public float castPower;
        [Range(0f, 1f)] public float castAccuracy;
        [Range(0f, 1f)] public float sensitivity;
        [Range(0f, 1f)] public float barbelResistance;
        [Range(0f, 1f)] public float durability;

        [Header("Line")]
        public float maxLineStrength = 10f;
        public float lineBreakResistance = 1f;

        [Header("Grip")]
        [Range(0f, 1f)] public float gripStrength;
        [Tooltip("Reduces chance of rod being pulled in by barbel")]
        [Range(0f, 1f)] public float rodSecureChance;

        [Header("Special")]
        public bool isBarbelSpecialist;
        public bool hasAntiSlimeGrip;
        public float snapChance;

        [Header("Economy")]
        public int purchasePrice;
        public int playerLevelRequired;
        public bool isStarterRod;

        [Header("Flavour")]
        [TextArea(1, 2)] public string equipMessage;

        public bool CanHandleBarbel()
        {
            return barbelResistance > 0.5f || isBarbelSpecialist;
        }
    }
}
