using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the enemy's next intended action to the player.
/// Clean version that uses explicit intent types instead of string parsing.
/// </summary>
public class IntentDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image intentImage;
    [SerializeField] private Text intentText;
    [SerializeField] private TextMeshProUGUI intentTextTMP; // For TextMeshPro support

    [Header("Default Intent Icons")]
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite buffIcon;

    private void Awake()
    {
        // Auto-find components if not assigned
        if (intentImage == null)
            intentImage = transform.Find("IntentImage")?.GetComponent<Image>();

        if (intentText == null)
            intentText = transform.Find("IntentValue")?.GetComponent<Text>();

        if (intentTextTMP == null)
            intentTextTMP = transform.Find("IntentValue")?.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Sets the intent display based on the enemy's predicted action.
    /// </summary>
    /// <param name="intent">The enemy intent to display.</param>
    public void SetIntent(EnemyIntent intent)
    {
        if (intent == null)
        {
            ClearIntent();
            return;
        }

        // Set the icon based on intent type
        if (intentImage != null)
        {
            Sprite iconToUse = GetIconForIntent(intent);
            intentImage.sprite = iconToUse;
            intentImage.gameObject.SetActive(iconToUse != null);
        }

        // Set the value text
        string valueText = intent.Value > 0 ? intent.Value.ToString() : "";
        SetIntentText(valueText);

        // Make sure the display is visible
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Clears the intent display.
    /// </summary>
    public void ClearIntent()
    {
        if (intentImage != null)
        {
            intentImage.sprite = null;
            intentImage.gameObject.SetActive(false);
        }

        SetIntentText("");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the intent during player's turn.
    /// </summary>
    public void ShowIntent()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the intent during enemy's turn.
    /// </summary>
    public void HideIntent()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Gets the appropriate icon for the given intent.
    /// </summary>
    private Sprite GetIconForIntent(EnemyIntent intent)
    {
        // Use custom icon if provided
        if (intent.Icon != null)
            return intent.Icon;

        // Use default icon based on type
        switch (intent.Type)
        {
            case IntentType.Attack:
                return attackIcon;
            case IntentType.Buff:
                return buffIcon;
            default:
                return null;
        }
    }

    /// <summary>
    /// Sets text using either regular Text or TextMeshPro.
    /// </summary>
    private void SetIntentText(string text)
    {
        bool hasText = !string.IsNullOrEmpty(text);

        if (intentTextTMP != null)
        {
            intentTextTMP.text = text;
            intentTextTMP.gameObject.SetActive(hasText);
        }
        else if (intentText != null)
        {
            intentText.text = text;
            intentText.gameObject.SetActive(hasText);
        }
    }
}