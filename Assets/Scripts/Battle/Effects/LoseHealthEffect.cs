using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

[Serializable]
public class LoseHealthEffect : EffectData
{
    public int healthLoss = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target == null || healthLoss <= 0) return;

        target.LoseHealthDirect(healthLoss);

        // Session stats
        if (healthLoss > 0)
        {
            if (target is Enemy)
                GameSession.Instance?.AddDamageDealt(healthLoss);
            else
                GameSession.Instance?.AddDamageTaken(healthLoss);
        }

        Logger.Log($"[LoseHealthEffect] {target.name} lost {healthLoss} HP.");
    }
}