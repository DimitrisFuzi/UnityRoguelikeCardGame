using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private GameObject enemyPrefab;

    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(enemies);
    }


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
    }

    public void InitializeEnemies()
    {
        //Debug.Log("👿 Στήσιμο εχθρών...");
        EnemyData forestBeast = Resources.Load<EnemyData>("Enemies/Forest Beast");
        EnemyData spider = Resources.Load<EnemyData>("Enemies/Spider");
        SpawnEnemy(forestBeast);
        SpawnEnemy(spider);

    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ Το enemyPrefab είναι NULL! Ρύθμισέ το στο Inspector.");
            return;
        }
        GameObject enemyObject = Instantiate(enemyPrefab);
        enemyObject.transform.SetParent(GameObject.Find("EnemyCanvas").transform, false);
        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
        EnemyDisplay enemyDisplay = enemyObject.GetComponent<EnemyDisplay>(); // ✅ Βρίσκουμε το EnemyDisplay

        if (enemyScript != null && enemyDisplay != null)
        {
            enemyScript.InitializeEnemy(enemyData, enemyDisplay); // ✅ Περνάμε και το display
            enemies.Add(enemyScript);
        }
        else
        {
            Debug.LogError("❌ Το enemyPrefab δεν έχει τα Enemy.cs ή EnemyDisplay.cs!");
        }
    }

    public void PerformEnemyActions()
    {
        foreach (var enemy in enemies)
        {
            enemy.PerformAction();
        }
    }

    // ✅ Προσθέτουμε αυτή τη μέθοδο για να αφαιρεί τους νεκρούς εχθρούς
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            Destroy(enemy.gameObject);
            Debug.Log($"☠️ Ο {enemy.enemyName} πέθανε και αφαιρέθηκε από τη μάχη.");
        }

        // ✅ Αν όλοι οι εχθροί έχουν πεθάνει, ειδοποιούμε το BattleManager
        if (enemies.Count == 0)
        {
            BattleManager.Instance.SetBattleState(BattleManager.BattleState.WON);
            Debug.Log("🎉 Νίκησες τη μάχη!");
        }
    }

    public void ApplyDamageToEnemy(Enemy targetEnemy, int damage)
    {
        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(damage);
            Debug.Log($"💥 Ο {targetEnemy.enemyName} δέχτηκε {damage} damage!");
        }
    }

}
