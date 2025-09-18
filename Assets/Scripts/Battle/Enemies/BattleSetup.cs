using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // ensure this is available early
public class BattleSetup : MonoBehaviour
{
    [Tooltip("Enemy data to spawn in this battle, ordered left-to-right.")]
    public List<EnemyData> enemies = new();

    [Tooltip("Optional spawn points. If empty, a default layout under EnemyCanvas will be used.")]
    public List<Transform> spawnPoints = new();
}
