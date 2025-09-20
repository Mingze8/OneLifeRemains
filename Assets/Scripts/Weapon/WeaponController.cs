using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponInventory weaponInventory;
    public WeaponSO currentWeapon;    

    private GameObject currentWeaponInstance;
    private Transform handSocket;
    private Animator playerAnimator;


    void Start()
    {
        handSocket = GameObject.Find("P_Weapon_Right").transform;
        weaponInventory = GetComponent<WeaponInventory>();
        playerAnimator = GetComponent<Animator>();
        EquipWeapon();        
    }

    // Start is called before the first frame update
    void Update()
    {
        HandleWeaponDirection();

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
            if (currentWeapon != null)
            {
                currentWeapon.UseWeapon(gameObject, playerAnimator);
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
