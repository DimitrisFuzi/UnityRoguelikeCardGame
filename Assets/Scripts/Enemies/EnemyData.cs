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
    [Tooltip("Sprite for the heal intent.")]
    public Sprite healIntentIcon;
    [Tooltip("Sprite for the awaken intent.")]
    public Sprite awakenIntentIcon;

    [Header("Boss Summon Config (optional)")]
    public EnemyData summonLeftData;   // π.χ. WispLeft.asset
    public EnemyData summonRightData;  // π.χ. WispRight.asset

    [Header("Shadow Settings")]
    public ShadowMode shadowMode = ShadowMode.Auto;
    [Range(0.1f, 2f)] public float shadowWidthMultiplier = 0.75f;   // Auto
    [Range(0.05f, 0.6f)] public float shadowHeightToWidth = 0.20f;   // Auto
    public Vector2 shadowOffset = new Vector2(0f, -10f);             // Auto/Manual

    // Manual override (αν θες να ορίσεις ρητά μέγεθος)
    public Vector2 manualShadowSize = new Vector2(180f, 28f);

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

/// <summary>
/// Shadow sizing modes.
/// </summary>
public enum ShadowMode { None, Auto, Manual }


