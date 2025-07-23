using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

[Serializable]
public class DiscardRandomCardEffect : EffectData
{
    [SerializeField]

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        var hand = HandManager.Instance;
        var cards = hand.CardsInHand;

        if (cards.Count == 0)
        {
            Debug.Log("[Effect] No cards in hand to discard.");
            return;
        }

        int index = UnityEngine.Random.Range(0, cards.Count);
        GameObject randomCard = cards[index];

        Debug.Log($"[Effect] Randomly discarding card: {randomCard.name}");
        hand.RemoveCardFromHand(randomCard);

    }
}
