using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;    
    public TileBase defaultWallTile;

    [Header("Map Settings")]
    public int mapWidth = 80;
    public int mapHeight = 80;
    public int minRoomSize = 20;
    public int offset = 2;

    [Range(0.3f, 1f)]
    public float roomFillRatio = .9f; // Target coverage ratio for rooms

    [Range(0.1f, 0.7f)]
    public float centerBias = 0.1f; // Bias toward center

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform playerTransform;

    private List<Room> rooms;
    private HashSet<Vector2Int> allFloorTiles;
    private HashSet<Vector2Int> allWallTiles;
    private GameObject playerInstance;

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        allFloorTiles = new HashSet<Vector2Int>();
        allWallTiles = new HashSet<Vector2Int>();

        RectInt dungeonArea = new RectInt(0, 0, mapWidth, mapHeight);
        rooms = BSPGenerator.GenerateRooms(dungeonArea, minRoomSize, 8);

        Debug.Log($"=== DUNGEON GENERATION STARTED ===");
        Debug.Log($"Target: {rooms.Count} rooms on {mapWidth}x{mapHeight} map");
        Debug.Log($"=== ROOM GENERATION - COVERAGE METRICS ===");

        for (int i = 0; i < rooms.Count; i++)
        {
            GenerateRoomLayout(rooms[i].rect, i);
        }

        Debug.Log($"=== CORRIDOR GENERATION ===");
        ConnectRoomsWithCorridors(rooms);

        Debug.Log($"=== WALL GENERATION ===");
        GenerateWalls();

        Debug.Log($"=== PLAYER SPAWN ===");
        SpawnPlayer();

        LogOverallDungeonMetrics();
        Debug.Log($"=== DUNGEON GENERATION COMPLETE ===");
    }

    RectInt ApplyOffset(RectInt room, int offset)
    {
        return new RectInt(
            room.x + offset,
            room.y + offset,
            room.width - offset * 2,
            room.height - offset * 2
        );
    }

    void GenerateRoomLayout(RectInt room, int roomIndex)
    {
        RectInt paddedRoom = ApplyOffset(room, offset);
        HashSet<Vector2Int> roomTiles = RandomWalk(paddedRoom, 0);

        float actualCoverage = (float)roomTiles.Count / (paddedRoom.width * paddedRoom.height);
        Debug.Log($"Room {roomIndex + 1}: Coverage {actualCoverage:P1} (Target: {roomFillRatio:P1}), " +
                  $"Tiles: {roomTiles.Count}/{paddedRoom.width * paddedRoom.height}");

        foreach (var pos in roomTiles)
        {
            tilemap.SetTile((Vector3Int)pos, floorTile);
            allFloorTiles.Add(pos);
        }
    }

    HashSet<Vector2Int> RandomWalk(RectInt room, int steps)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        Vector2Int roomCenter = new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );

        // Calculate target tiles based on room sizes
        int roomArea = room.width * room.height;
        int targetTiles = Mathf.RoundToInt(roomArea * roomFillRatio);

        int walksCount = Mathf.Max(1, roomArea / 200); // 1 walk per 200 tiles
        walksCount = Mathf.Min(walksCount, 6);

        int totalSteps = 0;

        for (int walkNum = 0; walkNum < walksCount; walkNum++)
        {
            Vector2Int currentPos = roomCenter;
            path.Add(currentPos);

            // Each walk gets a portion of the target tiles
            int walkSteps = targetTiles / walksCount;

            for (int i = 0; i < walkSteps && path.Count < targetTiles; i++)
            {
                bool validStep = false;

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    Vector2Int dir;

                    // Use center bias to choose direction
                    if (Random.value < centerBias)
                    {
                        dir = GetDirectionTowardCenter(currentPos, roomCenter);
                    }
                    else
                    {
                        // Move randomly
                        dir = GetRandomDirection();
                    }

                    Vector2Int newPos = currentPos + dir;

                    if (room.Contains(newPos))
                    {
                        currentPos = newPos;
                        path.Add(currentPos);
                        totalSteps++;
                        validStep = true;
                        break;
                    }
                }

                if (!validStep)
                {
                    break;
                }
            }
        }
        return path;
    }

    Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        return directions[Random.Range(0, directions.Length)];
    }

    void ConnectRoomsWithCorridors(List<Room> rooms)
    {
        if (rooms.Count < 2) return;

        HashSet<(int, int)> connections = new HashSet<(int, int)>();
        List<int> connectionOrder = GetConnectionOrder(rooms);

        // Connect rooms with corridors
        for (int i = 0; i < connectionOrder.Count - 1; i++)
        {
            int roomA = connectionOrder[i];
            int roomB = connectionOrder[i + 1];
            connections.Add((Mathf.Min(roomA, roomB), Mathf.Max(roomA, roomB)));
        }

        // Calculate corridor metrics before drawing
        float totalCorridorLength = 0;
        int totalCorridorTiles = 0;

        foreach (var connection in connections)
        {
            Vector2Int centerA = rooms[connection.Item1].GetCenter();
            Vector2Int centerB = rooms[connection.Item2].GetCenter();

            float distance = Vector2Int.Distance(centerA, centerB);
            totalCorridorLength += distance;

            // Count tiles for each corridor segment
            var horizontalLine = GetLine(centerA, new Vector2Int(centerB.x, centerA.y));
            var verticalLine = GetLine(new Vector2Int(centerB.x, centerA.y), centerB);
            totalCorridorTiles += horizontalLine.Count + verticalLine.Count;

            // Draw the corridor
            foreach (var pos in horizontalLine)
            {
                tilemap.SetTile((Vector3Int)pos, floorTile);
                allFloorTiles.Add(pos);
            }
            foreach (var pos in verticalLine)
            {
                tilemap.SetTile((Vector3Int)pos, floorTile);
                allFloorTiles.Add(pos);
            }
        }

        float avgCorridorLength = totalCorridorLength / connections.Count;
        Debug.Log($"Connected {connections.Count} corridors");
        Debug.Log($"Corridor Stats - Total Length: {totalCorridorLength:F1}, Avg: {avgCorridorLength:F1}, Tiles: {totalCorridorTiles}");
    }

    List<int> GetConnectionOrder(List<Room> rooms)
    {
        List<int> order = new List<int>();
        HashSet<int> visited = new HashSet<int>();

        // Start from room 1
        int current = 0;
        order.Add(current);
        visited.Add(current);

        while (visited.Count < rooms.Count)
        {
            float closestDistance = float.MaxValue;
            int closestRoom = -1;

            Vector2Int currentCenter = rooms[current].GetCenter();

            for (int i = 0; i < rooms.Count; i++)
            {
                if (visited.Contains(i)) continue;

                Vector2Int otherCenter = rooms[i].GetCenter();
                float distance = Vector2Int.Distance(currentCenter, otherCenter);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRoom = i;
                }
            }

            if (closestRoom != -1)
            {
                order.Add(closestRoom);
                visited.Add(closestRoom);
                current = closestRoom;
            }
        }

        return order;
    }

    Vector2Int GetDirectionTowardCenter(Vector2Int currentPos, Vector2Int roomCenter)
    {
        Vector2Int diff = roomCenter - currentPos;

        // Normalize to unit direction
        int x = diff.x == 0 ? 0 : (diff.x > 0 ? 1 : -1);
        int y = diff.y == 0 ? 0 : (diff.y > 0 ? 1 : -1);

        // Add randomness to avoid straight line
        if (Random.value < 0.3f)
        {
            if (Random.value < 0.5f) x = Random.Range(-1, 2);
            else y = Random.Range(-1, 2);
        }

        return new Vector2Int(x, y);
    }

    List<Vector2Int> GetLine(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> line = new List<Vector2Int>();
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;

        int err = dx - dy;
        int x = from.x;
        int y = from.y;

        while (x != to.x || y != to.y)
        {
            line.Add(new Vector2Int(x, y));
            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        line.Add(to);
        return line;
    }

    // SIMPLIFIED WALL GENERATION - Just one wall type
    void GenerateWalls()
    {
        // Find all wall positions
        FindWallPositions();

        // Place single pattern walls everywhere
        PlaceSinglePatternWalls();

        Debug.Log($"Generated {allWallTiles.Count} wall tiles (single pattern)");
    }

    void FindWallPositions()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (var floorPos in allFloorTiles)
        {
            foreach (var direction in directions)
            {
                Vector2Int neighborPos = floorPos + direction;

                if (IsValidPosition(neighborPos) && !allFloorTiles.Contains(neighborPos))
                {
                    allWallTiles.Add(neighborPos);
                }
            }
        }
    }

    void PlaceSinglePatternWalls()
    {
        // Get the wall tile to use
        TileBase wallTileToUse = defaultWallTile;

        // Place the same wall tile everywhere
        foreach (var wallPos in allWallTiles)
        {
            wallTilemap.SetTile((Vector3Int)wallPos, wallTileToUse);
        }
    }

    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    void LogOverallDungeonMetrics()
    {
        if (rooms == null || rooms.Count == 0) return;

        // Calculate total metrics
        int totalRoomArea = 0;
        int totalWalkableArea = 0;

        foreach (var room in rooms)
        {
            RectInt paddedRoom = ApplyOffset(room.rect, offset);
            int roomArea = paddedRoom.width * paddedRoom.height;
            int expectedWalkable = Mathf.RoundToInt(roomArea * roomFillRatio);

            totalRoomArea += roomArea;
            totalWalkableArea += expectedWalkable;
        }

        int totalDungeonArea = mapWidth * mapHeight;
        float dungeonDensity = (float)totalWalkableArea / totalDungeonArea;
        float avgRoomSize = (float)totalRoomArea / rooms.Count;

        // Room size analysis
        var roomSizes = new List<int>();
        foreach (var room in rooms)
        {
            RectInt paddedRoom = ApplyOffset(room.rect, offset);
            roomSizes.Add(paddedRoom.width * paddedRoom.height);
        }
        roomSizes.Sort();

        int minRoomArea = roomSizes[0];
        int maxRoomArea = roomSizes[roomSizes.Count - 1];
        int medianRoomArea = roomSizes[roomSizes.Count / 2];

        Debug.Log($"=== OVERALL DUNGEON METRICS ===");
        Debug.Log($"Rooms Generated: {rooms.Count}");
        Debug.Log($"Room Sizes - Min: {minRoomArea}, Max: {maxRoomArea}, Avg: {avgRoomSize:F1}, Median: {medianRoomArea}");
        Debug.Log($"Total Walkable Area: {totalWalkableArea:N0} tiles");
        Debug.Log($"Dungeon Density: {dungeonDensity:P1} ({totalWalkableArea:N0}/{totalDungeonArea:N0})");
        Debug.Log($"Space Efficiency: {((float)totalRoomArea / totalDungeonArea):P1}");
    }

    void SpawnPlayer()
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No rooms generated! Cannot spawn player.");
            return;
        }

        // Get the first room
        Room startingRoom = rooms[0];        

        Vector3 spawnPosition = new Vector3(startingRoom.GetCenter().x, startingRoom.GetCenter().y, 0);

        // Handle player spawning
        if (playerTransform != null)
        {
            // Use existing player
            playerTransform.position = spawnPosition;
            playerInstance = playerTransform.gameObject;
            Debug.Log($"Moved existing player to room 0 at position: {spawnPosition}");
        }
        else if (playerPrefab != null)
        {
            // Spawn new player from prefab
            playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Spawned new player in room 0 at position: {spawnPosition}");
        }
        else
        {
            // Create basic player GameObject
            playerInstance = CreateBasicPlayer(spawnPosition);
            Debug.Log($"Created basic player in room 0 at position: {spawnPosition}");
        }

        SetupCinemachineFollow(playerInstance.transform);
    }

    void SetupCinemachineFollow(Transform target)
    {
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            vcam.Follow = target;            
            Debug.Log("Cinemachine set to follow the player.");
        }
        else
        {
            Debug.LogWarning("No CinemachineVirtualCamera found in the scene!");
        }
    }

    GameObject CreateBasicPlayer(Vector3 position)
    {
        // Create a basic player GameObject
        GameObject player = new GameObject("Player");
        player.transform.position = position;

        // Add visual representation
        SpriteRenderer spriteRenderer = player.AddComponent<SpriteRenderer>();

        // Create a simple colored square sprite
        Texture2D playerTexture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.blue;
        }
        playerTexture.SetPixels(pixels);
        playerTexture.Apply();

        Sprite playerSprite = Sprite.Create(playerTexture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        spriteRenderer.sprite = playerSprite;

        // Add collider
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);

        // Add player controller
        PlayerController controller = player.AddComponent<PlayerController>();

        return player;
    }

    void OnDrawGizmos()
    {
        if (rooms == null) return;

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            Vector3 center = new Vector3(room.rect.center.x, room.rect.center.y, 0);
            Vector3 size = new Vector3(room.rect.width, room.rect.height, 0.1f);

            // Highlight room 0 (starting room) in green
            Gizmos.color = i == 0 ? Color.green : Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}