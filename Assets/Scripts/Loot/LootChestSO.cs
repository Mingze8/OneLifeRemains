using UnityEngine;

[CreateAssetMenu(fileName = "New Chest", menuName = "Game/Chest")]
public class LootChestSO : ScriptableObject
{
    public string chestName;
    public GameObject chestPrefab;
    public int weight;
}
