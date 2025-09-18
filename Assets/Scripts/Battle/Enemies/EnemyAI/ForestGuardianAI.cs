using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Boss AI with locked next-turn intent preview, awaken logic, and scheduled summons.
/// </summary>
public class ForestGuardianAI : MonoBehaviour, IEnemyAI
{
    // Refs
    private Enemy boss;
    private CharacterStats player;
    private EnemyDisplay display;

    // Intent preview (locked per upcoming enemy turn)
    private EnemyIntent nextIntent;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite specialIcon;       // Summon icon
    [SerializeField] private Sprite awakenIntentIcon;  // Awaken icon

    public void SetAwakenIcon(Sprite s) => awakenIntentIcon = s;

    // Tunables
    [Header("Boss Damage")]
    [SerializeField] private int baseAttack = 7;
    [SerializeField] private int rampPerTurn = 1;

    [Header("Summon Timing")]
    [SerializeField] private int p1SummonEveryTurns = 3;

    // Turn/state
    private int enemyTurnIndex = 0;       // boss turn counter
    private int awakenTelegraphTurn = -1; // turn index when awaken was telegraphed

    private EnemyData wispLeftData;
    private EnemyData wispRightData;

    private int ramp = 0;                 // increases at the start of each boss turn
    private int absorbBonus = 0;          // permanent +damage from consume
    private bool awakened = false;
    private bool awakenTelegraphed = false;
    private bool doubleSummonNextTurn = false;
    private bool canSummonFurther = true; // locked after awaken
    private int p1SummonCounter = 0;

    // Locked plan for the next enemy turn
    private bool plannedSummonNextTurn = false;

    public bool IsAwakened => awakened;
    public int AbsorbBonus => absorbBonus;

    private void Awake()
    {
        boss = GetComponent<Enemy>();
    }

    // ===== IEnemyAI =====
    public void SetPlayerStats(CharacterStats playerStats) => player = playerStats;
    public void SetEnemyDisplay(EnemyDisplay enemyDisplay) => display = enemyDisplay;

    // attack = attack icon, buffOrSpecial = summon icon
    public void SetIntentIcons(Sprite attack, Sprite buffOrSpecial)
    {
        attackIcon = attack;
        specialIcon = buffOrSpecial;
    }

    public void Configure(EnemyData left, EnemyData right)
    {
        wispLeftData = left;
        wispRightData = right;
    }

    public void InitializeAI()
    {
        // Auto-config summon data from boss EnemyData if not set
        if ((wispLeftData == null || wispRightData == null) && boss != null && boss.Data != null)
        {
            if (wispLeftData == null) wispLeftData = boss.Data.summonLeftData;
            if (wispRightData == null) wispRightData = boss.Data.summonRightData;
            Logger.Log($"[ForestGuardianAI] Auto-config from EnemyData → left={(wispLeftData != null)} right={(wispRightData != null)}", this);
        }

        // Load icons from EnemyData if not set
        var data = (boss != null) ? boss.Data : null;
        if (data != null)
        {
            if (attackIcon == null) attackIcon = data.attackIntentIcon;
            if (specialIcon == null) specialIcon = data.buffIntentIcon;
            if (awakenIntentIcon == null) awakenIntentIcon = data.awakenIntentIcon;
        }

        // Plan & lock the first enemy turn preview
        PlanNextEnemyTurn();
        display?.SetIntent(nextIntent);
    }

    public void ExecuteTurn()
    {
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST) return;

        // Show locked preview (do not recompute here)
        display?.SetIntent(nextIntent);

        // Turn counter
        enemyTurnIndex++;

        // 1) Start-of-turn ramp
        ramp += rampPerTurn;

        // 2) Awaken with 1-turn latency (executes if telegraphed on a previous enemy turn)
        if (!awakened)
        {
            if (awakenTelegraphed && enemyTurnIndex > awakenTelegraphTurn)
            {
                DoAwaken();
                PlanNextEnemyTurn();
                display?.SetIntent(nextIntent);
                return;
            }
        }

        // 3) Scheduled double-summon immediately after Awaken with no minions
        if (doubleSummonNextTurn)
        {
            SpawnUntilFull();
            doubleSummonNextTurn = false;
            canSummonFurther = false;
            PlanNextEnemyTurn();
            display?.SetIntent(nextIntent);
            return;
        }

        // 4) Execute the locked plan for this enemy turn
        if (!awakened && canSummonFurther && plannedSummonNextTurn)
        {
            plannedSummonNextTurn = false; // consumed
            SummonOneInFirstEmptyType();
            p1SummonCounter = 0;           // reset timer after summon
        }
        else
        {
            p1SummonCounter++;             // increase timer only when no summon
            DoAttack(BaseDamage());
        }

