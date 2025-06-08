
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;


public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform canvasRectTranform;
    private Vector3 originalScale;
    private int currentState = 0;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;
    [SerializeField] private Vector3 playPosition;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private GameObject playArrow;
    [SerializeField] private float lerpFactor = 0.1f;
    [SerializeField] private int cardPlayDivider = 4;
    [SerializeField] private float cardPlayMultiplier = 1f;
    [SerializeField] private bool needUpdateCardPlayPosition = false;
    [SerializeField] private int playPositionYDivider = 2;
    [SerializeField] private float playPositionYMultiplier = 1f;
    [SerializeField] private int playPositionXDivider = 4;
    [SerializeField] private float playPositionXMultiplier = 1f;
    [SerializeField] private bool needUpdatePlayPosition = false;

    public Card cardData;

    void Start()
    {
        if (cardData == null)
        {
            cardData = GetComponent<CardDisplay>().cardData;
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRectTranform = canvas.GetComponent<RectTransform>();
        }

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.localPosition;
        originalRotation = rectTransform.localRotation;

        updateCardPlayPostion();
        updatePlayPostion();
    }

    void Update()
    {
        if (needUpdateCardPlayPosition)
        {
            updateCardPlayPostion();
        }

        if (needUpdatePlayPosition)
        {
            updatePlayPostion();
        }

        switch (currentState)
        {
            case 1:
                HandleHoverState();
                break;
            case 2:
                HandleDragState();
                if (!Input.GetMouseButton(0)) // Check if mouse button is released
                {
                    TransitionToState0();
                }
                break;
            case 3:
                HandlePlayState();
                if (!Input.GetMouseButton(0)) // Check if mouse button is released
                {
                    TransitionToState0();
                }
                break;
        }
    }

    private void TransitionToState0()
    {
        currentState = 0;
        rectTransform.localScale = originalScale; // Reset Scale
        rectTransform.localRotation = originalRotation; // Reset Rotation
        rectTransform.localPosition = originalPosition; // Reset Position
        glowEffect.SetActive(false); // Disable glow effect
        playArrow.SetActive(false); // Disable playArrow
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;

            currentState = 1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            TransitionToState0();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            currentState = 2;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState == 2)
        {
            if (Input.mousePosition.y > cardPlay.y)
            {
                currentState = 3;
                playArrow.SetActive(true);
                rectTransform.localPosition = Vector3.Lerp(rectTransform.position, playPosition, lerpFactor);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasRectTranform = canvas.GetComponent<RectTransform>();
        canvasRectTranform.SetAsLastSibling();

        Enemy targetEnemy = GetEnemyUnderCursor();

        if (cardData == null)
        {
            Debug.LogError("❌ cardData is NULL!");
            return;
        }

        if (!PlayerManager.Instance.CanPlayCard(cardData))
        {
            Debug.Log("❌ Δεν έχεις αρκετό Energy!");
            return;
        }

        // ✅ Έλεγχος αν η κάρτα χρειάζεται στόχο
        CharacterStats target = null;

        switch (cardData.targetType)
        {
            case Card.TargetType.SingleEnemy:
                if (targetEnemy == null)
                {
                    Debug.Log("❌ Η κάρτα απαιτεί στόχο αλλά δεν βρέθηκε!");
                    return;
                }
                target = targetEnemy;
                break;

            case Card.TargetType.AllEnemies:
            case Card.TargetType.Self:
            case Card.TargetType.AllAllies:
                target = PlayerStats.Instance; // Δεν απαιτεί εχθρό
                break;
        }

        foreach (EffectData effect in cardData.GetCardEffects())
        {
            effect.ApplyEffect(PlayerStats.Instance, target);
        }

        PlayerManager.Instance.UseCard(cardData);
        HandManager.Instance.RemoveCardFromHand(cardData);
    }


    private void HandleHoverState()
    {
        glowEffect.SetActive(true);
        rectTransform.localScale = originalScale * selectScale;
    }

    private void HandleDragState()
    {
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);
    }

    private void HandlePlayState()
    {
        rectTransform.localPosition = playPosition;
        rectTransform.localRotation = Quaternion.identity;

        if (Input.mousePosition.y < cardPlay.y)
        {
            currentState = 2;
            playArrow.SetActive(false);
        }
    }

    private void updateCardPlayPostion()
    {
        if (cardPlayDivider != 0 && canvasRectTranform != null)
        {
            float segment = cardPlayMultiplier / cardPlayDivider;
            cardPlay.y = canvasRectTranform.rect.height * segment;
        }
    }

    private void updatePlayPostion()
    {
        if (canvasRectTranform != null && playPositionYDivider != 0 && playPositionXDivider != 0)
        {
            float segmentX = playPositionXMultiplier / playPositionXDivider;
            float segmentY = playPositionYMultiplier / playPositionYDivider;

            playPosition.x = canvasRectTranform.rect.width * segmentX;
            playPosition.y = canvasRectTranform.rect.height * segmentY;
        }
    }

    private Enemy GetEnemyUnderCursor()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Debug.Log($"🔍 Raycast hit: {result.gameObject.name}");

            // Πάμε στο Root GameObject που περιέχει το Enemy
            Enemy enemy = result.gameObject.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"✅ Enemy found: {enemy.enemyName}");
                return enemy;
            }
        }
        return null;
    }
}

