using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum WeaponType
{
    Melee,
    Ranged,
    Magic
}

public enum Rarity
{
    Normal,
    Rare,
    Epic,
    Legendary
}

public abstract class WeaponSO : ScriptableObject
{
    [Header("Weapon Basic Info")]
    public string weaponName;
    public WeaponType weaponType;
    public Sprite weaponIcon;
    public GameObject weaponPrefab;
    public Rarity weaponRarity;
    public int weight;    

    [Header("Weapon Stats")]
    public int baseAttackPower;
    public float attackSpeed;    
    public float critChance;
    public float critDamage;

    public abstract void UseWeapon(GameObject player, Animator animator);
}