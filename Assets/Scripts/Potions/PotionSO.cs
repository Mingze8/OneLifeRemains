using UnityEngine;

public enum potionRarity
{
    Normal,
    Rare    
}
public class PotionSO : ScriptableObject
{
    public string potionName;     
    public int weight;
    public GameObject potionPrefab;
    public potionRarity potionRarity;

    public int GetPrice()
    {
        switch (potionRarity)
        {
            case potionRarity.Normal:
                return 10;
            case potionRarity.Rare:
                return 20;
            default:
                return 10;
        }
    }
    public virtual void UsePotion()
    {
        Debug.Log("Potion Used");
    }
}
