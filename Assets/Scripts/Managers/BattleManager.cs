using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;
using System.Collections;

namespace MyProjectF.Assets.Scripts.Managers
{
    /// <summary>
    /// Manages the overall battle flow and state transitions.
    /// </summary>
    public class BattleManager : SceneSingleton<BattleManager>
    {
        public enum BattleState { START, PLAYER_TURN, ENEMY_TURN, WON, LOST }
        public BattleState State { get; private set; }

        private TurnManager turnManager;
        private EnemyManager enemyManager;
        private PlayerManager playerManager;

        /// <summary>Controls whether player input is locked (disabled).</summary>
        public bool IsPlayerInputLocked { get; private set; } = false;

        [Header("Audio")]
        [SerializeField] private AudioClip defeatJingle;

        private void Start()
        {
            StartCoroutine(InitRoutine());
        }

        private IEnumerator InitRoutine()
        {
            yield return null;
            Time.timeScale = 1f;
            InitializeReferences();
            StartBattle();
        }

        /// <summary>Caches all necessary manager references.</summary>
        private void InitializeReferences()
        {
            turnManager = TurnManager.Instance;
            enemyManager = EnemyManager.Instance;
            playerManager = PlayerManager.Instance;

            if (turnManager == null) Logger.LogError("TurnManager is null.", this);
            if (enemyManager == null) Logger.LogError("EnemyManager is null.", this);
            if (playerManager == null) Logger.LogError("PlayerManager is null.", this);
        }

        /// <summary>Initializes the battle state, players, enemies, and deck.</summary>
        private void StartBattle()
        {
            SetBattleState(BattleState.START);

            playerManager.InitializePlayer();
            enemyManager.InitializeFromScene();

            DeckManager.Instance.InitializeDeck();
            Logger.Log($"After InitializeDeck: draw={DeckManager.Instance.GetDrawPileCount()}, discard={DeckManager.Instance.GetDiscardPileCount()}", this);
            DeckManager.Instance.ShuffleDeck();

            turnManager.StartPlayerTurn();
        }

        /// <summary>Updates the battle state and logs it.</summary>
        public void SetBattleState(BattleState newState)
        {
            State = newState;
            Logger.Log($"Battle state changed to: {newState}", this);
        }

        /// <summary>Locks player input to prevent interactions.</summary>
        public void LockPlayerInput()
        {
            IsPlayerInputLocked = true;
            Logger.Log("Player input LOCKED.", this);
        }

        /// <summary>Unlocks player input to allow interactions.</summary>
        public void UnlockPlayerInput()
        {
            IsPlayerInputLocked = false;
            Logger.Log("Player input UNLOCKED.", this);
        }

        /// <summary>Handles player defeat by changing the battle state to LOST.</summary>
        private void HandlePlayerDefeat()
        {
            if (State == BattleState.LOST || State == BattleState.WON)
                return;

            SetBattleState(BattleState.LOST);
            Logger.Log("Player defeat handled.", this);

            AudioManager.Instance?.StopMusic();
            if (defeatJingle != null)
                AudioManager.Instance?.PlayJingle(defeatJingle, 1f);

            GameOverUIManager.Instance.ShowGameOver();
        }

        /// <summary>Handles battle victory by changing the state to WON and loading the next scene.</summary>
        public void HandleBattleVictory()
        {
            if (State == BattleState.WON || State == BattleState.LOST)
                return;

            SetBattleState(BattleState.WON);
            SceneFlowManager.Instance.LoadNextAfterBattle();
        }

        /// <summary>Registers player events to handle defeat.</summary>
        public void RegisterPlayerEvents(PlayerStats playerStats)
        {
            if (playerStats != null)
                playerStats.OnDied += HandlePlayerDefeat;
        }

        /// <summary>Returns true if the battle is over.</summary>
        public bool IsBattleOver() => State == BattleState.LOST || State == BattleState.WON;
    }
}
