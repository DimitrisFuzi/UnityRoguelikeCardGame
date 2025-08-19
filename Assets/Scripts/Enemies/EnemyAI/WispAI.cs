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
    [SerializeField] private Sprite specialIcon; // heal icon

    // Refs
    private Enemy self;
    private Enemy boss;
    private ForestGuardianAI bossAI;
    private CharacterStats player;
    private EnemyDisplay display;

    // Intent
    private EnemyIntent nextIntent;

    // State: εναλλάξ Heal → Attack → Heal...
    private bool doHealNext = true;

    private void Awake()
    {
        self = GetComponent<Enemy>();
    }

    // ===== IEnemyAI =====
    public void SetPlayerStats(CharacterStats p) => player = p;
    public void SetEnemyDisplay(EnemyDisplay d) => display = d;
    public void SetIntentIcons(Sprite attack, Sprite heal) { attackIcon = attack; specialIcon = heal; }
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
            // ✅ Καθαρή θεραπεία στον boss
            var healFx = new HealEffect { healAmount = heal };
            healFx.ApplyEffect(self, boss);
        }
        else
        {
            // επίθεση στον παίκτη
            var dmgFx = new DamageEffect { damageAmount = atk };
            dmgFx.ApplyEffect(self, player);
        }

        doHealNext = !doHealNext;
        PredictNextIntent();
    }


    public EnemyIntent PredictNextIntent()
    {
        if (bossAI == null)
        {
            nextIntent = new EnemyIntent(IntentType.Special, "Heal", 0, specialIcon);
            return nextIntent;
        }

        bool awakened = bossAI.IsAwakened;
        int heal = awakened ? healAmountAwaken : healAmountP1;
        int atk = awakened ? attackAmountAwaken : attackAmountP1;

        if (doHealNext && boss != null && boss.CurrentHealth < boss.MaxHealth)
            nextIntent = new EnemyIntent(IntentType.Special, $"+{heal}", 0, specialIcon);
        else
            nextIntent = new EnemyIntent(IntentType.Attack, atk.ToString(), atk, attackIcon);

        return nextIntent;
    }

    public EnemyIntent GetCurrentIntent() => nextIntent;

    // Helper για να ορίζεις Side από EnemyData αν χρειάζεται
    public void SetSide(MinionSide newSide) => side = newSide;
}
