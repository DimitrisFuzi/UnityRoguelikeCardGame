using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using DG.Tweening;
using MyProjectF.Assets.Scripts.Player;

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
        Enemy enemyTarget = target as Enemy;

        if (target != null)
        {
            int realDamage = target.TakeDamage(damageAmount);

            if (target is PlayerStats playerTarget && playerTarget.playerDisplay != null)
            {
                if (realDamage > 0)
                    playerTarget.playerDisplay.ShowDamagePopup(realDamage);
            }

            GameObject scratchPrefab = Resources.Load<GameObject>("Effects/ScratchEffect");

            if (scratchPrefab != null &&
                enemyTarget != null &&
                enemyTarget.enemyDisplay != null &&
                enemyTarget.enemyDisplay.enemyImage != null)
            {
                // ✅ Instantiate the scratch effect prefab
                GameObject instance = GameObject.Instantiate(scratchPrefab);
                instance.transform.SetParent(enemyTarget.enemyDisplay.enemyImage.transform, false); // false = reset local pos

                // ✅ force reset το RectTransform
                RectTransform rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;
                }

                // ✅ Get the ScratchEffect component and play it
                var effect = instance.GetComponent<ScratchEffect>();
                if (effect != null)
                {
                    effect.PlayEffect();
                }

            }

            if (enemyTarget != null && enemyTarget.enemyDisplay != null)
            {
                enemyTarget.enemyDisplay.ShowDamagePopup(damageAmount);
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
