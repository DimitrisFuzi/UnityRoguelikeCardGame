using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;
using System.Linq;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects; // ✅ Προσθέτουμε το σωστό namespace για τα effects

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private PlayerStats playerStats;
    [SerializeField] private GameObject playerPrefab;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerStats = PlayerStats.Instance;
    }

    public void InitializePlayer()
    {


        if (playerPrefab == null)
        {
            Debug.LogError("❌ Το playerPrefab είναι NULL! Ρύθμισέ το στο Inspector.");
            return;
        }

        //Debug.Log("📌 Προσπάθεια δημιουργίας του PlayerPrefab...");
        GameObject playerObject = Instantiate(playerPrefab, GameObject.Find("PlayerCanvas").transform, false);
        
        if (playerObject == null)
        {
            Debug.LogError("❌ Αποτυχία δημιουργίας του PlayerPrefab!");
            return;
        }
        //Debug.Log("✅ PlayerStats βρέθηκε επιτυχώς!");

        playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("❌ Το PlayerPrefab δεν έχει PlayerStats component! Ελέγξτε το Prefab.");
            Debug.LogError($"🔍 Object Name: {playerObject.name}, Components: {string.Join(", ", playerObject.GetComponents<Component>().Select(c => c.GetType().Name))}");

            return;
        }

        playerStats.ResetArmor();
        playerStats.ResetEnergy();
    }

    public bool CanPlayCard(Card card)
    {
        return playerStats.energy >= card.energyCost;
    }

    public void UseCard(Card card)
    {
        if (!CanPlayCard(card))
        {
            Debug.Log("❌ Δεν έχεις αρκετό energy για να παίξεις αυτή την κάρτα!");
            return;
        }

        playerStats.UseEnergy(card.energyCost);
        Debug.Log($"🃏 Ο παίκτης έπαιξε την κάρτα {card.cardName}");
    }

    public void ApplyCardEffect(Enemy targetEnemy, EffectData effect, Card card)
    {
        if (effect == null)
        {
            Debug.LogError("❌ Το effect είναι NULL! Βεβαιώσου ότι η κάρτα έχει συνδεδεμένο effect.");
            return;
        }

        switch (card.targetType)
        {
            case Card.TargetType.SingleEnemy:
                if (targetEnemy != null)
                {
                    effect.ApplyEffect(playerStats, targetEnemy);
                }
                else
                {
                    Debug.LogError("❌ Η κάρτα απαιτεί στόχο αλλά δεν βρέθηκε εχθρός!");
                }
                break;

            case Card.TargetType.AllEnemies:
                foreach (Enemy enemy in EnemyManager.Instance.GetActiveEnemies())
                {
                    effect.ApplyEffect(playerStats, enemy);
                }
                break;

            case Card.TargetType.Self:
                effect.ApplyEffect(playerStats, playerStats);
                break;

            case Card.TargetType.AllAllies:
                foreach (PlayerStats ally in PlayerManager.Instance.GetAllies())
                {
                    effect.ApplyEffect(playerStats, ally);
                }
                break;
        }
    }

    public List<PlayerStats> GetAllies()
    {
        List<PlayerStats> allies = new List<PlayerStats>();

        // Προσθήκη του κύριου παίκτη
        if (playerStats != null)
        {
            allies.Add(playerStats);
        }

        // ✅ Αν υπάρχουν άλλοι σύμμαχοι (π.χ. σε co-op mode, summoned units), προσθέστε τους εδώ

        return allies;
    }

}
