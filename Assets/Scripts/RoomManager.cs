using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Room Tracking")]
    public List<Room> rooms;
    public Transform player;
    public int currentRoomIndex = -1;
    public float roomCheckInterval = 0.5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private float roomCheckTimer;
    private Dictionary<int, List<EnemyFSM>> roomEnemies;

    public delegate void RoomChangedHandler(int newRoomIndex, int previousRoomIndex);
    public event RoomChangedHandler OnRoomChanged;

    private void Start()
    {
        InitializeRoomTracking();
        roomCheckTimer = roomCheckInterval;
    }

    private void Update()
    {
        roomCheckTimer -= Time.deltaTime;
        if (roomCheckTimer <= 0)
        {
            CheckPlayerRoom();
            roomCheckTimer = roomCheckInterval;
        }
    }

    public void InitializeRoomTracking()
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("RoomManager: No rooms assigned!");
            return;
        }

        roomEnemies = new Dictionary<int, List<EnemyFSM>>();

        // Initialize enemy lists for each room
        for (int i = 0; i < rooms.Count; i++)
        {
            roomEnemies[i] = new List<EnemyFSM>();
        }

        // Find all enemies and assign them to rooms
        EnemyFSM[] allEnemies = FindObjectsOfType<EnemyFSM>();

        foreach (EnemyFSM enemy in allEnemies)
        {
            int roomIndex = GetRoomIndexForPosition(enemy.transform.position);
            if (roomIndex >= 0)
            {
                roomEnemies[roomIndex].Add(enemy);
                enemy.SetRoomIndex(roomIndex);

                if (showDebugLogs)
                {
                    Debug.Log($"Assigned enemy {enemy.name} to room {roomIndex}");
                }
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"RoomManager: Initialized tracking for {rooms.Count} rooms with {allEnemies.Length} total enemies");
        }
    }

    private void CheckPlayerRoom()
    {
        if (player == null) return;

        int newRoomIndex = GetRoomIndexForPosition(player.position);

        if (newRoomIndex != currentRoomIndex)
        {
            int previousRoom = currentRoomIndex;
            currentRoomIndex = newRoomIndex;

            if (showDebugLogs)
            {
                Debug.Log($"Player moved from room {previousRoom} to room {currentRoomIndex}");
            }

            UpdateEnemyStates();
            OnRoomChanged?.Invoke(currentRoomIndex, previousRoom);
        }
    }

    private int GetRoomIndexForPosition(Vector3 position)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].rect.Contains(gridPos))
            {
                return i;
            }
        }

        return -1; // Not in any room (probably in corridor)
    }

    private void UpdateEnemyStates()
    {
        // Set all enemies to idle first
        foreach (var roomEnemyPair in roomEnemies)
        {
            foreach (EnemyFSM enemy in roomEnemyPair.Value)
            {
                if (enemy != null)
                {
                    if (roomEnemyPair.Key == currentRoomIndex)
                    {
                        // Enable enemies in current room
                        enemy.SetRoomActive(true);
                    }
                    else
                    {
                        // Disable enemies in other rooms
                        enemy.SetRoomActive(false);
                    }
                }
            }
        }
    }

    public int GetCurrentRoomIndex()
    {
        return currentRoomIndex;
    }

    public List<EnemyFSM> GetEnemiesInRoom(int roomIndex)
    {
        if (roomEnemies != null && roomEnemies.ContainsKey(roomIndex))
        {
            return roomEnemies[roomIndex];
        }
        return new List<EnemyFSM>();
    }

    public void SetRooms(List<Room> newRooms)
    {
        rooms = newRooms;
        InitializeRoomTracking();
    }

    private void OnDrawGizmosSelected()
    {
        if (rooms == null) return;

        // Draw room boundaries
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            Vector3 center = new Vector3(room.rect.center.x, room.rect.center.y, 0);
            Vector3 size = new Vector3(room.rect.width, room.rect.height, 0.1f);

            // Highlight current room in green, others in blue
            Gizmos.color = i == currentRoomIndex ? Color.green : Color.blue;
            Gizmos.DrawWireCube(center, size);

            // Draw room index
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(center, $"Room {i}");
            #endif
        }
    }
}