using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using UnityEditor;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemap;
    public Tilemap wallTilemap;
    public Tilemap corridorTilemap;    
    public TileBase floorTile;
    public TileBase defaultWallTile;

    [Header("Door Objects")]
    public GameObject verticalDoor;
    public GameObject leftDoor;
    public GameObject rightDoor;
    public GameObject doorFolder;

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
    private GameObject playerInstance;
    //public Transform playerTransform;

    public DistributionManager distributionManager;

    private List<Room> rooms;
    private HashSet<Vector2Int> allFloorTiles;
    private HashSet<Vector2Int> allWallTiles;
    private HashSet<Vector2Int> allCorridorTiles;

    private List<GameObject> doorInstances = new List<GameObject>();

    private bool needRegenerate = false;

    void Start()
    {
        GenerateDungeon();
    }

    // main function to generate entire dungon
    void GenerateDungeon()
    {
        allFloorTiles = new HashSet<Vector2Int>();
        allWallTiles = new HashSet<Vector2Int>();
        allCorridorTiles = new HashSet<Vector2Int>();
        
        RectInt dungeonArea = new RectInt(0, 0, mapWidth, mapHeight);
        rooms = BSPGenerator.GenerateRooms(dungeonArea, minRoomSize, 8);       

        Debug.Log($"=== DUNGEON GENERATION STARTED ===");
        Debug.Log($"Target: {rooms.Count} rooms on {mapWidth}x{mapHeight} map");

        Debug.Log($"=== ROOM GENERATION - COVERAGE METRICS ===");
        for (int i = 0; i < rooms.Count; i++)
        {
            GenerateRoomLayout(rooms[i].rect, i);
        }

        bool corridorSuccess = ConnectRoomsWithCorridors(rooms);
        if(!corridorSuccess)
        {
            return;
        }

        GenerateWalls();
        SpawnPlayer();
        
        distributionManager.SpawnContent(rooms, allFloorTiles, offset);

        LogOverallDungeonMetrics();

        Debug.Log($"=== DUNGEON GENERATION COMPLETE ===");
    }    

    // To regenerate dungeon while some condition met
    public void RegenerateDungeon()
    {
        // Clear the current dungeon
        Debug.Log("=== REGENERATING DUNGEON ===");

        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        corridorTilemap.ClearAllTiles();
        ClearAllDoors();

        allFloorTiles.Clear();
        allWallTiles.Clear();
        allCorridorTiles.Clear();

        rooms.Clear();

        distributionManager.ClearAllEnemies();
        distributionManager.ClearAllLootChests();

        GenerateDungeon();
    }

    // -----------------------------------------------   ROOM GENERATION PART - START  ----------------------------------------------- //

    // Creates the floor tiles inside a given partitioned room and apply offset
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

    // Apply offset to a room to avoid room spawned connected
    RectInt ApplyOffset(RectInt room, int offset)
    {
        return new RectInt(
            room.x + offset,
            room.y + offset,
            room.width - offset * 2,
            room.height - offset * 2
        );
    }

    // Generates a random walk within a given room to fill it with walkable tiles + add bias toward center
    // Add bias for more concentrated room shape.
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

    // Determines the direction toward the center of a room from a given position with some random bias
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

    // Return a random direction for random walk generation
    Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        return directions[Random.Range(0, directions.Length)];
    }

    // -----------------------------------------------   ROOM GENERATION PART - END  ----------------------------------------------- //



    // -----------------------------------------------   CORRIDOR GENERATION PART - START  ----------------------------------------------- //

    // Connects rooms with corridors    
    bool ConnectRoomsWithCorridors(List<Room> rooms)
    {
        Debug.Log($"=== CORRIDOR GENERATION ===");        

        HashSet<(int, int)> connections = new HashSet<(int, int)>();
        List<int> connectionOrder = GetConnectionOrder(rooms);

        // Connect rooms with corridors
        for (int i = 0; i < connectionOrder.Count - 1; i++)
        {
            int roomA = connectionOrder[i];
            int roomB = connectionOrder[i + 1];
            connections.Add((Mathf.Min(roomA, roomB), Mathf.Max(roomA, roomB)));
        }

        foreach (var connection in connections)
        {
            Vector2Int centerA = rooms[connection.Item1].GetCenter();
            Vector2Int centerB = rooms[connection.Item2].GetCenter();

            //  Get the edge of the rooms
            List<Vector2Int> roomAEdges = GetRoomEdges(rooms[connection.Item1]);
            List<Vector2Int> roomBEdges = GetRoomEdges(rooms[connection.Item2]);

            Vector2Int start = GetClosestEdge(roomAEdges, centerB);
            Vector2Int end = GetClosestEdge(roomBEdges, centerA);

            GenerateCorridor(start, end);            
        }
        
        // Check if the door generation successfully (if not regenerate dungeon)
        if (needRegenerate)
        {
            Debug.LogWarning("Door placement failed, regenerating dungeon..");
            needRegenerate = false;
            RegenerateDungeon();
            return false;
        }

        Debug.Log($"Connected all corridors and wall");
        return true;
    }

    // Returns an ordered list of rooms
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

    // Find the return the edges of a room based on floor tiles
    List<Vector2Int> GetRoomEdges(Room room)
    {
        List<Vector2Int> edges = new List<Vector2Int>();
       
        foreach (var pos in allFloorTiles)
        {
            if (room.rect.Contains(pos))
            {
                Vector2Int[] directions = {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                };

                foreach (var dir in directions)
                {
                    Vector2Int neighborPos = pos + dir;
                    if (!allFloorTiles.Contains(neighborPos)) // if the neighbor is not a floor tile, it is an edge
                    {
                        edges.Add(pos);
                        break;
                    }
                }
            }
        }

        return edges;
    }

    // Finds the closest edge from a list to a given target point (to find the closest point of two room)
    Vector2Int GetClosestEdge(List<Vector2Int> edges, Vector2Int target)
    {
        Vector2Int closestEdge = edges[0];
        float closestDistance = Vector2Int.Distance(closestEdge, target);

        foreach (var edge in edges)
        {
            float distance = Vector2Int.Distance(edge, target);
            if (distance < closestDistance)
            {
                closestEdge = edge;
                closestDistance = distance;
            }
        }

        return closestEdge;
    }

    // Generates corridor between two points
    void GenerateCorridor(Vector2Int start, Vector2Int end)
    {
        Debug.Log($"Start Position: {start}, End Position: {end}");

        var horizontalLine = GetLine(start, new Vector2Int(end.x, start.y));
        var verticalLine = GetLine(new Vector2Int(end.x, start.y), end);        

        // Check if the corridor is vertical, horizontal, or L-Shaped
        if (start.x == end.x && start.y != end.y)
        {
            Debug.Log($"Vertical Corridor");

            foreach (var pos in verticalLine)
            {
                corridorTilemap.SetTile((Vector3Int)pos, floorTile);
                allCorridorTiles.Add(pos);
            }

            if (start.y <= end.y)
            {
                PlaceDoor(start, Vector2Int.up, Vector2Int.zero, end);
                PlaceDoor(end, Vector2Int.down, Vector2Int.zero, start);
            }
            else 
            {
                PlaceDoor(start, Vector2Int.down, Vector2Int.zero, end);
                PlaceDoor(end, Vector2Int.up, Vector2Int.zero, start);
            }            
        }
        else if (start.y == end.y && start.x != end.x)
        {
            Debug.Log($"Horizontal Corridor");

            foreach (var pos in horizontalLine)
            {
                corridorTilemap.SetTile((Vector3Int)pos, floorTile);
                allCorridorTiles.Add(pos);
            }

            PlaceDoor(start, Vector2Int.right, Vector2Int.zero, end);
            PlaceDoor(end, Vector2Int.left, Vector2Int.zero, start);

        }
        else if (start.x != end.x && start.y != end.y)
        {
            Debug.Log("L-shaped Corridor detected.");            

            // Generate the horizontal part of the corridor
            foreach (var pos in horizontalLine)
            {
                corridorTilemap.SetTile((Vector3Int)pos, floorTile);
                allCorridorTiles.Add(pos);
            }            

            // Generate the vertical part of the corridor
            foreach (var pos in verticalLine)
            {
                corridorTilemap.SetTile((Vector3Int)pos, floorTile);
                allCorridorTiles.Add(pos);
            }

            Vector2Int turnPoint = new Vector2Int(end.x, start.y);  // The point where the two corridors meet
            corridorTilemap.SetTile((Vector3Int)turnPoint, floorTile);
            allCorridorTiles.Add(turnPoint);

            PlaceDoor(start, GetDirectionFromStartToTurn(start, turnPoint), turnPoint, end);
            PlaceDoor(end, GetDirectionFromStartToTurn(end, turnPoint), turnPoint, start);            
        }
       
        Debug.Log($"Generated Corridor from {start} to {end}");
    }

    // Generates a list of positions between two points using Bresenham's line algorithm
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

    // Determines the direction from the given position to the turn point - FOR L-SHAPED CORRIDOR
    Vector2Int GetDirectionFromStartToTurn(Vector2Int start, Vector2Int turn)
    {
        if (start.x < turn.x) return Vector2Int.right;
        if (start.x > turn.x) return Vector2Int.left;
        if (start.y < turn.y) return Vector2Int.up;
        return Vector2Int.down;
    }

    // -----------------------------------------------   CORRIDOR GENERATION PART - END  ----------------------------------------------- //



    // -----------------------------------------------   DOOR GENERATION PART - START  ----------------------------------------------- //

    // Attempts to place door at a connection point between rooms or corridors
    void PlaceDoor(Vector2Int connectionPoint, Vector2Int corridorDirection, Vector2Int turnPoint, Vector2Int destinationPoint)
    {
        Debug.Log($"Attempting to place door at room connection: {connectionPoint}, direction: {corridorDirection}");

        Vector2Int currentPos = connectionPoint;
        Vector2Int currentDirection = corridorDirection;
        int maxAttempts = 5;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Debug.Log($"Door placement attempt {attempt + 1} at position: {currentPos}");

            // Check the pos is within the map and is corridor
            if (IsValidPosition(currentPos) && allCorridorTiles.Contains(currentPos))
            {
                if (IsSuitableForDoorWithNextCheck(currentPos, currentDirection))
                {
                    GameObject selectedDoor = GetDoorTypeByDirection(currentDirection);
                    Vector3 worldPosition = tilemap.CellToWorld(new Vector3Int(currentPos.x, currentPos.y, 0));

                    GameObject door = Instantiate(selectedDoor, worldPosition, Quaternion.identity);
                    door.transform.SetParent(doorFolder.transform);
                    doorInstances.Add(door);

                    Debug.Log($"Successfully placed door at: {currentPos} (attempt {attempt + 1})");
                    return;
                }
                else
                {
                    Debug.Log($"Position {currentPos} not suitable for door, adjusting position...");
                }
            }
            else
            {
                Debug.Log($"Position {currentPos} is not valid or not a corridor tile");
            }

            // Check if need to change direction at turn point
            if (turnPoint != Vector2Int.zero && ShouldChangeDirection(currentPos, currentDirection, turnPoint))
            {
                currentDirection = GetNewDirectionAtTurnPoint(currentPos, turnPoint, connectionPoint, destinationPoint);
                Debug.Log($"Changed direction at turn point. New direction: {currentDirection}");
            }

            // Change direction
            currentPos = AdjustPositionByDirection(currentPos, currentDirection);
        }

        Debug.LogWarning($"Failed to place door after {maxAttempts} attempts. Starting position: {connectionPoint}, Direction: {corridorDirection}");
        needRegenerate = true;
        return;
    }

    bool IsSuitableForDoorWithNextCheck(Vector2Int doorPos, Vector2Int corridorDirection)
    {
        if (!IsSuitableForDoor(doorPos, corridorDirection))
        {
            return false;
        }

        Vector2Int nextPos = AdjustPositionByDirection(doorPos, corridorDirection);

        if (!IsValidPosition(nextPos))
        {
            return false;   
        }

        if (allCorridorTiles.Contains(nextPos))
        {
            if (!IsSuitableForDoor(nextPos, corridorDirection))
            {
                return false;
            }
        }    
        
        return true;
    }

    // Checks if a position is suitable for placing a door by ensuring the surrounding tiles are walls.
    bool IsSuitableForDoor(Vector2Int doorPos, Vector2Int corridorDirection)
    {
        Vector2Int[] wallCheckDirections;

        if (corridorDirection == Vector2Int.up || corridorDirection == Vector2Int.down)
        {
            // Vertical corridor - check left / right
            wallCheckDirections = new Vector2Int[] { Vector2Int.left, Vector2Int.right };
        }
        else
        {
            // Horizontal corridor - check top / bottom
            wallCheckDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down };
        }

        // Check if both left/right or up/down are walls
        foreach (var wallDir in wallCheckDirections)
        {
            Vector2Int wallPos = doorPos + wallDir;

            // Check if the adjacent tile is not part of corridor or floor
            if (allFloorTiles.Contains(wallPos) || allCorridorTiles.Contains(wallPos) || !IsValidPosition(wallPos))
            {
                return false; // position is not wall, door cannot place.
            }
        }

        return true; // both direction arond got walls present
    }

    // Check whether the current position requires a change in direction at a turn point
    bool ShouldChangeDirection(Vector2Int currentPos, Vector2Int currentDirection, Vector2Int turnPoint)
    {
        Vector2Int nextPos = AdjustPositionByDirection(currentPos, currentDirection);

        if (currentPos == turnPoint)
        {
            return true;
        }

        return false;
    }

    // Determines the new direction at a turn point in a corridor
    Vector2Int GetNewDirectionAtTurnPoint(Vector2Int currentPos, Vector2Int turnPoint, Vector2Int originalStart, Vector2Int destinationPoint)
    {
        if (originalStart.y == turnPoint.y)
        {
            if (destinationPoint.y < originalStart.y)
            {
                return Vector2Int.down;
            } 
            else
            {
                return Vector2Int.up;
            }            
        }
        else
        {
            if (destinationPoint.x < originalStart.x)
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.right;
            }
        }
    }

    // Adjusts the current posision by a given direction
    Vector2Int AdjustPositionByDirection(Vector2Int currentPos, Vector2Int direction)
    {        
        if (direction == Vector2Int.up)
        {
            return currentPos + Vector2Int.up;
        }
        else if (direction == Vector2Int.down)
        {
            return currentPos + Vector2Int.down;
        }
        else if (direction == Vector2Int.right)
        {
            return currentPos + Vector2Int.right;
        }
        else if (direction == Vector2Int.left)
        {
            return currentPos + Vector2Int.left;
        }

        return currentPos;
    }

    // Checks if a given position is within the map
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    // Get the tile for door by direction
    GameObject GetDoorTypeByDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up || direction == Vector2Int.down)
        {
            return verticalDoor;
        }            
        else if (direction == Vector2Int.left)
        {
            return leftDoor;
        }            
        else if (direction == Vector2Int.right)
        {
            return rightDoor;
        }

        return verticalDoor;
    }

    // Clear all door
    void ClearAllDoors()
    {
        foreach(GameObject door in doorInstances)
        {
            Destroy(door);
        }

        doorInstances.Clear();
    }

    // -----------------------------------------------   DOOR GENERATION PART - END  ----------------------------------------------- //



    // -----------------------------------------------   WALL GENERATION PART - START  ----------------------------------------------- //


    // Finds and generates wall tiles around the dungeon
    void GenerateWalls()
    {
        Debug.Log($"=== WALL GENERATION ===");

        // Find all wall positions
        FindWallPositions();

        // Place single pattern walls everywhere
        PlaceSinglePatternWalls();

        Debug.Log($"Generated {allWallTiles.Count} wall tiles (single pattern)");
    }

    // Identifies all the positions where walls should be placed based on neighboring floor or corridor tiles
    void FindWallPositions()
    {
        Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

        // Combine both floor and corridor tiles into one set for wall detection
        HashSet<Vector2Int> allWalkableTiles = new HashSet<Vector2Int>(allFloorTiles);
        allWalkableTiles.UnionWith(allCorridorTiles);

        foreach (var floorPos in allWalkableTiles)
        {
            foreach (var direction in directions)
            {
                Vector2Int neighborPos = floorPos + direction;

                if (IsValidPosition(neighborPos) && !allWalkableTiles.Contains(neighborPos))
                {
                    allWallTiles.Add(neighborPos);
                }
            }
        }
    }

    // Places a wall at all the position identified by `FindWallPositions`
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

    // -----------------------------------------------   WALL GENERATION PART - END  ----------------------------------------------- //



    // -----------------------------------------------   PLAYER GENERATION PART - START  ----------------------------------------------- //
    
    // Spawns the player in the first room and setting up the camera to follow player
    void SpawnPlayer()
    {
        Debug.Log($"=== PLAYER SPAWN ===");

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No rooms generated! Cannot spawn player.");
            return;
        }        

        // Get the first room
        Room startingRoom = rooms[0];        

        Vector3 spawnPosition = new Vector3(startingRoom.GetCenter().x, startingRoom.GetCenter().y, 0);        

        if (playerInstance != null)
        {
            Debug.Log("Player existed, moving to new spawn position");
            playerInstance.transform.position = spawnPosition;
        }
        else
        {
            // Spawn player from prefab
            playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Spawned new player in room 0 at position: {spawnPosition}");
        }                        

        SetupCinemachineFollow(playerInstance.transform);
    }    

    // Sets up the Cinemachine camera to follow player
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

    // -----------------------------------------------   PLAYER GENERATION PART - END  ----------------------------------------------- //



    // -----------------------------------------------   DUNGEON QUALITY METRICS & DEBUGGING PART - START  ----------------------------------------------- //

    // Logs various statistics about the dungeon
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

    // Draws visual guides in unity scene view to visualize dungeon structure
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

    // -----------------------------------------------   DUNGEON QUALITY METRICS & DEBUGGING PART - END  ----------------------------------------------- //
}