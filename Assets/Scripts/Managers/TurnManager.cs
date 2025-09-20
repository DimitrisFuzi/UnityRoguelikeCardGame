using System;
using System.Collections;
using UnityEngine;
using MyProjectF.Assets.Scripts.Managers;
using MyProjectF.Assets.Scripts.Player;

/// <summary>
/// Manages the turn flow between player and enemies.
/// </summary>
public class TurnManager : SceneSingleton<TurnManager>
{
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyTurnEnd;

    [Header("References")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PlayerManager playerManager;

    private bool _endingTurn;
    public bool IsEndingTurn => _endingTurn;

    /// <summary>Returns true if it's currently the player's turn.</summary>
    public bool IsPlayerTurn { get; private set; }

    void Start()
    {
        if (enemyManager == null)
            enemyManager = FindFirstObjectByType<EnemyManager>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        // StartPlayerTurn(); // if needed
    }

    /// <summary>Starts the player's turn, resets player stats, unlocks input, draws cards.</summary>
    public void StartPlayerTurn()
    {
        Logger.Log("Player turn started.", this);
        IsPlayerTurn = true;

        // Reset player energy & armor at the start of each round
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetEnergy();
            PlayerStats.Instance.ResetArmor();
        }

        BattleManager.Instance.UnlockPlayerInput();

        OnPlayerTurnStart?.Invoke();
        HandManager.Instance.DrawCardsForTurn();
    }

    /// <summary>Ends the player's turn and locks input, then triggers enemy turn.</summary>
    public void EndPlayerTurn()
    {
        if (_endingTurn) return; // prevent re-entry
        StartCoroutine(EndPlayerTurnRoutine());
    }

    /// <summary>Ends the player's turn with a coroutine, ensuring all actions are complete.</summary>
    private IEnumerator EndPlayerTurnRoutine()
    {
        _endingTurn = true;

        // Lock input immediately to prevent extra interactions
        BattleManager.Instance.LockPlayerInput();

        // If drawing cards, wait for it to finish
        if (HandManager.Instance != null)
        {
            while (HandManager.Instance.IsDrawing)
                yield return null;
        }

        Logger.Log("Player turn ended.", this);
        IsPlayerTurn = false;

        // Discard hand before enemy turn for clean state
        yield return StartCoroutine(HandManager.Instance.DiscardHandRoutine(animated: true));

        OnPlayerTurnEnd?.Invoke();

        // Enemy turn
        yield return StartCoroutine(EnemyTurn());

        _endingTurn = false;
    }

    /// <summary>Handles enemy turn with delays and notifies listeners.</summary>
    private IEnumerator EnemyTurn()
    {
        Logger.Log("Enemy turn started.", this);
        OnEnemyTurnStart?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Early exit if battle already ended
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST ||
            BattleManager.Instance.State == BattleManager.BattleState.WON)
        {
            Logger.LogWarning("EnemyTurn cancelled: battle already ended.", this);
            yield break;
        }

        // Perform enemy actions (wait until all finish)
        yield return StartCoroutine(enemyManager.PerformEnemyActionsCoroutine());

        yield return new WaitForSeconds(1f); // small delay before intent setup

        if (BattleManager.Instance.IsBattleOver())
        {
            Logger.Log("EnemyTurn aborted: battle ended during enemy actions.", this);
            yield break;
        }

        // Set next intent for each enemy
        foreach (Enemy enemy in enemyManager.Enemies)
        {
            if (enemy == null) continue;

            EnemyDisplay enemyDisplay = enemy.GetComponent<EnemyDisplay>();
            if (enemy.EnemyAI != null && enemyDisplay != null)
            {
                EnemyIntent nextIntent = enemy.EnemyAI.PredictNextIntent();
                enemyDisplay.SetIntent(nextIntent);
            }
        }

        if (BattleManager.Instance.IsBattleOver())
        {
            Logger.Log("EnemyTurn aborted: battle ended during enemy actions.", this);
            yield break;
        }

        Logger.Log("Enemy turn ended.", this);
        OnEnemyTurnEnd?.Invoke();

        if (GameSession.Instance != null)
            GameSession.Instance.turnsTaken++;

        // Back to player
        StartPlayerTurn();
    }
}
