using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    public GameObject cardPrefab;
    public Transform handTransform;
    public float fanSpread = 7.5f;
    public float cardSpacing = 100f;
    public float verticalSpacing = 100f;

    private List<GameObject> cardsInHand = new List<GameObject>();
    [SerializeField] public int maxHandSize = 10;
    [SerializeField] public int startingHandSize = 5;
    public int currentHandSize => cardsInHand.Count;

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

    public void DrawCardsForTurn()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("❌ DeckManager.Instance is NULL! You cannot draw a card.");
            return;
        }

        for (int i = 0; i < startingHandSize; i++)
        {
            DeckManager.Instance.DrawCard();
        }
    }

    public void AddCardToHand(GameObject cardObject)
    {
        cardsInHand.Add(cardObject);
        UpdateHandVisuals();
    }

    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }

    public void RemoveCardFromHand(Card card)
    {
        GameObject cardObject = cardsInHand.Find(c => c.GetComponent<CardDisplay>().cardData == card);
        if (cardObject != null)
        {
            cardsInHand.Remove(cardObject);
            DeckManager.Instance.DiscardCard(card);
            Destroy(cardObject);
        }
    }

}
