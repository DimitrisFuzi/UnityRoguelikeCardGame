using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Handles user interactions with the card, including hover, drag, and play mechanics.
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

        [Header("Card Visual Feedback")]
        [SerializeField] private float selectScale = 1.1f; // Scale factor when hovered
        [SerializeField] private GameObject glowEffect;    // Glow effect on hover
        [SerializeField] private GameObject playArrow;     // Arrow shown when card is in play state

        [Header("Card Play Mechanics")]
        [SerializeField] private Vector2 cardPlay;         // Position threshold to enter play state
        [SerializeField] private Vector3 playPosition;     // Target position during play
        [SerializeField] private float lerpFactor = 0.1f;  // Lerp smoothing factor

        [Header("Position Calculations")]
        [SerializeField] private int cardPlayDivider = 4;
        [SerializeField] private float cardPlayMultiplier = 1f;
        [SerializeField] private bool needUpdateCardPlayPosition = false;

        [SerializeField] private int playPositionYDivider = 2;
        [SerializeField] private float playPositionYMultiplier = 1f;
        [SerializeField] private int playPositionXDivider = 4;
        [SerializeField] private float playPositionXMultiplier = 1f;
        [SerializeField] private bool needUpdatePlayPosition = false;

        public Card cardData;

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

        void Start()
        {
            if (cardData == null)
            {
                cardData = GetComponent<CardDisplay>()?.cardData;
            }
        }

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
        /// Resets card to original position and appearance.
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentState == 0)
            {
                SaveOriginalTransform();
                currentState = 1;

                originalSiblingIndex = transform.GetSiblingIndex();
                // bring to front
                transform.SetAsLastSibling();
            }
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentState == 1)
            {
                TransitionToIdle();
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
                    // Self: no arc, just drag
                    HandleDragState();
                }
            }
        }




        public void OnEndDrag(PointerEventData eventData)
        {
            canvasRectTransform?.SetAsLastSibling();

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

            // ✅ Apply each effect
            foreach (EffectData effect in cardData.GetCardEffects())
            {
                effect.ApplyEffect(PlayerStats.Instance, target);
            }

            PlayerManager.Instance.UseCard(cardData);
            HandManager.Instance.RemoveCardFromHand(cardData);

            TransitionToIdle();
        }




        /// <summary>
        /// Visual feedback when hovering.
        /// </summary>
        private void HandleHoverState()
        {
            glowEffect.SetActive(true);
            rectTransform.localScale = originalScale * selectScale;
        }

        /// <summary>
        /// Updates card position while dragging.
        /// </summary>
        private void HandleDragState()
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);
        }

        /// <summary>
        /// Positions card in play state.
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
        /// Stores current transform as the original.
        /// </summary>
        private void SaveOriginalTransform()
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
        }

        /// <summary>
        /// Calculates Y threshold for entering play state.
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
        /// Calculates target position for card during play state.
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
        /// Uses raycasting to find enemy under the cursor.
        /// </summary>
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
        /// Resolves the correct target for the card based on its target type.
        /// </summary>
        private CharacterStats ResolveTarget(Enemy enemy)
        {
            switch (cardData.targetType)
            {
                case Card.TargetType.SingleEnemy:
                    return enemy;
                case Card.TargetType.AllEnemies:
                    // Return null → τα effects πρέπει να το χειριστούν, πχ AoE loop
                    return null;
                case Card.TargetType.Self:
                    return PlayerStats.Instance;
                case Card.TargetType.AllAllies:
                    // Future use
                    return null;
                default:
                    return null;
            }
        }


    }
}
