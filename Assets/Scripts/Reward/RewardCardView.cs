using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;

/// <summary>
/// Visual wrapper for a single reward card choice (thumbnail + select button).
/// </summary>
public class RewardCardView : MonoBehaviour
{
    [Header("Thumbnail Prefab & Parent")]
    public Transform thumbnailParent;
    public GameObject cardThumbnailPrefab;

    [Header("UI")]
    public TMP_Text debugLabel;   // optional
    public Button button;         // overlay button on the root

    [HideInInspector] public RewardDefinition def;

    private System.Action<RewardCardView> onChosen;
    private GameObject spawnedThumb;

    public void Setup(RewardDefinition def, System.Action<RewardCardView> onChosen)
    {
        this.def = def;
        this.onChosen = onChosen;

        if (debugLabel) debugLabel.text = def.cardData ? def.cardData.cardName : "(null)";

        // Root sizing (safe defaults for a card-like container)
        var rt = GetComponent<RectTransform>();
        if (rt)
        {
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(250, 350);
        }

        // Stretch thumbnail parent to fill the card
        if (thumbnailParent is RectTransform tr)
        {
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = Vector2.zero;
            tr.localScale = Vector3.one;
        }

        // Clear previous thumbnail
        if (spawnedThumb) Destroy(spawnedThumb);

        if (cardThumbnailPrefab && thumbnailParent)
        {
            spawnedThumb = Instantiate(cardThumbnailPrefab, thumbnailParent, false);

            // Keep the button on top
            if (button) button.transform.SetAsLastSibling();

            // Disable raycasts on all thumbnail graphics to avoid stealing clicks
            var graphics = spawnedThumb.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++) graphics[i].raycastTarget = false;

            // Stretch the thumbnail to fill the parent
            var thumbRT = spawnedThumb.GetComponent<RectTransform>();
            if (thumbRT)
            {
                thumbRT.anchorMin = Vector2.zero;
                thumbRT.anchorMax = Vector2.one;
                thumbRT.pivot = new Vector2(0.5f, 0.5f);
                thumbRT.anchoredPosition = Vector2.zero;
                thumbRT.sizeDelta = Vector2.zero;
                thumbRT.localScale = Vector3.one;
            }

            // Bind data to CardDisplay (use existing API)
            var display = spawnedThumb.GetComponentInChildren<CardDisplay>(true);
            if (display != null && def.cardData != null)
            {
                display.cardData = def.cardData;
                display.UpdateCardDisplay();
            }
            else
            {
                Logger.LogWarning("[Reward] CardDisplay not found or cardData missing.", this);
            }
        }

        // Full-rect overlay button
        if (button)
        {
            var brt = button.GetComponent<RectTransform>();
            if (brt)
            {
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.pivot = new Vector2(0.5f, 0.5f);
                brt.anchoredPosition = Vector2.zero;
                brt.sizeDelta = Vector2.zero;
            }
            var img = button.GetComponent<Image>();
            if (img) { var c = img.color; c.a = 0f; img.color = c; }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onChosen?.Invoke(this));
            button.interactable = true;
        }
    }

    public void Interactable(bool value)
    {
        if (button) button.interactable = value;
    }
}
