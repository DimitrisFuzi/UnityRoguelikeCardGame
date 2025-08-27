// EnemyDisplay.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Image enemyImage; // Enemy sprite image
    [SerializeField] private HealthBar healthBar; // NEW: Reference to the HealthBar script
    [SerializeField] private IntentDisplay intentDisplay; // Reference to the IntentDisplay component
    [SerializeField] private Transform textSpawnAnchor;
    [SerializeField] private GameObject floatingDamageTextPrefab;



    private RectTransform enemyRect;

    /// <summary>
    /// Sets up the enemy display with provided enemy data and initializes UI.
    /// </summary>
    /// <param name="enemy">Reference to the Enemy instance.</param>
    /// <param name="enemyData">Data container with enemy sprite and display parameters.</param>
    public void Setup(Enemy enemy, EnemyData enemyData)
    {
        if (enemyImage == null)
        {
            Logger.LogError("[EnemyDisplay] enemyImage is not assigned!", this);
            return;
        }

        enemyImage.sprite = enemyData.enemySprite;
        enemyRect = GetComponent<RectTransform>();

        if (enemyRect != null)
        {
            enemyRect.anchorMin = new Vector2(1, 0.5f);
            enemyRect.anchorMax = new Vector2(1, 0.5f);
            enemyRect.pivot = new Vector2(1, 0.5f);
            enemyRect.anchoredPosition = enemyData.position;
            enemyRect.sizeDelta = enemyData.size;
        }
        else
        {
            Logger.LogWarning("[EnemyDisplay] RectTransform component missing!", this);
        }

        var shadowTr = transform.Find("Shadow");
        if (shadowTr)
        {
            var fitter = shadowTr.GetComponent<EnemyShadowFitter>();
            if (!fitter) fitter = shadowTr.gameObject.AddComponent<EnemyShadowFitter>();

            // Target = το οπτικό μέγεθος του εχθρού
            fitter.target = enemyImage.rectTransform;

            // Apply από τα EnemyData
            switch (enemyData.shadowMode)  // από EnemyData.cs  :contentReference[oaicite:1]{index=1}
            {
                case ShadowMode.None:
                    shadowTr.gameObject.SetActive(false);
                    break;

                case ShadowMode.Manual:
                    shadowTr.gameObject.SetActive(true);
                    fitter.mode = EnemyShadowFitter.Mode.Manual;
                    fitter.ApplyFromData(ShadowMode.Manual,
                                         enemyData.shadowWidthMultiplier,
                                         enemyData.shadowHeightToWidth,
                                         enemyData.shadowOffset,
                                         enemyData.manualShadowSize);
                    break;

                default: // Auto
                    shadowTr.gameObject.SetActive(true);
                    fitter.mode = EnemyShadowFitter.Mode.Auto;
                    fitter.ApplyFromData(ShadowMode.Auto,
                                         enemyData.shadowWidthMultiplier,
                                         enemyData.shadowHeightToWidth,
                                         enemyData.shadowOffset,
                                         Vector2.zero);
                    break;
            }
        }

        // Initialize health display using the HealthBar script
        UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);
    }

    /// <summary>
    /// Updates the health bar and text using the HealthBar component.
    /// </summary>
    /// <param name="currentHealth">Current health value.</param>
    /// <param name="maxHealth">Maximum health value.</param>
    public void UpdateDisplay(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
        {
            Logger.LogError("[EnemyDisplay] healthBar is not assigned! Health display will not work.", this);
            return;
        }
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    /// <summary>
    /// Sets the intent display with the provided EnemyIntent.
    /// </summary>
    /// <param name="intent">The EnemyIntent object to display.</param>
    public void SetIntent(EnemyIntent intent)
    {
        if (intentDisplay != null)
        {
            intentDisplay.SetIntent(intent);
        }
        else
        {
            Logger.LogWarning("[EnemyDisplay] intentDisplay is not assigned! Intent will not be shown.", this);
        }
    }

    /// <summary>
    /// Clears the intent display.
    /// </summary>
    public void ClearIntentDisplay()
    {
        if (intentDisplay != null)
        {
            intentDisplay.ClearIntent();
        }
    }

    /// <summary>
    /// Changes the enemy sprite's color to visually indicate enraged state.
    /// </summary>
    /// <param name="isEnraged">True to set enraged color, false to reset to normal.</param>
    public void SetEnragedVisual(bool isEnraged)
    {
        if (enemyImage == null) return;

        if (isEnraged)
        {
            // Set an enraged color (e.g., red tint)
            // FF2F3B in hex is R=255, G=47, B=59
            // Divide by 255 for Unity's Color (0-1f) format
            enemyImage.color = new Color(1f, 47f / 255f, 59f / 255f, 1f);
        }
        else
        {
            // Reset to normal color (white)
            enemyImage.color = Color.white;
        }
    }

    public void ShowDamagePopup(int damage)
    {
        if (floatingDamageTextPrefab == null || textSpawnAnchor == null) return;

        GameObject go = Instantiate(floatingDamageTextPrefab, textSpawnAnchor);
        RectTransform rect = go.GetComponent<RectTransform>();
        CanvasGroup group = go.GetComponent<CanvasGroup>();
        TMP_Text text = go.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = damage.ToString();
            text.color = Color.white; // base color
        }

        rect.localScale = Vector3.one * 1.6f;

        Sequence seq = DOTween.Sequence();

        // Shake
        seq.Append(rect.DOShakeScale(0.1f, 0.2f, 10));

        // Light flash (e.g. white glow)
        if (text != null)
        {
            seq.Append(text.DOColor(new Color(1f, 1f, 1f), 0.05f));  // Full white
           // seq.Append(text.DOColor(new Color32(0xFF, 0x7A, 0x7A, 255), 0.2f)); // Slight red tone
        }

        // Shrink, Float & Fade out
        seq.Append(rect.DOScale(0.8f, 0.6f).SetEase(Ease.InOutQuad))
           .Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 80f, 0.6f).SetEase(Ease.OutCubic))
           .Join(group.DOFade(0f, 0.6f))
           .AppendCallback(() => Destroy(go));
    }

    /// <summary>
    /// Shows a heal popup with the specified amount.
    /// </summary>
    public void ShowHealPopup(int amount)
    {
        if (floatingDamageTextPrefab == null || textSpawnAnchor == null) return;

        GameObject go = Instantiate(floatingDamageTextPrefab, textSpawnAnchor);
        var rect = go.GetComponent<RectTransform>();
        var group = go.GetComponent<CanvasGroup>();
        var text = go.GetComponentInChildren<TMPro.TMP_Text>();

        if (text != null)
        {
            text.text = $"+{amount}";
            text.color = Color.green;
        }

        rect.localScale = Vector3.one * 1.3f;

        var seq = DG.Tweening.DOTween.Sequence();
        seq.Append(rect.DOScale(1.0f, 0.2f));
        seq.Append(rect.DOAnchorPosY(rect.anchoredPosition.y + 70f, 0.6f).SetEase(Ease.OutCubic))
           .Join(group.DOFade(0f, 0.6f))
           .AppendCallback(() => Destroy(go));
    }

    /// <summary>
    /// Plays the death animation for the enemy.
    /// </summary> 
    public void PlayDeathAnimation(System.Action onComplete = null)
    {
        if (enemyImage == null)
        {
            onComplete?.Invoke();
            return;
        }

        enemyImage.DOFade(0f, 1f)
                  .SetEase(Ease.InOutQuad)
                  .OnComplete(() => {
                      onComplete?.Invoke();
                      gameObject.SetActive(false);
                  });
    }
}