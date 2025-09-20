using UnityEngine;

[CreateAssetMenu(fileName = "NewMagicWeapon", menuName = "Weapons/MagicWeapon")]
public class MagicWeaponSO : WeaponSO
{
    public int manaCost;
    public float castTime;

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        Debug.Log("Magic Weapon: " + weaponName);
    }
}