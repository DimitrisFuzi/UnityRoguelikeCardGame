using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Manages the player's hand: draw, add, remove, arrange.
/// Singleton for global access.
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

    private IEnumerator DrawStartingCardsCoroutine()
    {
        Debug.Log($"🃏 Starting hand: current={CurrentHandSize}, starting={startingHandSize}, max={MaxHandSize}");

        int cardsToDraw = Mathf.Min(startingHandSize, MaxHandSize - CurrentHandSize);

        for (int i = 0; i < cardsToDraw; i++)
        {
            yield return DeckManager.Instance.DrawCardAsync().AsCoroutine();
        }
    }

    private IEnumerator DrawMultipleCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return DeckManager.Instance.DrawCardAsync().AsCoroutine();
        }
    }


    /// <summary>
    /// Draw starting cards for a new turn.
    /// </summary>
    public void DrawCardsForTurn()
    {
        int spaceLeft = MaxHandSize - CurrentHandSize;
        int cardsToDraw = Mathf.Min(startingHandSize, spaceLeft);

        if (cardsToDraw > 0)
            StartCoroutine(DrawMultipleCards(cardsToDraw));
    }




    /// <summary>
    /// Adds a new card GameObject to the hand and rearranges.
    /// </summary>
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

    /// <summary>
    /// Updates the hand fan layout and flags each card as in hand.
    /// </summary>
    private void UpdateHandLayout()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.identity;
            cardsInHand[0].transform.localPosition = Vector3.zero;

            // Mark as in hand
            var cm = cardsInHand[0].GetComponent<CardMovement>();
            if (cm) cm.isInHand = true;

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

            cm?.SaveOriginalTransform(); // ✅ save after layout
        }
    }

    /// <summary>
    /// Removes a card GameObject from the hand, discards it, and destroys the GameObject.
    /// </summary>
    /// <param name="cardObject">The card GameObject to remove.</param>
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
                    Debug.Log($"[HandManager] '{cd.cardData.cardName}' was exhausted.");
                    // exhaust → δεν πάει στη discard εδώ
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
            Logger.LogWarning("Tried to remove null or not found card", this);
        }
    }


    // NEW: Force-discard helper που αγνοεί το exhaustAfterUse (για END TURN / unplayed)
    private void RemoveCardFromHandToDiscard(GameObject cardObject)
    {
        if (cardObject != null && cardsInHand.Contains(cardObject))
        {
            cardsInHand.Remove(cardObject);

            // Disable CardMovement για να μην έχουμε hover/drag κατά την απομάκρυνση
            var cm = cardObject.GetComponent<CardMovement>();
            if (cm != null) cm.enabled = false;

            // ΠΑΝΤΑ discard για unplayed κάρτες στο τέλος γύρου
            var cd = cardObject.GetComponent<CardDisplay>();
            if (cd != null)
            {
                DeckManager.Instance?.DiscardCard(cd.cardData);
            }

            // Kill τυχόν DOTweens
            var rect = cardObject.GetComponent<RectTransform>();
            if (rect != null) rect.DOKill();

            UpdateHandLayout();
            Destroy(cardObject, 0.01f);
        }
        else
        {
            Logger.LogWarning("Tried to remove null or not found card (ToDiscard)", this);
        }
    }


    /// <summary>
    /// Returns the next available card slot position in the hand layout.
    /// </summary> 
    public Transform GetNextCardSlotPosition()
    {
        GameObject tempCard = new GameObject("TempCard", typeof(RectTransform));
        tempCard.transform.SetParent(handTransform, false);
        cardsInHand.Add(tempCard); // προσωρινά για layout
        UpdateHandLayout();

        Vector3 pos = tempCard.GetComponent<RectTransform>().anchoredPosition;
        cardsInHand.Remove(tempCard);
        Destroy(tempCard);

        return CreateTempAnchorAt(pos);
    }

    /// <summary>
    /// Creates a temporary anchor at the specified position.
    /// </summary>
    private Transform CreateTempAnchorAt(Vector3 anchoredPos)
    {
        GameObject anchor = new GameObject("CardTargetAnchor", typeof(RectTransform));
        anchor.transform.SetParent(handTransform, false);
        RectTransform rect = anchor.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        return anchor.transform;
    }

    public IEnumerator AnimateDiscardAndRemoveCard(GameObject card)
    {
        var rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Logger.LogWarning("Tried to discard a card without RectTransform", this);
            yield break;
        }

        if (discardPileAnchor == null)
        {
            Logger.LogWarning("Discard pile anchor is not assigned!", this);
            yield break;
        }

        float duration = 0.5f;

        rectTransform.SetAsLastSibling(); // να φαίνεται μπροστά

        rectTransform.DOMove(discardPileAnchor.position, duration).SetEase(Ease.InBack);
        rectTransform.DOScale(Vector3.zero, duration);
        rectTransform.DORotate(new Vector3(0, 0, 180f), duration, RotateMode.FastBeyond360);

        yield return new WaitForSeconds(duration);

        RemoveCardFromHandToDiscard(card); // ✅ ΠΑΝΤΑ discard στο end turn

    }

    public IEnumerator DiscardHandRoutine(bool animated = true)
    {
        // snapshot για ασφαλή iterate
        var snapshot = new List<GameObject>(cardsInHand);

        if (animated && discardPileAnchor != null)
        {
            // με animation (παράλληλα)
            foreach (var card in snapshot)
            {
                if (card != null)
                    StartCoroutine(AnimateDiscardAndRemoveCard(card));
            }
            // μικρό wait ώστε να προλάβουν τα tweens (ίδιο με το duration στο AnimateDiscardAndRemoveCard)
            yield return new WaitForSeconds(0.55f);
        }
        else
        {
            // άμεσο discard χωρίς animation
            foreach (var card in snapshot)
            {
                if (card != null)
                    RemoveCardFromHandToDiscard(card);
            }
            yield return null;
        }

        UpdateHandLayout();
    }

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

    public void DiscardHand()
    {
        // animated discard όλoυ του χεριού (ή άλλαξε σε false για instant)
        StartCoroutine(DiscardHandRoutine(animated: true));
    }

}
