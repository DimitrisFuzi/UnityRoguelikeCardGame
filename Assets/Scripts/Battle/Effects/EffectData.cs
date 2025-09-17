using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;


namespace MyProjectF.Assets.Scripts.Effects

{
    

    [Serializable] // Attribute to allow serialization in Unity
    public abstract class EffectData
    {
        public Card.TargetType targetType;

        public abstract void ApplyEffect(CharacterStats source, CharacterStats target);
    }


}
