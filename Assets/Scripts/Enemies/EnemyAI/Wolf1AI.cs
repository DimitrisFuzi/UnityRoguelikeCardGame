using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Enemy AI logic specific to the Wolf enemy.
/// </summary>
public class Wolf1AI : MonoBehaviour, IEnemyAI
{
    private EnemyStats stats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite buffIcon;

    [SerializeField] private int damageAmount = 8;
    [SerializeField] private int armorAmount = 10;

    // Reference to the player stats, set externally (e.g., TurnManager)
    private CharacterStats playerStats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        PredictNextIntent(); // Predict first turn's intent
    }

    /// <summary>
    /// Sets the player stats reference so the AI ξέρει ποιον θα επιτεθεί.
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
            var buff = new ArmorEffect() { armorAmount = armorAmount };
            buff.ApplyEffect(stats, stats);
        }
        else
        {
            var damage = new DamageEffect() { damageAmount = damageAmount };
            if (playerStats != null)
            {
                damage.ApplyEffect(stats, playerStats);
            }
            else
            {
                Debug.LogWarning("PlayerStats reference is missing in WolfAI.");
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
            nextIntent = new EnemyIntent("Gain " + armorAmount + " Armor", buffIcon);
        }
        else
        {
            nextIntent = new EnemyIntent("Attack for " + damageAmount, attackIcon);
        }

        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent()
    {
        return nextIntent;
    }
}
