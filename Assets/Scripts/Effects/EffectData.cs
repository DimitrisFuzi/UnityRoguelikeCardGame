using System;
using UnityEngine;

namespace MyProjectF.Assets.Scripts.Effects
{
    

    [Serializable] // ✅ Επιτρέπει την αποθήκευση μέσα στο ScriptableObject
    public abstract class EffectData
    {
        public abstract void ApplyEffect(CharacterStats source, CharacterStats target);
    }


}
