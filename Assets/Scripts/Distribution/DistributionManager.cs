using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DistributionManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public List<EnemySO> enemies; // List of enemy ScriptableObjects

    [Header("Loot Chest Settings")]
    public List<LootChestSO> lootChests; // List of loot chest ScriptableObjects

    [Header("Spawn Settings")]
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 4;

    public int minLootChestsPerRoom = 1;
    public int maxLootChestsPerRoom = 2;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Probability of spawning a loot chest for next room
    private float lootChestSpawnChance = 0.5f;

    [Header("Loot Distribution Settings")]
    public List<LootSO> availableLoot;
    public int minLootItems = 1;
    public int maxLootItems = 3;
    

    // Used iterate through each room and spawn contents in the room
    public void SpawnContent(List<Room> rooms, HashSet<Vector2Int> allFloorTiles, int offset)
    {
        Debug.Log($"=== CONTENT SPAWN ===");

        if (enemies == null || enemies.Count == 0) return;

        if (showDebugLogs)
        {
            Debug.Log($"=== CONTENT SPAWN IN {rooms.Count} ROOMS ===");

            for (int i = 0; i < rooms.Count; i++)
            {
                SpawnContentsInRoom(rooms[i], allFloorTiles, offset, i);
            }
        }
    }

    // Handles spawning of enemies and loot chest for specific room
    // Organizing room content and handle spawn position
    private void SpawnContentsInRoom(Room room, HashSet<Vector2Int> allFloorTiles, int offset, int roomIndex)
    {
        // Create a new GameObject to act as the parent for this room's content
        GameObject roomParent = new GameObject($"Room{roomIndex + 1}");

        // Set the parent as the current transform to group everything in the DistributionManager
        roomParent.transform.SetParent(transform);

        // Get the padded room area
        RectInt paddedRoom = new RectInt(
            room.rect.x + offset,
            room.rect.y + offset,
            room.rect.width - offset * 2,
            room.rect.height - offset * 2
        );

        // Get valid spawn positions within the room
        List<Vector2Int> validSpawnPositions = GetValidSpawnPositionsInRoom(paddedRoom, allFloorTiles);
        
        if (validSpawnPositions.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"Room {roomIndex}: No Valid spawn position found!");
                return;
            }
        }

        // Track used positions to prevent overlap
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        // Spawn Enemies
        SpawnEnemiesInRooms(room, validSpawnPositions, roomIndex, usedPositions, roomParent.transform);

        // Spawn Loot Chest
        SpawnLootChestInRoom(room, validSpawnPositions, roomIndex, usedPositions, roomParent.transform);

        // Update loot chest spawn chance for next room
        UpdateLootChestSpawnChance();        
    }

    // Find the valid spawn position
    private List<Vector2Int> GetValidSpawnPositionsInRoom(RectInt roomBounds, HashSet<Vector2Int> allFloorTiles)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        // Check each position within the room bounds
        for (int x = roomBounds.x; x < roomBounds.x + roomBounds.width; x++)
        {
            for (int y = roomBounds.y; y < roomBounds.y + roomBounds.height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                // Only add positions that have floor tiles
                if (allFloorTiles.Contains(position) && IsPositionInRoom(position, roomBounds))
                {                    
                    validPositions.Add(position); ;                                        
                }
            }
        }

        return validPositions;
    }

    // Check if a given position lies within the room boundary
    private bool IsPositionInRoom(Vector2Int position, RectInt roomBounds)
    {
        return position.x >= roomBounds.x && position.x < roomBounds.x + roomBounds.width &&
            position.y >= roomBounds.y && position.y < roomBounds.y + roomBounds.height;
    }


    // -----------------------------------------------   ENEMY SPAWNING PART - START  ----------------------------------------------- //


    // Spawns a random number of enemies in a room
    private void SpawnEnemiesInRooms(Room room, List<Vector2Int> validSpawnPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        int enemiesToSpawn = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

        if (showDebugLogs)
        {
            Debug.Log($"Room {roomIndex}: Spawning {enemiesToSpawn} enemies");
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            List<Vector2Int> availablePositions = validSpawnPositions.FindAll(pos => !usedPos.Contains(pos));

            if (availablePositions.Count > 0)
            {
                SpawnEnemyAtPosition(availablePositions, roomIndex, usedPos, roomParentTransform);
            }            
        }
    }

    // Instantiates an enemy at a given spawn position within the room based on random selection from the enemies list
    private void SpawnEnemyAtPosition(List<Vector2Int> validPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        if (validPositions.Count == 0) return;

        // Select random position and remove it to avoid duplicate spawns
        int randomIndex = Random.Range(0, validPositions.Count);
        Vector2Int spawnPosition = validPositions[randomIndex];
        validPositions.RemoveAt(randomIndex);

        usedPos.Add(spawnPosition);

        // Select a random enemy based on weight
        EnemySO selectedEnemy = WeightedRandom.SelectRandom(enemies, enemy => enemy.weight);

        if (selectedEnemy != null && selectedEnemy.enemyPrefab != null)
        {
            // Convert Vector2Int to Vector3 for world position
            Vector3 worldPosition = new Vector3(spawnPosition.x + 0.5f, spawnPosition.y + 0.25f, 0);

            //Instantiate the selected enemy
            GameObject spawnedEnemy = Instantiate(selectedEnemy.enemyPrefab, worldPosition, Quaternion.identity);

            // Attach FSM to the enemy
            EnemyFSM enemyFSM = spawnedEnemy.GetComponent<EnemyFSM>();

            spawnedEnemy.transform.SetParent(roomParentTransform);

            //if (showDebugLogs)
            //    Debug.Log($"Room {roomIndex}: Spawned {selectedEnemy.name} at position {spawnPosition}");
        }
        else
        {
            Debug.LogError("Selected enemy is null or has no prefab!");
        }
    }


    // Clear all enemies from the current scene
    public void ClearAllEnemies()
    {
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            } 
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }


    // -----------------------------------------------   ENEMY SPAWNING PART - END  ----------------------------------------------- //


    // -----------------------------------------------   LOOT CHEST SPAWNING PART - START  ----------------------------------------------- //


    // Spawn loot chests in the room based on a random spawn chance, handle number of loot chest spawns
    private void SpawnLootChestInRoom(Room room, List<Vector2Int> validSpawnPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        // Generate a random value to compare with the spawn chance
        if (Random.value <= lootChestSpawnChance)
        {
            // Determine the number of loot chests to spawn
            int lootChestsToSpawn = Random.Range(minLootChestsPerRoom, maxLootChestsPerRoom + 1);

            if (showDebugLogs)
            {
                Debug.Log($"Room {roomIndex}: Spawning {lootChestsToSpawn} loot chests / Spawn Rate: {lootChestSpawnChance}");
            }

            for (int i = 0; i < lootChestsToSpawn; i++)
            {
                List<Vector2Int> availablePositions = validSpawnPositions.FindAll(pos => !usedPos.Contains(pos));

                if (availablePositions.Count > 0)
                {
                    SpawnLootChestAtPosition(availablePositions, roomIndex, usedPos, roomParentTransform);
                }
            }

            // If loot chest is spawned, reset probability
            lootChestSpawnChance = 0.5f;
        }
    }

    // Increase the probability of loot chst spawning for next room
    private void UpdateLootChestSpawnChance()
    {
        if (lootChestSpawnChance < 1.0f)
        {
            lootChestSpawnChance += 0.1f;
        }
    }

    // instantiates a loot chest at a valid positin within the room
    private void SpawnLootChestAtPosition(List<Vector2Int> validPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        if (validPositions.Count == 0) return;

        // Select random position and remove it to avoid duplicate spawns

        int randomIndex = Random.Range(0, validPositions.Count);
        Vector2Int spawnPosition = validPositions[randomIndex];
        validPositions.RemoveAt(randomIndex);

        // Add the position to the used positions set
        usedPos.Add(spawnPosition);

        LootChestSO selectedLootChest = WeightedRandom.SelectRandom(lootChests, chest => chest.weight);

        if (selectedLootChest != null && selectedLootChest.chestPrefab != null)
        {
            // Convert Vector2Int to Vector3 for world position
            Vector3 worldPositon = new Vector3(spawnPosition.x, spawnPosition.y, 0);

            // Instantiate the selected loot chest
            GameObject spawnedLootChest = Instantiate(selectedLootChest.chestPrefab, worldPositon, Quaternion.identity);

            spawnedLootChest.transform.SetParent(roomParentTransform);

            //if (showDebugLogs)
            //{
            //    Debug.Log($"Room {roomIndex}: Spawned loot chest at position {spawnPosition}");
            //}
        }
        else
        {
            Debug.LogError("Selected loot chest is null or has no prefab!");
        }
    }

    // clear all loot chests from the scene
    public void ClearAllLootChests()
    {
        foreach (Transform roomParent in transform)
        {
            foreach (Transform child in roomParent)
            {
                if (child.CompareTag("LootChest"))
                {
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }

    // -----------------------------------------------   LOOT CHEST SPAWNING PART - END  ----------------------------------------------- //


    // -----------------------------------------------  CHEST LOOT DISTRIBUTION PART - START  ----------------------------------------------- //

    // Generates random number of loot when chest is opened
    public List<GameObject> GenerateChestLoot(LootChestSO chestType = null)
    {
        List<GameObject> generatedLoot = new List<GameObject>();

        if (availableLoot == null)
        {
            Debug.LogWarning("GenerateChestLoot: GenerateChestLoot: No loot available");
            return generatedLoot;
        }

        // Determine how many loot items to generate
        int lootItemsToGenerate = Random.Range(minLootItems, maxLootItems + 1);

        if (showDebugLogs)
        {
            Debug.Log($"GenerateChestLoot: Generating {lootItemsToGenerate} loot items from chest");
        }

        // Generate each loot item
        for (int i = 0; i < lootItemsToGenerate; i++)
        {
            GameObject lootItem = GenerateRandomLootItem();
            if (lootItem != null)
            {
                generatedLoot.Add(lootItem);
            }
        }

        return generatedLoot;
    }

    private GameObject GenerateRandomLootItem()
    {
        LootSO selectedLootType = WeightedRandom.SelectRandom(availableLoot, loot => loot.weight);

        if (selectedLootType == null)
        {
            Debug.LogError("GenerateRandomLootItem: Failed to select loot item!");
            return null;
        }

        // Handle different loot types
        switch (selectedLootType)
        {
            case EnemyLootSO enemyLoot:
                return GenerateEnemyLoot(enemyLoot);

            case WeaponLootSO weaponLoot:
                return GenerateWeaponLoot(weaponLoot);

            default:
                Debug.LogWarning($"GenerateRandomLootItem: Unknown loot type: {selectedLootType.GetType().Name}");
                return null;
        }
    }

    // Generate enemy-based loot
    private GameObject GenerateEnemyLoot(EnemyLootSO enemyLoot)
    {
        if (enemyLoot.possibleEnemies == null)
        {
            Debug.LogError("GenerateEnemyLoot: Enemy loot has no possible enemies configured");
            return null;
        }

        // Create a container for this enemy loot
        GameObject enemyLootContainer = new GameObject($"EnemyLoot_{enemyLoot.lootName}");

        //Determine how many enemies to spawn
        int enemyCount = Random.Range(enemyLoot.minEnemyCount, enemyLoot.maxEnemyCount + 1);

        if (showDebugLogs)
        {
            Debug.Log($"GenerateEnemyLoot: Generating {enemyCount} enemies from {enemyLoot.lootName}");
        }

        // Convert array to list for weighted selection
        List<EnemySO> enemyList = new List<EnemySO>(enemyLoot.possibleEnemies);

        for (int i = 0; i < enemyCount; i++)
        {
            // Select enemy based on weight
            EnemySO selectedEnemy = WeightedRandom.SelectRandom(enemyList, enemy => enemy.weight);

            if (selectedEnemy != null && selectedEnemy.enemyPrefab != null)
            {
                // Create the enemy GameObject
                GameObject spawnedEnemy = Instantiate(selectedEnemy.enemyPrefab);
                spawnedEnemy.transform.SetParent(enemyLootContainer.transform);               

                if (showDebugLogs)
                {
                    Debug.Log($"GenerateEnemyLoot: Spawned {selectedEnemy.enemyName} as loot (Weight: {selectedEnemy.weight})");
                }
            }
            else
            {
                Debug.LogError("GenerateEnemyLoot: Selected enemy is null or has no prefab");
            }
        }

        return enemyLootContainer;
    }

    // Generate weapon-based loot using weighted selection
    private GameObject GenerateWeaponLoot(WeaponLootSO weaponLoot)
    {
        if (weaponLoot.possibleWeapon == null)
        {
            Debug.LogError("GenerateWeaponLoot: Weapon loot has no possible weapons configured");
            return null;
        }

        // Convert array to list for weighted selection
        List<WeaponSO> weaponList = new List<WeaponSO>(weaponLoot.possibleWeapon);

        // Select weapon based on weight
        WeaponSO selectedWeapon = WeightedRandom.SelectRandom(weaponList, weapon => weapon.weight);

        if (selectedWeapon == null)
        {
            Debug.LogError("GenerateWeaponLoot: Failed to select weapon based on weight");
            return null;
        }

        // Create weapon loot item using the weapon's prefab
        GameObject weaponLootItem = CreateWeaponLootItem(selectedWeapon);

        if (showDebugLogs && weaponLootItem != null)
        {
            Debug.Log($"GenerateWeaponLoot: Generated weapon loot: {selectedWeapon.weaponName} (Weight: {selectedWeapon.weight})");
        }

        return weaponLootItem;
    }

    // Create a weapon loot item GameObject using the weapon prefab
    private GameObject CreateWeaponLootItem(WeaponSO weaponSO)
    {
        if (weaponSO.weaponPrefab == null)
        {
            Debug.LogError($"CreateWeaponLootItem: Weapon {weaponSO.weaponName} has no prefab assigned!");
            return null;
        }

        // Instantiate the weapon prefab
        GameObject weaponItem = Instantiate(weaponSO.weaponPrefab);
        weaponItem.name = $"WeaponLoot_{weaponSO.weaponName}";

        return weaponItem;
    }

    // Distributes loot at a specific world position
    public void DistributeLootAtPosition(Vector3 chestPosition, LootChestSO chestType = null)
    {
        List<GameObject> lootItems = GenerateChestLoot(chestType);

        if (lootItems.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("DistributeLootAtPosition: No loot generated from chest");
            }
            return;
        }

        // Create a parent object for all loot from this chest
        GameObject lootParent = new GameObject("ChestLoot");
        lootParent.transform.position = chestPosition;

        // Position each loot item around the chest
        for (int i = 0; i < lootItems.Count; i++)
        {
            if (lootItems[i] != null)
            { 
                lootItems[i].transform.position = chestPosition;
                lootItems[i].transform.SetParent(lootParent.transform);
            }
        }       
    }

    public void OnChestOpened(GameObject chest)
    {
        Vector3 chestPosition = chest.transform.position;

        LootChestSO chestData = null;

        DistributeLootAtPosition(chestPosition, chestData);        
    }

    // -----------------------------------------------  CHEST LOOT DISTRIBUTION PART - END  ----------------------------------------------- //
}
