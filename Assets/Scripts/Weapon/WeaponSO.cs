using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public int weight;
    public GameObject weaponPrefab;
}
