using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using MyProjectF.Assets.Scripts.Player;

public class PlayerDisplay : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("UI Elements")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Image armorImage;
    [SerializeField] private GameObject floatingDamageTextPrefab;
    [SerializeField] private RectTransform textSpawnAnchor;

    private void Start()
    {
        playerStats = PlayerStats.Instance ?? playerStats;

        if (playerStats != null)
        {
            UpdatePlayerUI(); // initial paint
        }
        else
        {
            Logger.LogWarning("PlayerStats not found. UI will show fallback values.", this);
            SetFallbackDisplay();
        }
    }

    private void OnEnable()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.OnStatsChanged += UpdatePlayerUI;
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.OnStatsChanged -= UpdatePlayerUI;
    }

    /// <summary>Updates the UI elements with the current player stats.</summary>
    public void UpdatePlayerUI()
    {
        if (playerStats == null)
        {
            SetFallbackDisplay();
            return;
        }

        if (playerHealthBar != null)
        {
            playerHealthBar.UpdateHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
        }
        else
        {
            Logger.LogWarning("[PlayerDisplay] playerHealthBar is not assigned.", this);
        }

        if (armorText != null) armorText.text = $"{playerStats.Armor}";
        if (energyText != null) energyText.text = $"{playerStats.energy}";
    }

    /// <summary>Sets placeholder values when no PlayerStats is available.</summary>
    private void SetFallbackDisplay()
    {
        if (playerHealthBar != null)
            playerHealthBar.UpdateHealthBar(0, 0);

        if (armorText != null) armorText.text = "--";
        if (energyText != null) energyText.text = "--";
    }

    public void ShowArmorGainEffect()
    {
        if (armorImage == null) return;

        armorImage.rectTransform.DOKill();
        armorImage.rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 8, 1.0f);

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

    /// <summary>Displays a floating damage text popup at the anchor position.</summary>
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
        if (text != null)
        {
            seq.Append(text.DOColor(Color.white, 0.05f));
            seq.Append(text.DOColor(new Color32(0xFF, 0x7A, 0x7A, 255), 0.2f));
        }

        seq.Append(rect.DOScale(0.8f, 0.6f).SetEase(Ease.InOutQuad))
           .Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 80f, 0.6f).SetEase(Ease.OutCubic));

        if (group != null)
            seq.Join(group.DOFade(0f, 0.6f));

        seq.AppendCallback(() => Destroy(go));
    }
}
