using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Weapons/RangedWeapon")]
public class RangedWeaponSO : WeaponSO
{
    public float attackRange;
    public int ammoCapacity;
    public float reloadSpeed;

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        animator.SetTrigger("AttackRanged");
        Debug.Log("Ranged Weapon: " + weaponName);
    }
}