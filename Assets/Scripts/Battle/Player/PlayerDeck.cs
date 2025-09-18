using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;

/// <summary>
/// Singleton responsible for storing and managing the player's deck.
/// Loads all available cards and initializes a starting deck.
/// </summary>
public class PlayerDeck : MonoBehaviour
{
    public static PlayerDeck Instance { get; private set; }
    public IReadOnlyList<Card> CurrentDeck => playerDeck;

    [SerializeField] private List<Card> playerDeck = new List<Card>();
    private Dictionary<string, Card> allCardsDictionary = new();

    [Header("Starting Deck (asset)")]
    [SerializeField] private StartingDeckData startingDeckData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCards();
        GameSession.Instance?.RegisterDeck(this);
    }

    private void Start()
    {
        InitializeStartingDeck();
    }

    /// <summary>Loads all card assets from Resources/Cards into a dictionary for fast lookup.</summary>
    private void LoadAllCards()
    {
        allCardsDictionary.Clear();
        Card[] allCards = Resources.LoadAll<Card>("Cards");

        foreach (Card card in allCards)
        {
            if (card != null && !allCardsDictionary.ContainsKey(card.cardName))
            {
                allCardsDictionary[card.cardName] = card;
            }
        }
    }

    /// <summary>Initializes the player's starting deck using StartingDeck.asset (or fallback).</summary>
    public void InitializeStartingDeck()
    {
        playerDeck.Clear();

        // From asset if present
        if (startingDeckData != null && startingDeckData.startingCards != null && startingDeckData.startingCards.Count > 0)
        {
            foreach (var cardName in startingDeckData.startingCards)
            {
                if (allCardsDictionary.TryGetValue(cardName, out Card card))
                {
                    playerDeck.Add(card);
                }
                else
                {
                    Logger.LogError($"Card '{cardName}' not found in card dictionary.", this);
                }
            }
            Logger.Log("[PlayerDeck] Deck initialized from StartingDeck asset.", this);
            return;
        }

        // Fallback defaults
        string[] defaultCards =
        {
            "Blood Rush","Blood Rush","Blood Rush","Blood Rush","Blood Rush",
            "Last Resort","Last Resort","Last Resort"
        };

        foreach (var cardName in defaultCards)
        {
            if (allCardsDictionary.TryGetValue(cardName, out Card card))
                playerDeck.Add(card);
            else
                Logger.LogError($"Card '{cardName}' not found in card dictionary.", this);
        }

        Logger.Log("[PlayerDeck] Deck initialized from fallback defaults.", this);
    }

    /// <summary>Returns a new copy of the player's current deck.</summary>
    public List<Card> GetDeck() => new List<Card>(playerDeck);

    /// <summary>Adds a card to the player's deck by name.</summary>
    public void AddCardToDeck(string cardName)
    {
        if (allCardsDictionary.TryGetValue(cardName, out Card card))
        {
            playerDeck.Add(card);
            Logger.Log($"Card '{card.cardName}' added to deck.", this);
        }
        else
        {
            Logger.LogError($"Card '{cardName}' not found in card dictionary.", this);
        }
    }
}
