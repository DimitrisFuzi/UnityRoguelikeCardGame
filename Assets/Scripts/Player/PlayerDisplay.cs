// PlayerDisplay.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using MyProjectF.Assets.Scripts.Player;

public class PlayerDisplay : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("UI Elements")]
    [SerializeField] private HealthBar playerHealthBar; // NEW: Reference to HealthBar script for player
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Image armorImage;
    [SerializeField] private GameObject floatingDamageTextPrefab;
    [SerializeField] private RectTransform textSpawnAnchor;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            Logger.Log($"PlayerDisplay has pre-assigned playerStats = {playerStats}", this);
        }

        if (playerStats != null)
        {
            PlayerStats.OnStatsChanged += UpdatePlayerUI;
            UpdatePlayerUI(); // Update UI immediately on start
        }
        else
        {
            Logger.LogWarning("⚠️ PlayerStats not found! UI will show fallback values.", this);
            SetFallbackDisplay();
        }
    }

    /// <summary>
    /// Updates the UI elements with the current player stats.
    /// </summary>
    public void UpdatePlayerUI()
    {
        if (playerStats == null)
        {
            SetFallbackDisplay();
            return;
        }

        // Update player health using the HealthBar script
        if (playerHealthBar != null)
        {
            playerHealthBar.UpdateHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
        }
        else
        {
            Logger.LogWarning("[PlayerDisplay] playerHealthBar is not assigned! Player health will not display.", this);
            // Fallback for health text if health bar isn't set up
            // You might want to have a separate TextMeshProUGUI for health here if HealthBar isn't always used.
        }

        armorText.text = $"{playerStats.Armor}";
        energyText.text = $"{playerStats.energy}";
    }

    /// <summary>
    /// Sets placeholder values when no PlayerStats is available.
    /// </summary>
    private void SetFallbackDisplay()
    {
        // For health, you'd need to decide if HealthBar handles fallback or if there's a separate text field.
        // For now, this will show 0/0 on the health bar.
        if (playerHealthBar != null)
        {
            playerHealthBar.UpdateHealthBar(0, 0);
        }
        else
        {
            // If playerHealthBar is null AND you removed healthText, this will not show health.
            // You might need a separate TextMeshProUGUI for player health if HealthBar isn't mandatory.
        }

        armorText.text = "--";
        energyText.text = "--";
    }

    private void OnEnable()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.OnStatsChanged += UpdatePlayerUI;
        }
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.OnStatsChanged -= UpdatePlayerUI;
        }
    }

    public void ShowArmorGainEffect()
    {
        if (armorImage == null) return;

        armorImage.rectTransform.DOKill();
        armorImage.rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 8, 1.0f);

        // Armor Text
        if (armorText != null)
        {
            armorText.rectTransform.DOKill();
            armorText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.8f);

            Color originalTextColor = armorText.color;
            armorText.DOColor(Color.cyan, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => armorText.color = originalTextColor);
        }
    }
    // <summary>
    /// Displays a floating damage text popup at the specified anchor position.
    /// </summary>
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

        seq.Append(rect.DOShakeScale(0.1f, 0.2f, 10));
        seq.Append(text.DOColor(Color.white, 0.05f));
        seq.Append(text.DOColor(new Color32(0xFF, 0x7A, 0x7A, 255), 0.2f));

        seq.Append(rect.DOScale(0.8f, 0.6f).SetEase(Ease.InOutQuad))
           .Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 80f, 0.6f).SetEase(Ease.OutCubic))
           .Join(group.DOFade(0f, 0.6f))
           .AppendCallback(() => Destroy(go));
    }


}