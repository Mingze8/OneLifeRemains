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
    public int stepLength = 800;

    private List<Room> rooms;

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
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
        HashSet<Vector2Int> roomTiles = RandomWalk(paddedRoom, stepLength);

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

        path.Add(currentPos);

        for (int i = 0; i < steps; i++)
        {
            Vector2Int dir = GetRandomDirection();
            Vector2Int newPos = currentPos + dir;

            if (room.Contains(newPos))
            {
                currentPos = newPos;
                path.Add(currentPos);
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

            // 用不同颜色标记房间
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
