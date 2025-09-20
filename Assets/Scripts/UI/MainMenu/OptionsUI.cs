using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [SerializeField] private float defaultGameplaySFXVolume = 0.5f;
    [SerializeField] private float defaultMenuSFXVolume = 0.5f;

    [Header("UI")]
    [SerializeField] private Slider menuSFXSlider;
    [SerializeField] private Slider gameplaySFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text menuSFXVolumeText;
    [SerializeField] private TMP_Text gameplaySFXVolumeText;

    [SerializeField] private Button backButton;

    [Header("SFX")]
    [Tooltip("Name of the SFX clip to play on hover (must match AudioManager entry).")]
    public string hoverSFX = "MainMenuHover";

    [Tooltip("Name of the SFX clip to play on click (must match AudioManager entry).")]
    public string clickSFX = "MainMenuClick";

    private void Awake()
    {
        if (panel == null)
        {
            Logger.LogError("OptionsUI: Panel is not assigned.", this);
            return;
        }

        canvasGroup = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
        panel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        if (menuSFXSlider == null || gameplaySFXSlider == null || musicSlider == null)
        {
            Logger.LogError("OptionsUI: One or more sliders are not assigned.", this);
            return;
        }

        float savedMenuSFX = defaultMenuSFXVolume;
        float savedGameplaySFX = defaultGameplaySFXVolume;
        float savedMusicVolume = defaultMusicVolume;

        menuSFXSlider.value = savedMenuSFX;
        gameplaySFXSlider.value = savedGameplaySFX;
        musicSlider.value = savedMusicVolume;

        menuSFXSlider.onValueChanged.AddListener(SetMenuSFXVolume);
        gameplaySFXSlider.onValueChanged.AddListener(SetGameplaySFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        SetMenuSFXVolume(savedMenuSFX);
        SetGameplaySFXVolume(savedGameplaySFX);
        SetMusicVolume(savedMusicVolume);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseOptions);
        }
    }

    public void OpenOptions()
    {
        if (panel == null || canvasGroup == null) return;

        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, 0.3f);
    }

    public void CloseOptions()
    {
        if (canvasGroup == null || panel == null) return;

        canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            panel.SetActive(false);
        });

        AudioManager.Instance?.PlaySFX(clickSFX);
    }

    public void SetMenuSFXVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Menu", volume);
        PlayerPrefs.SetFloat("MenuSFXVolume", volume);
        UpdateVolumeLabel(menuSFXVolumeText, volume);
    }

    public void SetGameplaySFXVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Gameplay", volume);
        PlayerPrefs.SetFloat("GameplaySFXVolume", volume);
        UpdateVolumeLabel(gameplaySFXVolumeText, volume);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Music", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        UpdateVolumeLabel(musicVolumeText, volume);
    }

    private void UpdateVolumeLabel(TMP_Text label, float value)
    {
        if (label == null) return;
        int percent = Mathf.RoundToInt(value * 100f);
        label.text = percent.ToString();
    }
}
