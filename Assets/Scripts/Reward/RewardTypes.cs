public enum RewardType { Heal, Card }

public struct RewardOutcome
{
    public RewardType type;
    public int amount;       // για Heal
    public string cardName;  // για Card
}
