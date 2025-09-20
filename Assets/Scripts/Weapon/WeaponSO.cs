using UnityEngine;

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

[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Weapons/MeleeWeapon")]
public class MeleeWeaponSO : WeaponSO
{
    public float attackRange;

    public override void UseWeapon(GameObject player, Animator animator)
    {
        animator.SetTrigger("AttackMelee");
        Debug.Log("MeleeWeapon: " + weaponName);
    }
}

[CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Weapons/RangedWeapon")]
public class RangedWeaponSO : WeaponSO
{
    public float attackRange;
    public int ammoCapacity;
    public float reloadSpeed;

    public override void UseWeapon(GameObject player, Animator animator)
    {
        animator.SetTrigger("AttackRanged");
        Debug.Log("Ranged Weapon: " + weaponName);
    }
}

[CreateAssetMenu(fileName = "NewMagicWeapon", menuName = "Weapons/MagicWeapon")]
public class MagicWeaponSO : WeaponSO
{
    public int manaCost;
    public float castTime;

    public override void UseWeapon(GameObject player, Animator animator)
    {
        Debug.Log("Magic Weapon: " + weaponName);
    }
}