using System;
using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyTurnEnd;

    [Header("References")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PlayerManager playerManager;

    public bool IsPlayerTurn { get; private set; }

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
        if (enemyManager == null)
            enemyManager = FindFirstObjectByType<EnemyManager>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("🎮 Player Turn Started!");
        IsPlayerTurn = true;

        OnPlayerTurnStart?.Invoke();
    }

    public void EndPlayerTurn()
    {
        Debug.Log("🎮 Player Turn Ended!");
        IsPlayerTurn = false;

        OnPlayerTurnEnd?.Invoke();

        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("👿 Enemy Turn Started!");
        OnEnemyTurnStart?.Invoke();

        yield return new WaitForSeconds(1f);

        enemyManager.PerformEnemyActions();

        yield return new WaitForSeconds(1f);

        Debug.Log("👿 Enemy Turn Ended!");
        OnEnemyTurnEnd?.Invoke();

        StartPlayerTurn();
    }
}
