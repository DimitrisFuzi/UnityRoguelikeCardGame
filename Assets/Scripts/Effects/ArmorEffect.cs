using System;
using UnityEngine;

namespace MyProjectF.Assets.Scripts.Effects
{
    /// <summary>
    /// Effect that adds armor to the target.
    /// </summary>
    [Serializable]
    public class ArmorEffect : EffectData
    {
        /// <summary>
        /// Amount of armor to add.
        /// </summary>
        public int armorAmount = 5;

        /// <summary>
        /// Applies the armor effect by adding armor to the target.
        /// </summary>
        public override void ApplyEffect(CharacterStats source, CharacterStats target)
        {
            if (target != null)
            {
                target.AddArmor(armorAmount);
            }
        }
    }
}
