// HealthBar.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the visual representation of a health bar,
/// including the fill progress and health text.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The Image component that represents the filled portion of the health bar.")]
    [SerializeField] private Image healthFillImage;

    [Tooltip("The TextMeshProUGUI component that displays the current health / maximum health.")]
    [SerializeField] private TextMeshProUGUI healthText;

    /// <summary>
    /// Updates the visual state of the health bar.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    /// <param name="maxHealth">The maximum health value.</param>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthFillImage == null)
        {
            Debug.LogError("Health Fill Image is not assigned to the HealthBar!", this);
            return;
        }

        if (maxHealth <= 0) // Avoid division by zero
        {
            healthFillImage.fillAmount = 0;
            if (healthText != null)
            {
                healthText.text = "0 / 0";
            }
            return;
        }

        // Calculate health percentage and update the fillAmount of the image
        float healthPercentage = (float)currentHealth / maxHealth;
        healthFillImage.fillAmount = healthPercentage;

        // Update the health text (if assigned)
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    /// <summary>
    /// Activates or deactivates the display of the health bar.
    /// </summary>
    /// <param name="isActive">True to show, false to hide.</param>
    public void SetHealthBarActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}