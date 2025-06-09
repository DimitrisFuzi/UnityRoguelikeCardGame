using System.Collections.Generic;
using System;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [SerializeField] private List<Card> drawPile = new List<Card>();
    [SerializeField] private List<Card> discardPile = new List<Card>();
    [SerializeField] private GameObject cardPrefab;

    public static event Action OnDrawPileChanged;
    public static event Action OnDiscardPileChanged;

    void Awake()
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

    public void InitializeDeck()
    {
        drawPile = new List<Card>(PlayerDeck.Instance.GetDeck());
        discardPile.Clear();
    }

    public void ShuffleDeck()
    {

        System.Random rng = new System.Random();
        int n = drawPile.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card temp = drawPile[k];
            drawPile[k] = drawPile[n];
            drawPile[n] = temp;
        }

        Logger.Log("🔀 Shuffling deck after: " + string.Join(", ", drawPile.ConvertAll(c => c.cardName)), this);
    }

    public void DrawCard()
    {
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
        }

        if (drawPile.Count > 0 && HandManager.Instance.CurrentHandSize < HandManager.Instance.MaxHandSize)
        {
            Card drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            if (drawnCard == null)
            {
                Logger.LogError("❌ DrawCard: Drawn card is NULL!", this);
            }
            /*else
            {
                Logger.Log($"🃏 DrawCard: Drew card {drawnCard.cardName}", this);
            }*/

            GameObject newCardObject = Instantiate(cardPrefab, HandManager.Instance.handTransform, false);
            CardDisplay cardDisplay = newCardObject.GetComponent<CardDisplay>();

            if (cardDisplay == null)
            {
                Logger.LogError("❌ CardDisplay is NULL on instantiated card!", this);
                return;
            }

            cardDisplay.cardData = drawnCard;
            cardDisplay.UpdateCardDisplay();
            HandManager.Instance.AddCardToHand(newCardObject);
            NotifyDrawPileUI();
        }
    }

    public void DiscardCard(Card card)
    {
        discardPile.Add(card);
        NotifyDiscardPileUI();
        Logger.Log($"🗑️ Card {card.cardName} moved to Discard Pile.", this);
    }

    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        NotifyDiscardPileUI();
    }

    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }

    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }

    private void NotifyDrawPileUI()
    {
        OnDrawPileChanged?.Invoke();
    }

    private void NotifyDiscardPileUI()
    {
        OnDiscardPileChanged?.Invoke();
    }
}
