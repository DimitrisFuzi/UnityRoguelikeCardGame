using UnityEngine;
using UnityEngine.UI;

/// Auto-sizes & positions a shadow ellipse under a target RectTransform (enemy).
/// Works with dynamic sizes/scales (even during tweens).
[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class EnemyShadowFitter : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;          // �.�. EnemyDisplay (root) � enemyImage.rectTransform
    public bool followEveryFrame = true;  // �� ������� �������� �������/scale

    [Header("Sizing")]
    [Tooltip("������ ����� �� ������� ��� ������� ��� ������.")]
    [Range(0.1f, 2f)] public float widthMultiplier = 0.65f;
    [Tooltip("���� ����� �� ������� ��� ������� ��� ����� (ellipse).")]
    [Range(0.05f, 0.6f)] public float heightToWidth = 0.22f;

    [Header("Offsets (in canvas units)")]
    public float yOffset = -8f;           // ���� ���� ��� �� �����
    public float xOffset = 0f;

    [Header("Clamp (�����������)")]
    public Vector2 minSize = new Vector2(60f, 10f);
    public Vector2 maxSize = new Vector2(800f, 180f);

    RectTransform rt;
    Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        // �� ����� ���� ��� �� enemyImage:
        // (���������� sibling index)
        if (target) transform.SetSiblingIndex(Mathf.Max(0, target.GetSiblingIndex() - 1));
        FitNow();
    }

    void OnEnable() => FitNow();
    void OnRectTransformDimensionsChange() { if (!Application.isPlaying) FitNow(); }
    void Update() { if (followEveryFrame && Application.isPlaying) FitNow(); }

    public void FitNow()
    {
        if (!rt || !target) return;

        // 1) ���� world corners ��� target
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        // ������/���� �� world (0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right)
        float widthWorld = Vector3.Distance(corners[0], corners[3]); // bottom edge
        float heightWorld = Vector3.Distance(corners[0], corners[1]); // left edge

        // 2) ��������� world->pixels (��� UI) ��� ���������� ��������
        float widthPx = WorldToScreenLength(widthWorld);
        float heightPx = WorldToScreenLength(heightWorld);

        float w = Mathf.Clamp(widthPx * widthMultiplier, minSize.x, maxSize.x);
        float h = Mathf.Clamp(w * heightToWidth, minSize.y, maxSize.y);
        rt.sizeDelta = new Vector2(w, h);

        // 3) ������ ��� bottom edge ��� target �� **parent space** ��� Shadow
        //    (��� �� canvas space)
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

        // 4) �������/����� ��� ������ ��� ���������� �� ����� offset ���� �� ����
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPointInParent + new Vector2(xOffset, yOffset);
    }

    float WorldToScreenLength(float worldLen)
    {
        // ��� UI �� ����� ������� �� pixels (screen space)
        Camera cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
        Vector3 a = Vector3.zero;
        Vector3 b = new Vector3(worldLen, 0f, 0f);
        Vector3 sa = RectTransformUtility.WorldToScreenPoint(cam, a);
        Vector3 sb = RectTransformUtility.WorldToScreenPoint(cam, b);
        return Vector3.Distance(sa, sb);
    }


    float WorldToCanvasLength(float worldLen)
    {
        // �� Screen Space - Overlay �� world==screen pixels. �� ����� �����������:
        if (!canvas || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return worldLen;

        // ScreenSpaceCamera/WorldSpace: project ��� ������ �� screen ��� ����� pixels
        Vector3 a = Vector3.zero;
        Vector3 b = new Vector3(worldLen, 0f, 0f);
        Camera cam = canvas.worldCamera;
        Vector3 sa = RectTransformUtility.WorldToScreenPoint(cam, a);
        Vector3 sb = RectTransformUtility.WorldToScreenPoint(cam, b);
        return Vector3.Distance(sa, sb);
    }
}
