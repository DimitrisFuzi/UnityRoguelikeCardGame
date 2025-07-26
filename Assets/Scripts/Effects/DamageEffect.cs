using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using DG.Tweening;

/// <summary>
/// Represents an effect that deals damage to a target character.
/// Can be serialized and configured within card data.
/// </summary>
[Serializable]
public class DamageEffect : EffectData
{
    [Tooltip("Amount of damage to deal to the target.")]
    public int damageAmount;

    /// <summary>
    /// Sets the amount of damage this effect will deal.
    /// </summary>
    public void SetDamageAmount(int amount)
    {
        damageAmount = amount;
    }

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

            // 🔥 Visual damage effect (ScratchEffect)
            GameObject scratchPrefab = Resources.Load<GameObject>("Effects/ScratchEffect");

            if (scratchPrefab != null && target.characterVisualTransform != null)
            {
                GameObject instance = GameObject.Instantiate(scratchPrefab);
                var effect = instance.GetComponent<ScratchEffect>();
                if (effect != null)
                {
                    effect.PlayEffect(target.characterVisualTransform.position);
                }
            }

        }

        // 🔄 Visual "attack movement" animation
        if (source != null && source.characterVisualTransform != null)
        {
            Debug.Log($"[AttackAnimation] {source.name} is attacking", target);
            Vector3 originalPos = source.characterVisualTransform.localPosition;
            Vector3 attackOffset = Vector3.right * 20f;

            if (source is Enemy)
            {
                attackOffset = Vector3.left * 100f;
            }

            Sequence attackSeq = DOTween.Sequence();
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos + attackOffset, 0.1f));
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos, 0.1f));
        }
    }




}
