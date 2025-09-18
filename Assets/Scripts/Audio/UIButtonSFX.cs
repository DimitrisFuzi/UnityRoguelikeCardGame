using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[AddComponentMenu("UI/UIButton SFX")]
/// <summary>
/// Simple hover/click SFX for UI buttons using AudioManager.
/// </summary>
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private string clickSFX = "MainMenuClick";
    [SerializeField] private string hoverSFX = "MainMenuHover";

    /// <summary>Play hover SFX (if set) when the pointer enters.</summary>
    public void OnPointerEnter(PointerEventData e)
    {
        if (!string.IsNullOrEmpty(hoverSFX))
            AudioManager.Instance?.PlaySFX(hoverSFX);
    }

    /// <summary>Play click SFX (if set) on left click.</summary>
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && !string.IsNullOrEmpty(clickSFX))
            AudioManager.Instance?.PlaySFX(clickSFX);
    }
}
