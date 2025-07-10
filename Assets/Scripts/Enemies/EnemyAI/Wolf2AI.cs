using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Enemy AI logic specific to the Wolf2 enemy.
/// Pattern: Attack -> Enrage -> Attack with double damage -> Attack with double damage...
/// </summary>
public class Wolf2AI : MonoBehaviour, IEnemyAI
{
    private Enemy enemyStats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    [Header("Combat Stats")]
    [SerializeField] private int baseDamageAmount = 8;

    // Reference to the player stats
    private CharacterStats playerStats;

    private EnemyDisplay enemyDisplay; // This reference is correct

    private void Awake()
    {
        enemyStats = GetComponent<Enemy>();
        if (enemyStats == null)
        {
            Debug.LogError("Wolf2AI requires an Enemy component!");
        }
    }

    private void Start()
    {
        // Predict initial intent
        PredictNextIntent();
    }

    /// <summary>
    /// Sets the player stats reference for targeting.
    /// </summary>
    public void SetPlayerStats(CharacterStats player)
    {
        playerStats = player;
    }

    /// <summary>
    /// Sets the EnemyDisplay reference for visual updates.
    /// </summary>
    /// <param name="display">The EnemyDisplay component.</param>
    public void SetEnemyDisplay(EnemyDisplay display)
    {
        enemyDisplay = display;
    }

    /// <summary>
    /// Executes the enemy's turn action.
    /// </summary>
    public void ExecuteTurn()
    {
        Debug.Log($"🐺 Wolf2AI ExecuteTurn called on turn {currentTurn} for {enemyStats.enemyName}."); // Debug log added for verification

        if (currentTurn == 4)
        {
            // Second turn: Apply rage effect
            var rageEffect = new RageEffect();
            rageEffect.ApplyEffect(enemyStats, enemyStats);

            Debug.Log($"{enemyStats.enemyName} becomes enraged!");

            // *** ADD THIS BLOCK HERE ***
            if (enemyDisplay != null)
            {
                enemyDisplay.SetEnragedVisual(true);
                Debug.Log($"🎨 Called SetEnragedVisual(true) for {enemyStats.enemyName}."); // Debug log for verification
            }
            else
            {
                Debug.LogWarning("EnemyDisplay reference is null in Wolf2AI. Cannot set enraged visual."); // Debug log if reference is missing
            }
            // ***************************
        }
        else
        {
            // All other turns: Attack
            PerformAttack();
        }

        currentTurn++;
        PredictNextIntent();
    }

    /// <summary>
    /// Performs an attack on the player.
    /// </summary>
    private void PerformAttack()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats reference is missing in Wolf2AI!");
            return;
        }

        int finalDamage = CalculateDamage();
        var damageEffect = new DamageEffect { damageAmount = finalDamage };
        damageEffect.ApplyEffect(enemyStats, playerStats);

        Debug.Log($"{enemyStats.enemyName} attacks for {finalDamage} damage!");
    }

    /// <summary>
    /// Calculates damage based on current state.
    /// </summary>
    private int CalculateDamage()
    {
        int damage = baseDamageAmount;

        if (enemyStats != null && enemyStats.IsEnraged)
        {
            damage *= 2;
        }

        return damage;
    }

    /// <summary>
    /// Predicts what the enemy will do next turn.
    /// </summary>
    public EnemyIntent PredictNextIntent()
    {
        if (currentTurn == 4)
        {
            // Next turn will be enrage (buff)
            nextIntent = EnemyIntent.CreateBuffIntent("Enrage");
        }
        else
        {
            // Next turn will be attack
            int previewDamage = CalculateDamage();
            nextIntent = EnemyIntent.CreateAttackIntent(previewDamage);
        }

        return nextIntent;
    }

    /// <summary>
    /// Gets the current predicted intent.
    /// </summary>
    public EnemyIntent GetCurrentIntent()
    {
        return nextIntent;
    }
}