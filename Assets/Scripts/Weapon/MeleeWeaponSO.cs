using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Weapons/MeleeWeapon")]
public class MeleeWeaponSO : WeaponSO
{
    public float attackRange;

    public override void UseWeapon(GameObject player, Animator animator)
    {
        animator.SetTrigger("AttackMelee");

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(player.transform.position, attackRange);
        foreach (var enemy in enemiesInRange)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(baseAttackPower);
            }
        }
    }
}