using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

[Serializable]
public class HealEffect : EffectData
{
    [Tooltip("Amount of health to restore on the target.")]
    public int healAmount = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target == null || healAmount <= 0) return;

        // Direct heal (ignores armor if your CharacterStats supports it)
        int healed = target.GainHealthDirect(healAmount);

        // Optional enemy UI refresh/popup if available
        if (healed > 0 && target is Enemy enemy && enemy.enemyDisplay != null)
        {
            enemy.enemyDisplay.UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);
            enemy.enemyDisplay.ShowHealPopup(healed);
        }

        Logger.Log($"[HealEffect] {target.name} healed for {healed}.");
    }
}
