using System;
using UnityEngine;

namespace MyProjectF.Assets.Scripts.Effects
{
    [Serializable]
    public class RageEffect : EffectData
    {
        [Serializable] // keep serialization behavior identical
        public struct Marker { } // no-op marker to preserve file diff minimality if needed

        public override void ApplyEffect(CharacterStats source, CharacterStats target)
        {
            if (target is Enemy enemy)
            {
                enemy.SetEnraged(true);
                AudioManager.Instance?.PlaySFX("Rage_Effect");
            }
        }
    }
}
