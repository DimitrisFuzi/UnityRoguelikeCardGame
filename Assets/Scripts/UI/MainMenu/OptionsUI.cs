using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Slider menuSFXSlider;
    [SerializeField] private Slider gameplaySFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text menuSFXVolumeText;
    [SerializeField] private TMP_Text gameplaySFXVolumeText;

    [Header("SFX")]
    [Tooltip("Name of the SFX clip to play on hover (must match AudioManager entry).")]
    public string hoverSFX = "MainMenuHover";

    [Tooltip("Name of the SFX clip to play on click (must match AudioManager entry).")]
    public string clickSFX = "MainMenuClick";

    private void Awake()
    {
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        panel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        float savedMenuSFX = PlayerPrefs.GetFloat("MenuSFXVolume", 0.5f);
        float savedGameplaySFX = PlayerPrefs.GetFloat("GameplaySFXVolume", 0.5f);
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        menuSFXSlider.value = savedMenuSFX;
        gameplaySFXSlider.value = savedGameplaySFX;
        musicSlider.value = savedMusicVolume;

        menuSFXSlider.onValueChanged.AddListener(SetMenuSFXVolume);
        gameplaySFXSlider.onValueChanged.AddListener(SetGameplaySFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        SetMenuSFXVolume(savedMenuSFX);
        SetGameplaySFXVolume(savedGameplaySFX);
        SetMusicVolume(savedMusicVolume); 
    }

    public void OpenOptions()
    {
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, 0.3f);
    }

    public void CloseOptions()
    {
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
        int percent = Mathf.RoundToInt(value * 100f);
        label.text = percent.ToString();
    }

}
