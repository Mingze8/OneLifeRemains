using Unity.VisualScripting;
using UnityEngine;

public enum LootType
{
    Weapon,
    Enemy,
    Potion,
    Coin
}

[CreateAssetMenu(fileName = "New Loot", menuName = "Loot/Possible Loot")]
public class LootSO : ScriptableObject
{
    [Header("General Loot Settings")]
    public string lootName;
    [TextArea]public string description;        
    public Sprite icon;
    public LootType lootType;
    public GameObject lootPrefab;
    public int stackSize = 5;

    [Header("Loot Type Specific Settings")]
    public WeaponSO weapon;
    public EnemySO enemy;
    public PotionSO potion;

    public static LootType SelectRandomLootType()
    {
        System.Array enumValues = System.Enum.GetValues(typeof(LootType));

        int randomIndex = Random.Range(0, enumValues.Length);

        return (LootType)enumValues.GetValue(randomIndex);
    }

    public virtual void Use()
    {
        Debug.Log("Using" + lootName);
    }
}