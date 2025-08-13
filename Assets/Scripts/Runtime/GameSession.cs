using UnityEngine;

/// <summary>
/// Runtime counters and selected encounter. Minimal now; extended in later milestones.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    
    public int turnsTaken;
    public int totalDamageDealt;
    public int totalDamageTaken;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
