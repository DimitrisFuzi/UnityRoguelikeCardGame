using UnityEngine;
using System.Collections.Generic;
using System;
using MyProjectF.Assets.Scripts.Cards;

[CreateAssetMenu(menuName = "Game/Reward Pool")]
public class RewardPool : ScriptableObject
{
    public List<RewardDefinition> candidates = new();

    [Header("Rarity weights (relative)")]
    public float commonWeight = 1f;
    public float uncommonWeight = 0.35f;
    public float rareWeight = 0.12f;
    public float legendaryWeight = 0.03f;

    [Header("Database (optional)")]
    public ScriptableObject database;                // expects a field or property named "allCards"
    public bool autoPopulateFromDatabaseOnPlay = true;

    private void OnEnable()
    {
        if (autoPopulateFromDatabaseOnPlay && database != null)
            PopulateFromDatabase();
    }

    /// <summary>
    /// Fills the pool from database.allCards (casts entries to Card and ignores non-Card items).
    /// </summary>
    public void PopulateFromDatabase()
    {
        if (database == null) return;

        var allCardsField = database.GetType().GetField("allCards");
        var allCardsProp = database.GetType().GetProperty("allCards");

        IEnumerable<UnityEngine.Object> source = null;
        if (allCardsField != null)
            source = allCardsField.GetValue(database) as IEnumerable<UnityEngine.Object>;
        else if (allCardsProp != null)
            source = allCardsProp.GetValue(database) as IEnumerable<UnityEngine.Object>;

        candidates = new List<RewardDefinition>();

        if (source != null)
        {
            foreach (var so in source)
            {
                var card = so as Card;
                if (card == null) continue;

                candidates.Add(new RewardDefinition { cardData = card, weight = 1 });
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>Selects distinct card definitions with weighted rarity.</summary>
    public List<RewardDefinition> RollCardChoices(int count, int seed)
        => RollCardChoicesFromSource(candidates, count, new System.Random(seed));

    public List<RewardDefinition> RollCardChoicesFromSource(List<RewardDefinition> source, int count, System.Random rng)
    {
        var pool = new List<RewardDefinition>(source ?? new List<RewardDefinition>());
        var picked = new List<RewardDefinition>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            float total = 0f;
            for (int j = 0; j < pool.Count; j++) total += Mathf.Max(0.0001f, EffectiveWeight(pool[j]));

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
            Logger.LogWarning("[Reward] Pool had fewer unique choices than requested.");

        return picked;
    }

    private float EffectiveWeight(RewardDefinition def)
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
