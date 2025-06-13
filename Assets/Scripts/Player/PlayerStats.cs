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

        [Header("Energy Settings")]
        public int initialEnergy = 5;
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
                InitializeStats(100); // Example starting health

                energy = initialEnergy;
                NotifyUI();
            }
            else
            {
                Logger.LogWarning($"Duplicate PlayerStats detected! Destroying this instance. ID: {GetInstanceID()}", this);
                Destroy(gameObject);
            }
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
            // Add game over screen, effects, etc...
        }
    }
}
