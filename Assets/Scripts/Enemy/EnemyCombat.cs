using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private EnemyFSM fsm;

    [Header("Base Combat Stats")]
    public int baseDamage = 1;
    private int scaledDamage;

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

        // Apply difficulty scaling to damage
        if (DifficultyManager.Instance != null)
        {
            scaledDamage = DifficultyManager.Instance.GetScaledEnemyDamage(baseDamage);

            // Subscribe to difficulty changes
            DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
        }
        else
        {
            scaledDamage = baseDamage;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }

    private void OnDifficultyChanged(float newMultiplier)
    {
        // Update scaled damage when difficulty changes
        scaledDamage = DifficultyManager.Instance.GetScaledEnemyDamage(baseDamage);

        if (DifficultyManager.Instance.showDebugInfo)
        {
            Debug.Log($"Enemy {gameObject.name} damage updated: {baseDamage} -> {scaledDamage}");
        }
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
            hits[0].GetComponent<PlayerHealth>().changeHealth(-scaledDamage);
            hits[0].GetComponent<PlayerController>().Stunned(stunTime);
        }
    }

    public void RangedAttack()
    {
        
        // Get direction from attack point towards player
        Vector2 attackDirection = fsm.GetAttackDirection();
        if (attackDirection == Vector2.zero) return;

        // Check if it's a magic attack
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
                // Apply scaled damage to projectile
                projectileScript.damage = scaledDamage;
                projectileScript.Initialize(attackDirection, fsm.playerDetectRange, gameObject);
            }
        }        
    }

    // Getter for current scaled damage
    public int GetCurrentDamage()
    {
        return scaledDamage;
    }

    // Getter for base damage
    public int GetBaseDamage()
    {
        return baseDamage;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}
