using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

[Serializable] // ✅ Επιτρέπει να εμφανίζεται στον Inspector ως instance
public class DamageEffect : EffectData
{
    public int damageAmount = 10; // ✅ Ορισμός damage μέσα στην κάρτα

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target != null)
        {
            target.TakeDamage(damageAmount);
            Debug.Log($"🔥 {source.gameObject.name} προκάλεσε {damageAmount} damage στον {target.gameObject.name}!");
        }
    }
}


