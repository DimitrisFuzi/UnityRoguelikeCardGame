using UnityEngine;

public class UIObjectPositioner : MonoBehaviour
{
    public RectTransform objectToPosition;

    public int widthDivider = 2;
    public int heightDivider = 2;
    public float widthMultiplier = 1f;
    public float heightMultiplier = 1f;

    public bool updatePosition = false;

    void Start()
    {
        SetUIObjectPosition();
    }

    void Update()
    {
        if (updatePosition)
        {
            SetUIObjectPosition();
        }
    }

    public void SetUIObjectPosition()
    {
        if (objectToPosition != null && widthDivider != 0 && heightDivider != 0)
        {
            // Calculate the anchor position
            float anchorX = widthMultiplier / widthDivider;
            float anchorY = heightMultiplier / heightDivider;

            // Set the anchor and pivot
            objectToPosition.anchorMin = new Vector2(anchorX, anchorY);
            objectToPosition.anchorMax = new Vector2(anchorX, anchorY);
            objectToPosition.pivot = new Vector2(0.5f, 0.5f);

            // Set the local position to zero to align with the anchor point
            objectToPosition.anchoredPosition = Vector2.zero;
        }
    }
}
