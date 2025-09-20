using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles dragging of a UI element within a canvas using Unity's event system.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DragUIObject : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Tooltip("Controls how fast or slow the UI element moves during drag.")]
    [SerializeField] private float movementSensitivity = 1.0f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            Logger.LogError("DragUIObject: No Canvas found in parent hierarchy.", this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out originalLocalPointerPosition
        );

        originalPanelLocalPosition = rectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            localPointerPosition /= canvas.scaleFactor;

            Vector3 offsetToOriginal = (localPointerPosition - originalLocalPointerPosition) * movementSensitivity;
            rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }
    }
}
