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

    private PlayerDeck playerDeck;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddDamageDealt(int amount)
    {
        if (amount > 0) totalDamageDealt += amount;
    }

    public void AddDamageTaken(int amount)
    {
        if (amount > 0) totalDamageTaken += amount;
    }

    public void RegisterDeck(PlayerDeck deck)
    {
        playerDeck = deck;
        Logger.Log("[GameSession] PlayerDeck registered.", this);
    }

    public void ResetStats()
    {
        turnsTaken = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;

        if (playerDeck != null)
        {
            playerDeck.InitializeStartingDeck();
            Logger.Log("[GameSession] Deck reset.", this);
        }
        else
        {
            Logger.Log("[GameSession] No PlayerDeck yet. Deck will be reset when battle starts.", this);
        }
    }
}
