using UnityEngine;

/// <summary>
/// Predicted enemy action shown to the player (type, text, value, icon).
/// </summary>
public enum IntentType
{
    Attack,
    Buff,
    Special, // e.g., summoning or healing
    // Add more if needed (Defense, Heal, etc.)
}

/// <summary>
/// Represents the predicted action the enemy will take next turn.
/// </summary>
public class EnemyIntent
{
    public IntentType Type { get; private set; }
    public string Description { get; private set; }
    public int Value { get; private set; }
    public Sprite Icon { get; private set; }

    public EnemyIntent(IntentType type, string description, int value, Sprite icon)
    {
        Type = type;
        Description = description;
        Value = value;
        Icon = icon;
    }
}
