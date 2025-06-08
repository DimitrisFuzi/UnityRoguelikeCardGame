using UnityEngine;
using UnityEngine.EventSystems; //This allows us to use Unity's event system to detect our mouse inputs

public class DragUIObject : MonoBehaviour, IDragHandler, IPointerDownHandler //These classes hold the methods required to handle UI interactions that we need
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    public float movementSensitivity = 1.0f; // Adjustable sensitivity if needed

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); //Get the RectTransform component of the attached GameObject
        canvas = GetComponentInParent<Canvas>(); //Get the Canvas component of the attached GameObject
    }

    public void OnPointerDown(PointerEventData eventData) //This is inherited from the IPointerDownHandler class referenced above
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out originalLocalPointerPosition); //Using the event system to detect what is clicked on
        originalPanelLocalPosition = rectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData) //This is inherited from the IDragHandler class referenced above
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
        {
            localPointerPosition /= canvas.scaleFactor;

            // Adjusting the movement based on sensitivity
            Vector3 offsetToOriginal = (localPointerPosition - originalLocalPointerPosition) * movementSensitivity;
            rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;

            // Debug output
            Debug.Log($"Drag - LocalPointerPosition: {localPointerPosition}, Offset: {offsetToOriginal}, New Position: {rectTransform.localPosition}"); //Comment out this line if not debugging an issue, otherwise it will flood the console unnecessarily
        }
    }
}
