using UnityEngine;
using UnityEngine.UI;

public class RewardUIDebug : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform cardRow;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>(true);
        if (canvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        foreach (var cg in GetComponentsInChildren<CanvasGroup>(true))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        if (cardRow)
        {
            cardRow.anchorMin = Vector2.zero;
            cardRow.anchorMax = Vector2.one;
            cardRow.anchoredPosition = Vector2.zero;
            cardRow.sizeDelta = Vector2.zero;
            cardRow.localScale = Vector3.one;

            var glg = cardRow.GetComponent<GridLayoutGroup>();
            if (!glg) glg = cardRow.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(250, 350);
            glg.spacing = new Vector2(24, 0);
        }

        // Log visible rect
        if (cardRow)
        {
            Vector3[] corners = new Vector3[4];
            cardRow.GetWorldCorners(corners);
            Debug.Log($"[RewardUIDebug] CardRow corners: {corners[0]} .. {corners[2]}");
        }
    }
}
