using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CreateAssetMenu(menuName = "Game/Reward Pool")]
public class RewardPool : ScriptableObject
{
    // Τα RewardDefinition εδώ είναι wrappers που δείχνουν στο κανονικό card asset
    public List<RewardDefinition> candidates = new();

    [Header("Rarity weights (relative)")]
    public float commonWeight = 1f;
    public float rareWeight = 0.35f;
    public float epicWeight = 0.12f;
    public float legendaryWeight = 0.03f;

    [Header("Database (optional)")]
    public CardDatabase database;
    public bool autoPopulateFromDatabaseOnPlay = true;

    void OnEnable()
    {
        if (autoPopulateFromDatabaseOnPlay && database != null)
            PopulateFromDatabase();
    }

    // Γεμίζει το pool από το CardDatabase (χωρίς περιγραφή/εικονίδιο)
    public void PopulateFromDatabase()
    {
        if (database == null) return;

        candidates.Clear();
        foreach (var card in database.allCards)
        {
            if (!card) continue;

            // Δημιουργούμε runtime RewardDefinition wrapper
            var def = ScriptableObject.CreateInstance<RewardDefinition>();
            def.name = "RT_" + card.name;
            def.cardAsset = card;

            candidates.Add(def);
        }
    }

    // Weighted επιλογή από ΟΛΟ το pool
    public List<RewardDefinition> RollCardChoices(int count, int seed)
        => RollCardChoicesFromSource(candidates, count, new System.Random(seed));

    // Weighted επιλογή χωρίς επανάληψη από συγκεκριμένη λίστα (helper για guaranteed legendary κ.λπ.)
    public List<RewardDefinition> RollCardChoicesFromSource(List<RewardDefinition> source, int count, System.Random rng)
    {
        var pool = new List<RewardDefinition>(source);
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
            Debug.LogWarning("[Reward] Το pool/υποσύνολο δεν είχε αρκετές μοναδικές επιλογές.");

        return picked;
    }

    // Υπολογισμός weight από το rarity του card asset
    float EffectiveWeight(RewardDefinition def)
    {
        string rar = GetCardRarityName(def.cardAsset);
        return rar switch
        {
            "Common" => commonWeight,
            "Rare" => rareWeight,
            "Epic" => epicWeight,
            "Legendary" => legendaryWeight,
            _ => commonWeight
        };
    }

    // Διαβάζει rarity από το card asset.
    // Αν το έχεις string (π.χ. "Epic"), το παίρνει ως έχει.
    // Αν είναι int/enum, επιστρέφει εύλογο όνομα.
    string GetCardRarityName(ScriptableObject cardAsset)
    {
        if (!cardAsset) return "Common";
        var t = cardAsset.GetType();

        // 1) Προσπάθησε ως string πεδίο "cardRarity"
        var f = t.GetField("cardRarity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            if (f.FieldType == typeof(string))
            {
                try { return (string)f.GetValue(cardAsset); } catch { }
            }
            else if (f.FieldType.IsEnum)
            {
                try { return f.GetValue(cardAsset).ToString(); } catch { }
            }
            else if (f.FieldType == typeof(int))
            {
                try
                {
                    int v = (int)f.GetValue(cardAsset);
                    // Προσαρμόζεις mapping αν στο project είναι άλλο
                    return v switch { 0 => "Common", 1 => "Rare", 2 => "Epic", 3 => "Legendary", _ => "Common" };
                }
                catch { }
            }
        }

        // 2) Εναλλακτικό όνομα πεδίου (αν στο project σου λέγεται αλλιώς)
        var fAlt = t.GetField("rarity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (fAlt != null)
        {
            if (fAlt.FieldType == typeof(string))
            {
                try { return (string)fAlt.GetValue(cardAsset); } catch { }
            }
            else if (fAlt.FieldType.IsEnum)
            {
                try { return fAlt.GetValue(cardAsset).ToString(); } catch { }
            }
            else if (fAlt.FieldType == typeof(int))
            {
                try
                {
                    int v = (int)fAlt.GetValue(cardAsset);
                    return v switch { 0 => "Common", 1 => "Rare", 2 => "Epic", 3 => "Legendary", _ => "Common" };
                }
                catch { }
            }
        }

        return "Common";
    }

    // Optional helpers αν τα χρειαστείς για guaranteed legendary κ.λπ.
    public List<RewardDefinition> OfRarity(string rarityName)
    {
        var list = new List<RewardDefinition>();
        foreach (var r in candidates)
            if (GetCardRarityName(r.cardAsset) == rarityName) list.Add(r);
        return list;
    }
}
