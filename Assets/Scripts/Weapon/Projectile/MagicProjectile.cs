using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 2f;
    public int damage = 2;  // Adjust damage as needed    
    public float aoeRadius = 2f;

    private Vector2 moveDirection;
    private Vector2 spawnPosition;
    private float maxRange;  // Maximum range for the projectile

    public GameObject shooter;    

    public void Initialize(Vector2 direction, float attackRange, GameObject firedBy)
    {        
        moveDirection = direction.normalized;  // Normalize the direction to avoid inconsistent speed
        spawnPosition = transform.position;
        maxRange = attackRange;
        shooter = firedBy;
    }

    void Start()
    {
        RotateProjectile(moveDirection);
    }

    private void RotateProjectile(Vector2 direction)
    {
        // Calculate the angle based on the direction of movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the rotation of the projectile to match the direction it's facing
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void Update()
    {
        // Move the projectile in the given direction at the specified speed
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Check if the projectile has exceeded the maximum range
        if (Vector2.Distance(spawnPosition, transform.position) >= maxRange)
        {
            // Destroy the projectile once it exceeds the attack range
            Destroy(gameObject);
            Debug.Log("Projectile exceeded max range and is destroyed.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {        
        // Detect all colliders within AoE radius on collision
        if (collision.gameObject.CompareTag("Wall"))
        {
            Explode();  // Trigger AoE damage on collision with wall
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Explode();  // Trigger AoE damage on collision with enemy
            Destroy(gameObject);  // Destroy the projectile after explosion
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Explode();  // Trigger AoE damage on collision with player
            Destroy(gameObject);  // Destroy the projectile after explosion
        }
    }

    private void Explode()
    {
        // Detect all enemies and players within the AoE radius
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, aoeRadius);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag(shooter.tag))
                continue;

            if (hit.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                hit.GetComponent<EnemyHealth>().TakeDamage(damage);                
            }
            else if (hit.CompareTag("Player"))
            {
                // Apply damage to the player
                hit.GetComponent<PlayerHealth>().changeHealth(-damage);
                hit.GetComponent<PlayerController>().Stunned(0.5f);                
            }
        }
    }
}
