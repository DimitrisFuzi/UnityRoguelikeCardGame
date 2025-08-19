using UnityEngine;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Cards;

[CreateAssetMenu(menuName = "Game/Reward Pool")]
public class RewardPool : ScriptableObject
{
    // Τελικές επιλογές (serializable class, όχι ScriptableObjects)
    public List<RewardDefinition> candidates = new();

    [Header("Rarity weights (relative)")]
    public float commonWeight = 1f;
    public float uncommonWeight = 0.35f;
    public float rareWeight = 0.12f;
    public float legendaryWeight = 0.03f;

    [Header("Database (optional)")]
    public ScriptableObject database;                // ΔΕΝ αλλάζω τον τύπο σου — κρατάμε ό,τι έχεις
    public bool autoPopulateFromDatabaseOnPlay = true;

    void OnEnable()
    {
        if (autoPopulateFromDatabaseOnPlay && database != null)
            PopulateFromDatabase();
    }

    /// <summary>
    /// Γεμίζει το pool από το database.allCards.
    /// Κάνει cast σε Card και αγνοεί οτιδήποτε δεν είναι Card.
    /// </summary>
    public void PopulateFromDatabase()
    {
        if (database == null) return;

        // Περιμένουμε ότι το database έχει ένα public πεδίο/ιδιότητα "allCards" που είναι IEnumerable
        var allCardsField = database.GetType().GetField("allCards");
        var allCardsProp = database.GetType().GetProperty("allCards");
        IEnumerable<Object> source = null;

        if (allCardsField != null)
            source = allCardsField.GetValue(database) as IEnumerable<Object>;
        else if (allCardsProp != null)
            source = allCardsProp.GetValue(database) as IEnumerable<Object>;

        candidates = new List<RewardDefinition>();

        if (source != null)
        {
            foreach (var so in source)
            {
                if (so == null) continue;
                var card = so as Card;                 // 🔑 εδώ γίνεται το cast
                if (card == null) continue;            // αγνόησε οτιδήποτε δεν είναι Card

                candidates.Add(new RewardDefinition
                {
                    cardData = card,
                    weight = 1
                });
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

    }

    // Επιλογή 3 καρτών με βάρη βάσει rarity, χωρίς επανάληψη
    public List<RewardDefinition> RollCardChoices(int count, int seed)
        => RollCardChoicesFromSource(candidates, count, new System.Random(seed));

    public List<RewardDefinition> RollCardChoicesFromSource(List<RewardDefinition> source, int count, System.Random rng)
    {
        var pool = new List<RewardDefinition>(source ?? new List<RewardDefinition>());
        var picked = new List<RewardDefinition>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            float total = 0f;
            foreach (var c in pool) total += Mathf.Max(0.0001f, EffectiveWeight(c));

            double roll = rng.NextDouble() * total;
            float acc = 0f;
            int chosenIndex = pool.Count - 1;

            for (int j = 0; j < pool.Count; j++)
            {
                acc += Mathf.Max(0.0001f, EffectiveWeight(pool[j]));
                if (roll <= acc) { chosenIndex = j; break; }
            }

            picked.Add(pool[chosenIndex]);
            pool.RemoveAt(chosenIndex);
        }

        if (picked.Count < count)
            Debug.LogWarning("[Reward] Το pool δεν είχε αρκετές μοναδικές επιλογές.");

        return picked;
    }

    // Βάρος με βάση το rarity του Card
    float EffectiveWeight(RewardDefinition def)
    {
        if (def == null || def.cardData == null) return commonWeight;

        switch (def.cardData.cardRarity)
        {
            case Card.CardRarity.Common: return commonWeight;
            case Card.CardRarity.Uncommon: return uncommonWeight;
            case Card.CardRarity.Rare: return rareWeight;
            case Card.CardRarity.Legendary: return legendaryWeight;
            default: return commonWeight;
        }
    }
}
