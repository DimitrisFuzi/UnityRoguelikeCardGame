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
            Debug.LogWarning("[AOE] Skipping ApplyEffect: battle is over.");
            return;
        }

        // snapshot των ενεργών εχθρών για ασφάλεια (σε περίπτωση που κάποιοι πεθάνουν στο loop)
        List<Enemy> enemies = EnemyManager.Instance.GetActiveEnemies();
        if (enemies == null || enemies.Count == 0) return;

        // 🔊/🎬 Προαιρετικά: ΜΙΑ φορά το “attack animation” του source (όπως στο DamageEffect)
        if (source != null && source.characterVisualTransform != null)
        {
            Vector3 originalPos = source.characterVisualTransform.localPosition;
            Vector3 attackOffset = Vector3.right * 20f;
            if (source is Enemy) attackOffset = Vector3.left * 100f;

            Sequence attackSeq = DOTween.Sequence();
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos + attackOffset, 0.1f));
            attackSeq.Append(source.characterVisualTransform.DOLocalMove(originalPos, 0.1f));
        }

        // VFX prefab όπως στο DamageEffect
        GameObject scratchPrefab = Resources.Load<GameObject>("Effects/ScratchEffect");

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            // 1) Εφάρμοσε damage (παίρνει υπόψη armor μέσω CharacterStats.TakeDamage)
            int realDamage = enemy.TakeDamage(damageAmount);
            Debug.Log($"[AOE] {enemy.name} took {damageAmount} damage.");

            // 2) VFX: ίδιο scratch effect, προσαρτημένο στο enemyImage
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

            // 3) SFX + popup: ίδιοι κωδικοί με το DamageEffect (χρησιμοποιεί damageAmount για συνέπεια)
            if (enemy.enemyDisplay != null)
            {
                GameSession.Instance?.AddDamageDealt(damageAmount);
                enemy.enemyDisplay.ShowDamagePopup(damageAmount); // αν θέλεις “μετά από armor”, βάλ’ το realDamage
                AudioManager.Instance?.PlaySFX("Enemy_Hit");
            }

            // Αν η μάχη τελείωσε στη μέση (π.χ. σκοτώθηκε ο τελευταίος), σταμάτα
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver())
                break;
        }
    }
}

