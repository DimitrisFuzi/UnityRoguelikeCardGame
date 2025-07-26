using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;


namespace MyProjectF.Assets.Scripts.Effects
{
    /// <summary>
    /// Effect that adds armor to the target.
    /// </summary>
    [Serializable]
    public class ArmorEffect : EffectData
    {
        public int armorAmount;

        public void SetAmount(int amount)
        {
            armorAmount = amount;
        }

        public override void ApplyEffect(CharacterStats source, CharacterStats target)
        {
            if (target != null)
            {
                // Προσθήκη armor στον στόχο
                target.AddArmor(armorAmount);

                // Αν είναι ο παίκτης, ενεργοποίησε το visual
                PlayerStats player = target as PlayerStats;
                if (player != null && player.playerDisplay != null)
                {
                    player.playerDisplay.ShowArmorGainEffect();
                }
            }
        }
    }
}
