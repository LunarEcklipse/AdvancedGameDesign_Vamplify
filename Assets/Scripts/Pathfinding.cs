using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Pathfinding
{
    public static List<PathfindingNode> FindPath(PathfindingNode startNode, PathfindingNode targetNode, CreatureMovementType movementType = CreatureMovementType.All)
    {
        if (startNode == null || targetNode == null) return null; // Return null if either node is null
        if (startNode == targetNode) return null; // Return null if the start and target nodes are the same
        if (!targetNode.IsTraversible(movementType)) return null; // Return null if the target node is not traversible

        List<PathfindingNode> openSet = new(); // Nodes to be evaluated
        HashSet<PathfindingNode> closedSet = new(); // Nodes already evaluated

        Dictionary<PathfindingNode, PathfindingNode> cameFrom = new(); // Track the path
        Dictionary<PathfindingNode, float> gCost = new(); // Cost from start to current node
        Dictionary<PathfindingNode, float> fCost = new(); // Estimated total cost (gCost + hCost)

        // Initialize the start node
        gCost[startNode] = 0;
        fCost[startNode] = GetHeuristic(startNode, targetNode);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get the node in openSet with the lowest fCost
            PathfindingNode currentNode = GetNodeWithLowestFCost(openSet, fCost);

            // If we reached the target node, reconstruct the path
            if (currentNode == targetNode)
            {
                List<PathfindingNode> path = ReconstructPath(cameFrom, currentNode);
                // Check if the every node in the path is traversible. If not, then there is no valid path.
                for (int i = 0; i < path.Count - 1; i++)
                {
                    if (!path[i].IsTraversible(movementType))
                    {
                        return null;
                    }
                }
                return path;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Explore neighbors
            foreach (PathfindingNode neighbor in currentNode.GetNeighbors())
            {
                if (closedSet.Contains(neighbor)) continue; // Skip evaluated nodes

                float tentativeGCost = gCost[currentNode] + neighbor.GetCostOfNode(movementType);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGCost >= gCost[neighbor])
                {
                    continue; // Not a better path
                }

                // Record the best path so far
                cameFrom[neighbor] = currentNode;
                gCost[neighbor] = tentativeGCost;
                fCost[neighbor] = gCost[neighbor] + GetHeuristic(neighbor, targetNode);
            }
        }

        // If we reach here, no path was found
        return null;

    }
    private static PathfindingNode GetNodeWithLowestFCost(List<PathfindingNode> openSet, Dictionary<PathfindingNode, float> fCost)
    {
        PathfindingNode lowestFCostNode = openSet[0];
        float lowestFCost = fCost[lowestFCostNode];

        foreach (PathfindingNode node in openSet)
        {
            if (fCost[node] < lowestFCost)
            {
                lowestFCost = fCost[node];
                lowestFCostNode = node;
            }
        }

        return lowestFCostNode;
    }
    private static float GetHeuristic(PathfindingNode nodeA, PathfindingNode nodeB) { return Mathf.Abs(nodeA.GetX() - nodeB.GetX()) + Mathf.Abs(nodeA.GetZ() - nodeB.GetZ()); }
    private static List<PathfindingNode> ReconstructPath(Dictionary<PathfindingNode, PathfindingNode> cameFrom, PathfindingNode currentNode)
    {
        List<PathfindingNode> path = new();
        while (cameFrom.ContainsKey(currentNode))
        {
            path.Add(currentNode);
            currentNode = cameFrom[currentNode];
        }
        path.Reverse(); // Reverse the path to get from start to target
        return path;
    }
    public static int GetDistance(List<PathfindingNode> path)
    {
        if (path == null || path.Count == 0) return 0;
        return path.Count - 1;
    }

    public static int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB, CreatureMovementType movementType = CreatureMovementType.All)
    {
        List<PathfindingNode> path = FindPath(nodeA, nodeB, movementType);
        return GetDistance(path);
    }
}