using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

[Serializable]
public class SetHealthEffect : EffectData
{
    public int newHealth = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        // Keep original behavior: set SOURCE health (as in your current implementation)
        if (source == null) return;

        source.SetCurrentHealth(newHealth);
        Logger.Log($"[SetHealthEffect] Set {source.name}'s health to {newHealth}.");
    }
}
