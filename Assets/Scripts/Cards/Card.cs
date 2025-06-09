using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects; // ✅ Σωστό namespace για τα effects

namespace MyProjectF.Assets.Scripts.Cards
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        [SerializeField] private string resourcePath;

        public string cardName;
        public string cardDescription;
        public CardType cardType;
        public Sprite cardSprite;
        public int energyCost;
        public Sprite cardTypeSprite;

        [SerializeReference] // ✅ Επιτρέπει να ορίζουμε τα effects μέσα στην κάρτα
        public List<EffectData> effects = new List<EffectData>();

        public List<EffectData> GetCardEffects()
        {
            return effects ?? new List<EffectData>();
        }

        public enum CardType
        {
            Attack,
            Defence,
            Utility,
            Special
        }

        public enum TargetType
        {
            SingleEnemy,   // Εφαρμόζεται σε έναν εχθρό
            AllEnemies,    // Εφαρμόζεται σε όλους τους εχθρούς
            Self,          // Εφαρμόζεται στον παίκτη
            AllAllies      // (Μελλοντική προσθήκη) Εφαρμόζεται σε όλους τους συμμάχους
        }

        public TargetType targetType; // ✅ Προσθήκη του target type


        public string GetResourcePath()
        {
            return resourcePath;
        }

        
    }
}
