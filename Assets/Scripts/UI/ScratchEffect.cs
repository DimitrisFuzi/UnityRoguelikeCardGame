using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

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

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void PlayEffect(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;

        rectTransform.DOPunchScale(Vector3.one * punchScale, 0.3f, 6, 0.5f);
        canvasGroup.DOFade(0f, showDuration).SetDelay(fadeDelay).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
