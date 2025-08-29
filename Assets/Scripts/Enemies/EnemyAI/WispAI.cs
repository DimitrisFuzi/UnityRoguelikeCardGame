using UnityEngine;
using System.Linq;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;

public class WispAI : MonoBehaviour, IEnemyAI
{
    public enum MinionSide { Left, Right }

    [SerializeField] private MinionSide side = MinionSide.Left;
    public MinionSide Side => side;

    [Header("Numbers (Phase1 / Awakened)")]
    [SerializeField] private int healAmountP1 = 3;
    [SerializeField] private int attackAmountP1 = 3;
    [SerializeField] private int healAmountAwaken = 6;
    [SerializeField] private int attackAmountAwaken = 6;

    [Header("Icons")]
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite healIcon;   // ✅ ξεκάθαρα heal (αντικαθιστά το specialIcon)

    // Refs
    private Enemy self;
    private Enemy boss;
    private ForestGuardianAI bossAI;
    private CharacterStats player;

    // Intent state
    private EnemyIntent nextIntent;
    private bool doHealNext = true; // εναλλάξ Heal → Attack → Heal...
    private bool plannedHealThisTurn = false;

    private void Awake()
    {
        self = GetComponent<Enemy>();
    }

    // ===== IEnemyAI =====
    public void SetPlayerStats(CharacterStats p) => player = p;
    public void SetEnemyDisplay(EnemyDisplay d) { /* display not needed here */ }

    // Εξωτερικό wiring αν θέλεις (π.χ. από EnemyManager)
    public void SetIntentIcons(Sprite attack, Sprite heal)
    {
        attackIcon = attack;
        healIcon = heal;
    }

    public void InitializeAI()
    {
        // Βρες το Boss (ForestGuardianAI)
        var all = EnemyManager.Instance.GetActiveEnemies();
        var bossEnemy = all.FirstOrDefault(e => e != null && e.EnemyAI is ForestGuardianAI);
        if (bossEnemy != null)
        {
            boss = bossEnemy;
            bossAI = bossEnemy.EnemyAI as ForestGuardianAI;
        }

        // Φόρτωσε icons από το ΔΙΚΟ του EnemyData (fallback αν δεν έχουν οριστεί από Inspector/wiring)
        if (self != null && self.Data != null)
        {
            if (attackIcon == null) attackIcon = self.Data.attackIntentIcon;
            if (healIcon == null) healIcon = self.Data.healIntentIcon;
        }

        // ΠΑΝΤΑ φόρτωσε από EnemyData αν υπάρχουν (override τυχόν prefab/wiring)
        if (self != null && self.Data != null)
        {
            if (self.Data.attackIntentIcon != null) attackIcon = self.Data.attackIntentIcon;
            if (self.Data.healIntentIcon != null) healIcon = self.Data.healIntentIcon;
        }


        PredictNextIntent();
    }

    public void ExecuteTurn()
    {
        if (boss == null || bossAI == null) { PredictNextIntent(); return; }

        bool awakened = bossAI.IsAwakened;
        int heal = awakened ? healAmountAwaken : healAmountP1;
        int atk = awakened ? attackAmountAwaken : attackAmountP1;

        // ✅ ΜΗΝ ξαναποφασίζεις εδώ. Εκτέλεσε αυτό που είχε κλειδωθεί στο preview:
        if (plannedHealThisTurn)
        {
            var healFx = new HealEffect { healAmount = heal };
            healFx.ApplyEffect(self, boss);

            // Μετά από Heal → επόμενος κύκλος θα πάει σε Attack
            doHealNext = false;
        }
        else
        {
            var dmgFx = new DamageEffect { damageAmount = atk };
            dmgFx.ApplyEffect(self, player);

            // Μετά από Attack → επόμενος κύκλος θα «προσπαθήσει» Heal
            doHealNext = true;
        }

        // Υπολόγισε & κλείδωσε από τώρα το επόμενο intent (εκτός player turn)
        PredictNextIntent();
    }




    public EnemyIntent PredictNextIntent()
    {
        // Αν είμαστε στο player turn, ΜΗΝ αλλάζεις το σχέδιο/preview
        if (TurnManager.Instance != null && TurnManager.Instance.IsPlayerTurn && nextIntent != null)
            return nextIntent;

        // icons από EnemyData με fallback
        Sprite atkIcon = (self != null ? self.Data?.attackIntentIcon : null) ?? attackIcon;
        Sprite healIco = (self != null ? self.Data?.healIntentIcon : null) ?? healIcon;

        bool awakened = bossAI != null && bossAI.IsAwakened;
        int heal = awakened ? healAmountAwaken : healAmountP1;
        int atk = awakened ? attackAmountAwaken : attackAmountP1;

        bool bossWounded = boss != null && boss.CurrentHealth < boss.MaxHealth;

        // NEW: κλειδώνουμε το σχέδιο που θα εκτελεστεί στο enemy phase
        plannedHealThisTurn = doHealNext && bossWounded;

        if (plannedHealThisTurn)
            nextIntent = new EnemyIntent(IntentType.Special, $"{heal}", 0, healIco); // heal preview
        else
            nextIntent = new EnemyIntent(IntentType.Attack, atk.ToString(), atk, atkIcon);

        return nextIntent;
    }


    public EnemyIntent GetCurrentIntent() => nextIntent;

    // Helper
    public void SetSide(MinionSide newSide) => side = newSide;
}
