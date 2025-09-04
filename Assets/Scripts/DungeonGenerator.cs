using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    public int mapWidth = 100;
    public int mapHeight = 100;
    public int minRoomSize = 10;

    public int offset = 2;
    //public int stepLength = 800;

    [Range(0.3f, 0.8f)]
    public float roomFillRatio = 0.8f; // Target coverage ratio for rooms

    [Range(0.1f, 0.7f)]
    public float centerBias = 0.15f; // Bias toward center

    private List<Room> rooms;

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        tilemap.ClearAllTiles();

        RectInt dungeonArea = new RectInt(0, 0, mapWidth, mapHeight);
        rooms = BSPGenerator.GenerateRooms(dungeonArea, minRoomSize, 8);

        foreach (var room in rooms)
        {
            GenerateRoomLayout(room.rect);
        }

        ConnectRoomsWithCorridors(rooms);
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

    void GenerateRoomLayout(RectInt room)
    {

        RectInt paddedRoom = ApplyOffset(room, offset);
        HashSet<Vector2Int> roomTiles = RandomWalk(paddedRoom, 0);

        foreach (var pos in roomTiles)
        {
            tilemap.SetTile((Vector3Int)pos, floorTile);
        }


        // Check BSP Boundary

        //for (int x = room.xMin; x < room.xMax; x++)
        //{
        //    for (int y = room.yMin; y < room.yMax; y++)
        //    {
        //        tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
        //    }
        //}
    }

    HashSet<Vector2Int> RandomWalk(RectInt room, int steps)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        Vector2Int currentPos = new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );

        // Store roomCenter for bias calculations
        Vector2Int roomCenter = currentPos;

        path.Add(currentPos);

        // Calculate target tiles based on room sizes
        int roomArea = room.width * room.height;
        int targetTiles = Mathf.RoundToInt(roomArea * roomFillRatio);

        Debug.Log($"Room Size: {room.width}x{room.height}, Area: {roomArea}, Target: {targetTiles}");

        int stepCounter = 0;

        for (int i = 0; i < targetTiles && path.Count < targetTiles; i++)
        {

            bool validStep = false;

            for(int attempt = 0; attempt < 5; attempt++)
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
                    stepCounter++;
                    validStep = true;
                    break;
                }
            }

            if(!validStep)
            {
                Debug.Log("Randomwalk: Ended Early");
                break;
            }
            
        }

        Debug.Log($"Randomwalk: Step taken: {stepCounter}, Coverage: {(float)path.Count/roomArea:P1}");
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
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int centerA = rooms[i].GetCenter();
            Vector2Int centerB = rooms[i + 1].GetCenter();

            foreach (var pos in GetLine(centerA, new Vector2Int(centerB.x, centerA.y)))
            {
                tilemap.SetTile((Vector3Int)pos, floorTile);
            }
            foreach (var pos in GetLine(new Vector2Int(centerB.x, centerA.y), centerB))
            {
                tilemap.SetTile((Vector3Int)pos, floorTile);
            }
        }
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

    void OnDrawGizmos()
    {
        if (rooms == null) return;

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            Vector3 center = new Vector3(room.rect.center.x, room.rect.center.y, 0);
            Vector3 size = new Vector3(room.rect.width, room.rect.height, 0.1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
