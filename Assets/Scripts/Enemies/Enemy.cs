using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;

public class Enemy : CharacterStats
{
    public string enemyName;
    public bool IsEnraged = false;

    private EnemyDisplay enemyDisplay; // Reference to the UI component
    public IEnemyAI EnemyAI { get; private set; } // Reference to the AI logic for this enemy

    /// <summary>
    /// Performs the enemy's AI-defined action.
    /// </summary>
    public void PerformAction()
    {
        if (EnemyAI != null)
        {
            EnemyAI.ExecuteTurn();
        }
        else
        {
            Debug.LogWarning($"⚠️ {enemyName} has no AI component attached!", this);
            if (PlayerStats.Instance != null)
                PerformAttack(PlayerStats.Instance);
        }
    }

    /// <summary>
    /// Initializes enemy stats and connects UI + AI.
    /// </summary>
    public void InitializeEnemy(EnemyData enemyData, EnemyDisplay enemyDisplay)
    {
        enemyName = enemyData.enemyName;

        InitializeStats(enemyData.health);

        this.enemyDisplay = enemyDisplay;
        if (enemyDisplay != null)
            enemyDisplay.Setup(this, enemyData);

        // Attach AI
        AttachAI(enemyData.enemyAIType);

        // After attaching AI, predict and display initial intent
        if (EnemyAI != null) // Add this block
        {
            EnemyIntent initialIntent = EnemyAI.PredictNextIntent();
            this.enemyDisplay.SetIntent(initialIntent);
        }
    }


    /// <summary>
    /// Dynamically attaches AI component based on EnemyData enum value.
    /// </summary>
    private void AttachAI(EnemyAIType aiType)
    {
        switch (aiType)
        {
            case EnemyAIType.Wolf1:
                EnemyAI = gameObject.AddComponent<Wolf1AI>();
                break;
            case EnemyAIType.Wolf2:
                EnemyAI = gameObject.AddComponent<Wolf2AI>();
                break;
            case EnemyAIType.None:
            default:
                Debug.LogWarning($"⚠️ No AI assigned for enemy {enemyName}. Default AI will be used.");
                break;
        }

        // If assigned, pass PlayerStats reference
        if (EnemyAI != null)
        {
            EnemyAI.SetPlayerStats(PlayerStats.Instance);

            if (EnemyAI is Wolf1AI wolf1AI)
            {
                wolf1AI.SetEnemyDisplay(enemyDisplay);
            }
            else if (EnemyAI is Wolf2AI wolf2AI)
            {
                wolf2AI.SetEnemyDisplay(enemyDisplay);
            }
        }
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        if (enemyDisplay != null)
            enemyDisplay.UpdateDisplay(CurrentHealth, MaxHealth);

        Logger.Log($"🔥 {enemyName} took {amount} damage! New HP: {CurrentHealth}/{MaxHealth}", this);
    }

    protected override void Die()
    {
        Logger.Log($"☠️ {enemyName} died!", this);
        EnemyManager.Instance.RemoveEnemy(this);
        // Clear intent display when enemy dies
        if (enemyDisplay != null)
        {
            enemyDisplay.ClearIntentDisplay();
            enemyDisplay.SetEnragedVisual(false); // NEW: Reset color on death
        }
        Destroy(gameObject);
    }
}