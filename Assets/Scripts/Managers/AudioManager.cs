using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Manages audio playback for SFX with volume overrides and mixer category control.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer audioMixer;
    //[SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSFXSource;
    [SerializeField] private AudioSource gameplaySFXSource;


    [SerializeField] private List<SFXEntry> sfxList = new List<SFXEntry>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    // Volume overrides per SFX name (0.0 to 1.0)
    private Dictionary<string, float> sfxVolumeOverrides = new()
    {
        { "Card_Hover", 0.05f },
        { "Card_Select", 0.1f },
        { "Enemy_Hit", 0.2f },
        { "Card_Draw", 0.2f },
        { "Block_Gain", 0.2f },
        { "Enemy_Death", 0.1f },
        { "End_Turn", 0.3f },
        { "Player_Hit_Blocked", 0.2f },
        { "Player_Hit", 0.1f },
        { "Rage_Effect", 0.2f },
        { "MainMenuHover", 0.5f },
        { "MainMenuClick", 0.5f }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in sfxList)
        {
            if (!sfxDictionary.ContainsKey(entry.name))
                sfxDictionary.Add(entry.name, entry.clip);
        }
    }

    /// <summary>
    /// Plays an SFX by name with optional volume scaling.
    /// </summary>
    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out var clip))
        {
            float volume = GetVolumeForSFX(name);

            // Επιλογή κατάλληλης πηγής
            if (name.StartsWith("MainMenu"))
            {
                uiSFXSource?.PlayOneShot(clip, volume);
            }
            else
            {
                gameplaySFXSource?.PlayOneShot(clip, volume);
            }
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{name}' not found.");
        }
    }


    /// <summary>
    /// Gets custom volume scale for a given SFX name.
    /// </summary>
    private float GetVolumeForSFX(string name)
    {
        if (sfxVolumeOverrides.TryGetValue(name, out float volume))
            return volume;

        return 1f;
    }

    /// <summary>
    /// Updates the volume of a specific audio category via AudioMixer.
    /// Supported categories: "Menu", "Gameplay", "Music"
    /// </summary>
    public void SetCategoryVolume(string category, float volume)
    {

        /*string paramName = category switch
        {
            "Menu" => "MenuSFXVolume",
            "Gameplay" => "SFXVolume",
            "Music" => "MusicVolume",
            _ => null
        };

        if (string.IsNullOrEmpty(paramName))
        {
            Debug.LogWarning($"[AudioManager] Unknown category: {category}");
            return;
        }

        float db = (volume <= 0f) ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(paramName, db);*/

        float dbVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;

        switch (category)
        {
            case "Music":
                audioMixer.SetFloat("MusicVolume", dbVolume);
                break;
            case "Gameplay":
                audioMixer.SetFloat("SFXVolume", dbVolume);
                break;
            case "Menu":
                audioMixer.SetFloat("MenuSFXVolume", dbVolume);
                break;
        }

        Debug.Log($"[AudioManager] Set {category} volume: {volume} → {dbVolume} dB");
    }

    /// <summary>
    /// Legacy support - sets general SFX volume (mapped to Gameplay).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        SetCategoryVolume("Gameplay", volume);
    }
}

[System.Serializable]
public class SFXEntry
{
    public string name;
    public AudioClip clip;
}
