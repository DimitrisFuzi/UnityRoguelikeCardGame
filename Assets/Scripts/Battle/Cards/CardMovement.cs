using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Managers;

namespace MyProjectF.Assets.Scripts.Cards
{
    /// <summary>
    /// Handles hover, drag and play interactions for cards in the player's hand, with DOTween visuals.
    /// No CanvasGroup dependency required.
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

        // 0: Idle, 1: Hover, 2: Drag, 3: Play
        private int currentState = 0;

        [Header("Hand State")]
        [Tooltip("True if the card is currently in the player's hand.")]
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

        [Header("Drag Threshold Tuning")]
        [Tooltip("Negative values lower the threshold, positive values raise it.")]
        [SerializeField] private float playThresholdOffsetPx = -90f;
        [Tooltip("Hysteresis for stability when leaving the play zone.")]
        [SerializeField] private float hysteresisPx = 24f;

        private float EnterPlayY => cardPlay.y + playThresholdOffsetPx;
        private float ExitPlayY => EnterPlayY - hysteresisPx;

        [Header("Preview Visuals (no hard dependency)")]
        [SerializeField] private float targetPreviewScale = 0.92f;
        [SerializeField] private float targetPreviewAlpha = 0.80f;

        /// <summary>Card data reference for this instance.</summary>
        public Card cardData;

        private void Awake()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
                Logger.LogError("[CardMovement] No EventSystem in scene. UI hover will not work.", this);

            var img = GetComponent<Image>();
            if (img != null && !img.raycastTarget)
            {
                img.raycastTarget = true;
                Logger.Log("[CardMovement] Enabled raycastTarget on main Image.", this);
            }

            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasRectTransform = canvas ? canvas.GetComponent<RectTransform>() : null;

            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;

