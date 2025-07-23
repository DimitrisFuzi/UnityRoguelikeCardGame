using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using System.Collections.Generic;

[Serializable]
public class AOEDamageEffect : EffectData
{
    public int damageAmount = 1;

    [SerializeField]
    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        Debug.Log("[AOE] ApplyEffect triggered.");
        List<Enemy> allEnemies = EnemyManager.Instance.GetActiveEnemies();

        foreach (Enemy enemy in allEnemies)
        {
            enemy.TakeDamage(damageAmount);
            Debug.Log($"[AOE] {enemy.name} took {damageAmount} damage.");
        }
    }
}
