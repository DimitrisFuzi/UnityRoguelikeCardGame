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
    public SceneType nextSceneAfterReward = SceneType.Victory; // άλλαξέ το αν θες

    private readonly List<RewardCardView> spawned = new();
    private bool choiceMade = false;

    void Start()
    {
        if (continueButton) continueButton.gameObject.SetActive(false);

        SpawnCards();
    }

    void SpawnCards()
    {
        ClearSpawned();
        int seed = System.Environment.TickCount; // ή πέρασε deterministic seed αν θέλεις
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

        // outcome: πάντα Card
        var outcome = chosen.def.ToOutcome();


        ApplyCard(outcome.cardName);

        if (headerText) headerText.text = $"Πήρες: {outcome.cardName}";

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
            Debug.LogWarning($"[Reward] ’κυρο cardName ή λείπει PlayerDeck: '{cardName}'");

    }

    void GoNext()
    {
        var sf = FindAnyObjectByType<SceneFlowManager>();
        if (sf != null) sf.LoadScene(nextSceneAfterReward);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneAfterReward.ToString());
    }

}
