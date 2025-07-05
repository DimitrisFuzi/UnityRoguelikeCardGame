using System;
using UnityEngine;

namespace MyProjectF.Assets.Scripts.Effects
{
    

    [Serializable] // Attribute to allow serialization in Unity
    public abstract class EffectData
    {
        public abstract void ApplyEffect(CharacterStats source, CharacterStats target);
    }


}
