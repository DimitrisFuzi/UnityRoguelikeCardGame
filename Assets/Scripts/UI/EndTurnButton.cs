using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the functionality of the End Turn button, including enabling/disabling
/// the button based on the game state and managing hover animations.
/// </summary>
public class EndTurnButton : MonoBehaviour
{
    [SerializeField] private Button endTurnButton; // Reference to the Button component
    [SerializeField] private Image buttonImage; // Reference to the Image component for color changes
    [SerializeField] private ButtonHoverAnimator hoverAnimator; // Reference to the hover animation script
    [SerializeField] private Animator buttonAnimator; // Reference to the Animator component

    private Color defaultColor = Color.white; // Default color of the button
    private Color disabledColor = new Color(0.5f, 0.5f, 0.5f); // Disabled color of the button

    /// <summary>
    /// Initializes the button and subscribes to TurnManager events.
    /// </summary>
    private void Start()
    {
        // Automatically find components if not assigned in the Inspector
        if (endTurnButton == null)
            endTurnButton = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (hoverAnimator == null)
            hoverAnimator = GetComponent<ButtonHoverAnimator>();

        if (buttonAnimator == null)
            buttonAnimator = GetComponent<Animator>();

        // Add listener for button click
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(EndTurn);
        else
            Debug.LogError("EndTurnButton: No Button component assigned.");

        // Subscribe to TurnManager events
        TurnManager.Instance.OnPlayerTurnStart += EnableButton;
        TurnManager.Instance.OnEnemyTurnStart += DisableButton;

        // Set initial button state based on the current turn
        if (TurnManager.Instance.IsPlayerTurn)
            EnableButton();
        else
            DisableButton();
    }

    /// <summary>
    /// Unsubscribes from TurnManager events when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= EnableButton;
            TurnManager.Instance.OnEnemyTurnStart -= DisableButton;
        }
    }

    /// <summary>
    /// Called when the End Turn button is clicked. Ends the player's turn.
    /// </summary>
    private void EndTurn()
    {
        Debug.Log("⏭️ End Turn Button Pressed");
        TurnManager.Instance?.EndPlayerTurn();
    }

    /// <summary>
    /// Enables the End Turn button and re-enables hover animations.
    /// </summary>
    private void EnableButton()
    {
        endTurnButton.interactable = true;

        if (buttonImage != null)
            buttonImage.color = defaultColor;

        if (hoverAnimator != null)
            hoverAnimator.enabled = true; // Enable hover animation

        if (buttonAnimator != null)
            buttonAnimator.enabled = true; // Enable the Animator
    }

    /// <summary>
    /// Disables the End Turn button and disables hover animations.
    /// </summary>
    private void DisableButton()
    {
        endTurnButton.interactable = false;

        if (buttonImage != null)
            buttonImage.color = disabledColor;

        if (hoverAnimator != null)
            hoverAnimator.enabled = false; // Disable hover animation

        if (buttonAnimator != null)
            buttonAnimator.enabled = false; // Disable the Animator to stop all animations
    }
}