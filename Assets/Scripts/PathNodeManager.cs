using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeManager : MonoBehaviour
{
    public static PathNodeManager Instance { get; private set; }
    public Dictionary<Tuple<int, int>, PathfindingNode> nodes = new();
    public Dictionary<Tuple<int, int>, Combatant> combatantPositions = new();

    /* *** PATHFINDING *** */
    public PathfindingNode GetPathfindingNode(int x, int z) { return DoesNodeExist(x, z) ? nodes[new Tuple<int, int>(x, z)] : null; }
    public bool DoesNodeExist(int x, int z) { return nodes.ContainsKey(new Tuple<int, int>(x, z)); }
    public void AddNode(int x, int z, PathfindingNode node)
    {
        if (nodes.ContainsKey(new Tuple<int, int>(x, z)))
        {
            // Destroy the existing node
            PathfindingNode oldNode = nodes[new Tuple<int, int>(x, z)];
            nodes[new Tuple<int, int>(x, z)] = node;
            Destroy(oldNode.gameObject);
        }
        else
        {
            nodes.Add(new Tuple<int, int>(x, z), node);
        }
    }
    public void RemoveNode(int x, int z)
    {
        // Check if the requested node to remove exists. If not, don't do anything.
        if (!nodes.ContainsKey(new Tuple<int, int>(x, z))) return;
        PathfindingNode node = nodes[new Tuple<int, int>(x, z)];
        nodes.Remove(new Tuple<int, int>(x, z));
        Destroy(node.gameObject);
    }
    public void RemoveNode(PathfindingNode node)
    {
        if (!nodes.ContainsKey(new Tuple<int, int>(node.GetX(), node.GetZ()))) return;
        PathfindingNode outNode = nodes[new Tuple<int, int>(node.GetX(), node.GetZ())];
        // Check if both nodes are the same gameobject
        if (outNode == node)
        {
            nodes.Remove(new Tuple<int, int>(node.GetX(), node.GetZ()));
            Destroy(node.gameObject);
        }
    }
    public void RemoveNodeNoDestroy(PathfindingNode node)
    {
        if (!nodes.ContainsKey(new Tuple<int, int>(node.GetX(), node.GetZ()))) return;
        PathfindingNode outNode = nodes[new Tuple<int, int>(node.GetX(), node.GetZ())];
        // Check if both nodes are the same gameobject
        if (outNode == node)
        {
            nodes.Remove(new Tuple<int, int>(node.GetX(), node.GetZ()));
        }
    }
    public void RemoveNodeNoDestroy(int x, int z)
    {
        if (!nodes.ContainsKey(new Tuple<int, int>(x, z))) return;
        nodes.Remove(new Tuple<int, int>(x, z));
    }
    public bool IsNodesEmpty() { return nodes.Count == 0; }
    public bool IsNodeOccupied(int x, int z) { return combatantPositions.ContainsKey(new Tuple<int, int>(x, z)); }
    public WalkState GetNodeWalkState(int x, int z) { return nodes[new Tuple<int, int>(x, z)].walkState; }
    public void SetNodeWalkState(int x, int z, WalkState walkState) { nodes[new Tuple<int, int>(x, z)].walkState = walkState; }
    public List<PathfindingNode> GetNeighbors(int x, int z)
    {
        List<PathfindingNode> neighbors = new();
        if (nodes.ContainsKey(new Tuple<int, int>(x + 1, z))) { neighbors.Add(nodes[new Tuple<int, int>(x + 1, z)]); }
        if (nodes.ContainsKey(new Tuple<int, int>(x - 1, z))) { neighbors.Add(nodes[new Tuple<int, int>(x - 1, z)]); }
        if (nodes.ContainsKey(new Tuple<int, int>(x, z + 1))) { neighbors.Add(nodes[new Tuple<int, int>(x, z + 1)]); }
        if (nodes.ContainsKey(new Tuple<int, int>(x, z - 1))) { neighbors.Add(nodes[new Tuple<int, int>(x, z - 1)]); }
        return neighbors;
    }
    public List<PathfindingNode> GetAllPathfindingNodes()
    {
        List<PathfindingNode> allNodes = new();
        foreach (var node in nodes)
        {
            allNodes.Add(node.Value);
        }
        return allNodes;
    }
    public int GetPathfindingNodesMinimumX()
    {
        int minX = int.MaxValue;
        foreach (var node in nodes)
        {
            if (node.Key.Item1 < minX)
            {
                minX = node.Key.Item1;
            }
        }
        return minX;
    }
    public int GetPathfindingNodesMinimumZ()
    {
        int minY = int.MaxValue;
        foreach (var node in nodes)
        {
            if (node.Key.Item2 < minY)
            {
                minY = node.Key.Item2;
            }
        }
        return minY;
    }
    public int GetPathfindingNodesMaximumX()
    {
        int maxX = int.MinValue;
        foreach (var node in nodes)
        {
            if (node.Key.Item1 > maxX)
            {
                maxX = node.Key.Item1;
            }
        }
        return maxX;
    }
    public int GetPathfindingNodesMaximumZ()
    {
        int maxY = int.MinValue;
        foreach (var node in nodes)
        {
            if (node.Key.Item2 > maxY)
            {
                maxY = node.Key.Item2;
            }
        }
        return maxY;
    }
    public Tuple<int, int> GetPathfindingNodesMinimumCoords() { return new Tuple<int, int>(GetPathfindingNodesMinimumX(), GetPathfindingNodesMinimumZ()); }
    public Vector2 GetPathfindingNodesMinimumCoordsAsVec2() { return new Vector2((float)GetPathfindingNodesMinimumX(), (float)GetPathfindingNodesMinimumZ()); }
    public Tuple<int, int> GetPathfindingNodesMaximumCoords() { return new Tuple<int, int>(GetPathfindingNodesMaximumX(), GetPathfindingNodesMaximumZ()); }
    public Vector2 GetPathfindingNodesMaximumCoordsAsVec2() { return new Vector2((float)GetPathfindingNodesMaximumX(), (float)GetPathfindingNodesMaximumZ());}
    public void ClearPathfindingNodes() { nodes.Clear(); }
    public PathfindingNode GetClosestUnoccupiedNode(PathfindingNode node, CreatureMovementType movementType = CreatureMovementType.All) // Gets the closest node that isn't occupied to another node.
    {
        if (IsNodesEmpty()) return null;
        if (!IsNodeOccupied(node.GetX(), node.GetZ())) return node;
        PathfindingNode closestNode = null;
        int xDiff = 1000000; // Hopefully we don't ever have a map that's this big. Previous choice of maxvalue was causing an integer overflow of all things.
        int zDiff = 1000000;
        foreach (PathfindingNode otherNode in GetAllPathfindingNodes())
        {
            if (IsNodeOccupied(otherNode.GetX(), otherNode.GetZ())) continue;
            int xDiffNew = Mathf.Abs(otherNode.GetX() - node.GetX());
            int zDiffNew = Mathf.Abs(otherNode.GetZ() - node.GetZ());
            if (xDiffNew + zDiffNew < xDiff + zDiff)
            {
                xDiff = xDiffNew;
                zDiff = zDiffNew;
                closestNode = otherNode;
            }
        }
        return closestNode;
    }
    public PathfindingNode GetClosestUnoccupiedNode(Vector3 position, CreatureMovementType movementType = CreatureMovementType.All)
    {
        if (IsNodesEmpty()) return null;
        PathfindingNode closestNode = null;
        int xDiff = 1000000; // Hopefully we don't ever have a map that's this big. Previous choice of maxvalue was causing an integer overflow of all things.
        int zDiff = 1000000;
        foreach (PathfindingNode node in GetAllPathfindingNodes())
        {
            if (IsNodeOccupied(node.GetX(), node.GetZ())) continue;
            int xDiffNew = Mathf.Abs(node.GetX() - Mathf.RoundToInt(position.x));
            int zDiffNew = Mathf.Abs(node.GetZ() - Mathf.RoundToInt(position.z));
            if (xDiffNew + zDiffNew < xDiff + zDiff)
            {
                xDiff = xDiffNew;
                zDiff = zDiffNew;
                closestNode = node;
            }
        }
        return closestNode;
    }
    public PathfindingNode GetClosestTraversiblePathfindingNode(Vector3 position, CreatureMovementType movementType = CreatureMovementType.Ground)
    {
        int xPos = Mathf.RoundToInt(position.x); int zPos = Mathf.RoundToInt(position.z);
        // First, check if the closest node exists directly. If so, just go there.
        if (DoesNodeExist(xPos, zPos))
        {
            PathfindingNode node = GetPathfindingNode(xPos, zPos);
            if (node.IsTraversible(movementType)) { return node; }
        }
        PathfindingNode closestNode = null;
        int xDiff = int.MaxValue;
        int zDiff = int.MaxValue;
        foreach (PathfindingNode node in GetAllPathfindingNodes())
        {
            // Iterate through each node and get the one that's physically closest which meets the criteria
            if (!node.IsTraversible(movementType)) continue;
            int xDiffNew = Mathf.Abs(node.GetX() - xPos);
            int zDiffNew = Mathf.Abs(node.GetZ() - zPos);
            if (xDiffNew + zDiffNew < xDiff + zDiff)
            {
                xDiff = xDiffNew;
                zDiff = zDiffNew;
                closestNode = node;
            }
        }
        return closestNode;
    }
    public PathfindingNode GetPlayerSpawnNode()
    {
        // Iterate through nodes and return the one with the player spawn flag
        foreach (PathfindingNode node in GetAllPathfindingNodes())
        {
            if (node.isPlayerSpawn) return node;
        }
        Debug.LogError("No pathfinding node on this tile has a Player Spawn flag!");
        return null;
    }
    public List<PathfindingNode> GetEnemySpawnNodes()
    {
        List<PathfindingNode> enemyNodes = new();
        foreach (PathfindingNode node in GetAllPathfindingNodes())
        {
            if (node.isEnemySpawn) enemyNodes.Add(node);
        }
        return enemyNodes;
    }
    public PathfindingNode GetClosestNodeToOrigin() // Gets the node closest to 0, 0
    {
        if (IsNodesEmpty()) return null;
        if (DoesNodeExist(0, 0)) return GetPathfindingNode(0, 0);
        PathfindingNode closestNode = null;
        int xDiff = int.MaxValue;
        int zDiff = int.MaxValue;
        foreach (PathfindingNode node in GetAllPathfindingNodes())
        {
            int xDiffNew = Mathf.Abs(node.GetX());
            int zDiffNew = Mathf.Abs(node.GetZ());
            if (xDiffNew + zDiffNew < xDiff + zDiff)
            {
                xDiff = xDiffNew;
                zDiff = zDiffNew;
                closestNode = node;
            }
        }
        return closestNode;
    }
    public PathfindingNode GetFurthestNodeFromOrigin() // Gets the node furthest from 0, 0
    {
        if (IsNodesEmpty()) return null;
        PathfindingNode furthestNode = null;
        foreach(PathfindingNode node in GetAllPathfindingNodes())
        {
            if (furthestNode == null)
            {
                furthestNode = node;
            }
            else
            {
                if (Vector3.Distance(Vector3.zero, node.transform.position) > Vector3.Distance(Vector3.zero, furthestNode.transform.position))
                {
                    furthestNode = node;
                }
            }
        }
        return furthestNode;
    }
    public float GetDistanceFromOriginToFurthestNode()
    {
        PathfindingNode furthestNode = GetFurthestNodeFromOrigin();
        if (furthestNode == null) return 0.0f;
        return Vector3.Distance(Vector3.zero, furthestNode.transform.position);
    }

    /* *** COMBATANTS *** */
    public Combatant GetCombatant(int x, int z)
    {
        if (combatantPositions.ContainsKey(new Tuple<int, int>(x, z))) return combatantPositions[new Tuple<int, int>(x, z)];
        return null;
    }
    public Combatant GetCombatantOnNode(PathfindingNode node)
    {
        Tuple<int, int> position = new(node.GetX(), node.GetZ());
        if (combatantPositions.ContainsKey(position)) return combatantPositions[position];
        return null;
    }

    public void SetCombatantPosition(Combatant combatant, int x, int z)
    {
        Tuple<int, int> position = new(x, z);
        if (combatantPositions.ContainsKey(position))
        {
            combatantPositions[position] = combatant;
        }
        else
        {
            combatantPositions.Add(position, combatant);
        }
    }
    public void ClearCombatantPosition(int x, int z)
    {
        if (combatantPositions.ContainsKey(new Tuple<int, int>(x, z)))
        {
            combatantPositions.Remove(new Tuple<int, int>(x, z));
            // Set the node at this position to unoccupied
        }
    }
    public Tuple<int, int> GetCombatantPosition(Combatant combatant)
    {
        foreach (var position in combatantPositions)
        {
            if (position.Value == combatant)
            {
                return position.Key;
            }
        }
        return null;
    }
    public PathfindingNode GetCombatantNode(Combatant combatant)
    {
        Tuple<int, int> position = GetCombatantPosition(combatant);
        if (position == null) return null;
        return GetPathfindingNode(position.Item1, position.Item2);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


}