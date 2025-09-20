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

    void Start()
    {
        handSocket = GameObject.Find("P_Weapon_Right").transform;
        playerController = GetComponent<PlayerController>();
        weaponInventory = GetComponent<WeaponInventory>();
        playerAnimator = GetComponent<Animator>();
        EquipWeapon();        
    }

    // Start is called before the first frame update
    void Update()
    {
        HandleWeaponDirection();

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
            if (currentWeapon != null && timer <= 0)
            {
                currentWeapon.UseWeapon(gameObject, playerAnimator);                
                timer = attackCooldown;
            }
        }
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
        }
        else
        {
            Debug.Log("No Weapon Found");
        }
    }

    private void HandleWeaponDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate direction from player to mouse
        Vector2 direction = (mousePos - transform.position).normalized;

        // Rotate weapon to face the mouse direction
        currentWeaponInstance.transform.up = direction;
    }
}
