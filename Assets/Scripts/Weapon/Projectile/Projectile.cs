using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 3f;
    public int damage = 1;
    private Vector2 moveDirection;
    private Vector2 startPosition;
    private float maxRange;

    // Set the direction of the projectile when it's created
    public void Initialize(Vector2 direction, float attackRange)
    {
        moveDirection = direction.normalized;  // Normalize the direction to avoid inconsistent speed
        startPosition = transform.position;
        maxRange = attackRange;
    }

    void Start()
    {
        // Make sure the projectile faces the direction it’s going when it starts
        RotateProjectile(moveDirection);
    }

    void Update()
    {
        // Move the projectile in the given direction at the specified speed
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Check if the projectile has exceeded the maximum range
        if (Vector2.Distance(startPosition, transform.position) >= maxRange)
        {
            // Destroy the projectile once it exceeds the attack range
            Destroy(gameObject);
            Debug.Log("Projectile exceeded max range and is destroyed.");
        }
    }

    private void RotateProjectile(Vector2 direction)
    {
        // Calculate the angle based on the direction of movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the rotation of the projectile to match the direction it's facing
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy the projectile when it collides with a wall, enemy, or any other object
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);            
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
    }
}
