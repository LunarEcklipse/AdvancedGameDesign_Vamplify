using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode : MonoBehaviour
{
    public WalkState walkState = WalkState.Walkable;
    private int x;
    private int z;

    public float baseMoveCost = 1.0f;

    public bool isPlayerSpawn = false;
    public bool isEnemySpawn = false;
    public bool isLootSpawn = false;
    public bool isRoomExit = false;

    public Vector3 GetVector3Position() { return transform.position; }
    public Tuple<int, int> GetCoordinates() { return new Tuple<int, int>(x, z); }
    public int GetX() { return x; }
    public int GetZ() { return z; }
    public List<PathfindingNode> GetNeighbors() { return PathNodeManager.Instance.GetNeighbors(x, z); }
    
    /* *** OCCUPANTS *** */
    public bool IsNodeOccupied() { return PathNodeManager.Instance.IsNodeOccupied(x, z); }
    public Combatant GetOccupant() { return PathNodeManager.Instance.GetCombatantOnNode(this); }
    public void SetOccupant(Combatant newOccupant) { PathNodeManager.Instance.SetCombatantPosition(newOccupant, x, z); }
    public void ClearOccupant() { PathNodeManager.Instance.ClearCombatantPosition(x, z); }
    public bool IsTraversible(CreatureMovementType type)
    {
        if (IsNodeOccupied()) return false;
        return type switch
        {
            // Can move across ground nodes normally and water nodes at double cost
            CreatureMovementType.Ground => walkState == WalkState.Walkable || walkState == WalkState.Swimmable,
            // Can move across water nodes normally and ground nodes at double cost
            CreatureMovementType.Swimming => walkState == WalkState.Walkable || walkState == WalkState.Swimmable,
            // Covers all and flying
            _ => walkState != WalkState.Unwalkable,
        };
    }
    public float GetCostOfNode(CreatureMovementType type)
    {
        if (!IsTraversible(type)) return float.MaxValue;
        return type switch
        {
            CreatureMovementType.Ground => walkState == WalkState.Swimmable ? baseMoveCost * 2 : baseMoveCost,
            CreatureMovementType.Swimming => walkState == WalkState.Walkable ? baseMoveCost * 2 : baseMoveCost,
            _ => baseMoveCost,
        };
    }
    private void OnDrawGizmos()
    {
        switch(walkState)
        {
            case WalkState.Walkable:
                Gizmos.color = Color.green;
                break;
            case WalkState.Unwalkable:
                Gizmos.color = Color.red;
                break;
            case WalkState.Flyable:
                Gizmos.color = Color.yellow;
                break;
            case WalkState.Swimmable:
                Gizmos.color = Color.cyan;
                break;
        }
        if (isPlayerSpawn) Gizmos.color = Color.blue;
        if (isEnemySpawn) Gizmos.color = new Color(0.94510f, 0.35294f, 0.13333f);
        if (isLootSpawn) Gizmos.color = new Color(1.0f, 8431372549019608f, 0.0f);
        if (isRoomExit) Gizmos.color = Color.white;
        if (Application.isPlaying)
        {
            if (IsNodeOccupied()) Gizmos.color = Color.magenta;
        }
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }

    private void Start()
    {
        x = Mathf.RoundToInt(transform.position.x);
        z = Mathf.RoundToInt(transform.position.z);
        this.name = $"PathfindingNode_({x},{z})";
        // Add self to the list of nodes in GSM
        PathNodeManager.Instance.AddNode(x, z, this);
    }

    private void OnDestroy()
    {
        PathNodeManager.Instance.RemoveNodeNoDestroy(this);
    }
}