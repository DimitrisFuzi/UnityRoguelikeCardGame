using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
public class PlayerDeck : MonoBehaviour
{
    public static PlayerDeck Instance { get; private set; }

    [SerializeField] private List<Card> playerDeck = new List<Card>();
    private Dictionary<string, Card> allCardsDictionary = new Dictionary<string, Card>();

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
        LoadAllCards(); // ✅ Φορτώνουμε όλες τις κάρτες μία φορά
    }


    private void Start()
    {
        InitializeStartingDeck();
    }

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

    public void InitializeStartingDeck()
    {
        playerDeck.Clear();

        string[] selectedCards = new string[]
        {
            "Bat Attack",
            "Bat Attack",
            "Punch Attack",
            "Punch Attack",
            "Punch Attack",
            "Arm Block",
            "Arm Block",
            "Arm Block",
            "Arm Block",
            "Flash Bomb"
        };

        foreach (string cardName in selectedCards)
        {
            if (allCardsDictionary.TryGetValue(cardName, out Card card))
            {
                playerDeck.Add(card);
                
            }
            else
            {
                Debug.LogError($"❌ Η κάρτα '{cardName}' δεν βρέθηκε στο Dictionary!");
            }
        }

        Debug.Log($"📜 Συνολικές κάρτες στο αρχικό deck: {playerDeck.Count}");
    }

    public List<Card> GetDeck()
    {
        return new List<Card>(playerDeck);
    }

    public void AddCardToDeck(string cardName)
    {
        if (allCardsDictionary.TryGetValue(cardName, out Card card))
        {
            playerDeck.Add(card);
            Debug.Log($"➕ Προστέθηκε η κάρτα {card.cardName} στο Deck!");
        }
        else
        {
            Debug.LogError($"❌ Η κάρτα '{cardName}' δεν βρέθηκε στο Dictionary!");
        }
    }
}
