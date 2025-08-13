using UnityEngine;
using MyProjectF.Assets.Scripts.Player;
using System.Collections;
/// <summary>
/// On player death: hard-stop all looping BGM and play a short defeat jingle once.
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
        if (player == null) player = FindObjectOfType<PlayerStats>();
    }

    private IEnumerator Start()
    {
        if (player == null)
        {
            // περίμενε μέχρι να εμφανιστεί ο PlayerStats
            while (player == null)
            {
                player = FindObjectOfType<PlayerStats>();
                if (player != null)
                {
                    player.OnDied += OnPlayerDied;
                    break;
                }
                yield return null;
            }
        }
    }

    private void OnEnable()
    {
        if (player != null) player.OnDied += OnPlayerDied;    // PlayerStats.OnDied: Action (no args)
    }

    private void OnDisable()
    {
        if (player != null) player.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (fired) return;
        fired = true;

        // 1) Stop music from AudioManager if used
        var am = AudioManager.Instance;
        if (am != null) am.StopMusic();

        // 2) Hard-stop ANY other looping AudioSources that are currently playing (e.g., Play On Awake BGM)
        var sources = FindObjectsOfType<AudioSource>(true);
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
