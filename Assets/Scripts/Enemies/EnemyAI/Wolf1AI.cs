using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Enemy AI logic specific to the Wolf1 enemy.
/// Pattern: Attack -> Enrage -> Attack with double damage -> Attack with double damage...
/// </summary>
public class Wolf1AI : MonoBehaviour, IEnemyAI
{
    private Enemy enemyStats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    [Header("Combat Stats")]
    [SerializeField] private int baseDamageAmount = 8;

    // Reference to the player stats
    private CharacterStats playerStats;

    private void Awake()
    {
        enemyStats = GetComponent<Enemy>();
        if (enemyStats == null)
        {
            Debug.LogError("Wolf1AI requires an Enemy component!");
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
    /// Executes the enemy's turn action.
    /// </summary>
    public void ExecuteTurn()
    {
        if (currentTurn == 2)
        {
            // Second turn: Apply rage effect
            var rageEffect = new RageEffect();
            rageEffect.ApplyEffect(enemyStats, enemyStats);

            Debug.Log($"{enemyStats.enemyName} becomes enraged!");
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
            Debug.LogWarning("PlayerStats reference is missing in Wolf1AI!");
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
        if (currentTurn == 2)
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