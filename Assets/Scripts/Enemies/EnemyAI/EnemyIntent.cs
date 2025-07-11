// EnemyIntent.cs
using UnityEngine;

/// <summary>
/// Defines the type of intent an enemy has.
/// </summary>
public enum IntentType
{
    Attack,
    Buff,
    // Add other types as needed, e.g., Defense, Heal, Special
}

/// <summary>
/// Represents the predicted action the enemy will take next turn.
/// Used to show the intent to the player.
/// </summary>
public class EnemyIntent
{
    public IntentType Type { get; private set; } // New field for intent type
    public string Description { get; private set; }
    public int Value { get; private set; }       // New field for numerical value (e.g., damage amount)
    public Sprite Icon { get; private set; }

    /// <summary>
    /// Constructor for EnemyIntent.
    /// </summary>
    /// <param name="type">The type of action (Attack, Buff, etc.).</param>
    /// <param name="description">A textual description of the intent.</param>
    /// <param name="value">A numerical value associated with the intent (e.g., damage, armor gained). Use 0 if not applicable.</param>
    /// <param name="icon">The sprite icon representing the intent.</param>
    public EnemyIntent(IntentType type, string description, int value, Sprite icon)
    {
        Type = type;
        Description = description;
        Value = value;
        Icon = icon;
    }
}