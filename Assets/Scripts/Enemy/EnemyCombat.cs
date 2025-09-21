using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyFSM fsm;
    public int damage = 1;

    [Header("Attack Area Settings")]
    public Transform attackPoint;
    public float weaponRange;
    public float stunTime;
    public LayerMask playerLayer;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    public void Start()
    {
        fsm = GetComponent<EnemyFSM>();
    }

    private void Update()
    {
        attackPoint.position = fsm.GetAttackPoint();
    }    

    public void MeleeAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);

        if (hits.Length > 0) 
        {
            hits[0].GetComponent<PlayerHealth>().changeHealth(-damage);
            hits[0].GetComponent<PlayerController>().Stunned(stunTime);
        }
    }

    public void RangedAttack()
    {
        if (projectilePrefab != null)
        {
            // Get direction from attack point towards player
            Vector2 attackDirection = fsm.GetAttackDirection();
            if (attackDirection == Vector2.zero) return;

            // Spawn projectile at attack point
            GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);

            // Initialize projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(attackDirection, fsm.playerDetectRange, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}
