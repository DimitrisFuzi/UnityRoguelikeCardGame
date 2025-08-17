using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Cards;

public class RewardSceneController : MonoBehaviour
{
    [Header("Setup")]
    public RewardPool pool;
    public Transform cardParent;      // container με Horizontal/Grid Layout
    public RewardCardView cardPrefab;
    public Button continueButton;
    public TMP_Text headerText;

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

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)cardParent);
        Canvas.ForceUpdateCanvases();

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

        // ⬇️ πάρε το όνομα
        string cardName = chosen?.def?.cardData ? chosen.def.cardData.cardName : null;
        ApplyCard(cardName);

        if (headerText) headerText.text = $"Πήρες: {cardName}";
        if (continueButton)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoNext);
        }
    }




    void ApplyCard(string cardName)
    {
        if (string.IsNullOrEmpty(cardName))
        {
            Debug.LogWarning("[Reward] Άκυρο cardName.");
            return;
        }

        var deck = PlayerDeck.Instance ?? FindAnyObjectByType<PlayerDeck>();
        if (deck == null)
        {
            Debug.LogError("[Reward] PlayerDeck λείπει. Κάνε το persistent (DontDestroyOnLoad) ή βάλε ένα PlayerDeck στη Reward1 για testing.");
            return;
        }

        deck.AddCardToDeck(cardName);  // ✅ ταιριάζει με την υπάρχουσα υπογραφή
    }



    void GoNext()
    {
        var sf = FindAnyObjectByType<SceneFlowManager>();
        if (sf != null)
        {
            sf.LoadNextAfterBattle();   // χρησιμοποίησε το flow map
        }
        else
        {
            Debug.LogWarning("[Reward] SceneFlowManager not found, loading Victory as fallback.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneType.Victory.ToString());
        }
    }

}
