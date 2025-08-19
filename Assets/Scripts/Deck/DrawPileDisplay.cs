using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI display of the 
/// pile count.
/// Listens to draw pile changes and updates the UI accordingly.
/// </summary>
public class DrawPileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text drawPileText; // Reference to the TMP text element displaying the draw pile count

    /// <summary>
    /// Called on Start. Initializes the draw pile UI if the DeckManager is available.
    /// </summary>
    void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDrawPileUI(); // Initialize UI with current draw pile count
        }
        else
        {
            Debug.LogError("❌ DeckManager instance was not found on Start.");
        }
    }

    /// <summary>
    /// Updates the draw pile text to reflect the current number of cards in the draw pile.
    /// </summary>
    public void UpdateDrawPileUI()
    {
        if (DeckManager.Instance != null)
        {
            drawPileText.text = DeckManager.Instance.GetDrawPileCount().ToString();
        }
    }

    /// <summary>
    /// Subscribes to the OnDrawPileChanged event when this component is enabled.
    /// </summary>
    void OnEnable()
    {
        DeckManager.OnDrawPileChanged += UpdateDrawPileUI;
    }

    /// <summary>
    /// Unsubscribes from the OnDrawPileChanged event when this component is disabled.
    /// </summary>
    void OnDisable()
    {
        DeckManager.OnDrawPileChanged -= UpdateDrawPileUI;
    }
}
