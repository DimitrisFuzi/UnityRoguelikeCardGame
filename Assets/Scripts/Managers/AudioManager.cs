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

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSFXSource;
    [SerializeField] private AudioSource gameplaySFXSource;

    [SerializeField] private List<SFXEntry> sfxList = new List<SFXEntry>();
    private readonly Dictionary<string, AudioClip> sfxDictionary = new();

    // Volume overrides per SFX name (0.0 to 1.0)
    private readonly Dictionary<string, float> sfxVolumeOverrides = new()
    {
        { "Card_Hover", 0.05f },
        { "Card_Select", 0.1f },
        { "Enemy_Hit", 0.2f },
        { "Card_Draw", 0.3f },
        { "Block_Gain", 0.3f },
        { "Enemy_Death", 0.1f },
        { "End_Turn", 0.3f },
        { "Player_Hit_Blocked", 0.2f },
        { "Player_Hit", 0.1f },
        { "Rage_Effect", 0.2f },
        { "MainMenuHover", 0.4f },
        { "MainMenuClick", 0.4f }
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

    /// <summary>Plays an SFX by name with optional volume scaling.</summary>
    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out var clip))
        {
            float volume = GetVolumeForSFX(name);

            // Route UI sounds to UI source, everything else to gameplay
            if (name.StartsWith("MainMenu"))
                uiSFXSource?.PlayOneShot(clip, volume);
            else
                gameplaySFXSource?.PlayOneShot(clip, volume);
        }
        else
        {
            Logger.LogWarning($"[AudioManager] SFX '{name}' not found.", this);
        }
    }

    /// <summary>Gets custom volume scale for a given SFX name.</summary>
    private float GetVolumeForSFX(string name)
    {
        return sfxVolumeOverrides.TryGetValue(name, out float volume) ? volume : 1f;
    }

    /// <summary>Updates the volume of a specific audio category via AudioMixer ("Menu", "Gameplay", "Music").</summary>
    public void SetCategoryVolume(string category, float volume)
    {
        float dbVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;

        switch (category)
        {
            case "Music":
                audioMixer?.SetFloat("MusicVolume", dbVolume);
                break;
            case "Gameplay":
                audioMixer?.SetFloat("SFXVolume", dbVolume);
                break;
            case "Menu":
                audioMixer?.SetFloat("MenuSFXVolume", dbVolume);
                break;
        }
    }

    /// <summary>Legacy support – sets general SFX volume (mapped to Gameplay).</summary>
    public void SetSFXVolume(float volume) => SetCategoryVolume("Gameplay", volume);

    /// <summary>Plays background music with optional looping.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (musicSource == null || clip == null) return;
        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        musicSource.Play();
    }

    /// <summary>Stops background music playback (if any).</summary>
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    /// <summary>Plays a short, non-looping jingle on the music channel (stops current music first).</summary>
    public void PlayJingle(AudioClip clip, float volume = 1f)
    {
        if (musicSource == null || clip == null) return;
        musicSource.Stop();
        musicSource.loop = false;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        musicSource.Play();
    }
}

[System.Serializable]
public class SFXEntry
{
    public string name;
    public AudioClip clip;
}
