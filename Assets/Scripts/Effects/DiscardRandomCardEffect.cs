using System;
using UnityEngine;
using System.Collections;
using MyProjectF.Assets.Scripts.Effects;

public class DiscardRandomCardEffect : EffectData, ICoroutineEffect
{
    public override void ApplyEffect(CharacterStats source, CharacterStats target) { }

    public IEnumerator ApplyEffectRoutine(CharacterStats source, CharacterStats target)
    {
        var hand = HandManager.Instance;
        var cards = hand.CardsInHand;

        if (cards.Count == 0)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        int index = UnityEngine.Random.Range(0, cards.Count);
        GameObject randomCard = cards[index];
        hand.RemoveCardFromHand(randomCard);
    }
}
