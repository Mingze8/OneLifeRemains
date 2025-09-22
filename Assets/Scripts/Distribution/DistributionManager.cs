using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DistributionManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public List<EnemySO> enemies; // List of enemy ScriptableObjects

    [Header("Boss Enemy")]
    public List<EnemySO> bossEnemies; // List of boss enemy

    [Header("Weapon Settings")]
    public List<WeaponSO> weapons;

    [Header("Potion Settings")]
    public List<PotionSO> potions;

    [Header("Loot Chest Settings")]
    public List<LootChestSO> lootChests; // List of loot chest ScriptableObjects

    [Header("Loot Settings")]
    public List<LootSO> loots;

    [Header("Base Spawn Settings")]
    public int baseMinEnemiesPerRoom = 2;
    public int baseMaxEnemiesPerRoom = 4;
    public int baseMinLootChestsPerRoom = 1;
    public int baseMaxLootChestsPerRoom = 2;   

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Base Loot Distribution Settings")]
    public float baseLootChestSpawnChance = 0.5f;

    public int minLootItems = 1;
    public int maxLootItems = 3;

    // Track current scaled values
    private int scaledMinEnemiesPerRoom;
    private int scaledMaxEnemiesPerRoom;
    private float scaledLootChestSpawnChance;

    void Start()
    {        
        UpdateScaledValues();

        // Subscribe to difficulty changes
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }

    private void OnDifficultyChanged(float newMultiplier)
    {
        UpdateScaledValues();
    }

    private void UpdateScaledValues()
    {
        if (DifficultyManager.Instance != null)
        {
            // Update enemy spawn counts
            scaledMinEnemiesPerRoom = Mathf.RoundToInt(baseMinEnemiesPerRoom * DifficultyManager.Instance.GetDifficultyMultiplier());
            scaledMaxEnemiesPerRoom = Mathf.RoundToInt(baseMaxEnemiesPerRoom * DifficultyManager.Instance.GetDifficultyMultiplier());

            // Update loot chest spawn chance
            scaledLootChestSpawnChance = DifficultyManager.Instance.GetScaledLootChestChance(baseLootChestSpawnChance);

            if (DifficultyManager.Instance.showDebugInfo)
            {
                Debug.Log($"Distribution scaling updated: Enemies {scaledMinEnemiesPerRoom}-{scaledMaxEnemiesPerRoom}, " +
                         $"Chest chance: {scaledLootChestSpawnChance:P1}");
            }
        }
        else
        {
            scaledMinEnemiesPerRoom = baseMinEnemiesPerRoom;
            scaledMaxEnemiesPerRoom = baseMaxEnemiesPerRoom;
            scaledLootChestSpawnChance = baseLootChestSpawnChance;
        }
    }

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

        if (room.roomType == RoomType.Shop)
        {
            SpawnLootInShopRoom(validSpawnPositions, roomParent.transform, roomIndex);
        }
        else if(room.roomType == RoomType.Boss)
        {
            SpawnBossInRoom(room, validSpawnPositions, roomParent.transform, roomIndex);
        }
        else
        {
            // Spawn Enemies
            SpawnEnemiesInRooms(room, validSpawnPositions, roomIndex, usedPositions, roomParent.transform);

            // Spawn Loot Chest
            SpawnLootChestInRoom(room, validSpawnPositions, roomIndex, usedPositions, roomParent.transform);            
        }

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


    // -----------------------------------------------   SHOP SPAWNING PART - START  ----------------------------------------------- //

    private void SpawnLootInShopRoom(List<Vector2Int> validSpawnPositions, Transform roomParentTransform, int roomIndex)
    {        
        // Pick 3 random items from LootSO
        List<LootSO> lootItems = new List<LootSO>();
        int lootCount = 0;

        var validLootItems = loots.Where(loot => loot.lootType != LootType.Enemy &&
                                               ((loot.weapon != null && loot.weapon.weaponRarity >= Rarity.Rare) ||
                                                (loot.potion != null && loot.potion.weight >= 1) ||
                                                (loot.weapon != null && loot.weapon.weight >= 1))
                                               ).ToList();

        while (lootCount < 3 && validLootItems.Count > 0)
        {
            LootSO lootItem = WeightedRandom.SelectRandom(validLootItems, loot => loot.weight);
            lootItems.Add(lootItem);
            lootCount++;
        }

        foreach (var loot in lootItems)
        {
            // Choose a valid spawn position
            Vector2Int spawnPos = validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
            validSpawnPositions.Remove(spawnPos);            

            // Instantiate the loot at the position
            GameObject lootObject = Instantiate(loot.lootPrefab, new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.25f, 0), Quaternion.identity);
            lootObject.transform.SetParent(roomParentTransform);

            if (showDebugLogs)
            {
                Debug.Log($"Room {roomIndex + 1}: Spawned loot item {loot.lootName} at position {spawnPos}");
            }
        }
    }

    // -----------------------------------------------   SHOP SPAWNING PART - END  ----------------------------------------------- //


    // -----------------------------------------------   BOSS SPAWNING PART - START  ----------------------------------------------- //

    private void SpawnBossInRoom(Room room, List<Vector2Int> validSpawnPositions, Transform roomParentTransform, int roomIndex)
    {

        // Use weighted random selection to pick a boss enemy
        EnemySO selectedBossEnemy = WeightedRandom.SelectRandom(bossEnemies, enemy => enemy.weight);

        if (selectedBossEnemy != null && selectedBossEnemy.enemyPrefab != null)
        {
            // Spawn the boss at the center of the room
            Vector2Int spawnPos = room.GetCenter();
            Vector3 worldPosition = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.25f, 0);

            // Instantiate the selected boss enemy
            GameObject boss = Instantiate(selectedBossEnemy.enemyPrefab, worldPosition, Quaternion.identity);
            boss.transform.SetParent(roomParentTransform);

            if (showDebugLogs)
            {
                Debug.Log($"Room {roomIndex}: Spawned boss enemy {selectedBossEnemy.enemyName} at position {spawnPos}");
            }
        }
        else
        {
            Debug.LogWarning("Selected boss enemy has no prefab assigned!");
        }                  
    }

    // -----------------------------------------------   BOSS SPAWNING PART - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY SPAWNING PART - START  ----------------------------------------------- //


    // Updated enemy spawning method
    private void SpawnEnemiesInRooms(Room room, List<Vector2Int> validSpawnPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        int enemiesToSpawn;

        if (DifficultyManager.Instance != null)
        {
            enemiesToSpawn = DifficultyManager.Instance.GetScaledEnemySpawnCount(baseMinEnemiesPerRoom, baseMaxEnemiesPerRoom);
        }
        else
        {
            enemiesToSpawn = Random.Range(scaledMinEnemiesPerRoom, scaledMaxEnemiesPerRoom + 1);
        }

        if (showDebugLogs)
        {
            Debug.Log($"Room {roomIndex}: Spawning {enemiesToSpawn} enemies (Difficulty: {DifficultyManager.Instance?.GetDifficultyMultiplier():F2}x)");
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


    // Updated loot chest spawning method
    private void SpawnLootChestInRoom(Room room, List<Vector2Int> validSpawnPositions, int roomIndex, HashSet<Vector2Int> usedPos, Transform roomParentTransform)
    {
        // Use scaled spawn chance
        if (Random.value <= scaledLootChestSpawnChance)
        {
            // Use scaled chest counts
            int lootChestsToSpawn = Random.Range(baseMinLootChestsPerRoom, baseMaxLootChestsPerRoom + 1);

            if (showDebugLogs)
            {
                Debug.Log($"Room {roomIndex}: Spawning {lootChestsToSpawn} loot chests / " +
                         $"Spawn Rate: {scaledLootChestSpawnChance:P1} (Base: {baseLootChestSpawnChance:P1})");
            }

            for (int i = 0; i < lootChestsToSpawn; i++)
            {
                List<Vector2Int> availablePositions = validSpawnPositions.FindAll(pos => !usedPos.Contains(pos));

                if (availablePositions.Count > 0)
                {
                    SpawnLootChestAtPosition(availablePositions, roomIndex, usedPos, roomParentTransform);
                }
            }

            // Reset probability after spawning
            scaledLootChestSpawnChance = DifficultyManager.Instance?.GetScaledLootChestChance(baseLootChestSpawnChance) ?? baseLootChestSpawnChance;
        }
    }

    // Update loot chest spawn chance method
    private void UpdateLootChestSpawnChance()
    {
        if (DifficultyManager.Instance != null)
        {
            float baseIncrease = 0.1f;
            float scaledIncrease = baseIncrease * DifficultyManager.Instance.GetDifficultyMultiplier();
            scaledLootChestSpawnChance = Mathf.Min(scaledLootChestSpawnChance + scaledIncrease, 1.0f);
        }
        else
        {
            if (scaledLootChestSpawnChance < 1.0f)
            {
                scaledLootChestSpawnChance += 0.1f;
            }
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

    // Updated loot generation with rarity scaling
    private GameObject GenerateRandomLootItem()
    {
        LootType selectedLootType = LootSO.SelectRandomLootType();

        // Apply rarity scaling for weapon/equipment drops
        if (DifficultyManager.Instance != null && selectedLootType == LootType.Weapon)
        {
            float rareChance = DifficultyManager.Instance.GetScaledRareLootChance(0.3f); // Base 30% for rare+
            if (Random.value < rareChance)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Rolled for rare loot! Chance was: {rareChance:P1}");
                }
                // You could implement rarity-specific generation here
                // Filter weapons by rarity
                var rareWeapons = weapons.Where(weapon => weapon.weaponRarity != Rarity.Normal).ToList();

                if (rareWeapons.Count > 0)
                {
                    // Generate rare weapon instead of normal one
                    WeaponSO selectedRareWeapon = WeightedRandom.SelectRandom(rareWeapons, weapon => weapon.weight);
                    GameObject rareWeapon = Instantiate(selectedRareWeapon.weaponPrefab);
                    rareWeapon.name = $"WeaponLoot_{selectedRareWeapon.weaponName}";

                    if (showDebugLogs)
                    {
                        Debug.Log($"GenerateRandomLootItem: Rare_Generated weapon loot: {selectedRareWeapon.weaponName} (Weapon Weight: {selectedRareWeapon.weight})");
                    }
                    return rareWeapon;
                }
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"GenerateRandomLootItem: Selected loot type: {selectedLootType}");
        }

        // Handle different loot types
        switch (selectedLootType)
        {
            case LootType.Enemy:
                return GenerateEnemyLootByType();

            case LootType.Weapon:
                return GenerateWeaponLootByType();

            case LootType.Potion:
                return GeneratePotionLootByType();

            default:
                Debug.LogWarning($"GenerateRandomLootItem: Unknown loot type: {selectedLootType}");
                return null;
        }
    }

    // Generate enemy loot without needing a specific LootSO
    private GameObject GenerateEnemyLootByType()
    {
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogError("GenerateEnemyLootByType: No enemies available in the enemies list");
            return null;
        }

        // Use weighted random selection from the global enemies list
        EnemySO selectedEnemy = WeightedRandom.SelectRandom(enemies, enemy => enemy.weight);

        if (selectedEnemy == null || selectedEnemy.enemyPrefab == null)
        {
            Debug.LogError("GenerateEnemyLootByType: Selected enemy is null or has no prefab");
            return null;
        }

        // Create the enemy GameObject
        GameObject spawnedEnemy = Instantiate(selectedEnemy.enemyPrefab);
        spawnedEnemy.name = $"EnemyLoot_{selectedEnemy.enemyName}";

        if (showDebugLogs)
        {
            Debug.Log($"GenerateEnemyLootByType: Spawned {selectedEnemy.enemyName} as loot (Enemy Weight: {selectedEnemy.weight})");
        }

        return spawnedEnemy;
    }

    // Generate weapon loot without needing a specific LootSO
    private GameObject GenerateWeaponLootByType()
    {
        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogError("GenerateWeaponLootByType: No weapons available in the weapons list");
            return null;
        }

        // Use weighted random selection from the global weapons list
        WeaponSO selectedWeapon = WeightedRandom.SelectRandom(weapons, weapon => weapon.weight);

        if (selectedWeapon == null)
        {
            Debug.LogError("GenerateWeaponLootByType: Failed to select weapon using weighted random");
            return null;
        }        

        GameObject spawnedWeapon = Instantiate(selectedWeapon.weaponPrefab);
        spawnedWeapon.name = $"WeaponLoot_{selectedWeapon.weaponName}";        

        if (showDebugLogs)
        {
            Debug.Log($"GenerateWeaponLootByType: Generated weapon loot: {selectedWeapon.weaponName} (Weapon Weight: {selectedWeapon.weight})");
        }

        return spawnedWeapon;
    }

    // Generate weapon loot without needing a specific LootSO
    private GameObject GeneratePotionLootByType()
    {
        if (potions == null || potions.Count == 0)
        {
            Debug.LogError("GeneratePotionLootByType: No potion available in the potion list");
            return null;
        }

        // Use weighted random selection from the global weapons list
        PotionSO selectedPotion = WeightedRandom.SelectRandom(potions, potion => potion.weight);

        if (selectedPotion == null)
        {
            Debug.LogError("GeneratePotionLootByType: Failed to select potion using weighted random");
            return null;
        }

        GameObject spawnedPotion = Instantiate(selectedPotion.potionPrefab);
        spawnedPotion.name = $"PotionLoot_{selectedPotion.potionName}";

        if (showDebugLogs)
        {
            Debug.Log($"GeneratePotionLootByType: Generated potion loot: {selectedPotion.potionName} (Potion Weight: {selectedPotion.weight})");
        }

        return spawnedPotion;
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
