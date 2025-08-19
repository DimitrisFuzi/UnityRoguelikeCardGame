using UnityEngine;

public class MusicLoop : MonoBehaviour
{
    [SerializeField] private AudioClip track;
    [SerializeField] private bool loop = true;
    [SerializeField] private float volume = 1f;
    private void OnEnable() { AudioManager.Instance?.PlayMusic(track, loop, volume); }
}