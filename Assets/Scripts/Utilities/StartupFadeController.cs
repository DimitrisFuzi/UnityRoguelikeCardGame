using UnityEngine;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Ensures ScreenFader is in place, fades in the scene, and optionally preps/starts a background VideoPlayer.
/// Put this on a bootstrap object in the first scene.
/// </summary>
public class StartupFadeController : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private VideoPlayer backgroundVideo;
    [SerializeField] private float fadeInDuration = 0.6f;

    private IEnumerator Start()
    {
        // Ensure a fader exists and is on top
        if (ScreenFader.Instance == null)
            new GameObject("ScreenFader").AddComponent<ScreenFader>();

        ScreenFader.Instance.SetInstantOpaque();
        ScreenFader.Instance.BringToFront();

        // Prepare video so first frame is ready before fade
        if (backgroundVideo != null)
        {
            backgroundVideo.playOnAwake = false;
            backgroundVideo.waitForFirstFrame = true;

            backgroundVideo.Prepare();
            while (!backgroundVideo.isPrepared)
                yield return null;

            // Warm the first frame: Play -> wait a frame -> Pause
            backgroundVideo.Play();
            yield return null;
            backgroundVideo.Pause();
        }

        // Let any other startup work settle for one frame
        yield return null;

        // Fade the screen in
        yield return ScreenFader.Instance.FadeIn(fadeInDuration);

        // Start the video after we are visible
        if (backgroundVideo != null)
            backgroundVideo.Play();
    }
}
