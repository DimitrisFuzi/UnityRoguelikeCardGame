using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;

public class RewardCardView : MonoBehaviour
{
    [Header("Hook up your thumbnail prefab & parent")]
    public Transform thumbnailParent;        // κενό αντικείμενο μέσα στην κάρτα
    public GameObject cardThumbnailPrefab;   // prefab που περιέχει CardDisplay
    public TMP_Text debugLabel;              // optional label
    public Button button;                    // κουμπί επιλογής (στο root)

    [HideInInspector] public RewardDefinition def;

    private System.Action<RewardCardView> onChosen;
    private GameObject spawnedThumb;

    public void Setup(RewardDefinition def, System.Action<RewardCardView> onChosen)
    {
        this.def = def;
        this.onChosen = onChosen;

        if (debugLabel) debugLabel.text = def.cardData ? def.cardData.cardName : "(null)";

        // 🔧 Root RewardCard: 250x350
        var rt = GetComponent<RectTransform>();
        if (rt)
        {
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(250, 350);
        }

        // 🔧 ThumbnailRoot: stretch να γεμίζει το RewardCard
        if (thumbnailParent is RectTransform tr)
        {
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = Vector2.zero;
            tr.localScale = Vector3.one;
        }

        // Καθάρισε προηγούμενο thumbnail
        if (spawnedThumb) Destroy(spawnedThumb);

        if (cardThumbnailPrefab && thumbnailParent)
        {
            // ✅ instantiate με worldPositionStays = false
            spawnedThumb = Instantiate(cardThumbnailPrefab, thumbnailParent, false);


            if (button) button.transform.SetAsLastSibling();

            // 🛡️ κλείσε όλα τα raycasts στα γραφικά του thumbnail
            var graphics = spawnedThumb.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            for (int i = 0; i < graphics.Length; i++) graphics[i].raycastTarget = false;


            // 🔧 Stretch και το ίδιο το thumbnail
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

            // Δέσε τα δεδομένα
            var display = spawnedThumb.GetComponentInChildren<CardDisplay>(true);
            if (display != null && def.cardData != null)
                display.SetData(def.cardData);
            else
                Debug.LogWarning("[Reward] RewardCardView: Δεν βρέθηκε CardDisplay ή λείπει cardData.");
        }

        // Overlay button full-rect (και ελαφρώς ορατό για debug)
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
            var img = button.GetComponent<UnityEngine.UI.Image>();
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
