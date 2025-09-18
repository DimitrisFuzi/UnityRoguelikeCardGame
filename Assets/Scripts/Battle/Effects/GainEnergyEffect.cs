using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Grants energy to the player.
/// </summary>
[Serializable]
public class GainEnergyEffect : EffectData
{
    public int energyAmount = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        PlayerStats.Instance.GainEnergy(energyAmount);
        Logger.Log($"[Effect] Gained {energyAmount} energy.");
    }
}
