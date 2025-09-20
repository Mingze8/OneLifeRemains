using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Loot", menuName = "Loot/WeaponLoot")]
public class WeaponLootSO : LootSO
{
    public WeaponSO[] possibleWeapon;
}