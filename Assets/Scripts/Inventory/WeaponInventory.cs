using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    // All WeaponSO that player obtained
    public List<WeaponSO> weaponList;

    private int currentWeaponIndex = 0;

    private void Start()
    {
        LoadWeapons();
    }

    private void LoadWeapons()
    {
        weaponList.Clear();
        WeaponSO[] loadedWeapons = Resources.LoadAll<WeaponSO>("WeaponSO");

        if (loadedWeapons.Length > 0)
        {
            weaponList.AddRange(loadedWeapons);
            Debug.Log("Loaded" + loadedWeapons.Length + " weapons into inventory.");
        }
        else
        {
            Debug.LogError("No WeaponSO found in Resources/WeaponSO");
        }
    }

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

    public void SwitchToNextWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponList.Count;
    }

    public void SwitchToPreviousWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex - 1 + weaponList.Count) % weaponList.Count;
    }
}
