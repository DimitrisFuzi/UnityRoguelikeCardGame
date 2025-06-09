using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Represents an effect that deals damage to a target character.
/// Can be serialized and configured within card data.
/// </summary>
[Serializable]
public class DamageEffect : EffectData
{
    [Tooltip("Amount of damage to deal to the target.")]
    public int damageAmount = 10;

    /// <summary>
    /// Applies the damage effect to the target character.
    /// </summary>
    /// <param name="source">The character applying the effect.</param>
    /// <param name="target">The target character receiving the damage.</param>
    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target != null)
        {
            target.TakeDamage(damageAmount);
            Debug.Log($"🔥 {source.gameObject.name} dealt {damageAmount} damage to {target.gameObject.name}.");
        }
    }
}
