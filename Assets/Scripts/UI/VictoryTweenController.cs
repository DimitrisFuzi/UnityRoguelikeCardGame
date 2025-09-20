using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class VictoryTweenController : MonoBehaviour
{
    [Header("Overlays")]
    public CanvasGroup vignette;
    public CanvasGroup moonlightTint;

    [Header("Victory")]
    public RectTransform victoryTextRT;
    public CanvasGroup victoryTextCG;

    [Header("Stats")]
    public RectTransform statsRT;
    public CanvasGroup statsCG;
    public TMP_Text turnsValue;   // numeric only
    public TMP_Text dealtValue;   // numeric only
    public TMP_Text takenValue;   // numeric only

    [Header("Buttons")]
    public RectTransform mainMenuRT;
    public CanvasGroup mainMenuCG;
    public Image mainMenuEffect;         // child "effect" Image
    public RectTransform quitRT;
    public CanvasGroup quitCG;
    public Image quitEffect;             // child "effect" Image

    [Header("Timings")]
    public float fadeBg = 0.6f;
    public float popVictory = 0.35f;
    public float statsDelay = 0.15f;
    public float statsDur = 0.35f;
    public float buttonsDelay = 0.10f;
    public float buttonDur = 0.30f;
    public float stagger = 0.06f;

    [Header("CountUp (optional)")]
    public bool doCountUp = true;

    Vector2 statsTarget, mmTarget, quitTarget;

    void Awake()
    {
        // Initialize base state
        if (vignette) vignette.alpha = 0f;
        if (moonlightTint) moonlightTint.alpha = 0f;

        if (victoryTextCG) victoryTextCG.alpha = 0f;
        if (victoryTextRT) victoryTextRT.localScale = Vector3.one * 0.85f;

        if (statsRT) { statsTarget = statsRT.anchoredPosition; statsRT.anchoredPosition = statsTarget + new Vector2(0, 30f); }
        if (statsCG) statsCG.alpha = 0f;

        if (mainMenuRT) { mmTarget = mainMenuRT.anchoredPosition; mainMenuRT.anchoredPosition = mmTarget - new Vector2(0, 20f); }
        if (mainMenuCG) mainMenuCG.alpha = 0f;

        if (quitRT) { quitTarget = quitRT.anchoredPosition; quitRT.anchoredPosition = quitTarget - new Vector2(0, 20f); }
        if (quitCG) quitCG.alpha = 0f;

        if (mainMenuEffect) SetAlpha(mainMenuEffect, 0f);
        if (quitEffect) SetAlpha(quitEffect, 0f);

        PlaySequence();
    }

    void PlaySequence()
    {
        var seq = DOTween.Sequence().SetUpdate(true); // unscaled

        // 1) Background fade
        if (vignette) seq.Append(vignette.DOFade(1f, fadeBg));
        if (moonlightTint) seq.Join(moonlightTint.DOFade(1f, fadeBg * 0.7f));

        // 2) Victory pop
        if (victoryTextCG) seq.Append(victoryTextCG.DOFade(1f, popVictory));
        if (victoryTextRT)
        {
            seq.Join(victoryTextRT.DOScale(1.06f, popVictory).SetEase(Ease.OutCubic));
            seq.Append(victoryTextRT.DOScale(1.0f, 0.12f).SetEase(Ease.InCubic));
        }

        // 3) Stats slide + fade
        seq.AppendInterval(statsDelay);
        if (statsCG) seq.Append(statsCG.DOFade(1f, statsDur));
        if (statsRT) seq.Join(statsRT.DOAnchorPos(statsTarget, statsDur).SetEase(Ease.OutCubic));
        seq.OnStepComplete(() =>
        {
            if (doCountUp) StartCountUps();
        });

        // 4) Buttons with small stagger
        seq.AppendInterval(buttonsDelay);
        if (mainMenuCG) seq.Append(mainMenuCG.DOFade(1f, buttonDur));
        if (mainMenuRT) seq.Join(mainMenuRT.DOAnchorPos(mmTarget, buttonDur).SetEase(Ease.OutCubic));

        seq.AppendInterval(stagger);
        if (quitCG) seq.Append(quitCG.DOFade(1f, buttonDur));
        if (quitRT) seq.Join(quitRT.DOAnchorPos(quitTarget, buttonDur).SetEase(Ease.OutCubic));

        // 5) Subtle loop on button effects
        seq.OnComplete(() =>
        {
            PulseEffect(mainMenuEffect);
            PulseEffect(quitEffect);
        });
    }

    void StartCountUps()
    {
        int tv = ParseInt(turnsValue ? turnsValue.text : "0");
        int dv = ParseInt(dealtValue ? dealtValue.text : "0");
        int kv = ParseInt(takenValue ? takenValue.text : "0");

        if (turnsValue) DOTween.To(() => 0, v => turnsValue.text = v.ToString(), tv, 0.45f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (dealtValue) DOTween.To(() => 0, v => dealtValue.text = v.ToString(), dv, 0.55f).SetEase(Ease.OutCubic).SetUpdate(true);
        if (takenValue) DOTween.To(() => 0, v => takenValue.text = v.ToString(), kv, 0.55f).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    void PulseEffect(Image img)
    {
        if (!img) return;
        img.DOFade(0.16f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

    void SetAlpha(Graphic g, float a)
    {
        var c = g.color; c.a = a; g.color = c;
    }
}
