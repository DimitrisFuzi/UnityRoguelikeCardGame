using System.Collections;
using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public event Action OnPlayerTurnStart;
    public event Action OnEnemyTurnStart;

    private EnemyManager enemyManager;
    private PlayerManager playerManager;

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

    void Start()
    {
        enemyManager = GetComponent<EnemyManager>();
        playerManager = GetComponent<PlayerManager>();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("🎮 Γύρος του παίκτη!");
        OnPlayerTurnStart?.Invoke();
    }

    public void EndPlayerTurn()
    {
        Debug.Log("👿 Γύρος του εχθρού!");
        OnEnemyTurnStart?.Invoke();
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        enemyManager.PerformEnemyActions();
        yield return new WaitForSeconds(1f);

        StartPlayerTurn();
    }
}
