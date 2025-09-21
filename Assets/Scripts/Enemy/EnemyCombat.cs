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
    public GameObject magicProjectilePrefab;    

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
        
        // Get direction from attack point towards player
        Vector2 attackDirection = fsm.GetAttackDirection();
        if (attackDirection == Vector2.zero) return;

        // Check if it's a magic attack (you can set some condition for magic attack like a special flag or time)
        if (magicProjectilePrefab != null)
        {
            // Spawn magic projectile
            GameObject magicProjectile = Instantiate(magicProjectilePrefab, attackPoint.position, Quaternion.identity);

            // Initialize the magic projectile
            MagicProjectile magicProjectileScript = magicProjectile.GetComponent<MagicProjectile>();
            if (magicProjectileScript != null)
            {
                magicProjectileScript.Initialize(attackDirection, fsm.playerDetectRange, gameObject);
            }
        }
        else if (projectilePrefab != null) 
        {
            // Spawn regular ranged projectile (normal attack)
            GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);

            // Initialize regular projectile
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
