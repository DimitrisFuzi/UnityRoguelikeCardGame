using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;

namespace MyProjectF.Assets.Scripts.Effects
{
    /// <summary>
    /// Base class for all card effects. Each effect declares a target type and how it applies.
    /// </summary>
    [Serializable]
    public abstract class EffectData
    {
        public Card.TargetType targetType;
        public abstract void ApplyEffect(CharacterStats source, CharacterStats target);
    }
}
