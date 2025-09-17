using UnityEngine;

public class Room
{
    public RectInt rect;

    public Room(RectInt rect)
    {
        this.rect = rect;
    }

    public Vector2Int GetCenter()
    {
        return new Vector2Int(
            rect.x + rect.width / 2,
            rect.y + rect.height / 2
        );
    }
}
