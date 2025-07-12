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

    [SerializeField] private List<Card> playerDeck = new List<Card>();
    private Dictionary<string, Card> allCardsDictionary = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadAllCards();
    }

    private void Start()
    {
        InitializeStartingDeck();
    }

    /// <summary>
    /// Loads all card assets from the Resources/Cards folder into a dictionary for fast lookup.
    /// </summary>
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

    /// <summary>
    /// Initializes the player's starting deck with a predefined list of card names.
    /// </summary>
    public void InitializeStartingDeck()
    {
        playerDeck.Clear();

        string[] selectedCards =
        {
            "Deep Focus","Deep Focus","Deep Focus","Deep Focus","Deep Focus","Deep Focus","Deep Focus","Deep Focus",
            "Gut Reaction", "Gut Reaction", "Gut Reaction", "Gut Reaction", "Gut Reaction",
        
        };

        foreach (string cardName in selectedCards)
        {
            if (allCardsDictionary.TryGetValue(cardName, out Card card))
            {
                playerDeck.Add(card);
            }
            else
            {
                Logger.LogError($"❌ Card '{cardName}' not found in card dictionary!", this);
            }
        }

    }

    /// <summary>
    /// Returns a new copy of the player's current deck.
    /// </summary>
    public List<Card> GetDeck()
    {
        return new List<Card>(playerDeck);
    }

    /// <summary>
    /// Adds a card to the player's deck by name.
    /// </summary>
    /// <param name="cardName">The name of the card to add.</param>
    public void AddCardToDeck(string cardName)
    {
        if (allCardsDictionary.TryGetValue(cardName, out Card card))
        {
            playerDeck.Add(card);
            Logger.Log($"➕ Card '{card.cardName}' added to deck.", this);
        }
        else
        {
            Logger.LogError($"❌ Card '{cardName}' not found in card dictionary.", this);
        }
    }
}
