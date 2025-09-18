using UnityEngine;
using MyProjectF.Assets.Scripts.Player;
using System.Collections;

[DisallowMultipleComponent]
[AddComponentMenu("Audio/Battle Music Loss Jingle")]
/// <summary>
/// On player death: stop any looping BGM and play a short defeat jingle once.
/// Minimal & robust: works even if battle music isn't on AudioManager.musicSource.
/// </summary>
public class BattleMusicLossJingle : MonoBehaviour
{
    [SerializeField] private PlayerStats player;              // Auto-find if null
    [SerializeField] private AudioClip defeatJingle;          // Assign in Inspector
    [SerializeField, Range(0f, 1f)] private float jingleVolume = 1f;

    private bool fired;

    private void Awake()
    {
        if (player == null)
            player = Object.FindFirstObjectByType<PlayerStats>();
    }

    /// <summary>Waits until a PlayerStats exists (if needed) and subscribes to OnDied.</summary>
    private IEnumerator Start()
    {
        if (player == null)
        {
            // Wait until PlayerStats appears
            while (player == null)
            {
                player = Object.FindFirstObjectByType<PlayerStats>();
                if (player != null)
                {
                    player.OnDied += OnPlayerDied;
                    break;
                }
                yield return null; // wait a frame
            }
        }
    }

    private void OnEnable()
    {
        if (player != null)
            player.OnDied += OnPlayerDied; // PlayerStats.OnDied: Action (no args)
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnDied -= OnPlayerDied;
    }

    /// <summary>Stops music/loops and plays the defeat jingle once.</summary>
    private void OnPlayerDied()
    {
        if (fired) return;
        fired = true;

        var am = AudioManager.Instance;

        // 1) Stop music from AudioManager if used
        am?.StopMusic();

        // 2) Hard-stop ANY other looping AudioSources that are currently playing (e.g., Play On Awake BGM)
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src != null && src.isActiveAndEnabled && src.isPlaying && src.loop)
                src.Stop();
        }

        // 3) Play defeat jingle once (non-loop) via AudioManager
        if (am != null && defeatJingle != null)
            am.PlayJingle(defeatJingle, jingleVolume);
    }
}
