using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Triggers")]
    [Tooltip("Progress gained when completing a room")]
    public int roomCompletionProgress = 1;

    [Tooltip("Progress gained when defeating a boss enemy")]
    public int bossDefeatProgress = 3;

    [Tooltip("Progress gained when opening a loot chest")]
    public int chestOpenedProgress = 0;

    [Tooltip("Progress gained when picking up rare items")]
    public int rareItemProgress = 1;

    [Header("Tracking Stats")]
    [SerializeField] private int roomsCompleted = 0;
    [SerializeField] private int enemiesDefeated = 0;
    [SerializeField] private int chestsOpened = 0;
    [SerializeField] private int rareItemsFound = 0;

    // Start is called before the first frame update
    private void Start()
    {
        RoomManager.OnRoomCompleted += OnRoomCompleted;
        LootChestInteraction.OnChestOpened += OnChestOpened;
        Loot.OnRareItemLooted += OnRareItemLooted;
    }

    private void OnDestroy()
    {
        RoomManager.OnRoomCompleted -= OnRoomCompleted;
        LootChestInteraction.OnChestOpened -= OnChestOpened;
        Loot.OnRareItemLooted -= OnRareItemLooted;
    }

    private void OnRoomCompleted(int roomIndex)
    {
        roomsCompleted++;        

        if (DifficultyManager.Instance != null && roomCompletionProgress > 0)
        {
            DifficultyManager.Instance.IncreaseProgress(roomCompletionProgress);
            Debug.Log($"Room {roomIndex} completed! Total rooms completed: {roomsCompleted}");
        }
    }

    private void OnChestOpened()
    {
        chestsOpened++;

        if (DifficultyManager.Instance != null && chestOpenedProgress > 0)
        {
            DifficultyManager.Instance.IncreaseProgress(chestOpenedProgress);
            Debug.Log($"Chest opened! Progress increased by {chestOpenedProgress}");
        }
    }

    private void OnRareItemLooted(LootSO item)
    {
        // Check if item is rare or above
        if (item.weapon != null && (item.weapon.weaponRarity == Rarity.Rare ||
                                   item.weapon.weaponRarity == Rarity.Epic ||
                                   item.weapon.weaponRarity == Rarity.Legendary))
        {
            rareItemsFound++;

            if (DifficultyManager.Instance != null && rareItemProgress > 0)
            {
                DifficultyManager.Instance.IncreaseProgress(rareItemProgress);
                Debug.Log($"Rare item found: {item.lootName}! Progress increased by {rareItemProgress}");
            }
        }
    }

    public void AddProgress(int amount, string reason = "Manual")
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.IncreaseProgress(amount);
            Debug.Log($"Manual progress added: {amount} ({reason})");
        }
    }

    public void OnBossDefeated(GameObject boss)
    {
        enemiesDefeated++;

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.IncreaseProgress(bossDefeatProgress);
            Debug.Log($"Boss defeated: {boss.name}! Progress increased by {bossDefeatProgress}");
        }
    }

    //public int GetRoomsCompleted() => roomsCompleted;
    //public int GetEnemiesDefeated() => enemiesDefeated;
    //public int GetChestsOpened() => chestsOpened;
    //public int GetRareItemsFound() => rareItemsFound;

    //public void ResetStatistics()
    //{
    //    roomsCompleted = 0;
    //    enemiesDefeated = 0;
    //    chestsOpened = 0;
    //    rareItemsFound = 0;
    //}
}
