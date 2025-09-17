using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Cards;

//[CreateAssetMenu(fileName = "New DrawCardEffect", menuName = "Effects/Draw Card")]
public class DrawCardEffect : EffectData, ICoroutineEffect
{
    public int cardsToDraw = 1;

    public override void ApplyEffect(CharacterStats source, CharacterStats target) { }

    public IEnumerator ApplyEffectRoutine(CharacterStats source, CharacterStats target)
    {
        yield return HandManager.Instance.DrawCardsRoutine(cardsToDraw);
    }
}
