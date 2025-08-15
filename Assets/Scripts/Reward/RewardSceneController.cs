using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardSceneController : MonoBehaviour
{
    [Header("Setup")]
    public RewardPool pool;
    public Transform cardParent;      // container με Horizontal/Grid Layout
    public RewardCardView cardPrefab;
    public Button continueButton;
    public TMP_Text headerText;

    [Header("Flow")]
    public SceneType nextSceneAfterReward = SceneType.Victory;

    private readonly List<RewardCardView> spawned = new();
    private bool choiceMade = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (continueButton) continueButton.gameObject.SetActive(false);
        if (cardParent) cardParent.gameObject.SetActive(true);

        // αν το pool είναι άδειο, γέμισέ το από DB
        if (pool != null && (pool.candidates == null || pool.candidates.Count == 0))
        {
            var before = pool.candidates == null ? 0 : pool.candidates.Count;
            pool.PopulateFromDatabase();
            Debug.Log($"[Reward] Pool was empty ({before}); populated from DB -> {pool.candidates?.Count ?? 0}");
        }

        SpawnCards();
    }

    void SpawnCards()
    {
        ClearSpawned();
        int seed = System.Environment.TickCount;
        var defs = pool.RollCardChoices(3, seed);
        foreach (var d in defs)
        {
            var card = Instantiate(cardPrefab, cardParent);
            card.Setup(d, OnCardChosen);
            spawned.Add(card);
        }

        Debug.Log($"[Reward] candidates: {pool.candidates?.Count}");
    }

    void ClearSpawned()
    {
        foreach (var c in spawned) if (c) Destroy(c.gameObject);
        spawned.Clear();
        choiceMade = false;
    }

    void OnCardChosen(RewardCardView chosen)
    {
        if (choiceMade) return;
        choiceMade = true;

        foreach (var c in spawned) c.Interactable(false);

        // ✅ Παίρνουμε κατευθείαν το όνομα από την κάρτα
        string cardName = (chosen != null && chosen.def != null && chosen.def.cardData != null)
            ? chosen.def.cardData.cardName
            : null;

        ApplyCard(cardName);

        if (headerText) headerText.text = $"Πήρες: {(!string.IsNullOrEmpty(cardName) ? cardName : "(καμία)")}";

        if (continueButton)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoNext);
        }
    }


    void ApplyCard(string cardName)
    {
        if (!string.IsNullOrEmpty(cardName) && PlayerDeck.Instance != null)
            PlayerDeck.Instance.AddCardToDeck(cardName);
        else
            Debug.LogWarning($"[Reward] Άκυρο cardName ή λείπει PlayerDeck: '{cardName}'");
    }

    void GoNext()
    {
        var sf = FindAnyObjectByType<SceneFlowManager>();
        if (sf != null) sf.LoadScene(nextSceneAfterReward);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneAfterReward.ToString());
    }
}
