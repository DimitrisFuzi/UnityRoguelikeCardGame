using UnityEngine;

public class PositionObject : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject objectToPosition;
    public int widthDivider = 2;
    public int heightDivider = 2;
    public float widthMultiplier = 1f;
    public float heightMultiplier = 1f;

    public bool updatePosition = false;

    void Start()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        SetObjectPosition();
    }

    void Update()
    {
        if (updatePosition)
        {
            SetObjectPosition();
        }
    }

    void SetObjectPosition()
    {
        if(mainCamera != null && objectToPosition != null && widthDivider != 0 && heightDivider != 0)
        {
            float height = 2f * mainCamera.orthographicSize;
            float width = height * mainCamera.aspect;

            // Calculate segment size
            float segmentWidth = width / widthDivider;
            float segmentHeight = height / heightDivider;

            // Calculate new position based on segments
            float newX = (segmentWidth * (widthMultiplier - 0.5f)) - (width / 2);
            float newY = (segmentHeight * (heightMultiplier - 0.5f)) - (height / 2);

            objectToPosition.transform.position = new Vector3(newX, newY, objectToPosition.transform.position.z);
        }
    }
}
