using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public enum BattleState { START, PLAYER_TURN, ENEMY_TURN, WON, LOST }
    public BattleState State { get; private set; }

    private TurnManager turnManager;
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

        turnManager = TurnManager.Instance;
        enemyManager = EnemyManager.Instance;
        playerManager = PlayerManager.Instance;

        if (turnManager == null) Debug.LogError("❌ turnManager είναι NULL!");
        if (enemyManager == null) Debug.LogError("❌ enemyManager είναι NULL!");
        if (playerManager == null) Debug.LogError("❌ playerManager είναι NULL!");

        StartBattle();
    }

    private void StartBattle()
    {
        State = BattleState.START;

        playerManager.InitializePlayer();
        enemyManager.InitializeEnemies();

        DeckManager.Instance.InitializeDeck();
        DeckManager.Instance.ShuffleDeck();
        HandManager.Instance.DrawCardsForTurn();

        turnManager.StartPlayerTurn();
    }

    public void SetBattleState(BattleState newState)
    {
        State = newState;
        Debug.Log($"⚔️ Η κατάσταση της μάχης άλλαξε σε: {newState}");
    }

}
