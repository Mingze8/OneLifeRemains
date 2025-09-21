using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Weapons/MeleeWeapon")]
public class MeleeWeaponSO : WeaponSO
{
    public float attackRange;

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        animator.SetTrigger("AttackMelee");

        // attack point based on calculated attackPoint
        Vector2 attackPosition = attackPoint.position;
        
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackPosition, attackRange);

        foreach (var enemy in enemiesInRange)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(baseAttackPower);
                break;
            }
        }
    }
}
