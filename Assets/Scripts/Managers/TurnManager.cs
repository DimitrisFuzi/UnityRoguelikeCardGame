﻿using System;
using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Managers;


/// <summary>
/// Manages the turn flow between player and enemies.
/// </summary>
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

    /// <summary>
    /// Returns true if it's currently the player's turn.
    /// </summary>
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

    /// <summary>
    /// Starts the player's turn and unlocks input.
    /// </summary>
    public void StartPlayerTurn()
    {
        Debug.Log("🎮 Player Turn Started!");
        IsPlayerTurn = true;

        BattleManager.Instance.UnlockPlayerInput();

        OnPlayerTurnStart?.Invoke();
    }

    /// <summary>
    /// Ends the player's turn and locks input, then triggers enemy turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        Debug.Log("🎮 Player Turn Ended!");
        IsPlayerTurn = false;

        BattleManager.Instance.LockPlayerInput();

        OnPlayerTurnEnd?.Invoke();

        StartCoroutine(EnemyTurn());
    }

    /// <summary>
    /// Handles enemy turn with delays and notifies listeners.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator EnemyTurn()
    {
        Debug.Log("👿 Enemy Turn Started!");
        OnEnemyTurnStart?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Step 1: Perform enemy actions
        enemyManager.PerformEnemyActions();

        yield return new WaitForSeconds(1f); // Small delay before next intent setup

        // Set next intent for each enemy
        foreach (Enemy enemy in enemyManager.Enemies)
        {
            // Ensure enemyDisplay is available before trying to set intent
            EnemyDisplay enemyDisplay = enemy.GetComponent<EnemyDisplay>();
            if (enemy.EnemyAI != null && enemyDisplay != null)
            {
                EnemyIntent nextIntent = enemy.EnemyAI.PredictNextIntent();
                enemyDisplay.SetIntent(nextIntent); // Pass the intent to the display
            }
        }

        Debug.Log("👿 Enemy Turn Ended!");
        OnEnemyTurnEnd?.Invoke();

        // Step 3: Start player's turn
        StartPlayerTurn();
    }
}