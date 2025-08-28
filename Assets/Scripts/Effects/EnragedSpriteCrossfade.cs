using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(EnemyDisplay))]
public class EnrageSpriteCrossfade : MonoBehaviour
{
    [SerializeField] private Image baseImage;     // original
    [SerializeField] private Image enragedImage;  // overlay or baked variant
    [SerializeField] private float fadeTime = 0.2f;
    [SerializeField] private float punchScale = 1.12f;

    void Reset()
    {
        var imgs = GetComponentsInChildren<Image>(true);
        if (imgs.Length >= 2)
        {
            baseImage = imgs[0];
            enragedImage = imgs[1];
        }
    }

    public void SetEnraged(bool on)
    {
        if (!enragedImage) return;

        enragedImage.DOKill();
        enragedImage.DOFade(on ? 1f : 0f, fadeTime);

        // Προαιρετικό impact μόνο στο ON
        if (on && baseImage)
            baseImage.rectTransform.DOPunchScale(Vector3.one * (punchScale - 1f), 0.28f, 6, 0.6f);
    }
}
