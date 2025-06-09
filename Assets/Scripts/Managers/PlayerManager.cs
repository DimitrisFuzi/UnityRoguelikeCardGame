using UnityEngine;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;
using System.Collections.Generic;
using System.Linq;
using MyProjectF.Assets.Scripts.Effects;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Logger.LogWarning("Duplicate PlayerManager detected. Destroying duplicate.", this);
            Destroy(gameObject);
        }
    }

    public void InitializePlayer()
    {
        if (playerPrefab == null)
        {
            Logger.LogError("PlayerManager: playerPrefab is null! Please assign it in the Inspector.", this);
            return;
        }

        GameObject playerCanvas = GameObject.Find("PlayerCanvas");
        if (playerCanvas == null)
        {
            Logger.LogError("PlayerCanvas GameObject not found in scene!", this);
            return;
        }

        GameObject playerObject = Instantiate(playerPrefab, playerCanvas.transform, false);

        if (playerObject == null)
        {
            Logger.LogError("PlayerManager: Failed to instantiate playerPrefab!", this);
            return;
        }

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Logger.LogError("PlayerManager: playerPrefab does not have a PlayerStats component.", this);
            return;
        }

        playerStats.ResetEnergy();
        playerStats.ResetArmor();

        Logger.Log("PlayerManager: Player initialized successfully.", this);
    }


    public bool CanPlayCard(Card card)
    {
        bool canPlay = PlayerStats.Instance.energy >= card.energyCost;
        Logger.Log($"CanPlayCard check: card '{card.cardName}' costs {card.energyCost} energy. Player has {PlayerStats.Instance.energy}. Can play: {canPlay}", this);
        return canPlay;
    }

    public void UseCard(Card card)
    {
        if (!CanPlayCard(card))
        {
            Logger.LogWarning("Not enough energy to play this card.", this);
            return;
        }

        PlayerStats.Instance.UseEnergy(card.energyCost);
        Logger.Log($"Played card '{card.cardName}'.", this);
    }

    public void ApplyCardEffect(Enemy targetEnemy, EffectData effect, Card card)
    {
        if (effect == null)
        {
            Logger.LogError("EffectData is null.", this);
            return;
        }

        switch (card.targetType)
        {
            case Card.TargetType.SingleEnemy:
                if (targetEnemy != null)
                {
                    effect.ApplyEffect(PlayerStats.Instance, targetEnemy);
                    Logger.Log($"Applied effect of '{card.cardName}' to single enemy '{targetEnemy.name}'.", this);
                }
                else
                {
                    Logger.LogError("Card requires an enemy target, but none was provided.", this);
                }
                break;

            case Card.TargetType.AllEnemies:
                foreach (Enemy enemy in EnemyManager.Instance.GetActiveEnemies())
                {
                    effect.ApplyEffect(PlayerStats.Instance, enemy);
                }
                Logger.Log($"Applied effect of '{card.cardName}' to all enemies.", this);
                break;

            case Card.TargetType.Self:
                effect.ApplyEffect(PlayerStats.Instance, PlayerStats.Instance);
                Logger.Log($"Applied effect of '{card.cardName}' to self.", this);
                break;

            case Card.TargetType.AllAllies:
                foreach (PlayerStats ally in GetAllies())
                {
                    effect.ApplyEffect(PlayerStats.Instance, ally);
                }
                Logger.Log($"Applied effect of '{card.cardName}' to all allies.", this);
                break;

            default:
                Logger.LogWarning("Unknown card target type.", this);
                break;
        }
    }

    public List<PlayerStats> GetAllies()
    {
        List<PlayerStats> allies = new List<PlayerStats>();

        if (PlayerStats.Instance != null)
        {
            allies.Add(PlayerStats.Instance);
        }

        // Add other allies if applicable

        return allies;
    }
}
