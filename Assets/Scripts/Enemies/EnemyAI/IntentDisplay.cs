using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to include this namespace for TextMeshProUGUI

/// <summary>
/// Displays the enemy's next intended action to the player.
/// This version is compatible with EnemyIntent having only Description and Icon.
/// </summary>
public class IntentDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Image component that displays the intent icon.")]
    [SerializeField] private Image intentIconImage; // Renamed for clarity (was intentImage)

    [Tooltip("The TextMeshProUGUI component that displays the intent description/value.")]
    [SerializeField] private TextMeshProUGUI intentDescriptionText; // Renamed for clarity (was intentTextTMP, removed intentText)

    // Removed Awake() method's auto-finding as it can interfere with Inspector assignments.
    // Removed redundant Text intentText field.
    // Removed default icons as they should come from EnemyIntent.Icon directly.

    /// <summary>
    /// Sets the intent display based on the enemy's predicted action.
    /// </summary>
    /// <param name="intent">The enemy intent to display.</param>
    public void SetIntent(EnemyIntent intent)
    {
        if (intent == null)
        {
            ClearIntent(); // Clear display if intent is null
            return;
        }

        // Set the icon
        if (intentIconImage != null)
        {
            intentIconImage.sprite = intent.Icon;
            intentIconImage.gameObject.SetActive(intent.Icon != null); // Activate/Deactivate based on icon presence
        }
        else
        {
            Logger.LogWarning("[IntentDisplay] Intent Icon Image is not assigned in the Inspector!", this);
        }

        // Set the description text
        if (intentDescriptionText != null)
        {
            intentDescriptionText.text = intent.Description;
            intentDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(intent.Description)); // Activate/Deactivate based on text presence
        }
        else
        {
            Logger.LogWarning("[IntentDisplay] Intent Description Text (TextMeshProUGUI) is not assigned in the Inspector!", this);
        }

        // Ensure the root IntentDisplay GameObject is active if there's content
        gameObject.SetActive(intent.Icon != null || !string.IsNullOrEmpty(intent.Description));
    }

    /// <summary>
    /// Clears the intent display.
    /// </summary>
    public void ClearIntent()
    {
        if (intentIconImage != null)
        {
            intentIconImage.sprite = null;
            intentIconImage.gameObject.SetActive(false);
        }
        if (intentDescriptionText != null)
        {
            intentDescriptionText.text = string.Empty;
            intentDescriptionText.gameObject.SetActive(false);
        }
        gameObject.SetActive(false); // Hide the root IntentDisplay GameObject
    }

    // ShowIntent and HideIntent are not strictly necessary if SetIntent/ClearIntent manage visibility
    // based on whether an intent is provided. However, if you have specific turn-based show/hide logic,
    // you can keep them and call them from TurnManager or EnemyManager.
    public void ShowIntent()
    {
        gameObject.SetActive(true);
    }

    public void HideIntent()
    {
        gameObject.SetActive(false);
    }

    // Removed GetIconForIntent as it's now handled by EnemyIntent.Icon directly
    // Removed SetIntentText as it's now handled directly by intentDescriptionText
}