using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Manages the player's hand: draw, add, remove, arrange.
/// Singleton for global access.
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Logger.LogWarning("Duplicate HandManager instance found. Destroying duplicate.", this);
            Destroy(gameObject);
        }
    }

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
    public void RemoveCardFromHand(GameObject cardObject)
    {
        if (cardObject != null && cardsInHand.Contains(cardObject))
        {
            cardsInHand.Remove(cardObject);

            // Disable CardMovement to prevent hover/drag during destroy delay
            var cm = cardObject.GetComponent<CardMovement>();
            if (cm != null) cm.enabled = false;

            // Discard the card data if not exhausted
            var cd = cardObject.GetComponent<CardDisplay>();
            if (cd != null)
            {
                if (cd.cardData.exhaustAfterUse)
                {
                    Debug.Log($"[HandManager] '{cd.cardData.cardName}' was exhausted.");
                    // Skip discard
                }
                else
                {
                    DeckManager.Instance?.DiscardCard(cd.cardData);
                }
            }


            // ✅ Kill all active DOTween tweens (position, scale, etc.)
            var rect = cardObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill(); // Stop all tweens targeting this RectTransform
            }

            // Update hand layout
            UpdateHandLayout();

            // Destroy the GameObject after a tiny delay
            Destroy(cardObject, 0.01f);
        }
        else
        {
            Logger.LogWarning("Tried to remove null or not found card", this);
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

        RemoveCardFromHand(card); // Περιλαμβάνει το Destroy
    }


}
