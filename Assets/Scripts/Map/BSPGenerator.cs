using System.Collections.Generic;
using UnityEngine;

public class BSPGenerator
{
    public static List<Room> GenerateRooms(RectInt rootArea, int minRoomSize, int targetRoomCount = 8)
    {
        Queue<RectInt> roomsToSplit = new Queue<RectInt>();
        List<Room> finalRooms = new List<Room>();
        roomsToSplit.Enqueue(rootArea);

        while (roomsToSplit.Count > 0 && finalRooms.Count + roomsToSplit.Count < targetRoomCount)
        {
            var room = roomsToSplit.Dequeue();

            if (room.width >= minRoomSize * 2 || room.height >= minRoomSize * 2)
            {
                bool splitHorizontally = room.width < room.height;
                if (room.width > room.height)
                    splitHorizontally = false;

                if (splitHorizontally)
                {
                    int splitY = Random.Range(minRoomSize, room.height - minRoomSize);
                    var top = new RectInt(room.x, room.y + splitY, room.width, room.height - splitY);
                    var bottom = new RectInt(room.x, room.y, room.width, splitY);
                    roomsToSplit.Enqueue(top);
                    roomsToSplit.Enqueue(bottom);
                }
                else
                {
                    int splitX = Random.Range(minRoomSize, room.width - minRoomSize);
                    var left = new RectInt(room.x, room.y, splitX, room.height);
                    var right = new RectInt(room.x + splitX, room.y, room.width - splitX, room.height);
                    roomsToSplit.Enqueue(left);
                    roomsToSplit.Enqueue(right);
                }
            }
            else
            {
                finalRooms.Add(new Room(room));
            }
        }

        while (roomsToSplit.Count > 0 && finalRooms.Count < targetRoomCount)
        {
            finalRooms.Add(new Room(roomsToSplit.Dequeue()));
        }

        return finalRooms;
    }
}
