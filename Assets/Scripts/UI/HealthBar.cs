using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the visual representation of a health bar (fill + text).
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Image for the filled portion of the bar.")]
    [SerializeField] private Image healthFillImage;

    [Tooltip("Text displaying current/max health.")]
    [SerializeField] private TextMeshProUGUI healthText;

    /// <summary>Updates health visuals.</summary>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthFillImage == null)
        {
            Logger.LogError("HealthBar: healthFillImage is not assigned.", this);
            return;
        }

        if (maxHealth <= 0)
        {
            healthFillImage.fillAmount = 0f;
            if (healthText != null) healthText.text = "0 / 0";
            return;
        }

        float pct = Mathf.Clamp01((float)currentHealth / maxHealth);
        healthFillImage.fillAmount = pct;

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    /// <summary>Shows/hides the whole bar.</summary>
    public void SetHealthBarActive(bool isActive) => gameObject.SetActive(isActive);
}
