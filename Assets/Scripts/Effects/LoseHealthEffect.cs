using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

[Serializable]
public class LoseHealthEffect : EffectData
{
    public int healthLoss = 1;

    [SerializeField]

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        target.LoseHealthDirect(healthLoss);
        Debug.Log($"[Effect] {target.name} lost {healthLoss} HP (ignoring armor).");

        if (healthLoss > 0)
        {
            if (target is Enemy)
                GameSession.Instance?.AddDamageDealt(healthLoss);
            else
                GameSession.Instance?.AddDamageTaken(healthLoss);
        }
    }

}
