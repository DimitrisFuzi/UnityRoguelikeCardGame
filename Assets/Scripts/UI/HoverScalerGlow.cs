using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class HoverScalerGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    public float hoverScale = 1.08f;
    public float scaleDuration = 0.12f;

    [Header("Glow (optional)")]
    [Tooltip("Child Image with a soft-round sprite (additive look).")]
    public Image glowImage;
    public Color glowColor = new Color(1f, 0.95f, 0.6f, 0.35f);
    public float glowMaxAlpha = 0.35f;
    public float glowLerp = 0.12f;

    RectTransform rt;
    Coroutine scaleCo, glowCo;
    float baseScale = 1f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale.x;

        if (glowImage)
        {
            var c = glowImage.color; c.a = 0f;
            glowImage.color = c;
            glowImage.raycastTarget = false;
        }
    }

    public void OnPointerEnter(PointerEventData _)
    {
        StartScale(baseScale * hoverScale);
        StartGlow(glowMaxAlpha);
    }

    public void OnPointerExit(PointerEventData _)
    {
        StartScale(baseScale);
        StartGlow(0f);
    }

    void StartScale(float target)
    {
        if (scaleCo != null) StopCoroutine(scaleCo);
        scaleCo = StartCoroutine(ScaleTo(target));
    }

    IEnumerator ScaleTo(float target)
    {
        float t = 0f;
        Vector3 from = rt.localScale;
        Vector3 to = new Vector3(target, target, target);
        while (t < scaleDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / scaleDuration);
            rt.localScale = Vector3.Lerp(from, to, k);
            yield return null;
        }
        rt.localScale = to;
    }

    void StartGlow(float targetA)
    {
        if (!glowImage) return;
        if (glowCo != null) StopCoroutine(glowCo);
        glowCo = StartCoroutine(GlowTo(targetA));
    }

    IEnumerator GlowTo(float targetA)
    {
        Color start = glowImage.color;
        Color end = glowColor; end.a = targetA;
        float t = 0f;
        while (t < glowLerp)
        {
            t += Time.unscaledDeltaTime;
            glowImage.color = Color.Lerp(start, end, t / glowLerp);
            yield return null;
        }
        glowImage.color = end;
    }
}
