using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects; // ✅ Correct namespace for accessing card effects

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Represents a card asset used in the game, containing visual data, type, cost, and associated effects.
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        [SerializeField] private string resourcePath;

        [Header("Card Visual & Meta Info")]
        public string cardName;             // Display name of the card
        public string cardDescription;      // Description with placeholders like {damage}, {armor}
        public CardType cardType;           // Category of the card (Attack, Defense, etc.)
        public Sprite cardSprite;           // Visual sprite for the card's image
        public int energyCost;              // Energy cost to play the card
        public Sprite cardTypeSprite;       // Icon representing the card type (used in UI)
        public bool exhaustAfterUse = false; // Whether the card is exhausted after use

        [Header("Card Effects")]
        [SerializeReference]
        public List<EffectData> effects = new(); // List of effects this card applies when used

        [Header("Card Targeting")]
        public TargetType targetType; // Defines the type of target this card applies to

        /// <summary>
        /// Returns the list of effects associated with this card.
        /// </summary>
        /// <returns>List of EffectData</returns>
        public List<EffectData> GetCardEffects()
        {
            return effects ?? new List<EffectData>();
        }

        /// <summary>
        /// Returns the resource path used for loading or referencing this card.
        /// </summary>
        /// <returns>Path string</returns>
        public string GetResourcePath()
        {
            return resourcePath;
        }

        /// <summary>
        /// Enum representing card categories.
        /// </summary>
        public enum CardType
        {
            Attack,
            Guard,
            Tactic
        }

        /// <summary>
        /// Enum representing card rarity types.
        /// </summary>
        public enum CardRarity
        {
            Common,
            Uncommon,
            Rare,
            Legendary
        }

        /// <summary>
        /// Enum representing valid target types for card effects.
        /// </summary>
        public enum TargetType
        {
            SingleEnemy,   // Targets a single enemy unit
            AllEnemies,    // Targets all enemy units
            Self,          // Targets the player (self-buff, heal, etc.)
            AllAllies,
            None// (Future use) Targets all allies
        }
    }
}
