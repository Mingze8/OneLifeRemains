using UnityEngine;

public class PotionSO : ScriptableObject
{
    public string potionName;     
    public int weight;
    public GameObject potionPrefab;

    public virtual void UsePotion()
    {
        Debug.Log("Potion Used");
    }
}
