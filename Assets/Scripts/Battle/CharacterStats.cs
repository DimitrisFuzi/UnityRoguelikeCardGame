using System;
using UnityEngine;

/// <summary>
/// Base class for any character that has health, armor, and can take damage or perform attacks.
/// </summary>
public abstract class CharacterStats : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("The transform used for visual animations (e.g., attack move).")]
    public Transform characterVisualTransform;

    [Header("Stats")]
    public int MaxHealth { get; protected set; }

    protected int currentHealth;

    /// <summary>Virtual property for current health to allow override (e.g., UI sync).</summary>
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

    /// <summary>Event fired when the character dies.</summary>
    public event Action OnDied;

    /// <summary>
    /// Initialize the stats, usually called on spawn or setup.
    /// </summary>
    public virtual void InitializeStats(int maxHealth, int startingArmor = 0)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Armor = startingArmor;

        // Initial push to UI
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor);
    }

    /// <summary>
    /// Reduces character's health, accounting for armor. Triggers OnHealthChanged.
    /// Returns the damage that actually went through after armor.
    /// </summary>
    public virtual int TakeDamage(int amount)
    {
        int damageAfterArmor = Mathf.Max(0, amount - Armor);
        Armor = Mathf.Max(0, Armor - amount);

        CurrentHealth -= damageAfterArmor;

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnArmorChanged?.Invoke(Armor);

        if (CurrentHealth <= 0)
        {
            Die();
        }

        return damageAfterArmor;
    }

    /// <summary>Directly reduces health without considering armor.</summary>
    public virtual void LoseHealthDirect(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, MaxHealth);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        Logger.Log($"{gameObject.name} lost {amount} HP (ignoring armor).", this);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Sets the current health directly, clamping it between 0 and max health.</summary>
    public virtual void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    /// <summary>Performs a simple attack on another character.</summary>
    public virtual void PerformAttack(CharacterStats target)
    {
        int attackDamage = 10; // could come from data later
        target.TakeDamage(attackDamage);

        Logger.Log($"{gameObject.name} attacks {target.gameObject.name} for {attackDamage} damage.", this);
    }

    /// <summary>Heals the character by a specific amount, not exceeding max health.</summary>
    public virtual void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);

        Logger.Log($"{gameObject.name} healed for {amount} HP.", this);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>Adds armor to the character.</summary>
    public virtual void AddArmor(int amount)
    {
        Armor = Mathf.Max(0, Armor + amount);

        Logger.Log($"{gameObject.name} gained {amount} Armor.", this);
        OnArmorChanged?.Invoke(Armor);
    }

    /// <summary>Handles death logic for the character.</summary>
    protected virtual void Die()
    {
        OnDied?.Invoke();
    }

    /// <summary>Directly gains health without any checks. Returns the amount actually healed.</summary>
    public virtual int GainHealthDirect(int amount)
    {
        if (amount <= 0) return 0;

        int before = CurrentHealth;
        int after = Mathf.Min(MaxHealth, CurrentHealth + amount);
        int healed = after - before;

        if (healed > 0)
        {
            CurrentHealth = after;
        }

        return healed;
    }
}
