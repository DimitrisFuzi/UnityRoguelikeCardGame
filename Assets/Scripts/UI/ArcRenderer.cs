using System.Collections.Generic;
using UnityEngine;

public class ArcRenderer : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField]
    [Tooltip("Prefab used for the arrow at the end of the arc.")]
    private GameObject arrowPrefab;

    [SerializeField]
    [Tooltip("Prefab used for the dots forming the arc.")]
    private GameObject dotPrefab;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Number of dots to pre-instantiate for the arc.")]
    private int poolSize = 50;

    [SerializeField]
    [Tooltip("Reference screen width for scaling spacing.")]
    private float baseScreenWidth = 1920f;

    [SerializeField]
    [Tooltip("Spacing between dots before scaling.")]
    private float spacing = 50f;

    [SerializeField]
    [Tooltip("Angle adjustment applied to the arrow's rotation.")]
    private float arrowAngleAdjustment = 0f;

    [SerializeField]
    [Tooltip("Number of dots to skip at the end for arrow placement.")]
    private int dotsToSkip = 1;

    private List<GameObject> dotPool = new List<GameObject>();
    private GameObject arrowInstance;
    private Vector3 arrowDirection;
    private float spacingScale;


    /// <summary>
    /// Ensures required prefabs are assigned; disables script otherwise.
    /// </summary>
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

    /// <summary>
    /// Initializes the arrow instance and the dot pool, sets spacing scale.
    /// </summary>
    private void Start()
    {
        arrowInstance = Instantiate(arrowPrefab, transform);
        arrowInstance.transform.localPosition = Vector3.zero;

        InitializeDotPool(poolSize);
        UpdateSpacingScale();

        Logger.Log($"ArcRenderer initialized with pool size {poolSize} and spacing scale {spacingScale:F2}.", this);
    }

    /// <summary>
    /// Updates spacing scale on enable.
    /// </summary>
    private void OnEnable()
    {
        UpdateSpacingScale();
    }

    /// <summary>
    /// Updates the arc position and arrow orientation every frame based on mouse position.
    /// </summary>
    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;

        Vector3 startPos = transform.position;
        Vector3 midPoint = CalculateMidPoint(startPos, mousePos);

        UpdateArc(startPos, midPoint, mousePos);
        PositionAndRotateArrow(mousePos);
    }

    /// <summary>
    /// Updates the positions of dots along the quadratic bezier curve between start, mid, and end points.
    /// Also updates which dots are active.
    /// </summary>
    /// <param name="start">Start point of the arc.</param>
    /// <param name="mid">Control point (midpoint with height offset).</param>
    /// <param name="end">End point of the arc.</param>
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

            if (i == numDots - (dotsToSkip + 1) && i - dotsToSkip + 1 >= 0)
            {
                arrowDirection = dotPool[i].transform.position;
            }
        }

        // Deactivate unused dots
        for (int i = numDots - dotsToSkip; i < dotPool.Count; i++)
        {
            if (i >= 0)
            {
                dotPool[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Positions the arrow GameObject at the specified position and rotates it to point along the arc direction.
    /// </summary>
    /// <param name="position">The position to place the arrow.</param>
    private void PositionAndRotateArrow(Vector3 position)
    {
        arrowInstance.transform.position = position;

        Vector3 direction = arrowDirection - position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + arrowAngleAdjustment;

        arrowInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    /// <summary>
    /// Calculates the midpoint between start and end positions with an added vertical arc height.
    /// </summary>
    /// <param name="start">Start position of the arc.</param>
    /// <param name="end">End position of the arc.</param>
    /// <returns>Calculated midpoint with vertical offset.</returns>
    private Vector3 CalculateMidPoint(Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) * 0.5f;
        float arcHeight = Vector3.Distance(start, end) / 3f;
        midpoint.y += arcHeight;
        return midpoint;
    }

    /// <summary>
    /// Calculates a point on a quadratic bezier curve at parameter t.
    /// </summary>
    /// <param name="start">Start point.</param>
    /// <param name="control">Control (mid) point.</param>
    /// <param name="end">End point.</param>
    /// <param name="t">Interpolation parameter (0 to 1).</param>
    /// <returns>Point on the bezier curve.</returns>
    private Vector3 QuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return uu * start + 2 * u * t * control + tt * end;
    }

    /// <summary>
    /// Instantiates the dot pool GameObjects and disables them initially.
    /// </summary>
    /// <param name="count">Number of dots to instantiate.</param>
    private void InitializeDotPool(int count)
    {
        dotPool.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity, transform);
            dot.SetActive(false);
            dotPool.Add(dot);
        }

        Logger.Log($"ArcRenderer: Initialized dot pool with {count} dots.", this);
    }

    /// <summary>
    /// Updates the spacing scale based on current screen width relative to baseScreenWidth.
    /// </summary>
    private void UpdateSpacingScale()
    {
        spacingScale = Screen.width / baseScreenWidth;
    }
}