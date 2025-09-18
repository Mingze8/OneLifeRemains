using UnityEngine;

public class LootSO : ScriptableObject
{
    public string lootName;
    public int weight;
}

[CreateAssetMenu(fileName = "New Enemy Loot", menuName = "Loot/EnemyLoot")]
public class EnemyLootSO : LootSO
{
    public EnemySO[] possibleEnemies;
    public int minEnemyCount;
    public int maxEnemyCount;
}

[CreateAssetMenu(fileName = "New Weapon Loot", menuName = "Loot/WeaponLoot")]
public class WeaponLootSO : LootSO
{
    public WeaponSO[] possibleWeapon;    
}