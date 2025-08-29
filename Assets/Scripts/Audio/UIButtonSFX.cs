using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private string clickSFX = "MainMenuClick";
    [SerializeField] private string hoverSFX = "MainMenuHover";

    public void OnPointerEnter(PointerEventData e)
    {
        if (!string.IsNullOrEmpty(hoverSFX))
            AudioManager.Instance?.PlaySFX(hoverSFX);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && !string.IsNullOrEmpty(clickSFX))
            AudioManager.Instance?.PlaySFX(clickSFX);
    }
}
