using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class UIButtonPulse : MonoBehaviour
{
    [Header("Fade In")]
    public bool fadeInOnEnable = true;
    public float fadeInDuration = 0.25f;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseScale = 1.06f;
    public float pulsePeriod = 1.2f;

    [Header("Optional Glow")]
    public Image glowImage;                 // εξωτερικό glow Image (π.χ. soft-round, πίσω από το button)
    public float glowAlpha = 0.25f;

    CanvasGroup cg;
    RectTransform rt;
    Vector3 baseScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        StopAllCoroutines();
        if (fadeInOnEnable)
        {
            cg.alpha = 0f;
            StartCoroutine(FadeIn());
        }
        else
        {
            cg.alpha = 1f;
        }

        if (glowImage)
        {
            var c = glowImage.color; c.a = 0f; glowImage.color = c;
            StartCoroutine(GlowPulse());
        }

        if (pulse)
        {
            StartCoroutine(ScalePulse());
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t / fadeInDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator ScalePulse()
    {
        float t = 0f;
        while (enabled && gameObject.activeInHierarchy)
        {
            t += Time.unscaledDeltaTime;
            float k = (Mathf.Sin(t * (2f * Mathf.PI) / pulsePeriod) + 1f) * 0.5f; // 0..1
            float s = Mathf.Lerp(1f, pulseScale, k);
            rt.localScale = baseScale * s;
            yield return null;
        }
        rt.localScale = baseScale;
    }

    IEnumerator GlowPulse()
    {
        while (enabled && gameObject.activeInHierarchy)
        {
            float t = (Mathf.Sin(Time.unscaledTime * (2f * Mathf.PI) / pulsePeriod) + 1f) * 0.5f;
            var c = glowImage.color;
            c.a = Mathf.Lerp(0f, glowAlpha, t);
            glowImage.color = c;
            yield return null;
        }
    }
}
