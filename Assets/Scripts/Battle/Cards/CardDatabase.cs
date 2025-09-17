using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Tooltip("Drag here all the cards")]
    public List<ScriptableObject> allCards = new();
}
