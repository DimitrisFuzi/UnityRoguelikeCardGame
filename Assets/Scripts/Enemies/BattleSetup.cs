using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // �� ����� ��������� �����
public class BattleSetup : MonoBehaviour
{
    [Tooltip("�� enemy data �� ���� �� ���� �� ����, �� �� ����� spawn.")]
    public List<EnemyData> enemies = new();

    [Tooltip("����������� spawn points. �� ����� ����, �� ���� default layout ���� ��� �� EnemyCanvas.")]
    public List<Transform> spawnPoints = new();
}
