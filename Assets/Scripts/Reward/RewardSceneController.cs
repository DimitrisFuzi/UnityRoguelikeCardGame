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

    [SerializeField] private RectTransform selectedAnchor;
    [SerializeField] private float disappearDuration = 0.25f;
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float zoomScale = 1.15f;

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

        // ➜ κάνε fade-out/καταστροφή στις υπόλοιπες
        foreach (var c in spawned)
        {
            if (c != chosen && c != null)
                StartCoroutine(FadeAndDestroy(c.gameObject, disappearDuration));
        }

        // ➜ animate την επιλεγμένη προς κέντρο + zoom-in
        StartCoroutine(AnimateChosenToCenter(chosen));

        // business logic ως έχει
        string cardName = chosen?.def?.cardData ? chosen.def.cardData.cardName : null;
        ApplyCard(cardName);

        if (headerText) headerText.text = $"{cardName} was added to the deck";
        if (continueButton)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(GoNext);
        }
    }

    System.Collections.IEnumerator FadeAndDestroy(GameObject go, float duration)
    {
        if (!go) yield break;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        float t = 0f;
        float start = cg.alpha;

        // προαιρετικά: κλείσε raycasts
        cg.blocksRaycasts = false;
        cg.interactable = false;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
        Destroy(go);
    }

    System.Collections.IEnumerator AnimateChosenToCenter(RewardCardView chosen)
    {
        if (chosen == null) yield break;

        // 1) Απενεργοποίησε προσωρινά το layout του container
        var h = cardParent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        var g = cardParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        bool hadH = h && h.enabled;
        bool hadG = g && g.enabled;
        if (h) h.enabled = false;
        if (g) g.enabled = false;

        var rt = chosen.transform as RectTransform;

        // 2) Βεβαιώσου ότι έχουμε CanvasGroup για smooth fade/hold
        var cg = chosen.GetComponent<CanvasGroup>();
        if (!cg) cg = chosen.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; // ήδη μη-interactive

        // 3) Υπολόγισε στόχο (anchor αν δώσεις, αλλιώς κέντρο root canvas)
        RectTransform rootCanvasRt = null;
        var canvas = GetComponentInParent<Canvas>();
        if (!canvas) canvas = FindAnyObjectByType<Canvas>();
        if (canvas) rootCanvasRt = canvas.transform as RectTransform;

        // Κρατάμε αρχικές τιμές για smooth lerp
        Vector3 startPos = rt.position;
        Vector3 startScale = rt.localScale;

        Vector3 targetPos;
        Transform originalParent = rt.parent;

        if (selectedAnchor != null)
        {
            // Βάλε το chosen στον ίδιο parent με το anchor για απλό position tween στο ίδιο space
            rt.SetParent(selectedAnchor.parent, worldPositionStays: true);
            targetPos = selectedAnchor.position;
        }
        else if (rootCanvasRt != null)
        {
            // Χωρίς anchor: στόχος το κέντρο του καμβά
            rt.SetParent(rootCanvasRt, worldPositionStays: true);
            targetPos = rootCanvasRt.TransformPoint(rootCanvasRt.rect.center);
        }
        else
        {
            // Fallback: μικρό κέντρο οθόνης
            targetPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }

        // 4) Tween θέση/κλίμακα
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / moveDuration));
            rt.position = Vector3.Lerp(startPos, targetPos, k);
            rt.localScale = Vector3.Lerp(startScale, Vector3.one * zoomScale, k);
            yield return null;
        }
        rt.position = targetPos;
        rt.localScale = Vector3.one * zoomScale;

        // 5) (προαιρετικό) Αν θες να μείνει στο anchor layout-free, μην το επιστρέψεις στον αρχικό parent.
        // Αν θέλεις να ξαναενεργοποιηθεί το layout για τα υπόλοιπα UI, μπορείς τώρα:
        if (h && hadH) h.enabled = true;
        if (g && hadG) g.enabled = true;
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
