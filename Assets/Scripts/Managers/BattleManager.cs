﻿using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;

namespace MyProjectF.Assets.Scripts.Managers
{
    /// <summary>
    /// Manages the overall battle flow and state transitions.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public enum BattleState { START, PLAYER_TURN, ENEMY_TURN, WON, LOST }
        public BattleState State { get; private set; }

        private TurnManager turnManager;
        private EnemyManager enemyManager;
        private PlayerManager playerManager;

        /// <summary>
        /// Flag to control whether player input is locked (disabled).
        /// </summary>
        public bool IsPlayerInputLocked { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Duplicate BattleManager found. Destroying...");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeReferences();
            StartBattle();
        }

        /// <summary>
        /// Caches all necessary manager references.
        /// </summary>
        private void InitializeReferences()
        {
            turnManager = TurnManager.Instance;
            enemyManager = EnemyManager.Instance;
            playerManager = PlayerManager.Instance;

            if (turnManager == null) Logger.LogError("❌ TurnManager is NULL!", this);
            if (enemyManager == null) Logger.LogError("❌ EnemyManager is NULL!", this);
            if (playerManager == null) Logger.LogError("❌ PlayerManager is NULL!", this);
        }

        /// <summary>
        /// Initializes the battle state, players, enemies, and deck.
        /// </summary>
        private void StartBattle()
        {
            SetBattleState(BattleState.START);

            playerManager.InitializePlayer();
            enemyManager.InitializeEnemies();

            DeckManager.Instance.InitializeDeck();
            DeckManager.Instance.ShuffleDeck();
            HandManager.Instance.DrawCardsForTurn();

            turnManager.StartPlayerTurn();
        }

        /// <summary>
        /// Updates the battle state and logs it.
        /// </summary>
        /// <param name="newState">New battle state.</param>
        public void SetBattleState(BattleState newState)
        {
            State = newState;
            Debug.Log($"⚔️ Battle state changed to: {newState}");
        }

        /// <summary>
        /// Locks player input to prevent interactions.
        /// </summary>
        public void LockPlayerInput()
        {
            IsPlayerInputLocked = true;
            Debug.Log("🔒 Player Input LOCKED");
        }

        /// <summary>
        /// Unlocks player input to allow interactions.
        /// </summary>
        public void UnlockPlayerInput()
        {
            IsPlayerInputLocked = false;
            Debug.Log("🔓 Player Input UNLOCKED");
        }
    }
}
