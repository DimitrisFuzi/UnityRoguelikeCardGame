using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;

public class Enemy : CharacterStats
{
    public string enemyName;
    private EnemyDisplay enemyDisplay; // Reference to the UI component

    /// <summary>
    /// Performs an action, in this case attacking the player.
    /// </summary>
    public void PerformAction()
    {
        if (PlayerStats.Instance != null)
        {
            PerformAttack(PlayerStats.Instance); // Attack the player
        }
    }

    /// <summary>
    /// Initializes enemy stats and connects UI.
    /// </summary>
    /// <param name="enemyData">Data container with enemy info</param>
    /// <param name="enemyDisplay">UI display to update</param>
    public void InitializeEnemy(EnemyData enemyData, EnemyDisplay enemyDisplay)
    {
        enemyName = enemyData.enemyName;

        // Initialize health and armor via base method
        InitializeStats(enemyData.health);

        this.enemyDisplay = enemyDisplay; // Store UI reference

        if (enemyDisplay != null)
        {
            enemyDisplay.Setup(this, enemyData);
        }

    }

    /// <summary>
    /// Called when the enemy takes damage.
    /// Updates UI accordingly.
    /// </summary>
    /// <param name="amount">Amount of damage taken</param>
    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount); // Apply damage logic from base class

        if (enemyDisplay != null)
        {
            enemyDisplay.UpdateDisplay(CurrentHealth, MaxHealth); // Update health UI
        }

        Logger.Log($"🔥 {enemyName} took {amount} damage! New HP: {CurrentHealth}/{MaxHealth}", this);
    }

    /// <summary>
    /// Handles death behavior.
    /// </summary>
    protected override void Die()
    {
        Logger.Log($"☠️ {enemyName} died!", this);
        Destroy(gameObject);
    }
}
