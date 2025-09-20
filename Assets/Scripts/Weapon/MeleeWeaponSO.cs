using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeWeapon", menuName = "Weapons/MeleeWeapon")]
public class MeleeWeaponSO : WeaponSO
{
    public float attackRange;

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        animator.SetTrigger("AttackMelee");

        // 攻击位置基于计算后的 attackPoint
        Vector2 attackPosition = attackPoint.position;

        // 检测敌人是否在攻击范围内
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackPosition, attackRange);

        foreach (var enemy in enemiesInRange)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(baseAttackPower);
            }
        }
    }
}
