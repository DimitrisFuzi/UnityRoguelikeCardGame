using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the functionality of the End Turn button, including enabling/disabling
/// the button based on the game state and managing hover animations.
/// </summary>
public class EndTurnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Animator buttonAnimator;

    private Color defaultColor = Color.white;
    private Color disabledColor = new Color(0.5f, 0.5f, 0.5f);

    private bool isInteractable = true;

    private void Start()
    {
        if (endTurnButton == null)
            endTurnButton = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        if (buttonAnimator == null)
            buttonAnimator = GetComponent<Animator>();

        endTurnButton.onClick.AddListener(EndTurn);

        TurnManager.Instance.OnPlayerTurnStart += EnableButton;
        TurnManager.Instance.OnEnemyTurnStart += DisableButton;

        if (TurnManager.Instance.IsPlayerTurn)
            EnableButton();
        else
            DisableButton();
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= EnableButton;
            TurnManager.Instance.OnEnemyTurnStart -= DisableButton;
        }
    }

    private void EndTurn()
    {
        Debug.Log("⏭️ End Turn Button Pressed");
        TurnManager.Instance?.EndPlayerTurn();
    }

    private void EnableButton()
    {
        isInteractable = true;
        endTurnButton.interactable = true;

        if (buttonImage != null)
            buttonImage.color = defaultColor;

        if (buttonAnimator != null)
            buttonAnimator.SetBool("IsHovering", false);
    }

    private void DisableButton()
    {
        isInteractable = false;
        endTurnButton.interactable = false;

        if (buttonImage != null)
            buttonImage.color = disabledColor;

        if (buttonAnimator != null)
            buttonAnimator.SetBool("IsHovering", false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInteractable && buttonAnimator != null)
            buttonAnimator.SetBool("IsHovering", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractable && buttonAnimator != null)
            buttonAnimator.SetBool("IsHovering", false);
    }
}
