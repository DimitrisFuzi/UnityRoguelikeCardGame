using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Cards;

/// <summary>
/// Coroutine effect that draws a number of cards from the deck into the hand.
/// </summary>
public class DrawCardEffect : EffectData, ICoroutineEffect
{
    public int cardsToDraw = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target) { }

    public IEnumerator ApplyEffectRoutine(CharacterStats source, CharacterStats target)
    {
        yield return HandManager.Instance.DrawCardsRoutine(cardsToDraw);
    }
}
