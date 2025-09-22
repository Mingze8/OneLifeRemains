using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Normal,
    Boss,
    Shop,
    Treasure,
    Starting
}

[System.Serializable]
public class Room
{
    public RectInt rect;
    public RoomType roomType;
    public int roomIndex;
    public bool isDeadEnd;
    public List<int> connectedRooms;

    public Room(RectInt rect, RoomType type = RoomType.Normal)
    {
        this.rect = rect;
        this.roomType = type;
        this.connectedRooms = new List<int>();
        this.isDeadEnd = false;
    }

    public Vector2Int GetCenter()
    {
        return new Vector2Int(
            rect.x + rect.width / 2,
            rect.y + rect.height / 2
        );
    }

    public void SetRoomType(RoomType type)
    {
        this.roomType = type;
    }
}
