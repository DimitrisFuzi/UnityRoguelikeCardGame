using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI display of the discard pile count.
/// Listens to changes in the discard pile and updates the text accordingly.
/// </summary>
public class DiscardPileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text discardPileText; // Reference to the TMP text that displays the discard pile count

    /// <summary>
    /// Called on object start. Initializes the UI with the current discard pile count.
    /// </summary>
    void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDiscardPileUI(); // Initialize UI with current discard count
        }
        else
        {
            Debug.LogError("❌ DeckManager instance was not found on Start.");
        }
    }

    /// <summary>
    /// Updates the discard pile text to reflect the current discard pile count.
    /// </summary>
    public void UpdateDiscardPileUI()
    {
        if (DeckManager.Instance != null)
        {
            discardPileText.text = DeckManager.Instance.GetDiscardPileCount().ToString();
        }
    }

    /// <summary>
    /// Registers to the discard pile change event when this object is enabled.
    /// </summary>
    void OnEnable()
    {
        DeckManager.OnDiscardPileChanged += UpdateDiscardPileUI;
    }

    /// <summary>
    /// Unregisters from the discard pile change event when this object is disabled.
    /// </summary>
    void OnDisable()
    {
        DeckManager.OnDiscardPileChanged -= UpdateDiscardPileUI;
    }
}
