public enum RewardType { Heal, Card }

public struct RewardOutcome
{
    public RewardType type;
    public int amount;       // ��� Heal
    public string cardName;  // ��� Card
}
