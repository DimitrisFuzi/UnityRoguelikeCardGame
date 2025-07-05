using UnityEngine;

/// <summary>
/// Represents the predicted action the enemy will take next turn.
/// Used to show the intent to the player.
/// </summary>
public class EnemyIntent
{
    public string Description { get; private set; }
    public Sprite Icon { get; private set; }

    public EnemyIntent(string description, Sprite icon)
    {
        Description = description;
        Icon = icon;
    }
}
