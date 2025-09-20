// Wolf1AI.cs
using UnityEngine;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Enemy AI logic specific to the Wolf enemy.
/// </summary>
public class Wolf1AI : MonoBehaviour, IEnemyAI
{
    private Enemy stats;
    private int currentTurn = 1;
    private EnemyIntent nextIntent;

    private Sprite attackIcon;
    private Sprite buffIcon;

    [SerializeField] private int damageAmount = 12;

    private CharacterStats playerStats;
    private EnemyDisplay enemyDisplay; // Reference to the EnemyDisplay

    private void Awake()
    {
        stats = GetComponent<Enemy>();
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
    /// Executes the enemy's behavior depending on the current turn logic.
    /// </summary>
    public void ExecuteTurn()
    {
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST) return;

        if (currentTurn == 2)
        {
            var rage = new RageEffect();
            rage.ApplyEffect(stats, stats);
            Logger.Log($"{stats.enemyName} becomes enraged!", this);
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
                Logger.LogWarning("PlayerStats reference is missing in Wolf1AI.", this);
            }
        }

        currentTurn++;
        PredictNextIntent();
    }

    public EnemyIntent PredictNextIntent()
    {
        if (attackIcon == null || buffIcon == null)
        {
            Logger.LogWarning("Intent icons are not set for Wolf1AI. Intents might not display correctly.", this);
        }

        if (currentTurn == 2)
        {
            nextIntent = new EnemyIntent(IntentType.Buff, string.Empty, 0, buffIcon);
        }
        else
        {
            int previewDamage = damageAmount;
            if (stats != null && stats.IsEnraged)
                previewDamage *= 2;
            nextIntent = new EnemyIntent(IntentType.Attack, previewDamage.ToString(), previewDamage, attackIcon);
        }

        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent()
    {
        return nextIntent;
    }
}
