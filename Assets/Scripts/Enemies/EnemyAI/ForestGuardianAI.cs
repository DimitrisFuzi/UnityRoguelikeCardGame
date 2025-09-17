using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects; // DamageEffect

public class ForestGuardianAI : MonoBehaviour, IEnemyAI
{
    // ---- Refs
    private Enemy boss;                       // HP, MaxHP κλπ
    private CharacterStats player;            // στόχος για attack
    private EnemyDisplay display;             // για intents/visuals

    // ---- Intent preview (LOCKED per upcoming enemy turn)
    private EnemyIntent nextIntent;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite specialIcon;        // Summon icon
    [SerializeField] private Sprite awakenIntentIcon;   // Awaken icon (ΝΕΟ)

    // προαιρετικό public setter αν θέλεις να το περνάς από αλλού
    public void SetAwakenIcon(Sprite s) => awakenIntentIcon = s;

    // ---- Tunables (fixed numbers)
    [Header("Boss Damage")]
    [SerializeField] private int baseAttack = 7;       // σταθερό
    [SerializeField] private int rampPerTurn = 1;       // +1/γύρο

    [Header("Summon Timing")]
    [SerializeField] private int p1SummonEveryTurns = 3;

    // --- Awaken timing (1 γύρο latency)
    private int enemyTurnIndex = 0;         // μετρητής γύρων του boss
    private int awakenTelegraphTurn = -1;   // σε ποιο enemyTurnIndex τηλεγραφήθηκε

    private EnemyData wispLeftData;
    private EnemyData wispRightData;

    // ---- State
    private int ramp = 0;                   // αυξάνεται στην αρχή κάθε boss turn
    private int absorbBonus = 0;            // μόνιμο +damage από consume
    private bool awakened = false;           // έγινε Awaken;
    private bool awakenTelegraphed = false;  // intent έχει δείξει Awaken
    private bool doubleSummonNextTurn = false;
    private bool canSummonFurther = true;    // κλειδώνει μετά το Awaken
    private int p1SummonCounter = 0;

    // 🔒 Κλειδωμένο πλάνο για τον **επόμενο** enemy turn
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
        // auto-config των wisp data από το EnemyData του boss αν δεν έχουν οριστεί
        if ((wispLeftData == null || wispRightData == null) && boss != null && boss.Data != null)
        {
            if (wispLeftData == null) wispLeftData = boss.Data.summonLeftData;
            if (wispRightData == null) wispRightData = boss.Data.summonRightData;
            Debug.Log($"[ForestGuardianAI] Auto-config from EnemyData → left={(wispLeftData != null)} right={(wispRightData != null)}", this);
        }

        // φόρτωσε icons από EnemyData αν δεν έχουν ήδη οριστεί
        var data = (boss != null) ? boss.Data : null;
        if (data != null)
        {
            if (attackIcon == null) attackIcon = data.attackIntentIcon;
            if (specialIcon == null) specialIcon = data.buffIntentIcon;      // Summon icon
            if (awakenIntentIcon == null) awakenIntentIcon = data.awakenIntentIcon;    // Awaken icon
        }

