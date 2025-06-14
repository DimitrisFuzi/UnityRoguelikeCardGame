using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Handles card hover, drag, play mechanics.
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
        [Tooltip("Is the card currently in hand.")]
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
                    HandleDragState();
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag CALLED");

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

            Debug.Log("Playing card...");

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

        private void SaveOriginalTransform()
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
        }

        private void UpdateCardPlayPosition()
        {
            if (canvasRectTransform != null && cardPlayDivider != 0)
            {
                float segment = cardPlayMultiplier / cardPlayDivider;
                cardPlay.y = canvasRectTransform.rect.height * segment;
            }
        }

        private void UpdatePlayPosition()
        {
            if (canvasRectTransform != null && playPositionXDivider != 0 && playPositionYDivider != 0)
            {
                float x = canvasRectTransform.rect.width * (playPositionXMultiplier / playPositionXDivider);
                float y = canvasRectTransform.rect.height * (playPositionYMultiplier / playPositionYDivider);
                playPosition = new Vector3(x, y, 0f);
            }
        }

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
