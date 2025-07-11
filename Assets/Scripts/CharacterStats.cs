using System;
using UnityEngine;

/// <summary>
/// Base class for any character that has health, armor, and can take damage or perform attacks.
/// </summary>
public abstract class CharacterStats : MonoBehaviour
{
    [Header("Stats")]
    public int MaxHealth { get; protected set; }

    protected int currentHealth;

    /// <summary>
    /// Virtual property for current health to allow override (e.g., UI sync).
    /// </summary>
    public virtual int CurrentHealth
    {
        get => currentHealth;
        protected set => currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    public int Armor { get; protected set; }

    // Add energy stat here if it's common to all characters
    // For now, it's specific to PlayerStats, but we will add the event here.

    /// <summary>
    /// Event fired when health changes.
    /// Parameters: currentHealth, maxHealth
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// Event fired when armor changes.
    /// Parameter: currentArmor
    /// </summary>
    public event Action<int> OnArmorChanged;

    /// <summary>
    /// Event fired when energy changes.
    /// Parameter: currentEnergy
    /// </summary>
    public event Action<int> OnEnergyChanged; // ADDED: OnEnergyChanged event

    /// <summary>
    /// Event fired when the character dies.
    /// </summary>
    public event Action OnDied;

    /// <summary>
    /// Initialize the stats, usually called on spawn or setup.
    /// </summary>
    /// <param name="maxHealth">Maximum health value.</param>
    /// <param name="startingArmor">Starting armor value (optional, defaults to 0).</param>
    public virtual void InitializeStats(int maxHealth, int startingArmor = 0)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth; // Start with full health
        Armor = startingArmor;

        // Immediately invoke events to update UI on initialization
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor);
        // OnEnergyChanged will be invoked by PlayerStats if applicable
    }

    /// <summary>
    /// Reduces character's health, accounting for armor. Triggers OnHealthChanged.
    /// </summary>
    /// <param name="amount">Amount of damage to take.</param>
    public virtual void TakeDamage(int amount)
    {
        int damageAfterArmor = Mathf.Max(0, amount - Armor); // Damage absorbed by armor
        Armor = Mathf.Max(0, Armor - amount); // Armor reduced first

        CurrentHealth -= damageAfterArmor;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor); // Armor might have changed too

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Performs an attack on another character.
    /// </summary>
    /// <param name="target">The character receiving the attack.</param>
    public virtual void PerformAttack(CharacterStats target)
    {
        int attackDamage = 10; // This can later come from EnemyData or WeaponData
        target.TakeDamage(attackDamage);

        Logger.Log($"⚔️ {gameObject.name} attacks {target.gameObject.name} for {attackDamage} damage!", this);
    }

    /// <summary>
    /// Heals the character by a specific amount, not exceeding max health.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    public virtual void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);

        Logger.Log($"{gameObject.name} healed for {amount} HP.", this);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>
    /// Adds armor to the character.
    /// </summary>
    /// <param name="amount">Amount of armor to add.</param>
    public virtual void AddArmor(int amount)
    {
        Armor += amount;

        Logger.Log($"{gameObject.name} gained {amount} Armor.", this);
        OnArmorChanged?.Invoke(Armor);
    }

    /// <summary>
    /// Handles death logic for the character.
    /// Can be overridden by derived classes for custom behavior.
    /// </summary>
    protected virtual void Die()
    {
        OnDied?.Invoke(); // Notify listeners that the character has died
    }
}