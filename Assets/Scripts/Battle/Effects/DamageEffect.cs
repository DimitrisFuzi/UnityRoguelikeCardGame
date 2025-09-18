using System;
using UnityEngine;
using DG.Tweening;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Deals damage to a target and triggers basic VFX/SFX.
/// </summary>
[Serializable]
public class DamageEffect : EffectData
{
    [Tooltip("Amount of damage to deal to the target.")]
    public int damageAmount;

    public void SetDamageAmount(int amount) => damageAmount = amount;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver())
        {
            Logger.LogWarning("Skipping ApplyEffect: battle is over.");
            return;
        }

        Enemy enemyTarget = target as Enemy;

        if (target != null)
        {
            int realDamage = target.TakeDamage(damageAmount);

            // Player feedback
            if (target is PlayerStats playerTarget && playerTarget.playerDisplay != null)
            {
                if (realDamage > 0)
                {
                    GameSession.Instance?.AddDamageTaken(realDamage);
                    AudioManager.Instance?.PlaySFX("Player_Hit");
                    playerTarget.playerDisplay.ShowDamagePopup(realDamage);
                }
                else if (damageAmount > 0)
                {
                    AudioManager.Instance?.PlaySFX("Player_Hit_Blocked");
                }
            }

            // Enemy VFX (scratch)
            GameObject scratchPrefab = Resources.Load<GameObject>("Effects/ScratchEffect");

            if (scratchPrefab != null &&
                enemyTarget != null &&
                enemyTarget.enemyDisplay != null &&
                enemyTarget.enemyDisplay.enemyImage != null)
            {
                GameObject instance = GameObject.Instantiate(scratchPrefab);
                instance.transform.SetParent(enemyTarget.enemyDisplay.enemyImage.transform, false);

                RectTransform rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;
                }

                var effect = instance.GetComponent<ScratchEffect>();
                if (effect != null) effect.PlayEffect();
            }

            // Enemy feedback (popup + SFX) — uses configured damageAmount for consistency with other displays
            if (enemyTarget != null && enemyTarget.enemyDisplay != null)
            {
                GameSession.Instance?.AddDamageDealt(damageAmount);
                enemyTarget.enemyDisplay.ShowDamagePopup(damageAmount);
                AudioManager.Instance?.PlaySFX("Enemy_Hit");
            }
        }

        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver())
        {
            Logger.LogWarning("Skipping attack animation: battle is over.");
            return;
        }

        // Simple "attack nudge" animation on the source visuals
        if (source != null && source.characterVisualTransform != null)
        {
            Logger.Log($"[AttackAnimation] {source.name} attacking.");
            Vector3 originalPos = source.characterVisualTransform.localPosition;
            Vector3 attackOffset = Vector3.right * 20f;
            if (source is Enemy) attackOffset = Vector3.left * 100f;

            Sequence attackSeq = DOTween.Sequence();
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos + attackOffset, 0.1f));
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos, 0.1f));
        }
    }
}
