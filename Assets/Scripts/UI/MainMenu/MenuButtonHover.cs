using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MenuButtonHoverWithShadow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale Settings")]
    public float hoverScale = 1.1f;
    public float scaleDuration = 0.2f;

    [Header("Shadow")]
    public Image shadowImage;
    public float shadowFadeDuration = 0.2f;
    public float shadowTargetAlpha = 0.5f;

    [Header("Click Animation")]
    public float clickScale = 0.95f;
    public float clickDuration = 0.1f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (shadowImage != null)
        {
            Color c = shadowImage.color;
            c.a = 0f;
            shadowImage.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(hoverScale, scaleDuration).SetEase(Ease.OutSine);

        if (shadowImage != null)
        {
            Color c = shadowImage.color;
            c.a = shadowTargetAlpha;
            shadowImage.DOFade(c.a, shadowFadeDuration);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(1f, scaleDuration).SetEase(Ease.OutSine);

        if (shadowImage != null)
        {
            shadowImage.DOFade(0f, shadowFadeDuration);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        rectTransform.DOKill();
        rectTransform
            .DOScale(clickScale, clickDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() => rectTransform.DOScale(1f, clickDuration));
    }
}
