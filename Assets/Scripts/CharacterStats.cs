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
    /// Event fired when the character dies.
    /// </summary>
    public event Action OnDied;

    /// <summary>
    /// Initialize the stats, usually called on spawn or setup.
    /// </summary>
    /// <param name="maxHealth">Maximum health value.</param>
    /// <param name="startingArmor">Starting armor value (optional).</param>
    public virtual void InitializeStats(int maxHealth, int startingArmor = 0)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Armor = startingArmor;

        Logger.Log($"{gameObject.name} initialized with {MaxHealth} HP and {Armor} Armor.", this);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor);
    }

    /// <summary>
    /// Deals damage to the character, reducing armor first then health.
    /// </summary>
    /// <param name="damage">Amount of incoming damage.</param>
    public virtual void TakeDamage(int damage)
    {
        int damageToArmor = Mathf.Min(Armor, damage);
        Armor -= damageToArmor;

        int damageToHealth = damage - damageToArmor;
        CurrentHealth -= damageToHealth;

        Logger.Log($"{gameObject.name} took {damage} damage! HP: {CurrentHealth}/{MaxHealth}, Armor: {Armor}", this);

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Performs a default attack on another character.
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
        Logger.Log($"{gameObject.name} died.", this);
        OnDied?.Invoke();
    }
}
