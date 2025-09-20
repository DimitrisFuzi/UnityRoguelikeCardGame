using System;
using MyProjectF.Assets.Scripts.Cards;

/// <summary>
/// Serializable card choice with an optional weight for selection.
/// </summary>
[Serializable]
public class RewardDefinition
{
    public Card cardData;
    public int weight = 1;
}
