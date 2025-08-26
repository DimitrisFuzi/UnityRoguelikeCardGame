using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class UIButtonPulseScaleOnly : MonoBehaviour
{
    [Header("Pulse Settings")]
    public bool pulse = true;
    public float pulseScale = 1.06f;   // 1.0 -> 1.06
    public float pulsePeriod = 1.1f;   // seconds for a full in/out cycle
    public bool respectInteractable = true;

    RectTransform rt;
    Vector3 baseScale;
    Button btn;
    Coroutine pulseCo;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
        btn = GetComponent<Button>();
    }

    void OnEnable()
    {
        StartPulse();
    }

    void OnDisable()
    {
        StopPulse();
        rt.localScale = baseScale;
    }

    public void StartPulse()
    {
        if (!pulse) return;
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = StartCoroutine(Pulse());
    }

    public void StopPulse()
    {
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = null;
    }

    IEnumerator Pulse()
    {
        float t = 0f;
        while (true)
        {
            // Προαιρετικά σταμάτα το pulse όταν το κουμπί δεν είναι interactable
            if (respectInteractable && btn && !btn.interactable)
            {
                rt.localScale = baseScale;
                yield return null;
                continue;
            }

            t += Time.unscaledDeltaTime;
            float s = (Mathf.Sin(t * (2f * Mathf.PI) / pulsePeriod) + 1f) * 0.5f; // 0..1
            float k = Mathf.Lerp(1f, pulseScale, s);
            rt.localScale = baseScale * k;
            yield return null;
        }
    }
}
