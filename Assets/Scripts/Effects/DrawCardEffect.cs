using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

[Serializable]
public class DrawCardEffect : EffectData
{
    public int cardsToDraw = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            DeckManager.Instance.DrawCard();
        }

        Debug.Log($"[Effect] Drew {cardsToDraw} card(s).");
    }
}
