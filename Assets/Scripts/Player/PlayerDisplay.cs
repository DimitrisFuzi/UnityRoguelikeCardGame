// PlayerDisplay.cs
using UnityEngine;
using TMPro;
using MyProjectF.Assets.Scripts.Player;

public class PlayerDisplay : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("UI Elements")]
    // REMOVED: [SerializeField] private TextMeshProUGUI healthText; // Health text handled by HealthBar.cs
    [SerializeField] private HealthBar playerHealthBar; // NEW: Reference to HealthBar script for player
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI energyText;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            Logger.Log($"PlayerDisplay has pre-assigned playerStats = {playerStats}", this);
        }

        if (playerStats != null)
        {
            PlayerStats.OnStatsChanged += UpdatePlayerUI;
            UpdatePlayerUI(); // Update UI immediately on start
        }
        else
        {
            Logger.LogWarning("⚠️ PlayerStats not found! UI will show fallback values.", this);
            SetFallbackDisplay();
        }
    }

    /// <summary>
    /// Updates the UI elements with the current player stats.
    /// </summary>
    public void UpdatePlayerUI()
    {
        if (playerStats == null)
        {
            SetFallbackDisplay();
            return;
        }

        // Update player health using the HealthBar script
        if (playerHealthBar != null)
        {
            playerHealthBar.UpdateHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
        }
        else
        {
            Logger.LogWarning("[PlayerDisplay] playerHealthBar is not assigned! Player health will not display.", this);
            // Fallback for health text if health bar isn't set up
            // You might want to have a separate TextMeshProUGUI for health here if HealthBar isn't always used.
        }

        armorText.text = $"{playerStats.Armor}";
        energyText.text = $"{playerStats.energy}";
    }

    /// <summary>
    /// Sets placeholder values when no PlayerStats is available.
    /// </summary>
    private void SetFallbackDisplay()
    {
        // For health, you'd need to decide if HealthBar handles fallback or if there's a separate text field.
        // For now, this will show 0/0 on the health bar.
        if (playerHealthBar != null)
        {
            playerHealthBar.UpdateHealthBar(0, 0);
        }
        else
        {
            // If playerHealthBar is null AND you removed healthText, this will not show health.
            // You might need a separate TextMeshProUGUI for player health if HealthBar isn't mandatory.
        }

        armorText.text = "--";
        energyText.text = "--";
    }

    private void OnEnable()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.OnStatsChanged += UpdatePlayerUI;
        }
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.OnStatsChanged -= UpdatePlayerUI;
        }
    }
}