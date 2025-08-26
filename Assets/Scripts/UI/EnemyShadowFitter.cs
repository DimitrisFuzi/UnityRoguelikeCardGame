using UnityEngine;
using UnityEngine.UI;

/// Auto-sizes & positions a shadow ellipse under a target RectTransform (enemy).
/// Works with dynamic sizes/scales (even during tweens).
[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class EnemyShadowFitter : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;          // π.χ. EnemyDisplay (root) ή enemyImage.rectTransform
    public bool followEveryFrame = true;  // αν αλλάζει δυναμικά μέγεθος/scale

    [Header("Sizing")]
    [Tooltip("Πλάτος σκιού ως ποσοστό του πλάτους του εχθρού.")]
    [Range(0.1f, 2f)] public float widthMultiplier = 0.65f;
    [Tooltip("Ύψος σκιού ως ποσοστό του ΠΛΑΤΟΥΣ της σκιού (ellipse).")]
    [Range(0.05f, 0.6f)] public float heightToWidth = 0.22f;

    [Header("Offsets (in canvas units)")]
    public float yOffset = -8f;           // λίγο κάτω από τα πόδια
    public float xOffset = 0f;

    [Header("Clamp (προαιρετικό)")]
    public Vector2 minSize = new Vector2(60f, 10f);
    public Vector2 maxSize = new Vector2(800f, 180f);

    RectTransform rt;
    Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        // Να είναι ΠΙΣΩ από το enemyImage:
        // (χαμηλότερο sibling index)
        if (target) transform.SetSiblingIndex(Mathf.Max(0, target.GetSiblingIndex() - 1));
        FitNow();
    }

    void OnEnable() => FitNow();
    void OnRectTransformDimensionsChange() { if (!Application.isPlaying) FitNow(); }
    void Update() { if (followEveryFrame && Application.isPlaying) FitNow(); }

    public void FitNow()
    {
        if (!rt || !target) return;

        // 1) Πάρε world corners του target
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        // Πλάτος/ύψος σε world (0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right)
        float widthWorld = Vector3.Distance(corners[0], corners[3]); // bottom edge
        float heightWorld = Vector3.Distance(corners[0], corners[1]); // left edge

        // 2) Μετατροπή world->pixels (για UI) για υπολογισμό μεγέθους
        float widthPx = WorldToScreenLength(widthWorld);
        float heightPx = WorldToScreenLength(heightWorld);

        float w = Mathf.Clamp(widthPx * widthMultiplier, minSize.x, maxSize.x);
        float h = Mathf.Clamp(w * heightToWidth, minSize.y, maxSize.y);
        rt.sizeDelta = new Vector2(w, h);

        // 3) Κέντρο του bottom edge του target σε **parent space** του Shadow
        //    (όχι σε canvas space)
        Vector2 bottomCenterWorld = (corners[0] + corners[3]) * 0.5f;
        Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        Vector2 localPointInParent;
        var parentRT = rt.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRT,
            RectTransformUtility.WorldToScreenPoint(cam, bottomCenterWorld),
            cam,
            out localPointInParent
        );

        // 4) ’γκυρες/πίβοτ στο κέντρο και τοποθέτηση με μικρό offset προς τα κάτω
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPointInParent + new Vector2(xOffset, yOffset);
    }

    float WorldToScreenLength(float worldLen)
    {
        // Για UI το μήκος θέλουμε σε pixels (screen space)
        Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        Vector3 a = Vector3.zero;
        Vector3 b = new Vector3(worldLen, 0f, 0f);
        Vector3 sa = RectTransformUtility.WorldToScreenPoint(cam, a);
        Vector3 sb = RectTransformUtility.WorldToScreenPoint(cam, b);
        return Vector3.Distance(sa, sb);
    }


    float WorldToCanvasLength(float worldLen)
    {
        // Σε Screen Space - Overlay το world==screen pixels. Σε άλλες περιπτώσεις:
        if (!canvas || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return worldLen;

        // ScreenSpaceCamera/WorldSpace: project δύο σημεία σε screen και μέτρα pixels
        Vector3 a = Vector3.zero;
        Vector3 b = new Vector3(worldLen, 0f, 0f);
        Camera cam = canvas.worldCamera;
        Vector3 sa = RectTransformUtility.WorldToScreenPoint(cam, a);
        Vector3 sb = RectTransformUtility.WorldToScreenPoint(cam, b);
        return Vector3.Distance(sa, sb);
    }
}
