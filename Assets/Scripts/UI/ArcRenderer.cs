using System.Collections.Generic;
using UnityEngine;

public class ArcRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField, Tooltip("Prefab used for the arrow at the end of the arc.")]
    private GameObject arrowPrefab;

    [SerializeField, Tooltip("Prefab used for the dots forming the arc.")]
    private GameObject dotPrefab;

    [Header("Settings")]
    [SerializeField, Tooltip("Number of dots to pre-instantiate for the arc.")]
    private int poolSize = 50;

    [SerializeField, Tooltip("Reference screen width for scaling spacing.")]
    private float baseScreenWidth = 1920f;

    [SerializeField, Tooltip("Spacing between dots before scaling.")]
    private float spacing = 50f;

    [SerializeField, Tooltip("Angle adjustment applied to the arrow's rotation.")]
    private float arrowAngleAdjustment = 0f;

    [SerializeField, Tooltip("Number of dots to skip at the end for arrow placement.")]
    private int dotsToSkip = 1;

    private readonly List<GameObject> dotPool = new();
    private GameObject arrowInstance;
    private Vector3 arrowDirection;
    private float spacingScale;

    private void Awake()
    {
        if (arrowPrefab == null)
        {
            Logger.LogError("ArcRenderer: arrowPrefab is not assigned in the Inspector!", this);
            enabled = false;
            return;
        }

        if (dotPrefab == null)
        {
            Logger.LogError("ArcRenderer: dotPrefab is not assigned in the Inspector!", this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        arrowInstance = Instantiate(arrowPrefab, transform);
        arrowInstance.transform.localPosition = Vector3.zero;

        InitializeDotPool(poolSize);
        UpdateSpacingScale();
    }

    private void OnEnable() => UpdateSpacingScale();

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;

        Vector3 startPos = transform.position;
        Vector3 midPoint = CalculateMidPoint(startPos, mousePos);

        UpdateArc(startPos, midPoint, mousePos);
        PositionAndRotateArrow(mousePos);
    }

    private void UpdateArc(Vector3 start, Vector3 mid, Vector3 end)
    {
        int numDots = Mathf.CeilToInt(Vector3.Distance(start, end) / (spacing * spacingScale));
        numDots = Mathf.Min(numDots, dotPool.Count);

        for (int i = 0; i < numDots; i++)
        {
            float t = Mathf.Clamp01(i / (float)numDots);
            Vector3 position = QuadraticBezierPoint(start, mid, end, t);

            if (i != numDots - dotsToSkip)
            {
                dotPool[i].transform.position = position;
                dotPool[i].SetActive(true);
            }

            if (i == numDots - (dotsToSkip + 1))
            {
                arrowDirection = dotPool[i].transform.position;
            }
        }

        // Deactivate unused dots
        for (int i = numDots - dotsToSkip; i < dotPool.Count; i++)
        {
            if (i >= 0) dotPool[i].SetActive(false);
        }
    }

    private void PositionAndRotateArrow(Vector3 position)
    {
        if (arrowInstance == null) return;

        arrowInstance.transform.position = position;

        Vector3 direction = arrowDirection - position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + arrowAngleAdjustment;
            arrowInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private Vector3 CalculateMidPoint(Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) * 0.5f;
        float arcHeight = Vector3.Distance(start, end) / 3f;
        midpoint.y += arcHeight;
        return midpoint;
    }

    private Vector3 QuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return uu * start + 2 * u * t * control + tt * end;
    }

    private void InitializeDotPool(int count)
    {
        dotPool.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity, transform);
            dot.SetActive(false);
            dotPool.Add(dot);
        }
    }

    private void UpdateSpacingScale()
    {
        spacingScale = Screen.width / baseScreenWidth;
    }
}
