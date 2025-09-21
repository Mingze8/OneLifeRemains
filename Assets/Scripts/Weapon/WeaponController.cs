using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private PlayerController playerController;
    private WeaponInventory weaponInventory;
    public WeaponSO currentWeapon;

    private GameObject currentWeaponInstance;
    private Transform handSocket;
    private Animator playerAnimator;

    public float attackCooldown = 0.5f;
    private float timer;

    private float magicCooldownTimer = 0f;

    // Reference to the AttackPoint (for detecting collision)
    public Transform attackPoint;

    // Store the attack position for Gizmos visualization
    private Vector2 attackPosition;
    private Vector2 mouseDirection;
    
    public float horizontalOffset = 0f; // Horizontal offset
    public float verticalOffset = 0.35f; // Vertical offset
    public float attackPointDistance = 3f; // Default distance
    public float minAttackDistance = 1.2f; // Minimum attack distance from player

    void Start()
    {
        handSocket = GameObject.Find("P_Weapon_Right").transform;
        playerController = GetComponent<PlayerController>();
        weaponInventory = GetComponent<WeaponInventory>();
        playerAnimator = GetComponent<Animator>();
        EquipWeapon();

        // Ensure AttackPoint is assigned (find by name or assign in the inspector)
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint");
            if (attackPoint == null)
            {
                Debug.LogError("AttackPoint not found under Player!");
            }
        }
    }

    void Update()
    {
        HandleWeaponDirection();
        
        if (magicCooldownTimer > 0)
        {
            magicCooldownTimer -= Time.deltaTime;  // Decrease cooldown for magic weapon
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            weaponInventory.SwitchToNextWeapon();
            EquipWeapon();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            weaponInventory.SwitchToPreviousWeapon();
            EquipWeapon();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentWeapon is MagicWeaponSO magicWeapon && magicCooldownTimer <= 0)
            {
                // Use the magic weapon and start the cooldown timer
                magicWeapon.UseWeapon(gameObject, playerAnimator, attackPoint, mouseDirection);
                magicCooldownTimer = magicWeapon.spellCooldown;  // Set the cooldown for magic weapon
                timer = attackCooldown;  // General attack cooldown
            }
            else if (!(currentWeapon is MagicWeaponSO))
            {
                // Handle other weapon types (melee, ranged)
                currentWeapon.UseWeapon(gameObject, playerAnimator, attackPoint, mouseDirection);
                timer = attackCooldown;
            }
        }

        //// Update attack position every frame to always show the Gizmos
        //if (currentWeapon != null)
        //{
        //    attackPosition = currentWeapon.GetAttackPosition(gameObject, attackPoint, playerAnimator, mouseDirection);
        //}
    }

    private void EquipWeapon()
    {
        currentWeapon = weaponInventory.GetCurrentWeapon();

        if (currentWeapon != null)
        {
            if (currentWeaponInstance != null)
            {
                Destroy(currentWeaponInstance);
            }

            currentWeaponInstance = Instantiate(currentWeapon.weaponPrefab, handSocket);

            // Initialize ammo for ranged weapons
            if (currentWeapon is RangedWeaponSO rangedWeapon)
            {
                rangedWeapon.SetAmmo(rangedWeapon.ammoCapacity);
                rangedWeapon.StopReloading();
            }            
        }
        else
        {
            Debug.Log("No Weapon Found");
        }
    }

    private void HandleWeaponDirection()
    {        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        mouseDirection = (mousePos - transform.position).normalized;

        currentWeaponInstance.transform.up = mouseDirection;

        float weaponReach = 1f;

        if (currentWeapon is MeleeWeaponSO meleeWeapon)
        {
            weaponReach = meleeWeapon.attackRange;
        }                

        attackPoint.position = transform.position + (Vector3)mouseDirection * weaponReach;
        attackPoint.position += new Vector3(0, verticalOffset, 0);

        attackPosition = attackPoint.position;
    }

    // Visualize the attack position in Scene view
    private void OnDrawGizmos()
    {
        if (currentWeapon is MeleeWeaponSO meleeWeapon)
        {
            // Ensure attackPoint is assigned before drawing Gizmos
            if (attackPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(attackPosition, meleeWeapon.attackRange); // Visualize the attack position
            }
        }
    }
}
