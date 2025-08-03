using System;
using UnityEngine;

namespace MyProjectF.Assets.Scripts.Effects
{
    [Serializable]
    public class RageEffect : EffectData
    {
        public override void ApplyEffect(CharacterStats source, CharacterStats target)
        {
            if (target is Enemy enemy)
            {
                enemy.IsEnraged = true;
                AudioManager.Instance?.PlaySFX("Rage_Effect");

            }
        }
    }
}
