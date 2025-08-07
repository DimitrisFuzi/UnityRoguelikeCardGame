using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Slider menuSFXSlider;
    [SerializeField] private Slider gameplaySFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;

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
    }

    public void SetMenuSFXVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Menu", volume);
        PlayerPrefs.SetFloat("MenuSFXVolume", volume);
    }

    public void SetGameplaySFXVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Gameplay", volume);
        PlayerPrefs.SetFloat("GameplaySFXVolume", volume);


    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance?.SetCategoryVolume("Music", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}
