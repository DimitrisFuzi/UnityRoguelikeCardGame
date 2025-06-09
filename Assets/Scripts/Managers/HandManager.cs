using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;

/// <summary>
/// Manages the player's hand, including drawing, adding, removing, and arranging cards.
/// Implements singleton pattern for global access.
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

    private List<GameObject> cardsInHand = new();

    public int CurrentHandSize => cardsInHand.Count;

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

    /// <summary>
    /// Draws the starting number of cards for the turn.
    /// </summary>
    public void DrawCardsForTurn()
    {
        if (DeckManager.Instance == null)
        {
            Logger.LogError("❌ DeckManager.Instance is NULL! Cannot draw cards.", this);
            return;
        }

        for (int i = 0; i < startingHandSize; i++)
        {
            DeckManager.Instance.DrawCard();
        }
    }

    /// <summary>
    /// Adds a card GameObject to the hand and updates the hand layout.
    /// </summary>
    /// <param name="cardObject">Card GameObject to add.</param>
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
    /// Updates the visual layout of cards in the hand using fan arrangement.
    /// </summary>
    private void UpdateHandLayout()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.identity;
            cardsInHand[0].transform.localPosition = Vector3.zero;
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float angle = fanSpread * (i - (cardCount - 1) / 2f);
            float xOffset = cardSpacing * (i - (cardCount - 1) / 2f);
            float normalized = (2f * i / (cardCount - 1)) - 1f;
            float yOffset = verticalSpacing * (1f - normalized * normalized);

            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            cardsInHand[i].transform.localPosition = new Vector3(xOffset, yOffset, 0f);
        }
    }

    /// <summary>
    /// Removes a card from the hand, discards it, and destroys the GameObject.
    /// </summary>
    /// <param name="card">The card data to remove.</param>
    public void RemoveCardFromHand(Card card)
    {
        GameObject cardObject = cardsInHand.Find(c => c.GetComponent<CardDisplay>().cardData == card);

        if (cardObject != null)
        {
            cardsInHand.Remove(cardObject);
            DeckManager.Instance?.DiscardCard(card);
            Destroy(cardObject);
            UpdateHandLayout();
        }
        else
        {
            Logger.LogWarning($"Tried to remove card not found in hand: {card.cardName}", this);
        }
    }
}
