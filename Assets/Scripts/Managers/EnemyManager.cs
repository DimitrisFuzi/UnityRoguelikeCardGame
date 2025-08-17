using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;

/// <summary>
/// Manages enemy spawning, tracking, and behavior during battle.
/// Implements singleton pattern for global access.
/// </summary>
public class EnemyManager : SceneSingleton<EnemyManager>
{
    private readonly List<Enemy> activeEnemies = new();

    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;

    private bool _initialized;

    // =========================
    // NEW: Dynamic initialize from scene
    // =========================
    /// <summary>
    /// Initializes enemies based on a BattleSetup component found in the scene.
    /// Requires a valid BattleSetup; otherwise spawns nothing.
    /// </summary>
    public void InitializeFromScene()
    {
        if (_initialized)
        {
            Logger.LogWarning("⚠️ EnemyManager already initialized — skipping duplicate init.", this);
            return;
        }

        var setup = Object.FindFirstObjectByType<BattleSetup>();
        if (setup == null || setup.enemies == null || setup.enemies.Count == 0)
        {
            Logger.LogError("❌ BattleSetup missing or empty. No enemies will spawn.", this);
            return; // <-- STOP: no fallback
        }

        ClearExistingEnemiesIfAny();

        for (int i = 0; i < setup.enemies.Count; i++)
        {
            var data = setup.enemies[i];
            Transform parent = GameObject.Find("EnemyCanvas")?.transform;
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (setup.spawnPoints != null && i < setup.spawnPoints.Count && setup.spawnPoints[i] != null)
            {
                pos = setup.spawnPoints[i].position;
                rot = setup.spawnPoints[i].rotation;
                // parent = setup.spawnPoints[i].parent ?? parent; // optional
            }

            SpawnEnemyAt(data, parent, pos, rot);
        }

        _initialized = true;
        Logger.Log($"✅ EnemyManager initialized from scene. Spawned: {activeEnemies.Count}", this);
    }


    // =========================
    // LEGACY: Hardcoded wolves (kept for backwards compatibility)
    // =========================
    /// <summary>
    /// Legacy initializer that loads enemies from Resources and spawns them.
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

    // =========================
    // SPAWN HELPERS
    // =========================

    /// <summary>
    /// Spawns a single enemy (legacy method). Parent defaults to EnemyCanvas.
    /// </summary>
    private void SpawnEnemy(EnemyData enemyData)
    {
        Transform parent = GameObject.Find("EnemyCanvas")?.transform;
        SpawnEnemyAt(enemyData, parent, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// NEW: Spawns a single enemy at a given position/rotation/parent.
    /// </summary>
    private void SpawnEnemyAt(EnemyData enemyData, Transform parent, Vector3 pos, Quaternion rot)
    {
        if (enemyPrefab == null)
        {
            Logger.LogError("❌ Enemy prefab is NULL. Assign it in the inspector.", this);
            return;
        }

        Transform finalParent = parent != null ? parent : GameObject.Find("EnemyCanvas")?.transform;
        if (finalParent == null)
        {
            Logger.LogError("❌ Could not find EnemyCanvas as parent for enemy spawn.", this);
            return;
        }

        GameObject enemyObject = Instantiate(enemyPrefab, pos, rot, finalParent);

        if (enemyObject.TryGetComponent(out Enemy enemyScript) && enemyObject.TryGetComponent(out EnemyDisplay enemyDisplay))
        {
            enemyScript.InitializeEnemy(enemyData, enemyDisplay);

            // --- AI wiring (μετά το InitializeEnemy) ---
            var ai = enemyObject.GetComponent<IEnemyAI>();
            if (ai != null)
            {
                // ✅ ΠΑΝΤΑ πάρε το target από τον PlayerManager
                var playerStats = PlayerStats.Instance;
                if (playerStats == null)
                {
                    Logger.LogError("❌ EnemyManager: PlayerStats.Instance is null (player not spawned yet).", this);
                }
                else
                {
                    ai.SetPlayerStats(playerStats);
                }


                ai.SetEnemyDisplay(enemyDisplay);
                ai.InitializeAI();
            }

            // ------------------------------------------

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
            if (BattleManager.Instance.IsBattleOver())
            {
                Logger.Log("⚠️ Skipping enemy actions: battle ended.", this);
                yield break;
            }

            enemy.PerformAction();
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Removes a defeated enemy from the list and destroys its GameObject.
    /// </summary>
    public void RemoveEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy)) return;

        activeEnemies.Remove(enemy);
        Destroy(enemy.gameObject);

        Logger.Log($"☠️ {enemy.enemyName} has been defeated and removed.", this);

        if (activeEnemies.Count == 0)
        {
            Logger.Log("🎉 All enemies defeated! Battle won!", this);
            BattleManager.Instance.HandleBattleVictory();
        }
    }

    /// <summary>
    /// Applies damage directly to a specific enemy.
    /// </summary>
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

    // =========================
    // UTILS
    // =========================
    private void ClearExistingEnemiesIfAny()
    {
        foreach (var e in activeEnemies)
            if (e != null) Destroy(e.gameObject);
        activeEnemies.Clear();
    }

    /// <summary>
    /// Spawns an enemy at runtime with optional parent, position, and rotation overrides.
    /// </summary>

    public Enemy SpawnEnemyRuntime(EnemyData enemyData, Transform parentOverride = null, Vector3? pos = null, Quaternion? rot = null)
    {
        if (enemyPrefab == null)
        {
            Logger.LogError("❌ Enemy prefab is NULL. Assign it in the inspector.", this);
            return null;
        }

        Transform finalParent = parentOverride != null ? parentOverride
                              : GameObject.Find("EnemyCanvas")?.transform;

        if (finalParent == null)
        {
            Logger.LogError("❌ Could not find EnemyCanvas as parent for enemy spawn.", this);
            return null;
        }

        Vector3 p = pos ?? Vector3.zero;
        Quaternion r = rot ?? Quaternion.identity;

        GameObject enemyObject = Instantiate(enemyPrefab, p, r, finalParent);

        if (enemyObject.TryGetComponent(out Enemy enemyScript) && enemyObject.TryGetComponent(out EnemyDisplay enemyDisplay))
        {
            enemyScript.InitializeEnemy(enemyData, enemyDisplay);
            activeEnemies.Add(enemyScript);
            Logger.Log($"🌱 Runtime-spawned enemy: {enemyData.enemyName}", this);
            return enemyScript;
        }
        else
        {
            Logger.LogError("❌ Enemy prefab must contain both Enemy and EnemyDisplay components!", enemyObject);
            Destroy(enemyObject);
            return null;
        }
    }


}
