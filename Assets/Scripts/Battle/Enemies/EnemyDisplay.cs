// EnemyDisplay.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Image enemyImage;                           // Enemy sprite image
    [SerializeField] private HealthBar healthBar;      // Reference to the HealthBar script
    [SerializeField] private IntentDisplay intentDisplay;
    [SerializeField] private Transform textSpawnAnchor;
    [SerializeField] private GameObject floatingDamageTextPrefab;
    [SerializeField] private Image enragedImage;
    [SerializeField] private Image awakenedImage;

    private RectTransform enemyRect;

    /// <summary>Sets up visuals and layout from EnemyData.</summary>
    public void Setup(Enemy enemy, EnemyData enemyData)
    {
        if (enemyImage == null)
        {
            Logger.LogError("[EnemyDisplay] enemyImage is not assigned!", this);
            return;
        }

        enemyImage.sprite = enemyData.enemySprite;

        if (enragedImage != null)
        {
            var c = enragedImage.color; c.a = 0f; enragedImage.color = c;
        }

        if (awakenedImage != null)
        {
            var c2 = awakenedImage.color; c2.a = 0f; awakenedImage.color = c2;
        }

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

            // target = visual size of the enemy
            fitter.target = enemyImage.rectTransform;

            // Apply settings from EnemyData
            switch (enemyData.shadowMode)
            {
                case ShadowMode.None:
                    shadowTr.gameObject.SetActive(false);
                    break;

                case ShadowMode.Manual:
                    shadowTr.gameObject.SetActive(true);
                    fitter.mode = EnemyShadowFitter.Mode.Manual;
                    fitter.ApplyFromData(
                        ShadowMode.Manual,
                        enemyData.shadowWidthMultiplier,
                        enemyData.shadowHeightToWidth,
                        enemyData.shadowOffset,
                        enemyData.manualShadowSize
                    );
                    break;

                default: // Auto
                    shadowTr.gameObject.SetActive(true);
                    fitter.mode = EnemyShadowFitter.Mode.Auto;
                    fitter.ApplyFromData(
                        ShadowMode.Auto,
                        enemyData.shadowWidthMultiplier,
                        enemyData.shadowHeightToWidth,
                        enemyData.shadowOffset,
                        Vector2.zero
                    );
                    break;
            }
        }

        // Initialize health display
        UpdateDisplay(enemy.CurrentHealth, enemy.MaxHealth);

        // Wisp float motion for wisp enemies
        if (enemyData.enemyAIType == EnemyAIType.WispLeft || enemyData.enemyAIType == EnemyAIType.WispRight)
        {
            var floaty = gameObject.GetComponent<WispFloatMotion>() ?? gameObject.AddComponent<WispFloatMotion>();
            floaty.amplitude = Random.Range(5f, 8f);
            floaty.speed = Random.Range(1.6f, 2.4f);
        }
    }

    /// <summary>Updates the health bar and text via HealthBar.</summary>
    public void UpdateDisplay(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
        {
            Logger.LogError("[EnemyDisplay] healthBar is not assigned! Health display will not work.", this);
            return;
        }
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    /// <summary>Sets the intent display with the provided EnemyIntent.</summary>
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

    /// <summary>Clears the intent display.</summary>
    public void ClearIntentDisplay()
    {
        if (intentDisplay != null)
            intentDisplay.ClearIntent();
    }

    /// <summary>Sets or resets the enraged visual effect.</summary>
    public void SetEnragedVisual(bool isEnraged)
    {
        if (enragedImage != null)
        {
            enragedImage.DOKill();
            enragedImage.DOFade(isEnraged ? 1f : 0f, 0.2f);

            // keep base sprite clean
            if (enemyImage != null) enemyImage.color = Color.white;
        }
        else
        {
            // fallback if no separate enragedImage is assigned
            if (enemyImage == null) return;
            enemyImage.color = isEnraged ? new Color(1f, 47f / 255f, 59f / 255f, 1f) : Color.white;
        }
    }

    /// <summary>Awakened visual effect.</summary>
    public void SetAwakenVisual(bool on)
    {
        if (awakenedImage == null) return;

        awakenedImage.DOKill();
        var t = on ? 1.2f : 0.25f;
        awakenedImage.DOFade(on ? 1f : 0f, t);

        // optional impact on trigger
        if (on && enemyImage != null)
            enemyImage.rectTransform.DOPunchScale(Vector3.one * 0.12f, 0.28f, 6, 0.6f);
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
            text.color = Color.white;
        }

        rect.localScale = Vector3.one * 1.6f;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOShakeScale(0.1f, 0.2f, 10)); // shake
        if (text != null)
            seq.Append(text.DOColor(new Color(1f, 1f, 1f), 0.05f));  // light flash

        // shrink, float & fade out
        seq.Append(rect.DOScale(0.8f, 0.6f).SetEase(Ease.InOutQuad))
           .Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 80f, 0.6f).SetEase(Ease.OutCubic));

        if (group != null)
            seq.Join(group.DOFade(0f, 0.6f));

        seq.OnComplete(() => Destroy(go));
    }

    /// <summary>Shows a heal popup with the specified amount.</summary>
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

        var seq = DOTween.Sequence();
        seq.Append(rect.DOScale(1.0f, 0.2f));
        seq.Append(rect.DOAnchorPosY(rect.anchoredPosition.y + 70f, 0.6f).SetEase(Ease.OutCubic));

        if (group != null)
            seq.Join(group.DOFade(0f, 0.6f));

        seq.OnComplete(() => Destroy(go));
    }

    /// <summary>Plays the death animation for the enemy.</summary>
    public void PlayDeathAnimation(System.Action onComplete = null)
    {
        if (enemyImage == null) { onComplete?.Invoke(); return; }

        // kill any running tweens
        enemyImage.DOKill();
        enragedImage?.DOKill();
        awakenedImage?.DOKill();

        var seq = DOTween.Sequence()
            .Join(enemyImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));

        if (enragedImage != null)
            seq.Join(enragedImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));

        if (awakenedImage != null)
            seq.Join(awakenedImage.DOFade(0f, 1f).SetEase(Ease.InOutQuad));

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            gameObject.SetActive(false);
        });
    }
}
