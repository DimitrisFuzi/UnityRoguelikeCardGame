using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;

[Serializable]
public class AOEDamageEffect : EffectData
{
    public int damageAmount = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver())
        {
            Logger.LogWarning("[AOE] Skipping ApplyEffect: battle is over.");
            return;
        }

        // Snapshot active enemies (defensive copy if the list changes during iteration)
        List<Enemy> enemies = EnemyManager.Instance.GetActiveEnemies();
        if (enemies == null || enemies.Count == 0) return;

        // Optional: single "attack nudge" animation on the source (same idea as DamageEffect)
        if (source != null && source.characterVisualTransform != null)
        {
            Vector3 originalPos = source.characterVisualTransform.localPosition;
            Vector3 attackOffset = Vector3.right * 20f;
            if (source is Enemy) attackOffset = Vector3.left * 100f;

            Sequence attackSeq = DOTween.Sequence();
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos + attackOffset, 0.1f));
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos, 0.1f));
        }

        // Spawn scratch VFX per enemy (same prefab used by single-target damage)
        GameObject scratchPrefab = Resources.Load<GameObject>("Effects/ScratchEffect");

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            // Apply damage (CharacterStats.TakeDamage handles armor)
            int realDamage = enemy.TakeDamage(damageAmount);
            Logger.Log($"[AOE] {enemy.name} took {damageAmount} damage.");

            // Attach VFX to enemy image (if available)
            if (scratchPrefab != null && enemy.enemyDisplay != null && enemy.enemyDisplay.enemyImage != null)
            {
                GameObject instance = GameObject.Instantiate(scratchPrefab);
                instance.transform.SetParent(enemy.enemyDisplay.enemyImage.transform, false);

                var rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;
                }

                var effect = instance.GetComponent<ScratchEffect>();
                if (effect != null) effect.PlayEffect();
            }

            // SFX + popup (uses configured damageAmount for consistency with other displays)
            if (enemy.enemyDisplay != null)
            {
                GameSession.Instance?.AddDamageDealt(damageAmount);
                enemy.enemyDisplay.ShowDamagePopup(damageAmount);
                AudioManager.Instance?.PlaySFX("Enemy_Hit");
            }

            // Early exit if the battle ended mid-loop
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver())
                break;
        }
    }
}
