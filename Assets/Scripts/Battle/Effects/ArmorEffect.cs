using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Managers;

namespace MyProjectF.Assets.Scripts.Effects
{
    /// <summary>
    /// Adds armor to the target; plays player block visuals if applicable.
    /// </summary>
    [Serializable]
    public class ArmorEffect : EffectData
    {
        public int armorAmount;

        public void SetAmount(int amount) => armorAmount = amount;

        public override void ApplyEffect(CharacterStats source, CharacterStats target)
        {
            if (target == null) return;

            target.AddArmor(armorAmount);

            // If target is the player, play block SFX + visual
            if (target is PlayerStats player && player.playerDisplay != null)
            {
                AudioManager.Instance?.PlaySFX("Block_Gain");
                player.playerDisplay.ShowArmorGainEffect();
            }
        }
    }
}