            UpdateCardPlayPosition();
            UpdatePlayPosition();
        }

        private void Start()
        {
            if (cardData == null)
                cardData = GetComponent<CardDisplay>()?.cardData;

            SaveOriginalTransform();
        }

        private void OnDestroy()
        {
            DOTween.Kill(gameObject, complete: true);

            if (glowEffect != null)
            {
                var glowImage = glowEffect.GetComponent<Image>();
                if (glowImage != null)
                    DOTween.Kill(glowImage, complete: true);
            }
        }

        private void Update()
        {
            if (needUpdateCardPlayPosition) UpdateCardPlayPosition();
            if (needUpdatePlayPosition) UpdatePlayPosition();

            switch (currentState)
            {
                case 2:
                    HandleDragState();
                    if (!Input.GetMouseButton(0)) TransitionToIdle();
                    break;

                case 3:
                    if (cardData.targetType == Card.TargetType.SingleEnemy)
                    {
                        HandlePlayState();
                    }
                    else
                    {
                        if (Input.mousePosition.y < ExitPlayY)
                        {
                            currentState = 2;
                            StopPlayablePulse(1f);
                        }
                        else
                        {
                            HandleDragState();
                        }
                    }

                    if (!Input.GetMouseButton(0)) TransitionToIdle();
                    break;
            }
        }

        // ---------- Helpers (no hard dependency on CanvasGroup) ----------
        private void MaybeSetAlpha(float a)
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = a;
        }

        // ----------------------------------------------------------------

        private void TransitionToIdle()
        {
            currentState = 0;
            StopPlayablePulse(1f);

            if (glowEffect != null)
            {
                var glowImage = glowEffect.GetComponent<Image>();
                if (glowImage != null && glowImage.gameObject.activeInHierarchy)
                {
                    DOTween.Kill(glowImage);
                    glowImage.DOFade(0f, 0.2f);
                }
                glowEffect.SetActive(false);
            }

            if (playArrow != null) playArrow.SetActive(false);

            DOTween.Kill(gameObject, complete: true);

            if (rectTransform != null && gameObject.activeInHierarchy)
            {
                rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalMove(originalPosition, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalRotateQuaternion(originalRotation, 0.2f).SetEase(Ease.OutQuad);
            }

            transform.SetSiblingIndex(originalSiblingIndex);
            MaybeSetAlpha(1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;

            if (currentState == 0)
            {
                currentState = 1;

                AudioManager.Instance?.PlaySFX("Card_Hover");

                originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();

                if (glowEffect != null)
                {
                    var glowImage = glowEffect.GetComponent<Image>();
                    if (glowImage != null)
                    {
                        glowImage.color = GetColorByCardType();
                        glowImage.DOFade(0.2f, 0.7f)
                                 .SetLoops(-1, LoopType.Yoyo)
                                 .SetEase(Ease.InOutSine)
                                 .SetUpdate(true)
                                 .SetId(glowImage);
                    }
                    glowEffect.SetActive(true);
                }

                HandleHoverState();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentState == 1)
                TransitionToIdle();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;

            if (currentState == 1)
            {
                currentState = 2;
                AudioManager.Instance?.PlaySFX("Card_Select");
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;

            if (currentState == 2)
            {
                if (cardData.targetType == Card.TargetType.SingleEnemy)
                {
                    if (Input.mousePosition.y > EnterPlayY)
                    {
                        currentState = 3;
                        if (playArrow != null) playArrow.SetActive(true);

                        rectTransform.DOKill();
                        rectTransform.DOLocalMove(playPosition, 0.12f).SetEase(Ease.OutQuad);
                        rectTransform.DOScale(originalScale * targetPreviewScale, 0.12f).SetEase(Ease.OutQuad);

                        MaybeSetAlpha(targetPreviewAlpha);
                        StartPlayablePulse(targetPreviewScale);
                    }
                    else
                    {
                        HandleDragState();
                    }
                }
                else
                {
                    if (Input.mousePosition.y > EnterPlayY)
                    {
                        currentState = 3;
                        StartPlayablePulse(1f);
                    }
                    else
                    {
                        HandleDragState();
                    }
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Blocked()) return;

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

            if (cardData.targetType != Card.TargetType.SingleEnemy && Input.mousePosition.y < EnterPlayY)
            {
                TransitionToIdle();
                return;
            }

            bool validTargetSelected = true;
            foreach (var effect in cardData.GetCardEffects())
            {
                var effectTarget = ResolveTargetForEffect(effect.targetType);
                if (effect.targetType == Card.TargetType.SingleEnemy && effectTarget == null)
                {
                    Logger.LogWarning($"CardMovement: Effect {effect.GetType().Name} requires a target but none was found.", this);
                    validTargetSelected = false;
                    break;
                }
            }

            if (!validTargetSelected)
            {
                TransitionToIdle();
                return;
            }

            StartCoroutine(ApplyEffectsInSequence());
        }

        private CharacterStats ResolveTargetForEffect(Card.TargetType type)
        {
            return type switch
            {
                Card.TargetType.SingleEnemy => GetEnemyUnderCursor(),
                Card.TargetType.Self => PlayerStats.Instance,
                _ => null
            };
        }

        private void HandleHoverState()
        {
            if (rectTransform == null || !gameObject.activeInHierarchy) return;

            DOTween.Kill(gameObject, complete: true);

            transform.SetAsLastSibling();

            var targetScale = originalScale * selectScale;
            var targetRotation = Quaternion.identity;

            float cardHeight = rectTransform.rect.height * rectTransform.lossyScale.y;
            Vector3 worldPos = rectTransform.position;
            float newY = cardHeight / 2f;
            Vector3 targetWorldPos = new Vector3(worldPos.x, newY, worldPos.z);
            Vector3 targetLocalPos = rectTransform.parent.InverseTransformPoint(targetWorldPos);

            rectTransform.DOScale(targetScale, 0.2f).SetEase(Ease.OutQuad);
            rectTransform.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutQuad);
            rectTransform.DOLocalRotateQuaternion(targetRotation, 0.2f).SetEase(Ease.OutQuad);
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

            if (Input.mousePosition.y < ExitPlayY)
            {
                currentState = 2;
                if (playArrow != null) playArrow.SetActive(false);
                rectTransform.DOScale(originalScale, 0.12f);
                MaybeSetAlpha(1f);
                StopPlayablePulse(1f);
            }
        }

        public void SaveOriginalTransform()
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
            var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                var enemy = result.gameObject.GetComponentInParent<Enemy>();
                if (enemy != null) return enemy;
            }
            return null;
        }

        private CharacterStats ResolveTarget(Enemy enemy)
        {
            return cardData.targetType switch
            {
                Card.TargetType.SingleEnemy => enemy,
                Card.TargetType.Self => PlayerStats.Instance,
                _ => null
            };
        }

        private Color GetColorByCardType()
        {
            return cardData.cardType switch
            {
                Card.CardType.Attack => new Color32(180, 28, 34, 255),
                Card.CardType.Guard => new Color32(0, 128, 24, 255),
                Card.CardType.Tactic => new Color32(0, 37, 194, 255),
                _ => Color.white
            };
        }

        private IEnumerator ApplyEffectsInSequence()
        {
            PlayerManager.Instance.UseCard(cardData);
            Logger.Log($"[CardMovement] Played {cardData.cardName}, cost={cardData.energyCost}, remaining energy={PlayerStats.Instance.energy}", this);

            HandManager.Instance.RemoveCardFromHand(gameObject, destroyGO: false);

            isInHand = false;
            currentState = 0;

            if (playArrow != null) playArrow.SetActive(false);
            if (glowEffect != null)
            {
                var glowImg = glowEffect.GetComponent<Image>();
                if (glowImg != null) DOTween.Kill(glowImg, complete: true);
                glowEffect.SetActive(false);
            }

            DOTween.Kill(gameObject, complete: true);

            var selfImg = GetComponent<Image>();
            if (selfImg != null) selfImg.raycastTarget = false;

            // Respect existing CanvasGroup only if present; do not add one
            var cg = GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false; }

            var rt = GetComponent<RectTransform>();
            if (rt != null) rt.localScale = Vector3.zero;

            var gfx = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < gfx.Length; i++) gfx[i].enabled = false;

            var effects = cardData.GetCardEffects();
            foreach (var effect in effects)
            {
                CharacterStats target = ResolveTargetForEffect(effect.targetType);

                if (effect is ICoroutineEffect coroutineEffect)
                    yield return coroutineEffect.ApplyEffectRoutine(PlayerStats.Instance, target);
                else
                    effect.ApplyEffect(PlayerStats.Instance, target);
            }

            Destroy(gameObject);
        }

        private bool Blocked()
        {
            if (BattleManager.Instance != null && BattleManager.Instance.IsPlayerInputLocked) return true;
            if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return true;
            if (HandManager.Instance != null && HandManager.Instance.IsDrawing) return true;
            if (!isInHand) return true;
            return false;
        }

        private void StartPlayablePulse(float baseScaleFactor)
        {
            DOTween.Kill(rectTransform);
            var pulseBaseScale = originalScale * baseScaleFactor;
            rectTransform.localScale = pulseBaseScale;
            rectTransform.DOScale(pulseBaseScale * 1.05f, 0.25f)
                         .SetLoops(-1, LoopType.Yoyo)
                         .SetEase(Ease.InOutSine)
                         .SetUpdate(true);
        }

        private void StopPlayablePulse(float? returnToFactor = null)
        {
            DOTween.Kill(rectTransform);
            var target = returnToFactor.HasValue ? originalScale * returnToFactor.Value : originalScale;
            rectTransform.DOScale(target, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }
}
