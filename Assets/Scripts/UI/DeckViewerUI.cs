using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyProjectF.Assets.Scripts.Cards;

public class DeckViewerUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject windowRoot;     // DeckViewerWindow
    [SerializeField] private CanvasGroup windowCanvas;
    [SerializeField] private float fadeSeconds = 0.15f;

    [Header("Scroll & Grid")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentRoot;     // Content (GridLayoutGroup + ContentSizeFitter)
    [SerializeField] private CardDisplay cardThumbnailPrefab;

    private readonly List<CardDisplay> pool = new();
    private bool isOpen, isAnimating;

    public void Toggle()
    {
        if (isAnimating) return;
        if (isOpen) Hide(); else Show();
    }

    public void Show()
    {
        isOpen = true;
        SetWindowActive(true);
        Populate();
        if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
    }

    public void Hide()
    {
        isOpen = false;
        SetWindowActive(false);
    }

    private void Populate()
    {
        var deck = GetCurrentDeck();
        if (deck == null || contentRoot == null || cardThumbnailPrefab == null) return;

        EnsurePool(deck.Count);
        for (int i = 0; i < pool.Count; i++)
        {
            bool active = i < deck.Count;
            var view = pool[i];
            view.gameObject.SetActive(active);
            if (!active) continue;

            // bind πάνω στα δικά σου components
            view.cardData = deck[i];
            view.UpdateCardDisplay();
        }
    }

    private void EnsurePool(int needed)
    {
        while (pool.Count < needed)
        {
            var item = Instantiate(cardThumbnailPrefab, contentRoot);
            MakeReadOnly(item);
            pool.Add(item);
        }
    }

    private void MakeReadOnly(CardDisplay view)
    {
        // Αν έχεις interactive components (drag/hover/movement), κάν’ τα disable εδώ.
        // π.χ.:
        // var mv = view.GetComponent<CardMovement>(); if (mv) mv.enabled = false;
        // var hover = view.GetComponent<YourHoverScript>(); if (hover) hover.enabled = false;
    }

    // ---- Deck source ----
    private List<Card> GetCurrentDeck()
    {
        var pdSingleton = Object.FindFirstObjectByType<PlayerDeck>();
        if (pdSingleton != null)
        {
            // Αν έβαλες την property:
            var prop = pdSingleton.GetType().GetProperty("CurrentDeck");
            if (prop != null)
            {
                var list = prop.GetValue(pdSingleton) as IEnumerable<Card>;
                if (list != null) return list.ToList();
            }

            // αλλιώς δοκίμασε κοινά field names:
            var f = pdSingleton.GetType().GetField("playerDeck") ??
                    pdSingleton.GetType().GetField("cards") ??
                    pdSingleton.GetType().GetField("deck");
            if (f != null)
            {
                var listObj = f.GetValue(pdSingleton) as IEnumerable<Card>;
                if (listObj != null) return listObj.ToList();
            }
        }

        return new List<Card>();
    }

    // ---- Window visuals ----
    private void SetWindowActive(bool active)
    {
        if (!windowRoot) return;

        if (!windowCanvas)
        {
            // αν δεν έχεις CanvasGroup, απλώς κάνε toggle
            windowRoot.SetActive(active);
            return;
        }

        StopAllCoroutines();

        if (active)
        {
            // ΠΡΩΤΑ ενεργοποιούμε το GO για να μπορεί να ξεκινήσει coroutine
            if (!windowRoot.activeSelf) windowRoot.SetActive(true);

            // προετοιμασία για fade-in
            windowCanvas.blocksRaycasts = true;
            windowCanvas.interactable = true;
            if (windowCanvas.alpha <= 0f) windowCanvas.alpha = 0f;
        }

        StartCoroutine(FadeRoutine(active));
    }

    private System.Collections.IEnumerator FadeRoutine(bool show)
    {
        isAnimating = true;

        float start = windowCanvas.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime; // λειτουργεί και σε pause
            windowCanvas.alpha = Mathf.Lerp(start, end, t / fadeSeconds);
            yield return null;
        }
        windowCanvas.alpha = end;

        if (!show)
        {
            windowCanvas.blocksRaycasts = false;
            windowCanvas.interactable = false;
            windowRoot.SetActive(false); // απενεργοποίησέ το στο τέλος του fade-out
        }

        isAnimating = false;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) Hide();
    }
}
