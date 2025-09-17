using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public int weight;
    public GameObject enemyPrefab;
}
