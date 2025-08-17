// EnemyData.cs
using UnityEngine;

/// <summary>
/// Holds basic data for an enemy, including stats, visuals, and layout info.
/// This ScriptableObject can be used to instantiate or define enemy prefabs in the scene.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Identity")]
    [Tooltip("Name of the enemy.")]
    public string enemyName;

    [Header("Stats")]
    [Tooltip("Total health of the enemy.")]
    public int health;

    [Header("Visuals")]
    [Tooltip("Sprite representing the enemy.")]
    public Sprite enemySprite;

    [Header("Layout")]
    [Tooltip("Position where the enemy should appear in the scene.")]
    public Vector2 position;

    [Tooltip("Visual size of the enemy in the scene.")]
    public Vector2 size;

    [Header("AI Behavior")]
    [Tooltip("What kind of AI this enemy should use.")]
    public EnemyAIType enemyAIType;

    [Header("Intent Icons")]
    [Tooltip("Sprite for the attack intent.")]
    public Sprite attackIntentIcon;
    [Tooltip("Sprite for the buff intent.")]
    public Sprite buffIntentIcon;

}

/// <summary>
/// Enum to choose which AI script to attach to an enemy dynamically.
/// </summary>
public enum EnemyAIType
{
    None,
    Wolf1,
    Wolf2,
    ForestGuardian,     
    WispLeft,      
    WispRight
}