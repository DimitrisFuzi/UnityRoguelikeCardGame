using UnityEngine;
using TMPro;
using MyProjectF.Assets.Scripts.Player;

public class PlayerDisplay : MonoBehaviour
{
    //[Header("Player Stats Reference")]
    //public PlayerStats playerStats; // Reference to PlayerStats (can be assigned manually or auto-detected)
    private PlayerStats playerStats;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI energyText;

    private void Start()
    {

        // Attempt to auto-assign if not set in Inspector
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
        healthText.text = $"{playerStats.CurrentHealth} / {playerStats.MaxHealth}";
        armorText.text = $"{playerStats.Armor}";
        energyText.text = $"{playerStats.energy}";
    }

    /// <summary>
    /// Sets placeholder values when no PlayerStats is available.
    /// </summary>
    private void SetFallbackDisplay()
    {
        healthText.text = "-- / --";
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
