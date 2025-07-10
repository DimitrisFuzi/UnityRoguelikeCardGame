using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Image enemyImage; // Enemy sprite image
    public TextMeshProUGUI healthText; // Text component to display health
    [SerializeField] private IntentDisplay intentDisplay; // Add this line

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

        // Initialize health display to full health
        UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);

        // Call to display initial intent
        if (enemy.EnemyAI != null)
        {
            SetIntent(enemy.EnemyAI.PredictNextIntent());
        }

        // Ensure enemy starts with normal color
        SetEnragedVisual(false); // NEW: Set initial color to normal
    }

    /// <summary>
    /// Updates the health text UI to reflect current health over max health.
    /// </summary>
    /// <param name="currentHealth">Current health value.</param>
    /// <param name="maxHealth">Maximum health value.</param>
    public void UpdateDisplay(int currentHealth, int maxHealth)
    {
        if (healthText == null)
        {
            Logger.LogError("[EnemyDisplay] healthText is not assigned!", this);
            return;
        }

        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    // New method to set the intent display
    public void SetIntent(EnemyIntent intent) // Add this method
    {
        if (intentDisplay != null)
        {
            intentDisplay.SetIntent(intent);
        }
    }

    // New method to clear the intent display
    public void ClearIntentDisplay() // Add this method
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
           
            // FF2F3B in hex is R=255, G=47, B=59
            // Divide by 255 for Unity's Color (0-1f) format
            enemyImage.color = new Color(255f / 255f, 47f / 255f, 59f / 255f, 1f);
        }
        else
        {
            enemyImage.color = Color.white; // Reset to original (white means no tint)
        }
    }
}