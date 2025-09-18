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

    /// <summary>
    /// Returns true if it's currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn { get; private set; }

    void Start()
    {
        if (enemyManager == null)
            enemyManager = FindFirstObjectByType<EnemyManager>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        //StartPlayerTurn();
    }

    /// <summary>
    /// Starts the player's turn, resets player stats, unlocks input, draws cards.
    /// </summary>
    public void StartPlayerTurn()
    {
        Debug.Log("🎮 Player Turn Started!");
        IsPlayerTurn = true;

        // ✅ Reset player energy & armor στην αρχή κάθε γύρου
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetEnergy();
            PlayerStats.Instance.ResetArmor();
        }

        BattleManager.Instance.UnlockPlayerInput();

        OnPlayerTurnStart?.Invoke();
        HandManager.Instance.DrawCardsForTurn();
    }

    /// <summary>
    /// Ends the player's turn and locks input, then triggers enemy turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        // Μη ξεκινάς δεύτερο end-turn αν ήδη τρέχει
        if (_endingTurn) return;
        StartCoroutine(EndPlayerTurnRoutine());
    }


    /// <summary>
    /// Ends the player's turn with a coroutine, ensuring all actions are complete.
    /// </summary>
    private IEnumerator EndPlayerTurnRoutine()
    {
        _endingTurn = true;

        // Κλείδωσε input αμέσως για να μη γίνουν άλλα clicks/plays
        BattleManager.Instance.LockPlayerInput();

        // Αν τραβάμε κάρτες (start-of-turn ή mid-turn effect), περίμενε να τελειώσει
        if (HandManager.Instance != null)
        {
            while (HandManager.Instance.IsDrawing)
                yield return null; // 1 frame
        }

        Debug.Log("🎮 Player Turn Ended!");
        IsPlayerTurn = false;

        // Περίμενε να αδειάσει το χέρι (ώστε να μην πέσουν animations πάνω στον enemy γύρο)
        yield return StartCoroutine(HandManager.Instance.DiscardHandRoutine(animated: true));

        OnPlayerTurnEnd?.Invoke();

        // Τώρα ξεκινά ο enemy γύρος
        yield return StartCoroutine(EnemyTurn());

        _endingTurn = false;
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

        // Check if the battle has already ended
        if (BattleManager.Instance.State == BattleManager.BattleState.LOST ||
        BattleManager.Instance.State == BattleManager.BattleState.WON)
        {
            Debug.LogWarning("⚠️ EnemyTurn cancelled: Battle already ended.");
            yield break;
        }

        // Step 1: Perform enemy actions (wait until ALL finish)
        yield return StartCoroutine(enemyManager.PerformEnemyActionsCoroutine());

        yield return new WaitForSeconds(1f); // Small delay before next intent setup

        if (BattleManager.Instance.IsBattleOver())
        {
            Logger.Log("⚠️ EnemyTurn aborted (battle ended during enemy actions).", this);
            yield break;
        }

        // Set next intent for each enemy
        foreach (Enemy enemy in enemyManager.Enemies)
        {
            if (enemy == null) continue;
            // Ensure enemyDisplay is available before trying to set intent
            EnemyDisplay enemyDisplay = enemy.GetComponent<EnemyDisplay>();
            if (enemy.EnemyAI != null && enemyDisplay != null)
            {
                EnemyIntent nextIntent = enemy.EnemyAI.PredictNextIntent();
                enemyDisplay.SetIntent(nextIntent); // Pass the intent to the display
            }
        }

        if (BattleManager.Instance.IsBattleOver())

        {
            Logger.Log("⚠️ EnemyTurn aborted (battle ended during enemy actions).", this);
            yield break;
        }

        Debug.Log("👿 Enemy Turn Ended!");
        OnEnemyTurnEnd?.Invoke();

        if (GameSession.Instance != null)
            GameSession.Instance.turnsTaken++;

        // Step 3: Start player's turn
        StartPlayerTurn();
    }
}