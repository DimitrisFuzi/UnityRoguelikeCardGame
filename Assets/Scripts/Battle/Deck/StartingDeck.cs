using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject holding the initial list of card names for the player's deck.
/// </summary>
[CreateAssetMenu(fileName = "StartingDeck", menuName = "Cards/Starting Deck")]
public class StartingDeckData : ScriptableObject
{
    [Tooltip("The list of card names the player starts with.")]
    public List<string> startingCards = new List<string>();
}
