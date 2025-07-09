using UnityEngine;

/// <summary>
/// Enum defining the types of intents an enemy can have.
/// </summary>
public enum IntentType
{
    Attack,
    Buff
}

/// <summary>
/// Represents the predicted action the enemy will take next turn.
/// Now includes a proper intent type instead of relying on description parsing.
/// </summary>
public class EnemyIntent
{
    public IntentType Type { get; private set; }
    public string Description { get; private set; }
    public int Value { get; private set; } // Damage amount, armor gain, etc.
    public Sprite Icon { get; private set; }

    /// <summary>
    /// Creates a new enemy intent with explicit type and value.
    /// </summary>
    /// <param name="type">The type of intent</param>
    /// <param name="description">Human-readable description</param>
    /// <param name="value">Numerical value (damage, armor, etc.)</param>
    /// <param name="icon">Optional custom icon (will use default if null)</param>
    public EnemyIntent(IntentType type, string description, int value, Sprite icon = null)
    {
        Type = type;
        Description = description;
        Value = value;
        Icon = icon;
    }

    /// <summary>
    /// Creates a new attack intent.
    /// </summary>
    public static EnemyIntent CreateAttackIntent(int damage, Sprite icon = null)
    {
        return new EnemyIntent(IntentType.Attack, $"Attack for {damage}", damage, icon);
    }

    /// <summary>
    /// Creates a new buff intent.
    /// </summary>
    public static EnemyIntent CreateBuffIntent(string buffName, int value = 0, Sprite icon = null)
    {
        string description = value > 0 ? $"{buffName} ({value})" : buffName;
        return new EnemyIntent(IntentType.Buff, description, value, icon);
    }
}