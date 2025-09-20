using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WispFloatMotion : MonoBehaviour
{
    [Header("Floating")]
    public float amplitude = 12f;   // vertical offset in canvas units
    public float speed = 2f;        // oscillation speed
    public float randomPhase = 0f;  // phase offset so wisps do not sync

    RectTransform rt;
    Vector2 startAnchored;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startAnchored = rt.anchoredPosition;
        randomPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed + randomPhase) * amplitude;
        rt.anchoredPosition = startAnchored + new Vector2(0f, y);
    }
}
