using UnityEngine;
using TMPro;

/// <summary>
/// UI display for the discard pile count; updates when the pile changes.
/// </summary>
public class DiscardPileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text discardPileText;

    private void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDiscardPileUI();
        }
        else
        {
            Logger.LogError("DiscardPileUI: DeckManager instance not found.", this);
        }
    }

    /// <summary>Refresh discard pile count text.</summary>
    public void UpdateDiscardPileUI()
    {
        if (DeckManager.Instance == null || discardPileText == null) return;
        discardPileText.text = DeckManager.Instance.GetDiscardPileCount().ToString();
    }

    private void OnEnable()
    {
        DeckManager.OnDiscardPileChanged += UpdateDiscardPileUI;
    }

    private void OnDisable()
    {
        DeckManager.OnDiscardPileChanged -= UpdateDiscardPileUI;
    }
}
