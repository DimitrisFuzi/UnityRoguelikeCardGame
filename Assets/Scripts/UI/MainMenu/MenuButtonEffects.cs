using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles hover and click effects for UI buttons, including:
/// - Scale animation
/// - Shadow fade
/// - Hover and click SFX playback
/// </summary>
public class MenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Animation Settings

    [Header("Scale Settings")]
    [Tooltip("Target scale when hovering.")]
    public float hoverScale = 1.1f;

    [Tooltip("Duration of the hover scale animation.")]
    public float scaleDuration = 0.2f;

    [Header("Click Animation")]
    [Tooltip("Target scale on click press.")]
    public float clickScale = 0.95f;

    [Tooltip("Duration of the click scale animation.")]
    public float clickDuration = 0.1f;

    #endregion

    #region Shadow Settings

    [Header("Shadow")]
    [Tooltip("Shadow image that fades in/out on hover.")]
    public Image shadowImage;

    [Tooltip("Duration of the shadow fade effect.")]
    public float shadowFadeDuration = 0.2f;

    [Tooltip("Target alpha of the shadow on hover.")]
    public float shadowTargetAlpha = 0.5f;

    #endregion

    #region SFX Settings

    [Header("SFX")]
    [Tooltip("Name of the SFX clip to play on hover (must match AudioManager entry).")]
    public string hoverSFX = "MainMenuHover";

    [Tooltip("Name of the SFX clip to play on click (must match AudioManager entry).")]
    public string clickSFX = "MainMenuClick";

    #endregion

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (shadowImage != null)
        {
            Color c = shadowImage.color;
            c.a = 0f;
            shadowImage.color = c;
        }
    }

    /// <summary>
    /// Called when the pointer enters the button area.
    /// Triggers scale up, shadow fade, and hover SFX.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(hoverScale, scaleDuration).SetEase(Ease.OutSine);

        if (shadowImage != null)
        {
            Color c = shadowImage.color;
            c.a = shadowTargetAlpha;
            shadowImage.DOFade(c.a, shadowFadeDuration);
        }

        AudioManager.Instance?.PlaySFX(hoverSFX);
    }

    /// <summary>
    /// Called when the pointer exits the button area.
    /// Reverses scale and fades out shadow.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(1f, scaleDuration).SetEase(Ease.OutSine);

        if (shadowImage != null)
        {
            shadowImage.DOFade(0f, shadowFadeDuration);
        }
    }

    /// <summary>
    /// Called on button click.
    /// Plays click scale animation and click SFX.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        rectTransform.DOKill(); // stop existing tweens
        rectTransform
            .DOScale(clickScale, clickDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() => rectTransform.DOScale(1f, clickDuration));

        AudioManager.Instance?.PlaySFX(clickSFX);
    }
}
