using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Enemy AI logic specific to the Wolf enemy.
/// </summary>
public class Wolf1AI : MonoBehaviour, IEnemyAI
{
    private Enemy stats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite buffIcon;

    [SerializeField] private int damageAmount = 8;

    // Reference to the player stats, set externally (e.g., TurnManager)
    private CharacterStats playerStats;

    private void Awake()
    {
        stats = GetComponent<Enemy>();
        PredictNextIntent(); // Predict first turn's intent
    }

    /// <summary>
    /// Sets the player stats reference so the AI knows whom to attack.
    /// </summary>
    public void SetPlayerStats(CharacterStats player)
    {
        playerStats = player;
    }

    /// <summary>
    /// Executes the enemy's behavior depending on the current turn logic.
    /// </summary>
    public void ExecuteTurn()
    {
        if (currentTurn == 2)
        {
            // Εφαρμογή RageEffect αντί για ArmorEffect
            var rage = new RageEffect();
            rage.ApplyEffect(stats, stats);
        }
        else
        {
            int finalDamage = damageAmount;
            if (stats != null && stats.IsEnraged)
                finalDamage *= 2;

            var damage = new DamageEffect() { damageAmount = finalDamage };
            if (playerStats != null)
            {
                damage.ApplyEffect(stats, playerStats);
            }
            else
            {
                Debug.LogWarning("PlayerStats reference is missing in Wolf1AI.");
            }
        }

        currentTurn++;
        PredictNextIntent(); // Update intent for next turn
    }

    /// <summary>
    /// Predicts the enemy's next action and stores it.
    /// </summary>
    public EnemyIntent PredictNextIntent()
    {
        if (currentTurn == 2)
        {
            nextIntent = new EnemyIntent("Enrage (double damage)", buffIcon);
        }
        else
        {
            int previewDamage = damageAmount;
            if (stats != null && stats.IsEnraged)
                previewDamage *= 2;
            nextIntent = new EnemyIntent("Attack for " + previewDamage, attackIcon);
        }

        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent()
    {
        return nextIntent;
    }
}