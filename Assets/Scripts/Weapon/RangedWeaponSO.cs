using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Weapons/RangedWeapon")]
public class RangedWeaponSO : WeaponSO
{
    [Header("Ranged Weapon Stats")]
    public GameObject projectilePrefab;
    public float attackRange;
    public int ammoCapacity;
    public float reloadSpeed;

    private int currentAmmo;
    private bool isReloading;

    public bool IsReloading => isReloading;


    // Start the reload process
    public void StartReloading()
    {
        isReloading = true;
    }

    // Stop the reload process
    public void StopReloading()
    {
        isReloading = false;
    }

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        if (isReloading)
        {
            Debug.Log("Reloading...");
            return;
        }

        if (currentAmmo > 0)
        {
            animator.SetTrigger("AttackRanged");  // Trigger the ranged attack animation
            FireProjectile(player, attackPoint, direction);
            currentAmmo--;  // Decrease ammo after firing
        }
        else
        {
            Debug.Log("Out of ammo! Reloading...");
            ReloadWeapon();
        }

    }

    private void FireProjectile(GameObject player, Transform attackPoint, Vector2 direction)
    {
        // Instantiate the projectile prefab
        GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * attackRange;  // Set the velocity in the firing direction
        }

        projectile.GetComponent<Projectile>().Initialize(direction, attackRange);
        Debug.Log("Fired ranged weapon!");
    }

    private void ReloadWeapon()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        // Use a coroutine to reload
        GameManager.Instance.StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadSpeed);
        currentAmmo = ammoCapacity;  // Refill ammo
        isReloading = false;
        Debug.Log("Reload complete. Ammo refilled.");
    }

    public void SetAmmo(int ammo)
    {
        currentAmmo = ammo;
    }

}