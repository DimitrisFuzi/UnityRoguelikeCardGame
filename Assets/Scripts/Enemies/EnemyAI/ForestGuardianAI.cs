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

    // ---- Intent preview
    private EnemyIntent nextIntent;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite specialIcon; // για Awaken/Summon

    // ---- Tunables (fixed numbers)
    [Header("Boss Damage")]
    [SerializeField] private int baseAttack = 10;       // σταθερό
    [SerializeField] private int rampPerTurn = 1;       // +1/γύρο

    [Header("Summon Timing")]
    [SerializeField] private int p1SummonEveryTurns = 3;

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

    public bool IsAwakened => awakened;
    public int AbsorbBonus => absorbBonus;

    private void Awake()
    {
        boss = GetComponent<Enemy>();
    }

    // ===== IEnemyAI =====
    public void SetPlayerStats(CharacterStats playerStats) => player = playerStats;
    public void SetEnemyDisplay(EnemyDisplay enemyDisplay) => display = enemyDisplay;
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
        if ((wispLeftData == null || wispRightData == null) && boss != null && boss.Data != null)
        {
            if (wispLeftData == null) wispLeftData = boss.Data.summonLeftData;
            if (wispRightData == null) wispRightData = boss.Data.summonRightData;
            Debug.Log($"[ForestGuardianAI] Auto-config from EnemyData → left={(wispLeftData != null)} right={(wispRightData != null)}", this);
        }
        PredictNextIntent();
    }

    public void ExecuteTurn()
    {
        // 1) Start of boss turn: ramp, scheduled spawns
        ramp += rampPerTurn;

        // Scheduled double-summon after Awaken with 0 summons
        if (doubleSummonNextTurn)
        {
            SpawnUntilFull();
            doubleSummonNextTurn = false;
            canSummonFurther = false; // δεν ξανακάνει summons μετά
            PredictNextIntent();
            return; // η κίνηση αυτόν τον γύρο είναι μόνο summon
        }

        // Phase 1 summon timer (αν δεν έχει awaken & επιτρέπονται summons)
        // ✅ ΜΗΝ κάνεις summon αν έχει ήδη τηλεγραφηθεί Awaken — δώσε προτεραιότητα στο Awaken.
        if (!awakened && canSummonFurther && !awakenTelegraphed)
        {
            p1SummonCounter++;
            if (p1SummonCounter >= p1SummonEveryTurns && AliveMinionsCount() < 2)
            {
                SummonOneInFirstEmptyType();
                p1SummonCounter = 0;
                PredictNextIntent();
                return; // το summon καταναλώνει το γύρο
            }
        }

        // 2) Awaken trigger/execute
        if (!awakened)
        {
            // Αν έχει telegraph-άρει Awaken από προηγούμενο Predict → εκτέλεσέ το
            if (awakenTelegraphed)
            {
                DoAwaken();
                PredictNextIntent();
                return;
            }

            // Μόλις πέσει ≤ 50% → telegraph Awaken για τον επόμενο enemy γύρο
            if (boss.CurrentHealth <= boss.MaxHealth / 2)
            {
                awakenTelegraphed = true;
                // Δεν κάνει κάτι τώρα – μόνο telegraph. Εκτέλεση στον επόμενο enemy γύρο.
                PredictNextIntent();
                return;
            }
        }

        // 3) Κανονικό Attack
        DoAttack(BaseDamage());
        PredictNextIntent();
    }

    public EnemyIntent PredictNextIntent()
    {
        // Αν έχει προγραμματιστεί double-summon
        if (doubleSummonNextTurn)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "Summon x2", 0, specialIcon);
            return nextIntent;
        }

        // Αν θα summon-άρει αυτό το turn (P1 κάθε 3 γύρους)
        if (!awakened && canSummonFurther && p1SummonCounter + 1 >= p1SummonEveryTurns && AliveMinionsCount() < 2)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "Summon", 0, specialIcon);
            return nextIntent;
        }

        // Αν θα κάνει Awaken
        if (!awakened && (awakenTelegraphed || boss.CurrentHealth <= boss.MaxHealth / 2))
        {
            // ✅ schedule το Awaken αν πέρασε το threshold, ώστε να ΜΗΝ χαθεί επειδή έγινε early return αλλού
            if (!awakenTelegraphed && boss.CurrentHealth <= boss.MaxHealth / 2)
                awakenTelegraphed = true;

            nextIntent = new EnemyIntent(IntentType.Special, "Awaken", 0, specialIcon);
            return nextIntent;
        }

        // Αλλιώς Attack preview
        int preview = BaseDamage();
        nextIntent = new EnemyIntent(IntentType.Attack, preview.ToString(), preview, attackIcon);
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
        // Προαιρετικά: display.ShowAttackFX();
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
            // Προαιρετικά: display.SetAwakenVisual(true);
            return;
        }

        // Χωρίς summons → προγραμμάτισε double-summon στο επόμενο boss turn
        doubleSummonNextTurn = true;
        // canSummonFurther θα κλειδώσει αμέσως μετά το double-summon
        // Προαιρετικά: display.SetAwakenVisual(true);
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
