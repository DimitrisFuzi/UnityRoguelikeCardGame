using System;
using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using DG.Tweening;


/// <summary>
/// Handles the draw and discard piles, deck shuffling, and card drawing logic.
/// </summary>
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Deck Settings")]
    [Tooltip("Cards available to draw.")]
    [SerializeField] private List<Card> drawPile = new List<Card>();

    [Tooltip("Cards that have been discarded.")]
    [SerializeField] private List<Card> discardPile = new List<Card>();

    [Tooltip("Prefab used to instantiate card objects.")]
    [SerializeField] private GameObject cardPrefab;

    [Tooltip("Anchor for the draw pile in the UI.")]
    [SerializeField] private Transform drawPileAnchor;

    public static event Action OnDrawPileChanged;
    public static event Action OnDiscardPileChanged;

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

    /// <summary>
    /// Loads the player's deck into the draw pile and clears the discard pile.
    /// </summary>
    public void InitializeDeck()
    {
        drawPile = new List<Card>(PlayerDeck.Instance.GetDeck());
        discardPile.Clear();
    }

    /// <summary>
    /// Randomly shuffles the draw pile.
    /// </summary>
    public void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = drawPile.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (drawPile[k], drawPile[n]) = (drawPile[n], drawPile[k]);
        }

        Logger.Log("🔀 Deck shuffled: " + string.Join(", ", drawPile.ConvertAll(c => c.cardName)), this);
    }

    /// <summary>
    /// Draws a card from the draw pile into the player's hand.
    /// </summary>
    public void DrawCard()
    {
        if (drawPile.Count == 0)
            ReshuffleDiscardPile();

        if (drawPile.Count > 0 && HandManager.Instance.CurrentHandSize < HandManager.Instance.MaxHandSize)
        {
            Card drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            // Δημιουργία χωρίς parent αρχικά
            GameObject newCardObject = Instantiate(cardPrefab);
            RectTransform cardRect = newCardObject.GetComponent<RectTransform>();

            // Ορισμός γονέα (UI canvas)
            newCardObject.transform.SetParent(HandManager.Instance.handTransform, false);

            // Τοποθέτηση πάνω στο drawPileAnchor με ακριβή anchoredPosition
            Vector2 uiStartPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                HandManager.Instance.handTransform as RectTransform,
                RectTransformUtility.WorldToScreenPoint(null, drawPileAnchor.position),
                null, // ή βάλε Camera αν έχεις Screen Space - Camera
                out uiStartPos
            );
            cardRect.anchoredPosition = uiStartPos;

            cardRect.localScale = Vector3.one * 0.5f;
            cardRect.SetAsLastSibling();

            // Ενημέρωση περιεχομένου κάρτας
            CardDisplay display = newCardObject.GetComponent<CardDisplay>();
            display.cardData = drawnCard;
            display.UpdateCardDisplay();

            // Υπολογισμός στόχου
            Transform slotTarget = HandManager.Instance.GetNextCardSlotPosition();
            RectTransform slotRect = slotTarget.GetComponent<RectTransform>();

            Vector2 midPoint = (cardRect.anchoredPosition + slotRect.anchoredPosition) / 2f + Vector2.up * 100f;

            // Animation με arc και scale
            Sequence drawSeq = DOTween.Sequence();
            drawSeq.Append(cardRect.DOAnchorPos(midPoint, 0.25f).SetEase(Ease.OutQuad));
            drawSeq.Append(cardRect.DOAnchorPos(slotRect.anchoredPosition, 0.25f).SetEase(Ease.InQuad));
            drawSeq.Join(cardRect.DOScale(1f, 0.3f));
            drawSeq.OnComplete(() =>
            {
                HandManager.Instance.AddCardToHand(newCardObject);
                Destroy(slotTarget.gameObject);
            });

            NotifyDrawPileUI();
        }
    }





    /// <summary>
    /// Moves the specified card to the discard pile.
    /// </summary>
    /// <param name="card">The card to discard.</param>
    public void DiscardCard(Card card)
    {
        discardPile.Add(card);
        NotifyDiscardPileUI();
    }

    /// <summary>
    /// Reshuffles the discard pile into the draw pile.
    /// </summary>
    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        NotifyDiscardPileUI();
    }

    /// <summary>
    /// Returns the number of cards left in the draw pile.
    /// </summary>
    public int GetDrawPileCount() => drawPile.Count;

    /// <summary>
    /// Returns the number of cards in the discard pile.
    /// </summary>
    public int GetDiscardPileCount() => discardPile.Count;

    private void NotifyDrawPileUI() => OnDrawPileChanged?.Invoke();
    private void NotifyDiscardPileUI() => OnDiscardPileChanged?.Invoke();
}
