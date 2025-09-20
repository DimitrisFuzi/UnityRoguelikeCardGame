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
        // Disable interactive components on the thumbnail so it behaves as a static preview.
        var mv = view.GetComponent<CardMovement>(); if (mv) mv.enabled = false;
        var raycast = view.GetComponent<UnityEngine.UI.Image>(); if (raycast) raycast.raycastTarget = false;
    }

    // ---- Deck source ----
    private List<Card> GetCurrentDeck()
    {
        var pdSingleton = Object.FindFirstObjectByType<PlayerDeck>();
        if (pdSingleton != null)
        {
            var prop = pdSingleton.GetType().GetProperty("CurrentDeck");
            if (prop != null)
            {
                var list = prop.GetValue(pdSingleton) as IEnumerable<Card>;
                if (list != null) return list.ToList();
            }

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
            windowRoot.SetActive(active);
            return;
        }

        StopAllCoroutines();

        if (active)
        {
            if (!windowRoot.activeSelf) windowRoot.SetActive(true);

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
            t += Time.unscaledDeltaTime; // works during pause
            windowCanvas.alpha = Mathf.Lerp(start, end, t / fadeSeconds);
            yield return null;
        }
        windowCanvas.alpha = end;

        if (!show)
        {
            windowCanvas.blocksRaycasts = false;
            windowCanvas.interactable = false;
            windowRoot.SetActive(false);
        }

        isAnimating = false;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) Hide();
    }
}
