using UnityEngine;
using System.Collections;

/// <summary>
/// Ensures ScreenFader is in place, fades in the scene,
/// and optionally shows a background image.
/// </summary>
public class StartupFadeControllerImage : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private GameObject backgroundImage; 
    [SerializeField] private float fadeInDuration = 0.6f;

    private IEnumerator Start()
    {
        // Ensure a fader exists and is on top
        if (ScreenFader.Instance == null)
            new GameObject("ScreenFader").AddComponent<ScreenFader>();

        ScreenFader.Instance.SetInstantOpaque();
        ScreenFader.Instance.BringToFront();


        if (backgroundImage != null)
            backgroundImage.SetActive(true);

        // Περίμενε 1 frame
        yield return null;

        // Fade in
        yield return ScreenFader.Instance.FadeIn(fadeInDuration);
    }
}
