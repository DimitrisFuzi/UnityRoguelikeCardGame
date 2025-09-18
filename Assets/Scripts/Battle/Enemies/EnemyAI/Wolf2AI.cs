// Wolf2AI.cs
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Enemy AI logic specific to the Wolf2 enemy.
/// Pattern: Attack -> Enrage -> Attack with double damage -> Attack with double damage...
/// </summary>
public class Wolf2AI : MonoBehaviour, IEnemyAI
{
    private Enemy enemyStats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    private Sprite attackIcon;
    private Sprite buffIcon;

    [Header("Combat Stats")]
    [SerializeField] private int baseDamageAmount = 14;

    private CharacterStats playerStats;
    private EnemyDisplay enemyDisplay; // Reference to the EnemyDisplay

    private void Awake()
    {
        enemyStats = GetComponent<Enemy>();
        if (enemyStats == null)
        {
            Logger.LogError("Wolf2AI requires an Enemy component!", this);
        }
    }

    public void InitializeAI()
    {
        PredictNextIntent();
    }

    public void SetPlayerStats(CharacterStats player)
    {
        playerStats = player;
    }

    public void SetIntentIcons(UnityEngine.Sprite attack, UnityEngine.Sprite buff)
    {
        attackIcon = attack;
        buffIcon = buff;
    }

    public void SetEnemyDisplay(EnemyDisplay display)
    {
        enemyDisplay = display;
    }

    /// <summary>
    /// Executes the enemy's turn action.
    /// </summary>
    public void ExecuteTurn()
    {
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST) return;

        if (currentTurn == 4) // enrage turn
        {
            var rageEffect = new RageEffect();
            rageEffect.ApplyEffect(enemyStats, enemyStats);

            Logger.Log($"{enemyStats.enemyName} becomes enraged!", this);
        }
        else
        {
            PerformAttack();
        }

        currentTurn++;
        PredictNextIntent();
    }

    private void PerformAttack()
    {
        if (playerStats == null)
        {
            Logger.LogWarning("PlayerStats reference is missing in Wolf2AI!", this);
            return;
        }

        int finalDamage = CalculateDamage();
        var damageEffect = new DamageEffect { damageAmount = finalDamage };
        damageEffect.ApplyEffect(enemyStats, playerStats);

        Logger.Log($"{enemyStats.enemyName} attacks for {finalDamage} damage!", this);
    }

    private int CalculateDamage()
    {
        int damage = baseDamageAmount;
        if (enemyStats != null && enemyStats.IsEnraged)
        {
            damage *= 2;
        }
        return damage;
    }

    public EnemyIntent PredictNextIntent()
    {
        if (attackIcon == null || buffIcon == null)
        {
            Logger.LogWarning("Intent icons are not set for Wolf2AI. Intents might not display correctly.", this);
        }

        if (currentTurn == 4)
        {
            nextIntent = new EnemyIntent(IntentType.Buff, string.Empty, 0, buffIcon);
        }
        else
        {
            int previewDamage = CalculateDamage();
            nextIntent = new EnemyIntent(IntentType.Attack, previewDamage.ToString(), previewDamage, attackIcon);
        }

        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent()
    {
        return nextIntent;
    }
}
