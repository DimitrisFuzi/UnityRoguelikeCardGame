using UnityEngine;
using System;
using MyProjectF.Assets.Scripts.Effects;

[Serializable] // ✅ Επιτρέπει την αποθήκευση μέσα στην κάρτα!
public class ArmorEffect : EffectData
{
    public int armorAmount = 5; // ✅ Ορίζεται απευθείας μέσα στην κάρτα

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (target != null)
        {
            target.AddArmor(armorAmount);
            Debug.Log($"🛡️ {source.gameObject.name} πρόσθεσε {armorAmount} armor στον {target.gameObject.name}!");
        }
    }
}
