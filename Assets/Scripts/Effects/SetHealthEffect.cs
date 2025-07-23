using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

[Serializable]
public class SetHealthEffect : EffectData
{
    [SerializeField]
    public int newHealth = 1;

    [SerializeField]

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (source != null)
        {
            source.SetCurrentHealth(newHealth);
            Debug.Log($"[Effect] Set {source.name}'s health to {newHealth}.");
        }
    }
}
