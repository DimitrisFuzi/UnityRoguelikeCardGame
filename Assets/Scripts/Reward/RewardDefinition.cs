using UnityEngine;

[CreateAssetMenu(menuName = "Game/Reward Definition")]
public class RewardDefinition : ScriptableObject
{
    [Header("Card reference")]
    public ScriptableObject cardAsset; // το κανονικό Card asset

    public string GetCardName() => cardAsset ? cardAsset.name : "(Missing Card)";

    public RewardOutcome ToOutcome() => new RewardOutcome
    {
        type = RewardType.Card,
        cardName = GetCardName()
    };
}
