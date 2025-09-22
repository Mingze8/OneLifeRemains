using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WeaponUIManager : MonoBehaviour
{
    public WeaponInventory weaponInventory;  // Reference to the WeaponInventory
    public Image leftWeaponImage;            // Slot for the previous weapon
    public Image middleWeaponImage;          // Slot for the current weapon
    public Image rightWeaponImage;           // Slot for the next weapon

    public TMP_Text leftWeaponText;          // Name for the previous weapon
    public TMP_Text middleWeaponText;        // Name for the current weapon
    public TMP_Text rightWeaponText;         // Name for the next weapon

    private bool isInitialized = false;      // Flag to check if initialization is done    

    void OnEnable()
    {
        // Subscribe to ammo change event
        RangedWeaponSO.OnAmmoChanged += UpdateWeaponUI;
    }    

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        RangedWeaponSO.OnAmmoChanged -= UpdateWeaponUI;
    }

    public void UpdateWeaponUI(int obj)
    {
        // Get the current, previous, and next weapons
        int currentWeaponIndex = weaponInventory.GetCurrentWeaponIndex();
        WeaponSO currentWeapon = weaponInventory.GetCurrentWeapon();
        WeaponSO previousWeapon = weaponInventory.GetPreviousWeapon();
        WeaponSO nextWeapon = weaponInventory.GetNextWeapon();

        // Update weapon images and names
        UpdateWeaponSlot(leftWeaponImage, leftWeaponText, previousWeapon);
        UpdateWeaponSlot(middleWeaponImage, middleWeaponText, currentWeapon);
        UpdateWeaponSlot(rightWeaponImage, rightWeaponText, nextWeapon);
    }

    void Update()
    {
        // Only try to find the player once
        if (!isInitialized)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                weaponInventory = player.GetComponent<WeaponInventory>();  // Get WeaponInventory from the player
                if (weaponInventory != null)
                {
                    UpdateWeaponUI(0);  // Update the UI with the player's weapon inventory
                    isInitialized = true;  // Mark as initialized
                }
            }
        }        
    }

    // Helper method to update a weapon slot
    public void UpdateWeaponSlot(Image weaponImage, TMP_Text weaponText, WeaponSO weapon)
    {
        if (weapon != null)
        {
            weaponImage.sprite = weapon.weaponIcon;            

            if ( weapon is RangedWeaponSO rangedWeapon)
            {                
                weaponText.text = rangedWeapon.GetCurrentAmmo() + "/" + rangedWeapon.ammoCapacity.ToString();
            }
            else
            {
                weaponText.text = "";
            }
        }
        else
        {
            weaponImage.sprite = null;  // Clear the image if no weapon
            weaponText.text = "None";   // Display "None" if no weapon
        }
    }
}
