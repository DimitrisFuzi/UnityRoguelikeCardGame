/// <summary>
/// Interface that defines the basic behavior of any enemy AI.
/// Allows for turn-based decision making and intent prediction.
/// </summary>
public interface IEnemyAI
{
    /// <summary>
    /// Called when it's the enemy's turn to act.
    /// </summary>
    void ExecuteTurn();

    /// <summary>
    /// Predicts what the enemy will do on their next turn.
    /// </summary>
    EnemyIntent PredictNextIntent();

    void SetPlayerStats(CharacterStats player);

}
