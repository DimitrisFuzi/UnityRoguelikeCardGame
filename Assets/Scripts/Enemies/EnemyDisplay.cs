// EnemyDisplay.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Image enemyImage; // Enemy sprite image
    [SerializeField] private HealthBar healthBar; // NEW: Reference to the HealthBar script
    [SerializeField] private IntentDisplay intentDisplay; // Reference to the IntentDisplay component

    private RectTransform enemyRect;

    /// <summary>
    /// Sets up the enemy display with provided enemy data and initializes UI.
    /// </summary>
    /// <param name="enemy">Reference to the Enemy instance.</param>
    /// <param name="enemyData">Data container with enemy sprite and display parameters.</param>
    public void Setup(Enemy enemy, EnemyData enemyData)
    {
        if (enemyImage == null)
        {
            Logger.LogError("[EnemyDisplay] enemyImage is not assigned!", this);
            return;
        }

        enemyImage.sprite = enemyData.enemySprite;
        enemyRect = GetComponent<RectTransform>();

        if (enemyRect != null)
        {
            enemyRect.anchorMin = new Vector2(1, 0.5f);
            enemyRect.anchorMax = new Vector2(1, 0.5f);
            enemyRect.pivot = new Vector2(1, 0.5f);
            enemyRect.anchoredPosition = enemyData.position;
            enemyRect.sizeDelta = enemyData.size;
        }
        else
        {
            Logger.LogWarning("[EnemyDisplay] RectTransform component missing!", this);
        }

        // Initialize health display using the HealthBar script
        UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);
    }

    /// <summary>
    /// Updates the health bar and text using the HealthBar component.
    /// </summary>
    /// <param name="currentHealth">Current health value.</param>
    /// <param name="maxHealth">Maximum health value.</param>
    public void UpdateDisplay(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
        {
            Logger.LogError("[EnemyDisplay] healthBar is not assigned! Health display will not work.", this);
            return;
        }
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    /// <summary>
    /// Sets the intent display with the provided EnemyIntent.
    /// </summary>
    /// <param name="intent">The EnemyIntent object to display.</param>
    public void SetIntent(EnemyIntent intent)
    {
        if (intentDisplay != null)
        {
            intentDisplay.SetIntent(intent);
        }
        else
        {
            Logger.LogWarning("[EnemyDisplay] intentDisplay is not assigned! Intent will not be shown.", this);
        }
    }

    /// <summary>
    /// Clears the intent display.
    /// </summary>
    public void ClearIntentDisplay()
    {
        if (intentDisplay != null)
        {
            intentDisplay.ClearIntent();
        }
    }

    /// <summary>
    /// Changes the enemy sprite's color to visually indicate enraged state.
    /// </summary>
    /// <param name="isEnraged">True to set enraged color, false to reset to normal.</param>
    public void SetEnragedVisual(bool isEnraged)
    {
        if (enemyImage == null) return;

        if (isEnraged)
        {
            // Set an enraged color (e.g., red tint)
            // FF2F3B in hex is R=255, G=47, B=59
            // Divide by 255 for Unity's Color (0-1f) format
            enemyImage.color = new Color(1f, 47f / 255f, 59f / 255f, 1f);
        }
        else
        {
            // Reset to normal color (white)
            enemyImage.color = Color.white;
        }
    }
}