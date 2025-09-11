using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributionManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public List<Enemy> enemies; // List of enemy ScriptableObjects

    [Header("Spawn Settings")]
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 4;    

    [Header("Debug")]
    public bool showDebugLogs = true;
    
    public void SpawnEnemy(List<Room> rooms, HashSet<Vector2Int> allFloorTiles, int offset)
    {
        if (enemies == null || enemies.Count == 0) return;

        if (showDebugLogs)
        {
            Debug.Log($"=== ENEMY SPAWNING IN {rooms.Count} ROOMS ===");

            for (int i = 0; i < rooms.Count; i++)
            {
                SpawnEnemiesInRoom(rooms[i], allFloorTiles, offset, i);
            }
        }
    }

    private void SpawnEnemiesInRoom(Room room, HashSet<Vector2Int> allFloorTiles, int offset, int roomIndex)
    {
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

        // Determine number of enemies to spawn
        int enemiesToSpawn = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

        if (showDebugLogs)
        {
            Debug.Log($"Room {roomIndex}: Spawning {enemiesToSpawn} enemies from {validSpawnPositions.Count} valid positions");
        }

        for (int i = 0; i < enemiesToSpawn; i++ )
        {
            SpawnEnemyAtPosition(validSpawnPositions, roomIndex);
        }
    }

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

    private bool IsPositionInRoom(Vector2Int position, RectInt roomBounds)
    {
        return position.x >= roomBounds.x && position.x < roomBounds.x + roomBounds.width &&
            position.y >= roomBounds.y && position.y < roomBounds.y + roomBounds.height;
    }   

    private void SpawnEnemyAtPosition(List<Vector2Int> validPositions, int roomIndex)
    {
        if (validPositions.Count == 0) return;

        // Select random position and remove it to avoid duplicate spawns
        int randomIndex = Random.Range(0, validPositions.Count);
        Vector2Int spawnPosition = validPositions[randomIndex];
        validPositions.RemoveAt(randomIndex);

        // Select a random enemy based on weight
        Enemy selectedEnemy = WeightedRandom.SelectRandom(enemies, enemy => enemy.weight);

        if (selectedEnemy != null && selectedEnemy.enemyPrefab != null)
        {
            // Convert Vector2Int to Vector3 for world position
            Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y, 0);

            //Instantiate the selected enemy
            GameObject spawnedEnemy = Instantiate(selectedEnemy.enemyPrefab, worldPosition, Quaternion.identity);

            spawnedEnemy.transform.SetParent(transform);            

            if (showDebugLogs)
                Debug.Log($"Room {roomIndex}: Spawned {selectedEnemy.name} at position {spawnPosition}");
        }
        else
        {
            Debug.LogError("Selected enemy is null or has no prefab!");
        }
    }

    public void ClearAlLEnemies()
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
}
