using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;

[Serializable]
public class GainEnergyEffect : EffectData
{
    public int energyAmount = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        PlayerStats.Instance.GainEnergy(energyAmount);
        Debug.Log($"[Effect] Gained {energyAmount} energy.");
    }
}
