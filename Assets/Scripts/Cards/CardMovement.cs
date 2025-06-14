using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Handles card hover, drag, and play interactions in the player's hand.
    /// Manages visual feedback and interaction states.
    /// </summary>
    public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
    {
        private RectTransform rectTransform;
        private Canvas canvas;
        private RectTransform canvasRectTransform;
        private Vector3 originalScale;
        private Quaternion originalRotation;
        private Vector3 originalPosition;
        private int originalSiblingIndex;

        private int currentState = 0; // 0: Idle, 1: Hover, 2: Drag, 3: Play

        [Header("Hand State")]
        [Tooltip("Is the card currently in the player's hand.")]
        public bool isInHand = true;

        [Header("Card Visual Feedback")]
        [SerializeField] private float selectScale = 1.1f;
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private GameObject playArrow;

        [Header("Card Play Mechanics")]
        [SerializeField] private Vector2 cardPlay;
        [SerializeField] private Vector3 playPosition;
        [SerializeField] private float lerpFactor = 0.1f;

        [Header("Position Calculations")]
        [SerializeField] private int cardPlayDivider = 4;
        [SerializeField] private float cardPlayMultiplier = 1f;
        [SerializeField] private bool needUpdateCardPlayPosition = false;

        [SerializeField] private int playPositionYDivider = 2;
        [SerializeField] private float playPositionYMultiplier = 1f;
        [SerializeField] private int playPositionXDivider = 4;
        [SerializeField] private float playPositionXMultiplier = 1f;
        [SerializeField] private bool needUpdatePlayPosition = false;

        /// <summary>
        /// The data representation of this card.
        /// </summary>
        public Card cardData;

        /// <summary>
        /// Initializes card transform references and calculates initial play positions.
        /// </summary>
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasRectTransform = canvas?.GetComponent<RectTransform>();

            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;

            UpdateCardPlayPosition();
            UpdatePlayPosition();
        }

        /// <summary>
        /// Ensures the card data is set from the CardDisplay component.
        /// </summary>
        void Start()
        {
            if (cardData == null)
            {
                cardData = GetComponent<CardDisplay>()?.cardData;
            }
        }

        /// <summary>
        /// Updates play position calculations and handles the current interaction state.
        /// </summary>
        void Update()
        {
            if (needUpdateCardPlayPosition) UpdateCardPlayPosition();
            if (needUpdatePlayPosition) UpdatePlayPosition();

            switch (currentState)
            {
                case 1: HandleHoverState(); break;
                case 2: HandleDragState(); if (!Input.GetMouseButton(0)) TransitionToIdle(); break;
                case 3: HandlePlayState(); if (!Input.GetMouseButton(0)) TransitionToIdle(); break;
            }
        }

        /// <summary>
        /// Resets the card to its original transform and disables visual effects.
        /// </summary>
        private void TransitionToIdle()
        {
            currentState = 0;
            rectTransform.localScale = originalScale;
            rectTransform.localRotation = originalRotation;
            rectTransform.localPosition = originalPosition;
            glowEffect.SetActive(false);
            playArrow.SetActive(false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        /// <summary>
        /// Called when the pointer enters the card area; triggers hover state.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInHand || !enabled)
            {
                Debug.Log($"{gameObject.name} | Enter IGNORED because not in hand or disabled");
                return;
            }

            if (currentState == 0)
            {
                SaveOriginalTransform();
                currentState = 1;
                originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Called when the pointer exits the card area; cancels hover state.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentState == 1)
            {
                TransitionToIdle();
            }
        }

        /// <summary>
        /// Called when the pointer clicks the card; initiates drag state.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentState == 1)
            {
                currentState = 2;
            }
        }

        /// <summary>
        /// Called while dragging the card; handles drag or play positioning.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (currentState == 2)
            {
                if (cardData.targetType == Card.TargetType.SingleEnemy)
                {
                    if (Input.mousePosition.y > cardPlay.y)
                    {
                        currentState = 3;
                        playArrow.SetActive(true);
                        rectTransform.localPosition = Vector3.Lerp(rectTransform.position, playPosition, lerpFactor);
                    }
                    else
                    {
                        HandleDragState();
                    }
                }
                else
                {
                    HandleDragState();
                }
            }
        }

        /// <summary>
        /// Called when dragging ends; checks play conditions and applies card effects.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnEndDrag(PointerEventData eventData)
        {

            if (cardData == null)
            {
                Logger.LogError("CardMovement: Card data is NULL.", this);
                TransitionToIdle();
                return;
            }

            if (!PlayerManager.Instance.CanPlayCard(cardData))
            {
                Logger.Log("CardMovement: Not enough energy to play this card.", this);
                TransitionToIdle();
                return;
            }

            CharacterStats target = null;

            if (cardData.targetType == Card.TargetType.SingleEnemy)
            {
                Enemy targetEnemy = GetEnemyUnderCursor();
                target = ResolveTarget(targetEnemy);

                if (target == null)
                {
                    Logger.LogWarning("CardMovement: Card requires a target but none found.", this);
                    TransitionToIdle();
                    return;
                }
            }
            else if (cardData.targetType == Card.TargetType.Self)
            {
                target = PlayerStats.Instance;
            }

            foreach (EffectData effect in cardData.GetCardEffects())
            {
                effect.ApplyEffect(PlayerStats.Instance, target);
            }

            PlayerManager.Instance.UseCard(cardData);
            HandManager.Instance.RemoveCardFromHand(this.gameObject);

            TransitionToIdle();
        }

        /// <summary>
        /// Handles scaling and glow when hovering.
        /// </summary>
        private void HandleHoverState()
        {
            glowEffect.SetActive(true);
            rectTransform.localScale = originalScale * selectScale;
        }

        /// <summary>
        /// Handles card position update while dragging.
        /// </summary>
        private void HandleDragState()
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);
        }

        /// <summary>
        /// Handles positioning when card is in play state for targeting.
        /// </summary>
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

        /// <summary>
        /// Saves the original transform properties for restoring later.
        /// </summary>
        private void SaveOriginalTransform()
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
        }

        /// <summary>
        /// Updates the Y position threshold for detecting when to play a card.
        /// </summary>
        private void UpdateCardPlayPosition()
        {
            if (canvasRectTransform != null && cardPlayDivider != 0)
            {
                float segment = cardPlayMultiplier / cardPlayDivider;
                cardPlay.y = canvasRectTransform.rect.height * segment;
            }
        }

        /// <summary>
        /// Updates the target play position on the canvas.
        /// </summary>
        private void UpdatePlayPosition()
        {
            if (canvasRectTransform != null && playPositionXDivider != 0 && playPositionYDivider != 0)
            {
                float x = canvasRectTransform.rect.width * (playPositionXMultiplier / playPositionXDivider);
                float y = canvasRectTransform.rect.height * (playPositionYMultiplier / playPositionYDivider);
                playPosition = new Vector3(x, y, 0f);
            }
        }

        /// <summary>
        /// Returns the enemy GameObject currently under the cursor, if any.
        /// </summary>
        /// <returns>The enemy under the cursor, or null if none.</returns>
        private Enemy GetEnemyUnderCursor()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                Enemy enemy = result.gameObject.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the final target for the card based on its target type and selection.
        /// </summary>
        /// <param name="enemy">The selected enemy.</param>
        /// <returns>The resolved target stats.</returns>
        private CharacterStats ResolveTarget(Enemy enemy)
        {
            switch (cardData.targetType)
            {
                case Card.TargetType.SingleEnemy:
                    return enemy;
                case Card.TargetType.AllEnemies:
                    return null; // AoE handled in effect loop
                case Card.TargetType.Self:
                    return PlayerStats.Instance;
                default:
                    return null;
            }
        }
    }
}
