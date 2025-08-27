using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WispFloatMotion : MonoBehaviour
{
    [Header("Floating")]
    public float amplitude = 12f;   // πόσα canvas units πάνω–κάτω
    public float speed = 2f;        // ταχύτητα ταλάντωσης
    public float randomPhase = 0f;  // για να μην κουνιούνται όλα ίδια

    RectTransform rt;
    Vector2 startAnchored;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startAnchored = rt.anchoredPosition;
        // δώσε ένα τυχαίο phase για να μη συγχρονίζονται τα Wisps
        randomPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed + randomPhase) * amplitude;
        rt.anchoredPosition = startAnchored + new Vector2(0f, y);
    }
}
