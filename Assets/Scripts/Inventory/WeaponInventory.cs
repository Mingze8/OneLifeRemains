using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    // All WeaponSO that player obtained
    private WeaponController weaponController;    

    public List<WeaponSO> weaponList;
    public List<WeaponSO> initialWeapon;

    public WeaponUIManager weaponUIManager;

    private int currentWeaponIndex = 0;

    private void Start()
    {         
        weaponUIManager = GameObject.FindAnyObjectByType<WeaponUIManager>();
        weaponController = GetComponent<WeaponController>();

        if (initialWeapon != null)
        {
            foreach (WeaponSO weapon in initialWeapon)
            {
                AddWeapon(weapon);
            }            
        }
        //LoadWeapons();
    }

    //private void LoadWeapons()
    //{
    //    weaponList.Clear();
    //    WeaponSO[] loadedWeapons = Resources.LoadAll<WeaponSO>("WeaponSO");

    //    if (loadedWeapons.Length > 0)
    //    {
    //        weaponList.AddRange(loadedWeapons);
    //        Debug.Log("Loaded" + loadedWeapons.Length + " weapons into inventory.");
    //    }
    //    else
    //    {
    //        Debug.LogError("No WeaponSO found in Resources/WeaponSO");
    //    }
    //}

    // Start is called before the first frame update
    public WeaponSO GetCurrentWeapon()
    {
        if (weaponList.Count > 0)
        {
            Debug.Log("Current Weapon: " + weaponList[currentWeaponIndex]);
            return weaponList[currentWeaponIndex];
        }
        Debug.Log("No Weapon Found");
        return null;
    }

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    public WeaponSO GetPreviousWeapon()
    {
        if (weaponList.Count > 0)
        {
            int previousIndex = (currentWeaponIndex - 1 + weaponList.Count) % weaponList.Count;
            return weaponList[previousIndex];
        }
        return null;
    }

    public WeaponSO GetNextWeapon()
    {
        if (weaponList.Count > 0)
        {
            int nextIndex = (currentWeaponIndex + 1) % weaponList.Count;
            return weaponList[nextIndex];
        }
        return null;
    }

    public void SwitchToNextWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponList.Count;
        weaponUIManager.UpdateWeaponUI(0);
    }

    public void SwitchToPreviousWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex - 1 + weaponList.Count) % weaponList.Count;
        weaponUIManager.UpdateWeaponUI(0);
    }

    public void AddWeapon(WeaponSO weaponSO)
    {
        // Initial the weapon inventory
        if (!weaponList.Exists(weapon => weapon.GetType() == weaponSO.GetType()))
        {
            weaponList.Add(weaponSO);            
        }
        else
        {
            // If the weapon already exists in the list, replace it with the new one
            WeaponSO existingWeapon = weaponList.Find(weapon => weapon.GetType() == weaponSO.GetType());
            if (existingWeapon != null)
            {
                int index = weaponList.IndexOf(existingWeapon);
                weaponList[index] = weaponSO;
                weaponUIManager.UpdateWeaponUI(0);
                weaponController.EquipWeapon();
            }
        }
    }
}
