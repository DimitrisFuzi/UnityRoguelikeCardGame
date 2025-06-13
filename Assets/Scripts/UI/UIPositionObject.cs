using UnityEngine;

/// <summary>
/// Dynamically positions a UI object (RectTransform) within its parent canvas based on configurable grid-based anchor ratios.
/// </summary>
public class UIObjectPositioner : MonoBehaviour
{
    [Tooltip("The RectTransform of the UI object to position.")]
    [SerializeField] private RectTransform objectToPosition;

    [Tooltip("How many horizontal divisions to make across the screen width.")]
    [SerializeField] private int widthDivider = 2;

    [Tooltip("How many vertical divisions to make across the screen height.")]
    [SerializeField] private int heightDivider = 2;

    [Tooltip("Column multiplier to determine the object's anchor X position.")]
    [SerializeField] private float widthMultiplier = 1f;

    [Tooltip("Row multiplier to determine the object's anchor Y position.")]
    [SerializeField] private float heightMultiplier = 1f;

    [Tooltip("If enabled, updates the object's position every frame.")]
    [SerializeField] private bool updatePosition = false;

    /// <summary>
    /// Sets the initial position of the UI object.
    /// </summary>
    private void Start()
    {
        SetUIObjectPosition();
    }

    /// <summary>
    /// Updates the position each frame if enabled.
    /// </summary>
    private void Update()
    {
        if (updatePosition)
        {
            SetUIObjectPosition();
        }
    }

    /// <summary>
    /// Positions the UI object by adjusting its anchors and position based on configured grid multipliers.
    /// </summary>
    public void SetUIObjectPosition()
    {
        if (objectToPosition == null)
        {
            return;
        }

        if (widthDivider == 0 || heightDivider == 0)
        {
            Logger.LogError("UIObjectPositioner: widthDivider and heightDivider must not be zero.", this);
            return;
        }

        float anchorX = widthMultiplier / widthDivider;
        float anchorY = heightMultiplier / heightDivider;

        objectToPosition.anchorMin = new Vector2(anchorX, anchorY);
        objectToPosition.anchorMax = new Vector2(anchorX, anchorY);
        objectToPosition.pivot = new Vector2(0.5f, 0.5f);
        objectToPosition.anchoredPosition = Vector2.zero;

        Logger.Log($"UIObjectPositioner: UI object positioned at anchor ({anchorX}, {anchorY}).", this);
    }
}
