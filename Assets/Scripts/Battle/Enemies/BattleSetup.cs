using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // να είναι διαθέσιμο νωρίς
public class BattleSetup : MonoBehaviour
{
    [Tooltip("Τι enemy data να μπει σε αυτή τη μάχη, με τη σειρά spawn.")]
    public List<EnemyData> enemies = new();

    [Tooltip("Προαιρετικά spawn points. Αν είναι κενό, θα μπει default layout κάτω από το EnemyCanvas.")]
    public List<Transform> spawnPoints = new();
}
