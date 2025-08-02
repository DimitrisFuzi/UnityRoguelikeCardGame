using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Manages audio playback for SFX with volume overrides and future mixer integration.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource sfxSource;

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
        { "End_Turn", 0.4f },
        { "Player_Hit_Blocked", 0.2f },
        { "Player_Hit", 0.2f },
        { "Rage_Effect", 0.2f },
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
            sfxSource.PlayOneShot(clip, volume);
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
    /// Updates the SFX group volume via AudioMixer (range: 0.0001 to 1.0).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (volume <= 0f)
            audioMixer.SetFloat("SFXVolume", -80f); // effectively muted
        else
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
    }

}


[System.Serializable]
public class SFXEntry
{
    public string name;
    public AudioClip clip;
}
