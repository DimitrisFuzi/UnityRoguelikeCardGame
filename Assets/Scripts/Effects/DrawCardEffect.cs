using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Cards;

//[CreateAssetMenu(fileName = "New DrawCardEffect", menuName = "Effects/Draw Card")]
public class DrawCardEffect : EffectData
{
    public int cardsToDraw = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target)
    {
        if (DeckManager.Instance == null || cardsToDraw <= 0) return;

        // Χρησιμοποιούμε Coroutine για σειριακό animation
        HandManager.Instance.StartCoroutine(DrawCardsAnimated(cardsToDraw));
    }

    private IEnumerator DrawCardsAnimated(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return DeckManager.Instance.DrawCardAsync().AsCoroutine();
        }
    }
}
