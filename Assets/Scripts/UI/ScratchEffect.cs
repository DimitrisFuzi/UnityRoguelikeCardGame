using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class ScratchEffect : MonoBehaviour
{
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float fadeDelay = 0.2f;
    [SerializeField] private float punchScale = 1.2f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void PlayEffect()
    {
        if (rectTransform == null || canvasGroup == null)
        {
            Logger.LogError("[ScratchEffect] Missing RectTransform or CanvasGroup!", this);
            return;
        }

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        canvasGroup.alpha = 1f;

        // Subtle punch
        rectTransform
            .DOPunchScale(Vector3.one * punchScale, 0.2f, 6, 0.8f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        // Smooth fade-out
        canvasGroup
            .DOFade(0f, showDuration)
            .SetDelay(fadeDelay)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .OnComplete(() => Destroy(gameObject));
    }
}