        // 5) If dropped to ≤50% now and not telegraphed → telegraph (execution on next enemy turn)
        if (!awakened && !awakenTelegraphed && boss.CurrentHealth <= boss.MaxHealth / 2)
        {
            awakenTelegraphed = true;
            awakenTelegraphTurn = enemyTurnIndex;
        }

        // 6) End of enemy turn → plan & lock preview for the next enemy turn
        PlanNextEnemyTurn();
        display?.SetIntent(nextIntent);
    }

    // ===== Locked planning for the next enemy turn (called from InitializeAI and end of each enemy turn) =====
    private void PlanNextEnemyTurn()
    {
        // 1) If double-summon is scheduled for the next turn
        if (doubleSummonNextTurn)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "", 0, specialIcon);
            plannedSummonNextTurn = true;
            return;
        }

        // 2) Awaken preview has UI priority
        if (!awakened && (awakenTelegraphed || boss.CurrentHealth <= boss.MaxHealth / 2))
        {
            var icon = (awakenIntentIcon != null) ? awakenIntentIcon : specialIcon;
            nextIntent = new EnemyIntent(IntentType.Special, "", 0, icon);
            plannedSummonNextTurn = false;
            return;
        }

        // 3) Otherwise, decide summon now and lock it for next turn
        bool willSummonNext =
            (!awakened
             && canSummonFurther
             && (p1SummonCounter + 1 >= p1SummonEveryTurns)
             && AliveMinionsCount() < 2);

        plannedSummonNextTurn = willSummonNext;

        if (willSummonNext)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "", 0, specialIcon);
            return;
        }

        // 4) Otherwise, attack preview for next turn (predicting next ramp)
        int preview = baseAttack + (ramp + rampPerTurn) + absorbBonus;
        nextIntent = new EnemyIntent(IntentType.Attack, preview.ToString(), preview, attackIcon);
    }

    public EnemyIntent PredictNextIntent() => nextIntent; // already locked
    public EnemyIntent GetCurrentIntent() => nextIntent;

    // ===== Helpers =====
    private int BaseDamage() => baseAttack + ramp + absorbBonus;

    private void DoAttack(int dmg)
    {
        if (player == null) { Logger.LogWarning("[ForestGuardianAI] player is null", this); return; }
        var effect = new DamageEffect { damageAmount = dmg };
        effect.ApplyEffect(boss, player);
    }

    private void DoAwaken()
    {
        awakenTelegraphed = false;
        awakened = true;

        // If there are summons → consume & gain permanent absorb bonus
        var minions = GetAliveMinions();
        if (minions.Count > 0)
        {
            int sumHP = 0;
            foreach (var m in minions) { sumHP += m.CurrentHealth; }
            absorbBonus += sumHP;

            foreach (var m in minions) EnemyManager.Instance.RemoveEnemy(m);

            canSummonFurther = false;
            display?.SetAwakenVisual(true);
            return;
        }

        // No summons → schedule double-summon next boss turn
        doubleSummonNextTurn = true;
        display?.SetAwakenVisual(true);
    }

    private int AliveMinionsCount() => GetAliveMinions().Count;

    private List<Enemy> GetAliveMinions()
    {
        return EnemyManager.Instance.GetActiveEnemies()
            .Where(e => e != null && e != boss && (e.Data == wispLeftData || e.Data == wispRightData))
            .ToList();
    }

    private bool HasMinionData(EnemyData data)
    {
        return EnemyManager.Instance.GetActiveEnemies()
            .Any(e => e != null && e != boss && e.Data == data);
    }

    private void SummonOneInFirstEmptyType()
    {
        bool hasLeft = HasMinionData(wispLeftData);
        bool hasRight = HasMinionData(wispRightData);

        if (!hasLeft && wispLeftData != null) { EnemyManager.Instance.SpawnEnemyRuntime(wispLeftData); return; }
        if (!hasRight && wispRightData != null) { EnemyManager.Instance.SpawnEnemyRuntime(wispRightData); return; }

        Logger.LogWarning($"[ForestGuardianAI] Summon failed: hasLeft={hasLeft}, hasRight={hasRight}, leftData={(wispLeftData != null)}, rightData={(wispRightData != null)}", this);
    }

    private void SpawnUntilFull()
    {
        if (!HasMinionData(wispLeftData) && wispLeftData != null) EnemyManager.Instance.SpawnEnemyRuntime(wispLeftData);
        if (!HasMinionData(wispRightData) && wispRightData != null) EnemyManager.Instance.SpawnEnemyRuntime(wispRightData);
    }
}