        // 🔒 Σχεδίασε και κλείδωσε το πλάνο για τον **πρώτο** enemy turn
        PlanNextEnemyTurn();
        display?.SetIntent(nextIntent);
    }

    public void ExecuteTurn()
    {
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST) return;

        // Δείξε το ΗΔΗ κλειδωμένο preview (ΔΕΝ ξαναϋπολογίζουμε στην αρχή του enemy turn)
        display?.SetIntent(nextIntent);

        // Μετρητής γύρων boss
        enemyTurnIndex++;

        // 1) Start-of-turn ramp
        ramp += rampPerTurn;

        // 2) Awaken με 1 γύρο latency (εκτελείται αν τηλεγραφήθηκε σε προηγούμενο enemy γύρο)
        if (!awakened)
        {
            if (awakenTelegraphed && enemyTurnIndex > awakenTelegraphTurn)
            {
                DoAwaken();
                // Μετά το Awaken, κλείδωσε νέο πλάνο για τον ΕΠΟΜΕΝΟ enemy turn
                PlanNextEnemyTurn();
                display?.SetIntent(nextIntent);
                return;
            }
        }

        // 3) Scheduled double-summon αμέσως μετά από Awaken χωρίς minions
        if (doubleSummonNextTurn)
        {
            SpawnUntilFull();
            doubleSummonNextTurn = false;
            canSummonFurther = false; // δεν ξανακάνει summons μετά
            // Κλείδωσε το πλάνο για τον επόμενο enemy turn
            PlanNextEnemyTurn();
            display?.SetIntent(nextIntent);
            return;
        }

        // 4) Εκτέλεση ΚΛΕΙΔΩΜΕΝΟΥ πλάνου για αυτόν τον enemy turn
        if (!awakened && canSummonFurther && plannedSummonNextTurn)
        {
            // Summon ήταν προγραμματισμένο για ΤΩΡΑ
            plannedSummonNextTurn = false; // καταναλώθηκε
            SummonOneInFirstEmptyType();
            p1SummonCounter = 0;           // reset timer μετά το summon
        }
        else
        {
            // Κανονικό Attack
            p1SummonCounter++;             // αυξάνουμε τον timer μόνο όταν δεν έγινε summon
            DoAttack(BaseDamage());
        }

        // 5) Αν πέσαμε ≤50% ΤΩΡΑ και δεν έχει τηλεγραφηθεί → τηλεγράφισε (εκτέλεση στον ΕΠΟΜΕΝΟ enemy γύρο)
        if (!awakened && !awakenTelegraphed && boss.CurrentHealth <= boss.MaxHealth / 2)
        {
            awakenTelegraphed = true;
            awakenTelegraphTurn = enemyTurnIndex;
        }

        // 6) Στο τέλος του enemy turn, ΣΧΕΔΙΑΣΕ & ΚΛΕΙΔΩΣΕ το πλάνο για τον ΕΠΟΜΕΝΟ enemy turn
        PlanNextEnemyTurn();
        display?.SetIntent(nextIntent);
    }

    // ======================
    // 🔒 ΚΛΕΙΔΩΜΑ ΠΛΑΝΟΥ ΓΙΑ ΤΟΝ ΕΠΟΜΕΝΟ ENEMY TURN
    // Καλείται στο InitializeAI() και ΣΤΟ ΤΕΛΟΣ ΚΑΘΕ enemy turn.
    // Δεν επηρεάζεται από αλλαγές που γίνονται κατά το player turn.
    // ======================
    private void PlanNextEnemyTurn()
    {
        // 1) Αν έχει προγραμματιστεί double-summon για τον επόμενο γύρο
        if (doubleSummonNextTurn)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "", 0, specialIcon);
            plannedSummonNextTurn = true; // θα γίνει summon (double) στον επόμενο γύρο
            return;
        }

        // 2) Awaken PREVIEW έχει προτεραιότητα στο UI
        if (!awakened && (awakenTelegraphed || boss.CurrentHealth <= boss.MaxHealth / 2))
        {
            var icon = (awakenIntentIcon != null) ? awakenIntentIcon : specialIcon;
            nextIntent = new EnemyIntent(IntentType.Special, "", 0, icon);
            plannedSummonNextTurn = false; // awaken δεν είναι summon
            return;
        }

        // 3) Διαφορετικά, αποφάσισε για Summon ΜΟΝΟ τώρα (τέλος enemy turn) και κλείδωσε για τον επόμενο
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

        // 4) Αλλιώς Attack preview για τον επόμενο γύρο
        //int preview = BaseDamage();
        //nextIntent = new EnemyIntent(IntentType.Attack, preview.ToString(), preview, attackIcon);

        int preview = baseAttack + (ramp + rampPerTurn) + absorbBonus; // 👈 προβλέπεις την αύξηση
        nextIntent = new EnemyIntent(IntentType.Attack, preview.ToString(), preview, attackIcon);
    }

    public EnemyIntent PredictNextIntent()
    {
        // Επιστρέφουμε το ΗΔΗ κλειδωμένο intent — δεν ξαναϋπολογίζουμε εδώ.
        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent() => nextIntent;

    // ===== Helpers =====
    private int BaseDamage() => baseAttack + ramp + absorbBonus;

    private void DoAttack(int dmg)
    {
        if (player == null) { Debug.LogWarning("[ForestGuardianAI] player is null"); return; }
        var effect = new DamageEffect { damageAmount = dmg };
        effect.ApplyEffect(boss, player);
    }

    private void DoAwaken()
    {
        awakenTelegraphed = false;
        awakened = true;

        // Αν υπάρχουν summons → consume & μόνιμο absorb bonus
        var minions = GetAliveMinions();
        if (minions.Count > 0)
        {
            int sumHP = 0;
            foreach (var m in minions) { sumHP += m.CurrentHealth; }
            absorbBonus += sumHP;

            // destroy όλα τα minions
            foreach (var m in minions) EnemyManager.Instance.RemoveEnemy(m);

            // μετά το consume → δεν ξανακάνει summons
            canSummonFurther = false;

            display?.SetAwakenVisual(true);
            return;
        }

        // Χωρίς summons → προγραμμάτισε double-summon στο επόμενο boss turn
        doubleSummonNextTurn = true;
        // canSummonFurther θα κλειδώσει αμέσως μετά το double-summon
        display.SetAwakenVisual(true);
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

        Debug.LogWarning($"[ForestGuardianAI] Summon failed: hasLeft={hasLeft}, hasRight={hasRight}, leftData={(wispLeftData != null)}, rightData={(wispRightData != null)}");
    }

    private void SpawnUntilFull()
    {
        if (!HasMinionData(wispLeftData) && wispLeftData != null) EnemyManager.Instance.SpawnEnemyRuntime(wispLeftData);
        if (!HasMinionData(wispRightData) && wispRightData != null) EnemyManager.Instance.SpawnEnemyRuntime(wispRightData);
    }
}
