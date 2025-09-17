using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingDeck", menuName = "Cards/Starting Deck")]
public class StartingDeckData : ScriptableObject
{
    [Tooltip("The list of card names the player starts with.")]
    public List<string> startingCards = new List<string>();
}
