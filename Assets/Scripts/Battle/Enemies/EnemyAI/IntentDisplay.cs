using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays an enemy's next intended action (icon + short text).
/// </summary>
public class IntentDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image that displays the intent icon.")]
    [SerializeField] private Image intentIconImage;

    [Tooltip("Text that displays the intent description/value.")]
    [SerializeField] private TextMeshProUGUI intentDescriptionText;

    /// <summary>Set the display from an EnemyIntent.</summary>
    public void SetIntent(EnemyIntent intent)
    {
        if (intent == null)
        {
            ClearIntent();
            return;
        }

        if (intentIconImage != null)
        {
            intentIconImage.sprite = intent.Icon;
            intentIconImage.gameObject.SetActive(intent.Icon != null);
        }
        else
        {
            Logger.LogWarning("[IntentDisplay] Intent Icon Image is not assigned!", this);
        }

        if (intentDescriptionText != null)
        {
            intentDescriptionText.text = intent.Description;
            intentDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(intent.Description));
        }
        else
        {
            Logger.LogWarning("[IntentDisplay] Intent Description Text is not assigned!", this);
        }

        gameObject.SetActive(intent.Icon != null || !string.IsNullOrEmpty(intent.Description));
    }

    /// <summary>Hide and clear current intent visuals.</summary>
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
        gameObject.SetActive(false);
    }

    public void ShowIntent() => gameObject.SetActive(true);
    public void HideIntent() => gameObject.SetActive(false);
}
