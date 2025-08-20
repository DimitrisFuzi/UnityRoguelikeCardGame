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
    [SerializeField] private int healAmountP1 = 6;
    [SerializeField] private int attackAmountP1 = 4;
    [SerializeField] private int healAmountAwaken = 8;
    [SerializeField] private int attackAmountAwaken = 5;

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

        if (doHealNext && boss.CurrentHealth < boss.MaxHealth)
        {
            var healFx = new HealEffect { healAmount = heal };
            healFx.ApplyEffect(self, boss);
        }
        else
        {
            var dmgFx = new DamageEffect { damageAmount = atk };
            dmgFx.ApplyEffect(self, player);
        }

        doHealNext = !doHealNext;
        PredictNextIntent();
    }

    public EnemyIntent PredictNextIntent()
    {
        // icons απευθείας από EnemyData (source of truth), με fallback στα πεδία αν λείπουν
        Sprite atkIcon = (self != null ? self.Data?.attackIntentIcon : null) ?? attackIcon;
        Sprite healIco = (self != null ? self.Data?.healIntentIcon : null) ?? healIcon;

        if (bossAI == null)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "Heal", 0, healIco);
            return nextIntent;
        }

        bool awakened = bossAI.IsAwakened;
        int heal = awakened ? healAmountAwaken : healAmountP1;
        int atk = awakened ? attackAmountAwaken : attackAmountP1;

        if (doHealNext && boss != null && boss.CurrentHealth < boss.MaxHealth)
            nextIntent = new EnemyIntent(IntentType.Special, $"+{heal}", 0, healIco);   // ✅ heal icon
        else
            nextIntent = new EnemyIntent(IntentType.Attack, atk.ToString(), atk, atkIcon);

        return nextIntent;
    }


    public EnemyIntent GetCurrentIntent() => nextIntent;

    // Helper
    public void SetSide(MinionSide newSide) => side = newSide;
}
