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
        //Debug.Log($"[Effect] {target.name} lost {healthLoss} HP (ignoring armor).");
        Debug.Log($"[Effect RUN] {GetType().Name}: source={source.name}, target={target.name}, health(before)={target.CurrentHealth}");


        if (healthLoss > 0)
        {
            if (target is Enemy)
                GameSession.Instance?.AddDamageDealt(healthLoss);
            else
                GameSession.Instance?.AddDamageTaken(healthLoss);
        }
    }

}
