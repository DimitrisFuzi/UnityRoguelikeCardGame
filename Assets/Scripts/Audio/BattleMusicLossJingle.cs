using UnityEngine;
using System.Collections;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Managers;

[DisallowMultipleComponent]
[AddComponentMenu("Audio/Battle Music Loss Jingle")]
/// <summary>
/// On player death: stop active music/loops and play a one-shot defeat jingle.
/// </summary>
public class BattleMusicLossJingle : MonoBehaviour
{
    [SerializeField] private PlayerStats player;              // Auto-find if null
    [SerializeField] private AudioClip defeatJingle;          // Assign in Inspector
    [SerializeField, Range(0f, 1f)] private float jingleVolume = 1f;

    private bool _fired;

    private void Awake()
    {
        if (player == null)
            player = Object.FindFirstObjectByType<PlayerStats>();
    }

    private IEnumerator Start()
    {
        if (player == null)
        {
            while (player == null)
            {
                player = Object.FindFirstObjectByType<PlayerStats>();
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
        if (player != null)
            player.OnDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (_fired) return;
        _fired = true;

        var am = AudioManager.Instance;
        am?.StopMusic();

        // Non-obvious but useful: stop ALL looping AudioSources in case some BGM bypasses AudioManager.
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src != null && src.isActiveAndEnabled && src.isPlaying && src.loop)
                src.Stop();
        }

        if (am != null && defeatJingle != null)
            am.PlayJingle(defeatJingle, jingleVolume);
    }
}
