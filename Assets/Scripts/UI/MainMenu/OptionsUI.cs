using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject panel;

    [SerializeField] private CanvasGroup canvasGroup;


    private void Awake()
    {
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        panel.SetActive(false); // ensure it's off at start
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        sfxSlider.value = savedSFX;

        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        SetSFXVolume(savedSFX);
    }

    public void OpenOptions()
    {
        Debug.Log("👀 OpenOptions was called");
        Debug.Log($"📦 Panel is: {panel?.name} | Active: {panel?.activeSelf}");


        panel.SetActive(true);                // Make sure it's visible
        canvasGroup.alpha = 0f;              // Start from 0 opacity
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, 0.3f);        // Fade in
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

    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance?.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
