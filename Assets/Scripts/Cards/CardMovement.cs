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
    /// Handles hover, drag, and play interactions of cards in the player's hand.
    /// Includes smooth visual transitions using DOTween.
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
        [Tooltip("Indicates if the card is currently in the player's hand.")]
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
        /// Reference to the data object for this card.
        /// </summary>
        public Card cardData;

        void Awake()
        {

            // Ensure EventSystem exists
                   if (FindFirstObjectByType<EventSystem>() == null)
                Logger.LogError("[CardMovement] No EventSystem in scene. UI hover will not work.", this);
            
                   // Ensure top Image is raycastable
            var img = GetComponent<Image>();
                    if (img != null && !img.raycastTarget)
                        {
                img.raycastTarget = true;
                Logger.Log("[CardMovement] Enabled raycastTarget on main Image.", this);
                        }

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
            SaveOriginalTransform(); // ✅ Store transform after layout
        }

        /// <summary>
        /// destroys the card GameObject and cleans up DOTween tweens.
        /// </summary> 
        /// 
        private void OnDestroy()
        {
            // Kill any tweens on this GameObject
            DOTween.Kill(gameObject, complete: true);

            // Kill tweens on glowEffect / glowImage
            if (glowEffect != null)
            {
                var glowImage = glowEffect.GetComponent<Image>();
                if (glowImage != null)
                {
                    DOTween.Kill(glowImage);
                }
            }
        }


        void Update()
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
                        HandleDragState(); // let non-target cards follow the mouse naturally
                    }

                    if (!Input.GetMouseButton(0)) TransitionToIdle();
                    break;

            }
        }

        /// <summary>
        /// Returns the card to its original state and disables all visual effects.
        /// </summary>
        private void TransitionToIdle()
        {
            currentState = 0;

            if (glowEffect != null)
            {
                Image glowImage = glowEffect.GetComponent<Image>();
                if (glowImage != null && glowImage.gameObject != null && glowImage.gameObject.activeInHierarchy)
                {
                    DOTween.Kill(glowImage);
                    if (glowImage != null)
                        glowImage.DOFade(0f, 0.2f);
                }

                glowEffect.SetActive(false);
            }

            if (playArrow != null)
                playArrow.SetActive(false);

            DOTween.Kill(gameObject, complete: true);

            if (rectTransform != null && gameObject.activeInHierarchy)
            {
                rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalMove(originalPosition, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalRotateQuaternion(originalRotation, 0.2f).SetEase(Ease.OutQuad);
            }

            transform.SetSiblingIndex(originalSiblingIndex);
        }


        /// <summary>
        /// Triggered when pointer enters card; enters hover state.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;
            

            if (currentState == 0)
            {
                currentState = 1;

                // sound for card hover
                AudioManager.Instance?.PlaySFX("Card_Hover");

                originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();

                if (glowEffect != null)
                {
                    Image glowImage = glowEffect.GetComponent<Image>();
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


        /// <summary>
        /// Triggered when pointer exits card; returns to idle.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentState == 1)
            {
                TransitionToIdle();
            }
        }

        /// <summary>
        /// Called when clicking down on the card; enters drag mode if hovering.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;

            if (currentState == 1)
            {
                currentState = 2;

                // sound for card selection
                AudioManager.Instance?.PlaySFX("Card_Select");
            }
        }

        /// <summary>
        /// Called during dragging; handles movement or play indication.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (Blocked() || !enabled) return;


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
                    if (Input.mousePosition.y > cardPlay.y)
                    {
                        currentState = 3;
                     
                    }
                    else
                    {
                        HandleDragState();
                    }
                }

            }
        }

        /// <summary>
        /// Finalizes card play and applies effects after drag ends.
        /// </summary>
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

            // Ensure the card was dragged high enough to be considered 'played'
            if (cardData.targetType != Card.TargetType.SingleEnemy && Input.mousePosition.y < cardPlay.y)
            {
                TransitionToIdle();
                return;
            }


            // Validate target selection
            bool validTargetSelected = true;

            foreach (EffectData effect in cardData.GetCardEffects())
            {
                CharacterStats effectTarget = ResolveTargetForEffect(effect.targetType);

                // If the effect requires a target and no valid target is found, mark as invalid
                if (effect.targetType == Card.TargetType.SingleEnemy && effectTarget == null)
                {
                    Logger.LogWarning($"CardMovement: Effect {effect.GetType().Name} requires a target but none was found.");
                    validTargetSelected = false;
                    break;
                }
            }

            // If no valid target is selected, return the card to the hand
            if (!validTargetSelected)
            {
                TransitionToIdle();
                return;
            }

            // If we reach here, the card is being played successfully
            StartCoroutine(ApplyEffectsInSequence());
            return;

            // Deduct energy and remove the card from the hand
            //PlayerManager.Instance.UseCard(cardData);
            //TransitionToIdle();
           // HandManager.Instance.RemoveCardFromHand(this.gameObject);
        }

        /// <summary>
        /// Resolves the target CharacterStats based on the effect's target type.
        /// </summary>
        private CharacterStats ResolveTargetForEffect(Card.TargetType type)
        {
            switch (type)
            {
                case Card.TargetType.SingleEnemy:
                    return GetEnemyUnderCursor();

                case Card.TargetType.Self:
                    return PlayerStats.Instance;

                case Card.TargetType.AllEnemies:
                case Card.TargetType.None:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Handles scale and position lift while in hover state using DOTween.
        /// </summary>
        private void HandleHoverState()
        {

            if (rectTransform == null || this == null || !gameObject.activeInHierarchy)
                return;

            Image glowImage = glowEffect.GetComponent<Image>();
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


            transform.SetAsLastSibling();

            //rectTransform.DOKill();
            DOTween.Kill(gameObject, complete: true);


            Vector3 targetScale = originalScale * selectScale;
            Quaternion targetRotation = Quaternion.identity;

            //Calculate new position to ensure bottom edge is at 0

            float cardHeight = rectTransform.rect.height * rectTransform.lossyScale.y;

            Vector3 worldPos = rectTransform.position;

            // we want the bottom edge to be at 0, so we need to adjust the Y position
            float newY = cardHeight / 2f; // since the pivot is at the center, we need to move it up by half the height

            Vector3 targetWorldPos = new Vector3(worldPos.x, newY, worldPos.z);

            // convert to local position relative to parent
            Vector3 targetLocalPos = rectTransform.parent.InverseTransformPoint(targetWorldPos);

                rectTransform.DOScale(targetScale, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalRotateQuaternion(targetRotation, 0.2f).SetEase(Ease.OutQuad);

            
        }


        /// <summary>
        /// Handles card movement while dragging.
        /// </summary>
        private void HandleDragState()
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.position = Vector3.Lerp(rectTransform.position, Input.mousePosition, lerpFactor);
        }

        /// <summary>
        /// Handles positioning and targeting during play state.
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
        /// Stores the original transform properties for restoring after hover.
        /// </summary>
        public void SaveOriginalTransform()
        {
            originalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
        }


        /// <summary>
        /// Updates Y threshold for detecting play zone on drag.
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
        /// Updates absolute canvas-space play position.
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
        /// Finds the enemy GameObject currently under the cursor.
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
        /// Resolves the target CharacterStats based on card target type.
        /// </summary>
        private CharacterStats ResolveTarget(Enemy enemy)
        {
            switch (cardData.targetType)
            {
                case Card.TargetType.SingleEnemy:
                    return enemy;
                case Card.TargetType.AllEnemies:
                    return null;
                case Card.TargetType.Self:
                    return PlayerStats.Instance;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Returns a color based on the card type for visual feedback.    
        /// </summary>
        private Color GetColorByCardType()
        {
            switch (cardData.cardType)
            {
                case Card.CardType.Attack: return new Color32(180, 28, 34, 255); // red
                case Card.CardType.Guard: return new Color32(0, 128, 24, 255);  // green
                case Card.CardType.Tactic: return new Color32(0, 37, 194, 255); // blue
                default: return Color.white;
            }
        }

        private IEnumerator ApplyEffectsInSequence()
        {

            // 1) Πλήρωσε energy πριν κάνεις οτιδήποτε
            PlayerManager.Instance.UseCard(cardData);
            Debug.Log($"[CardMovement] Played {cardData.cardName}, cost={cardData.energyCost}, remaining energy={PlayerStats.Instance.energy}");

            // 2) Βγάλε ΑΜΕΣΑ την κάρτα από το hand (χωρίς destroy για να συνεχίσουν τα effects)
            HandManager.Instance.RemoveCardFromHand(this.gameObject, destroyGO: false);

            // 3) Κρύψε εντελώς την κάρτα τώρα (χωρίς να σταματήσει η coroutine)
            isInHand = false;
            currentState = 0;

            // Σβήσε οπτικά βοηθήματα
            if (playArrow != null) playArrow.SetActive(false);
            if (glowEffect != null)
            {
                var glowImg = glowEffect.GetComponent<Image>();
                if (glowImg != null) DOTween.Kill(glowImg, complete: true);
                glowEffect.SetActive(false);
            }

            // Σκότωσε ό,τι tween τρέχει πάνω στο GO
            DOTween.Kill(gameObject, complete: true);

            // Μην ξαναδέχεται raycasts
            var selfImg = GetComponent<Image>();
            if (selfImg != null) selfImg.raycastTarget = false;

            // CanvasGroup για αόρατο/μη-κλικάμπλ
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Προληπτικά μηδένισε scale ώστε να μην “αναβοσβήσει” ποτέ
            var rt = GetComponent<RectTransform>();
            if (rt != null) rt.localScale = Vector3.zero;

            // (προαιρετικό) Απενεργοποίησε όλα τα child Graphics για σιγουριά
            var gfx = GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            for (int i = 0; i < gfx.Length; i++) gfx[i].enabled = false;

            // 4) Τρέξε τα effects ΣΕΙΡΙΑΚΑ (όχι StartCoroutine γύρω από το yield)
            var effects = cardData.GetCardEffects();
            foreach (EffectData effect in effects)
            {
                CharacterStats target = ResolveTargetForEffect(effect.targetType);

                if (effect is ICoroutineEffect coroutineEffect)
                {
                    yield return coroutineEffect.ApplyEffectRoutine(PlayerStats.Instance, target);
                }
                else
                {
                    effect.ApplyEffect(PlayerStats.Instance, target);
                }
            }

            // 5) Τώρα μπορεί να φύγει οριστικά το GameObject
            Destroy(this.gameObject);
        }

        private bool Blocked()
        {
            if (BattleManager.Instance != null && BattleManager.Instance.IsPlayerInputLocked) return true;
            if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn) return true;
            if (HandManager.Instance != null && HandManager.Instance.IsDrawing) return true; // << νέο
            if (!isInHand) return true;
            return false;
        }



    }
}

