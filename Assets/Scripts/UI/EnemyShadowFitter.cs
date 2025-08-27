using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class EnemyShadowFitter : MonoBehaviour
{
    public RectTransform target;                 // π.χ. enemyImage.rectTransform
    public bool followEveryFrame = true;

    public enum Mode { Auto, Manual }
    public Mode mode = Mode.Auto;

    [Header("Auto sizing")]
    [Range(0.1f, 2f)] public float widthMultiplier = 0.7f;
    [Range(0.05f, 0.6f)] public float heightToWidth = 0.22f;

    [Header("Manual sizing")]
    public Vector2 manualSize = new Vector2(180f, 28f);

    [Header("Offsets (canvas units)")]
    public Vector2 offsets = new Vector2(0f, -10f);

    [Header("Clamp (auto only)")]
    public Vector2 minSize = new Vector2(60f, 10f);
    public Vector2 maxSize = new Vector2(800f, 180f);

    RectTransform rt;
    Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        FitNow();
    }

    void OnEnable() => FitNow();
    void OnRectTransformDimensionsChange() { if (!Application.isPlaying) FitNow(); }
    void Update() { if (followEveryFrame && Application.isPlaying) FitNow(); }

    public void ApplyFromData(ShadowMode dataMode, float wMul, float hToW, Vector2 offs, Vector2 manual)
    {
        mode = (dataMode == ShadowMode.Manual) ? Mode.Manual
                                               : (dataMode == ShadowMode.None ? mode : Mode.Auto);
        widthMultiplier = wMul;
        heightToWidth = hToW;
        offsets = offs;
        manualSize = manual;
        FitNow();
    }

    public void FitNow()
    {
        if (!rt || !target) return;

        // 1) Υπολογισμός μεγέθους
        if (mode == Mode.Manual)
        {
            rt.sizeDelta = manualSize;
        }
        else // Auto
        {
            Vector3[] c = new Vector3[4];
            target.GetWorldCorners(c);

            float widthWorld = Vector3.Distance(c[0], c[3]); // bottom edge
            float widthPx = WorldToScreenLength(widthWorld);
            float w = Mathf.Clamp(widthPx * widthMultiplier, minSize.x, maxSize.x);
            float h = Mathf.Clamp(w * heightToWidth, minSize.y, maxSize.y);
            rt.sizeDelta = new Vector2(w, h);
        }

        // 2) Τοποθέτηση στον **γονέα** του Shadow (ίδιο local space με anchoredPosition)
        {
            Vector3[] c = new Vector3[4];
            target.GetWorldCorners(c);
            Vector2 bottomCenterWorld = (c[0] + c[3]) * 0.5f;

            Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            var parentRT = rt.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRT,
                RectTransformUtility.WorldToScreenPoint(cam, bottomCenterWorld),
                cam,
                out var localPointInParent
            );

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = localPointInParent + offsets;
        }
    }

    float WorldToScreenLength(float worldLen)
    {
        Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        Vector3 a = Vector3.zero;
        Vector3 b = new Vector3(worldLen, 0f, 0f);
        Vector3 sa = RectTransformUtility.WorldToScreenPoint(cam, a);
        Vector3 sb = RectTransformUtility.WorldToScreenPoint(cam, b);
        return Vector3.Distance(sa, sb);
    }
}
