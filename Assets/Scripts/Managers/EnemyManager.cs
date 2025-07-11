using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Manages enemy spawning, tracking, and behavior during battle.
/// Implements singleton pattern for global access.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private readonly List<Enemy> activeEnemies = new();
    
    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Logger.LogWarning("⚠️ Duplicate EnemyManager detected. Destroying new instance.", this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads enemies from Resources and spawns them into the scene.
    /// </summary>
    public void InitializeEnemies()
    {
        //EnemyData forestBeast = Resources.Load<EnemyData>("Enemies/Forest Beast");
        //EnemyData spider = Resources.Load<EnemyData>("Enemies/Spider");
        EnemyData wolf1 = Resources.Load<EnemyData>("Enemies/Wolf1");
        EnemyData wolf2 = Resources.Load<EnemyData>("Enemies/Wolf2");

        //SpawnEnemy(forestBeast);
        //SpawnEnemy(spider);
        SpawnEnemy(wolf1);
        SpawnEnemy(wolf2);
    }

    /// <summary>
    /// Spawns a single enemy and adds it to the active list.
    /// </summary>
    /// <param name="enemyData">The enemy data to initialize the enemy with.</param>
    private void SpawnEnemy(EnemyData enemyData)
    {
        if (enemyPrefab == null)
        {
            Logger.LogError("❌ Enemy prefab is NULL. Assign it in the inspector.", this);
            return;
        }

        GameObject enemyObject = Instantiate(enemyPrefab, GameObject.Find("EnemyCanvas").transform, false);

        if (enemyObject.TryGetComponent(out Enemy enemyScript) && enemyObject.TryGetComponent(out EnemyDisplay enemyDisplay))
        {
            enemyScript.InitializeEnemy(enemyData, enemyDisplay);
            activeEnemies.Add(enemyScript);
            Logger.Log($"👾 Spawned enemy: {enemyData.enemyName}", this);
        }
        else
        {
            Logger.LogError("❌ Enemy prefab must contain both Enemy and EnemyDisplay components!", enemyObject);
            Destroy(enemyObject);
        }
    }

    /// <summary>
    /// Returns a copy of the current active enemies list.
    /// </summary>
    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(activeEnemies);
    }

    /// <summary>
    /// Calls each enemy to perform their battle action one at a time with a delay.
    /// </summary>
    public void PerformEnemyActions()
    {
        StartCoroutine(PerformEnemyActionsCoroutine());
    }

    private IEnumerator PerformEnemyActionsCoroutine()
    {
        foreach (var enemy in activeEnemies)
        {
            enemy.PerformAction();
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Removes a defeated enemy from the list and destroys its GameObject.
    /// </summary>
    /// <param name="enemy">The enemy to remove.</param>
    public void RemoveEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy)) return;

        activeEnemies.Remove(enemy);
        Destroy(enemy.gameObject);

        Logger.Log($"☠️ {enemy.enemyName} has been defeated and removed.", this);

        if (activeEnemies.Count == 0)
        {
            Logger.Log("🎉 All enemies defeated! Battle won!", this);
            BattleManager.Instance.SetBattleState(BattleManager.BattleState.WON);
        }
    }

    /// <summary>
    /// Applies damage directly to a specific enemy.
    /// </summary>
    /// <param name="targetEnemy">The enemy to damage.</param>
    /// <param name="damage">Amount of damage.</param>
    public void ApplyDamageToEnemy(Enemy targetEnemy, int damage)
    {
        if (targetEnemy == null)
        {
            Logger.LogWarning("⚠️ Tried to apply damage to a null enemy.", this);
            return;
        }

        targetEnemy.TakeDamage(damage);
        Logger.Log($"💥 {targetEnemy.enemyName} took {damage} damage.", this);
    }

    public List<Enemy> Enemies => activeEnemies;
}
