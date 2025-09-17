using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Represents a healing effect that restores health to a target character.    
/// </summary>
[Serializable]
public class HealEffect : EffectData
{
    [Tooltip("Amount of health to restore on the target.")]
    public int healAmount = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target == null || healAmount <= 0) return;

        // ����������� �� ������� direct heal (��������� armor/block).
        // � ������� GainHealthDirect ����������� ��� CharacterStats ��� ���� #2.
        int healed = target.GainHealthDirect(healAmount);

        // �����������: ��������� UI ��� ������� (����� ����)
        if (healed > 0 && target is Enemy enemy && enemy.enemyDisplay != null)
        {
            enemy.enemyDisplay.UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);
            enemy.enemyDisplay.ShowHealPopup(healed);
        }

        Debug.Log($"[HealEffect] {target.name} healed for {healed}.");
    }
}
