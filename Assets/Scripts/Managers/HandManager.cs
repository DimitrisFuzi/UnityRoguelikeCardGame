using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Manages the player's hand: draw, add, remove, arrange.
/// SceneSingleton for global access.
/// </summary>
public class HandManager : SceneSingleton<HandManager>
{
    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform handTransform;

    [Header("Layout Settings")]
    public float fanSpread = 7.5f;
    public float cardSpacing = 100f;
    public float verticalSpacing = 100f;

    [Header("Hand Size Settings")]
    [SerializeField] private int maxHandSize = 10;
    [SerializeField] private int startingHandSize = 5;

    public int MaxHandSize => maxHandSize;
    public int StartingHandSize => startingHandSize;
    public int CurrentHandSize => cardsInHand.Count;
    public bool IsDrawing { get; private set; }

    private readonly List<GameObject> cardsInHand = new();
    public IReadOnlyList<GameObject> CardsInHand => cardsInHand;

    [SerializeField] private Transform discardPileAnchor;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateHandLayout();
        }
    }
#endif

    /// <summary>Draws multiple cards with animation; updates layout after all draw operations complete.</summary>
    public IEnumerator DrawCardsRoutine(int count)
    {
        if (count <= 0) yield break;

        IsDrawing = true;
        for (int i = 0; i < count; i++)
            yield return DeckManager.Instance.DrawCardAsync().AsCoroutine();

        // Give one frame for new GOs to initialize before layout
        yield return null;
        UpdateHandLayout();
        IsDrawing = false;
    }

    /// <summary>Draw starting cards for a new turn.</summary>
    public void DrawCardsForTurn()
    {
        int spaceLeft = MaxHandSize - CurrentHandSize;
        int cardsToDraw = Mathf.Min(startingHandSize, spaceLeft);

        if (cardsToDraw > 0)
            StartCoroutine(DrawCardsRoutine(cardsToDraw));
    }

    /// <summary>Adds a new card GameObject to the hand and rearranges.</summary>
    public void AddCardToHand(GameObject cardObject)
    {
        if (CurrentHandSize >= maxHandSize)
        {
            Logger.LogWarning("Hand is full. Cannot add more cards.", this);
            return;
        }

        cardsInHand.Add(cardObject);
        UpdateHandLayout();
    }

    /// <summary>Updates the hand fan layout and flags each card as in hand.</summary>
    private void UpdateHandLayout()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.identity;
            cardsInHand[0].transform.localPosition = Vector3.zero;

            var cm0 = cardsInHand[0].GetComponent<CardMovement>();
            if (cm0) cm0.isInHand = true;

            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            var cm = cardsInHand[i].GetComponent<CardMovement>();
            if (cm) cm.isInHand = true;

            float angle = fanSpread * (i - (cardCount - 1) / 2f);
            float xOffset = cardSpacing * (i - (cardCount - 1) / 2f);
            float normalized = (2f * i / (cardCount - 1)) - 1f;
            float yOffset = verticalSpacing * (1f - normalized * normalized);

            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            cardsInHand[i].transform.localPosition = new Vector3(xOffset, yOffset, 0f);

            cm?.SaveOriginalTransform(); // save after layout update
        }
    }

    /// <summary>
    /// Removes a card GameObject from the hand, optionally destroys it, and handles discard/exhaust.
    /// </summary>
    public void RemoveCardFromHand(GameObject cardObject, bool destroyGO = true)
    {
        if (cardObject != null && cardsInHand.Contains(cardObject))
        {
            cardsInHand.Remove(cardObject);

            if (destroyGO)
            {
                var cm = cardObject.GetComponent<CardMovement>();
                if (cm != null) cm.enabled = false;
            }

            var cd = cardObject.GetComponent<CardDisplay>();
            if (cd != null)
            {
                if (cd.cardData.exhaustAfterUse)
                {
                    Logger.Log($"[HandManager] '{cd.cardData.cardName}' was exhausted.", this);
                    // exhausted cards do not go to discard here
                }
                else
                {
                    DeckManager.Instance?.DiscardCard(cd.cardData);
                }
            }

            var rect = cardObject.GetComponent<RectTransform>();
            if (rect != null) rect.DOKill();

            UpdateHandLayout();

            if (destroyGO)
                Destroy(cardObject, 0.01f);
        }
        else
        {
            Logger.LogWarning("Tried to remove null or not found card.", this);
        }
    }

    // Force-discard helper that ignores exhaustAfterUse (used for end turn / unplayed cards)
    private void RemoveCardFromHandToDiscard(GameObject cardObject)
    {
        if (cardObject != null && cardsInHand.Contains(cardObject))
        {
            cardsInHand.Remove(cardObject);

            var cm = cardObject.GetComponent<CardMovement>();
            if (cm != null) cm.enabled = false;

            var cd = cardObject.GetComponent<CardDisplay>();
            if (cd != null)
            {
                DeckManager.Instance?.DiscardCard(cd.cardData);
            }

            var rect = cardObject.GetComponent<RectTransform>();
            if (rect != null) rect.DOKill();

            UpdateHandLayout();
            Destroy(cardObject, 0.01f);
        }
        else
        {
            Logger.LogWarning("Tried to remove null or not found card (ToDiscard).", this);
        }
    }

    /// <summary>Returns the next available card slot position in the hand layout.</summary>
    public Transform GetNextCardSlotPosition()
    {
        GameObject tempCard = new GameObject("TempCard", typeof(RectTransform));
        tempCard.transform.SetParent(handTransform, false);
        cardsInHand.Add(tempCard); // temporarily include in layout
        UpdateHandLayout();

        Vector3 pos = tempCard.GetComponent<RectTransform>().anchoredPosition;
        cardsInHand.Remove(tempCard);
        Destroy(tempCard);

        return CreateTempAnchorAt(pos);
    }

    /// <summary>Creates a temporary anchor at the specified local anchored position.</summary>
    private Transform CreateTempAnchorAt(Vector3 anchoredPos)
    {
        GameObject anchor = new GameObject("CardTargetAnchor", typeof(RectTransform));
        anchor.transform.SetParent(handTransform, false);
        RectTransform rect = anchor.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        return anchor.transform;
    }

    /// <summary>Animates card movement to the discard pile anchor and then removes it to discard.</summary>
    public IEnumerator AnimateDiscardAndRemoveCard(GameObject card)
    {
        var rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Logger.LogWarning("Tried to discard a card without RectTransform.", this);
            yield break;
        }

        if (discardPileAnchor == null)
        {
            Logger.LogWarning("Discard pile anchor is not assigned.", this);
            yield break;
        }

        float duration = 0.5f;

        rectTransform.SetAsLastSibling();

        rectTransform.DOMove(discardPileAnchor.position, duration).SetEase(Ease.InBack);
        rectTransform.DOScale(Vector3.zero, duration);
        rectTransform.DORotate(new Vector3(0, 0, 180f), duration, RotateMode.FastBeyond360);

        yield return new WaitForSeconds(duration);

        // Always discard unplayed cards at end turn
        RemoveCardFromHandToDiscard(card);
    }

    /// <summary>Discards all cards in hand; animated if requested.</summary>
    public IEnumerator DiscardHandRoutine(bool animated = true)
    {
        var snapshot = new List<GameObject>(cardsInHand);

        if (animated && discardPileAnchor != null)
        {
            // Animate all in parallel
            foreach (var card in snapshot)
            {
                if (card != null)
                    StartCoroutine(AnimateDiscardAndRemoveCard(card));
            }
            // Allow tweens to complete (matches duration in AnimateDiscardAndRemoveCard)
            yield return new WaitForSeconds(0.55f);
        }
        else
        {
            // Instant discard without animation
            foreach (var card in snapshot)
            {
                if (card != null)
                    RemoveCardFromHandToDiscard(card);
            }
            yield return null;
        }

        UpdateHandLayout();
    }

    /// <summary>Instantly discards the entire hand (no animation).</summary>
    public void DiscardEntireHandInstant()
    {
        var snapshot = new List<GameObject>(cardsInHand);
        foreach (var card in snapshot)
        {
            if (card != null)
                RemoveCardFromHandToDiscard(card);
        }
        UpdateHandLayout();
    }

    /// <summary>Convenience method to discard the whole hand with animation.</summary>
    public void DiscardHand()
    {
        StartCoroutine(DiscardHandRoutine(animated: true));
    }
}
