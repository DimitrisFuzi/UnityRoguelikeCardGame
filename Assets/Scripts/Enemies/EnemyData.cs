using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public Sprite enemySprite;
    public Vector2 position;
    public Vector2 size;
}
