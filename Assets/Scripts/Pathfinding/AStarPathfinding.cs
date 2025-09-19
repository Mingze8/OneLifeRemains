using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private HashSet<Vector2Int> allFloorTiles;

    private void Start()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        if (dungeonGenerator != null)
        {
            allFloorTiles = dungeonGenerator.GetAllFloorTiles(); // Access the floor tiles from DungeonGenerator
            Debug.Log($"DungeonGenerator found {allFloorTiles.Count} tiles.");
        }
    }

    // Find the shortest path from start to goal using A* algorithm
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> openList = new List<Vector2Int>(); // Nodes to be evaluated
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>(); // Nodes already evaluated
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // For path reconstruction

        // The cost from start to a given node
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        // The estimated total cost from start to goal through a given node
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        openList.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openList.Count > 0)
        {
            // Get the node with the lowest fScore
            Vector2Int current = GetNodeWithLowestFScore(openList, fScore);

            if (current.Equals(goal)) // Reached the goal
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            // Check each neighbor
            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || !allFloorTiles.Contains(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + Vector2Int.Distance(current, neighbor);

                if (!openList.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // Return an empty path if no path found
        return new List<Vector2Int>();
    }

    // Estimate the heuristic (Manhattan distance)
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Get the neighbors of a node (4 possible directions: up, down, left, right)
    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1), // Up
            new Vector2Int(0, -1), // Down
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = node + dir;
            neighbors.Add(neighbor);
        }

        return neighbors;
    }

    // Get the node with the lowest fScore
    private Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openList, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int lowest = openList[0];
        foreach (Vector2Int node in openList)
        {
            if (fScore.ContainsKey(node) && fScore[node] < fScore[lowest])
            {
                lowest = node;
            }
        }
        return lowest;
    }

    // Reconstruct the path from the cameFrom dictionary
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
