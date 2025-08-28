// Enemy.cs
using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Player;

public class Enemy : CharacterStats
{
    public string enemyName;
    public bool IsEnraged = false;

    public EnemyDisplay enemyDisplay; // Reference to the UI component
    public IEnemyAI EnemyAI { get; private set; } // Reference to the AI logic for this enemy
    public EnemyData Data { get; private set; }   // NEW
    public EnemyAIType AiType { get; private set; } 


    /// <summary>
    /// Performs the enemy's AI-defined action.
    /// </summary>
    public void PerformAction()
    {
        if (EnemyAI != null)
        {
            EnemyAI.ExecuteTurn();
            // After executing the turn, the AI has predicted its NEXT intent.
            // So, update the display.
            UpdateIntentDisplay();
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
    /// <param name="enemyData">The ScriptableObject containing enemy data.</param>
    /// <param name="enemyDisplay">The EnemyDisplay component for UI updates.</param>
    public void InitializeEnemy(EnemyData enemyData, EnemyDisplay enemyDisplay)
    {
        Data = enemyData;              
        AiType = enemyData.enemyAIType;     
        enemyName = enemyData.enemyName;

        InitializeStats(enemyData.health);

        this.enemyDisplay = enemyDisplay;
        if (enemyDisplay != null)
            enemyDisplay.Setup(this, enemyData);
        else
            Debug.LogError($"[Enemy.InitializeEnemy] enemyDisplay is NULL for {enemyName}!", this);


        // Attach AI
        AttachAI(enemyData.enemyAIType, enemyData, this.enemyDisplay);

        UpdateIntentDisplay();
    }


    /// <summary>
    /// Dynamically attaches AI component based on EnemyData enum value.
    /// </summary>
    /// <param name="aiType">The type of AI to attach.</param>
    /// <param name="enemyData">The EnemyData containing intent icons.</param>
    /// <param name="display">The EnemyDisplay component associated with this enemy.</param>
    private void AttachAI(EnemyAIType aiType, EnemyData enemyData, EnemyDisplay display)
    {
        switch (aiType)
        {
            case EnemyAIType.Wolf1: EnemyAI = gameObject.AddComponent<Wolf1AI>(); break;
            case EnemyAIType.Wolf2: EnemyAI = gameObject.AddComponent<Wolf2AI>(); break;

            case EnemyAIType.ForestGuardian:
                {
                    var fg = gameObject.AddComponent<ForestGuardianAI>();
                    // ΠΕΡΝΑΜΕ τα EnemyData των minions από το EnemyData του boss
                    fg.Configure(enemyData.summonLeftData, enemyData.summonRightData);
                    EnemyAI = fg;
                    break;
                }

            case EnemyAIType.WispLeft:
                {
                    var w = gameObject.AddComponent<WispAI>();
                    w.SetSide(WispAI.MinionSide.Left);
                    EnemyAI = w;
                    break;
                }
            case EnemyAIType.WispRight:
                {
                    var w = gameObject.AddComponent<WispAI>();
                    w.SetSide(WispAI.MinionSide.Right);
                    EnemyAI = w;
                    break;
                }
            default:
                Debug.LogWarning($"⚠️ No AI assigned for enemy {enemyName}. Using default.", this);
                break;
        }

        if (EnemyAI != null)
        {
            EnemyAI.SetPlayerStats(PlayerStats.Instance);
            EnemyAI.SetIntentIcons(enemyData.attackIntentIcon, enemyData.buffIntentIcon);
            EnemyAI.InitializeAI();
            EnemyAI.SetEnemyDisplay(display);
        }
    }


    /// <summary>
    /// Retrieves the current predicted intent from the AI and passes it to EnemyDisplay.
    /// </summary>
    public void UpdateIntentDisplay()
    {
        if (EnemyAI != null && enemyDisplay != null)
        {
            EnemyIntent currentIntent = EnemyAI.GetCurrentIntent();
            if (currentIntent != null)
            {
                enemyDisplay.SetIntent(currentIntent);
            }
            else
            {
                enemyDisplay.ClearIntentDisplay();
                Debug.LogWarning($"⚠️ No current intent available for {enemyName}. Clearing intent display.", this);
            }
        }
        else if (enemyDisplay != null)
        {
            enemyDisplay.ClearIntentDisplay();
        }
    }


    public override int TakeDamage(int amount)
    {
        int realDamage = base.TakeDamage(amount);

        if (enemyDisplay != null)
            enemyDisplay.UpdateDisplay(CurrentHealth, MaxHealth);

        Logger.Log($"🔥 {enemyName} took {realDamage} damage! New HP: {CurrentHealth}/{MaxHealth}", this);

        return realDamage;
    }

    protected override void Die()
    {
        base.Die();
        if (enemyDisplay != null)
        {

            AudioManager.Instance?.PlaySFX("Enemy_Death");
            enemyDisplay.PlayDeathAnimation(() => EnemyManager.Instance?.RemoveEnemy(this));


        }
    }

    /// <summary>
    /// Sets the enraged state and updates visual effects.
    /// </summary>
    public void SetEnraged(bool value)
    {
        IsEnraged = value;
        if (enemyDisplay != null)
            enemyDisplay.SetEnragedVisual(value);
    }

}