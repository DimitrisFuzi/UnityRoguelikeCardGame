using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Controls the End Turn button state and simple hover animation.
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
        if (endTurnButton == null) endTurnButton = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonAnimator == null) buttonAnimator = GetComponent<Animator>();

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(EndTurn);

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart += EnableButton;
            TurnManager.Instance.OnEnemyTurnStart += DisableButton;

            if (TurnManager.Instance.IsPlayerTurn) EnableButton();
            else DisableButton();
        }
        else
        {
            DisableButton();
        }
    }

    private void Update()
    {
        // live-guard against state changes mid-frame
        bool allow =
            TurnManager.Instance != null &&
            TurnManager.Instance.IsPlayerTurn &&
            !(BattleManager.Instance?.IsPlayerInputLocked ?? true) &&
            !(TurnManager.Instance?.IsEndingTurn ?? false);

        if (allow && !isInteractable) EnableButton();
        else if (!allow && isInteractable) DisableButton();
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
        if (!isInteractable) return;     // debounce
        DisableButton();                 // disable immediately on click
        AudioManager.Instance?.PlaySFX("End_Turn");
        TurnManager.Instance?.EndPlayerTurn();
    }

    private void EnableButton()
    {
        isInteractable = true;
        if (endTurnButton) endTurnButton.interactable = true;
        if (buttonImage) buttonImage.color = defaultColor;
        if (buttonAnimator) buttonAnimator.SetBool("IsHovering", false);
    }

    private void DisableButton()
    {
        isInteractable = false;
        if (endTurnButton) endTurnButton.interactable = false;
        if (buttonImage) buttonImage.color = disabledColor;
        if (buttonAnimator) buttonAnimator.SetBool("IsHovering", false);
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (isInteractable && buttonAnimator) buttonAnimator.SetBool("IsHovering", true);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (isInteractable && buttonAnimator) buttonAnimator.SetBool("IsHovering", false);
    }
}
