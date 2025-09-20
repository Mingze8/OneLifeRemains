using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    // All WeaponSO that player obtained
    public List<WeaponSO> weaponList;

    private int currentWeaponIndex = 0;

    // Start is called before the first frame update
    public WeaponSO GetCurrentWeapon()
    {
        if (weaponList.Count > 0)
        {
            return weaponList[currentWeaponIndex];
        }
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
