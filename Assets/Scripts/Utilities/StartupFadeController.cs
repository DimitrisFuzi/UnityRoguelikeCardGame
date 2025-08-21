using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartupFadeController : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private VideoPlayer backgroundVideo;   // τράβα το από Inspector
    [SerializeField] private float fadeInDuration = 0.6f;   // το “ωραίο” σου time

    private IEnumerator Start()
    {
        // βεβαιώσου ότι υπάρχει ScreenFader (singleton)
        if (ScreenFader.Instance == null)
            new GameObject("ScreenFader").AddComponent<ScreenFader>();

        // Κάνε τον fader opaque & πάνω από όλα
        ScreenFader.Instance.SetInstantOpaque();
        ScreenFader.Instance.BringToFront();

        // --- PREPARE VIDEO (ώστε να μην “σκάει” καθυστερημένα)
        if (backgroundVideo != null)
        {
            // συνήθεις ασφαλείς ρυθμίσεις
            backgroundVideo.playOnAwake = false;
            backgroundVideo.waitForFirstFrame = true;

            // Ξεκίνα prepare
            backgroundVideo.Prepare();
            while (!backgroundVideo.isPrepared)
                yield return null; // περίμενε να buffer-αριστεί το πρώτο frame

            // “θερμαίνουμε” το πρώτο frame: Play → 1 frame → Pause
            backgroundVideo.Play();
            // περιμένουμε 1 frame για να γραφτεί σίγουρα το frame στο target (RenderTexture/Camera plane)
            yield return null;
            backgroundVideo.Pause();
        }

        // Μικρό buffer frame να “κάτσει” ό,τι άλλο σηκώθηκε στη σκηνή
        yield return null;

        // ΤΩΡΑ κάνε fade‑in (χρησιμοποιεί το δικό σου duration εδώ)
        yield return ScreenFader.Instance.FadeIn(fadeInDuration);

        // Μόλις “ανοίξει η αυλαία”, ξεκίνα το βίντεο
        if (backgroundVideo != null)
            backgroundVideo.Play();
    }
}
