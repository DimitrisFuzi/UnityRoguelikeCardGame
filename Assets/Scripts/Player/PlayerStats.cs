using UnityEngine;
using System;

namespace MyProjectF.Assets.Scripts.Player
{
    /// <summary>
    /// Manages player-specific stats such as health, armor, and energy.
    /// Implements singleton pattern for easy access.
    /// </summary>
    public class PlayerStats : CharacterStats
    {
        public static PlayerStats Instance { get; private set; }
        public PlayerDisplay playerDisplay;

        [Header("Energy Settings")]
        public int initialEnergy;
        public int energy;

        /// <summary>
        /// Event fired whenever player stats change (health, armor, energy).
        /// </summary>
        public static event Action OnStatsChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeStats(75); // Example starting health

                energy = initialEnergy;

                playerDisplay = GetComponentInChildren<PlayerDisplay>();

                OnArmorChanged += HandleArmorRelay;
                NotifyUI();
            }
            else
            {
                Logger.LogWarning($"Duplicate PlayerStats detected! Destroying this instance. ID: {GetInstanceID()}", this);
                Destroy(gameObject);
            }
        }

        // ✅ Override SetCurrentHealth ώστε να περνάει από NotifyUI
        // PlayerStats.cs
        public override void SetCurrentHealth(int value)
        {
            base.SetCurrentHealth(value);   // καλεί το CharacterStats clamp logic
            NotifyUI();                     // σήκωσε event για το PlayerDisplay
        }


        // ✅ Override LoseHealthDirect ώστε να περνάει από NotifyUI
        public override void LoseHealthDirect(int amount)
        {
            base.LoseHealthDirect(amount);
            NotifyUI();
        }

        /// <summary>
        /// Override for CurrentHealth to notify UI whenever it changes.
        /// </summary>
        public override int CurrentHealth
        {
            get => base.CurrentHealth;
            protected set
            {
                base.CurrentHealth = value;
                NotifyUI();
              }
          }

        /// <summary>
        /// Resets the player's energy to initial value and notifies listeners.
        /// </summary>
        public void ResetEnergy()
        {
            energy = initialEnergy;
            NotifyUI();
        }

        /// <summary>
        /// Resets the player's armor to zero and notifies listeners.
        /// </summary>
        public void ResetArmor()
        {
            Armor = 0;
            NotifyUI();
        }

        /// <summary>
        /// Uses a specified amount of energy. Clamps to zero minimum.
        /// </summary>
        /// <param name="amount">Amount of energy to use.</param>
        public void UseEnergy(int amount)
        {
            energy -= amount;
            if (energy < 0) energy = 0;

            NotifyUI();
        }

        /// <summary>
        /// Increases the player's energy by a specified amount.
        /// </summary>
        public void GainEnergy(int amount)
        {
            energy += amount;
            NotifyUI();
        }


        /// <summary>
        /// Notifies listeners that player stats have changed.
        /// </summary>
        private void NotifyUI()
        {
            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Handles player death logic.
        /// </summary>
        protected override void Die()
        {
            Logger.Log("💀 Player died! Game Over.", this);
            base.Die();
        }

        private void HandleArmorRelay(int _)
        {
            NotifyUI();   // σηκώνει OnStatsChanged ώστε το UI σου να ανανεωθεί άμεσα
        }
    }
}
