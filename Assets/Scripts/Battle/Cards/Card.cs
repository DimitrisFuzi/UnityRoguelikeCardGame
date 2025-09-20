using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Card asset with visuals, type, cost, rarity, targeting and effect list.
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        [SerializeField] private string resourcePath;

        [Header("Card Visual & Meta Info")]
        public string cardName;
        [TextArea] public string cardDescription;  // Supports placeholders like {damage}, {armor}, etc.
        public CardType cardType;
        public Sprite cardSprite;
        public int energyCost;
        public CardRarity cardRarity;
        public bool exhaustAfterUse = false;

        [Header("Card Effects")]
        [SerializeReference] public List<EffectData> effects = new();

        [Header("Card Targeting")]
        public TargetType targetType;

        /// <summary>Returns associated effects (never null).</summary>
        public List<EffectData> GetCardEffects() => effects ?? new List<EffectData>();

        /// <summary>Resource path used for loading/reference.</summary>
        public string GetResourcePath() => resourcePath;

        public enum CardType { Attack, Guard, Tactic }

        public enum CardRarity { Common, Uncommon, Rare, Legendary }

        public enum TargetType
        {
            SingleEnemy,
            AllEnemies,
            Self,
            AllAllies,
            None
        }
    }
}
