using UnityEngine;

/// <summary>
/// Handles user options. Currently caches AudioManager reference.
/// </summary>
public class OptionsManager : MonoBehaviour
{
    private AudioManager audioManager;

    [Tooltip("If true, audio systems may be muted (not enforced here).")]
    public bool muteAudio = false;

    private void Start()
    {
        audioManager = GameManager.Instance != null ? GameManager.Instance.AudioManager : null;
        if (audioManager == null)
            Logger.LogWarning("OptionsManager: AudioManager not found via GameManager.", this);
    }
}
