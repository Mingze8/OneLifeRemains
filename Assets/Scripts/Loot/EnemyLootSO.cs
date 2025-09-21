using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Loot", menuName = "Loot/EnemyLoot")]
public class EnemyLootSO : LootSO
{
    public EnemySO[] possibleEnemies;
    public int minEnemyCount;
    public int maxEnemyCount;
}