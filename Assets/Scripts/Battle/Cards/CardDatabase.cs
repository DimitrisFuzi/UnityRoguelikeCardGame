using UnityEngine;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Cards;

[CreateAssetMenu(menuName = "Cards/Card Database", fileName = "CardDatabase")]
public class CardDatabase : ScriptableObject
{
    [Tooltip("All card assets included in this database.")]
    public List<Card> allCards = new();
}
