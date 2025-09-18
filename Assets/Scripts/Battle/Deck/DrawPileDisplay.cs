using UnityEngine;
using TMPro;

/// <summary>
/// UI display for the draw pile count; updates when the pile changes.
/// </summary>
public class DrawPileUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text drawPileText;

    private void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDrawPileUI();
        }
        else
        {
            Logger.LogError("DrawPileUI: DeckManager instance not found.", this);
        }
    }

    /// <summary>Refresh draw pile count text.</summary>
    public void UpdateDrawPileUI()
    {
        if (DeckManager.Instance == null || drawPileText == null) return;
        drawPileText.text = DeckManager.Instance.GetDrawPileCount().ToString();
    }

    private void OnEnable()
    {
        DeckManager.OnDrawPileChanged += UpdateDrawPileUI;
    }

    private void OnDisable()
    {
        DeckManager.OnDrawPileChanged -= UpdateDrawPileUI;
    }
}
