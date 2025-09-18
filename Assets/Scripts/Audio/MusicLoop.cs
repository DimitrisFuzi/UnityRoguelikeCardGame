using UnityEngine;
using MyProjectF.Assets.Scripts.Managers;

[DisallowMultipleComponent]
[AddComponentMenu("Audio/Music Loop")]
/// <summary>
/// Plays a music track via AudioManager on enable.
/// </summary>
public class MusicLoop : MonoBehaviour
{
    [SerializeField] private AudioClip track;
    [SerializeField] private bool loop = true;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private void OnEnable()
    {
        if (track != null)
            AudioManager.Instance?.PlayMusic(track, loop, volume);
    }
}
