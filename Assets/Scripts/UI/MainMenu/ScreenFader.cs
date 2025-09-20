using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private float defaultDuration = 0.35f;
    private Canvas canvas;
    private Image overlay;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Canvas setup (overlay UI on top)
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;       // allow custom sorting order
        canvas.sortingOrder = 32760;         // high but safe order

        // Raycaster blocks input while fading
        gameObject.AddComponent<GraphicRaycaster>();

        // Black overlay
        var go = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        overlay = go.GetComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0f);
        overlay.raycastTarget = true;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        SetInstantClear();
        BringToFront();
    }

    public void SetInstantOpaque() { SetAlpha(1f); }
    public void SetInstantClear() { SetAlpha(0f); }

    private void SetAlpha(float a)
    {
        var c = overlay.color; c.a = a; overlay.color = c;
        // Block input when the overlay is visible
        overlay.raycastTarget = a > 0.001f;
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        float t = 0f; float start = overlay.color.a;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, 1f, t / duration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(1f);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;
        float t = 0f; float start = overlay.color.a;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, 0f, t / duration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(0f);
    }

    public void BringToFront(int order = 32760)
    {
        if (canvas == null) return;
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
    }
}
