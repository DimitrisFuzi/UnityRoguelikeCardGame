using UnityEngine;

/// <summary>
/// Dynamically positions a target object within the screen based on camera dimensions and configurable grid-based multipliers.
/// </summary>
public class PositionObject : MonoBehaviour
{
    [Tooltip("Reference to the main camera. Defaults to Camera.main if left empty.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("The GameObject to position.")]
    [SerializeField] private GameObject objectToPosition;

    [Tooltip("How many horizontal divisions to make on the screen width.")]
    [SerializeField] private int widthDivider = 2;

    [Tooltip("How many vertical divisions to make on the screen height.")]
    [SerializeField] private int heightDivider = 2;

    [Tooltip("The column multiplier to determine the x-position in the divided grid.")]
    [SerializeField] private float widthMultiplier = 1f;

    [Tooltip("The row multiplier to determine the y-position in the divided grid.")]
    [SerializeField] private float heightMultiplier = 1f;

    [Tooltip("If true, updates the object position every frame.")]
    [SerializeField] private bool updatePosition = false;

    /// <summary>
    /// Initializes camera reference and sets the initial object position.
    /// </summary>

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Logger.Log("PositionObject: mainCamera not set. Defaulting to Camera.main", this);
        }

        SetObjectPosition();
    }

    /// <summary>
    /// Updates the object's position each frame if updatePosition is true.
    /// </summary>
    private void Update()
    {
        if (updatePosition)
        {
            SetObjectPosition();
        }
    }

    /// <summary>
    /// Calculates and sets the object's world position based on screen grid divisions and multipliers.
    /// </summary>
    private void SetObjectPosition()
    {
        if (mainCamera == null)
        {
            Logger.LogWarning("PositionObject: mainCamera is null.", this);
            return;
        }

        if (objectToPosition == null)
        {
            Logger.LogWarning("PositionObject: objectToPosition is not assigned.", this);
            return;
        }

        if (widthDivider == 0 || heightDivider == 0)
        {
            Logger.LogError("PositionObject: widthDivider or heightDivider cannot be zero.", this);
            return;
        }

        float screenHeight = 2f * mainCamera.orthographicSize;
        float screenWidth = screenHeight * mainCamera.aspect;

        float segmentWidth = screenWidth / widthDivider;
        float segmentHeight = screenHeight / heightDivider;

        float newX = (segmentWidth * (widthMultiplier - 0.5f)) - (screenWidth / 2);
        float newY = (segmentHeight * (heightMultiplier - 0.5f)) - (screenHeight / 2);

        objectToPosition.transform.position = new Vector3(newX, newY, objectToPosition.transform.position.z);

        Logger.Log($"PositionObject: Positioned at ({newX}, {newY})", this);
    }
}