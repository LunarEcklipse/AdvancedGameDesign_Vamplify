using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameWorldSpawner : MonoBehaviour
{
    public static GameWorldSpawner Instance;
    public List<GameObject> worldPrefabs = new();

    public GameObject currentRoom;

    /* *** Camera Space Constraint *** */
    public float circleMaxRadius = 0.0f;

    public bool IsCameraWithinCircleRadius(Vector2 position)
    {
        Vector2 circleCenter = new(0.0f, 0.0f);
        return Vector2.Distance(position, circleCenter) <= circleMaxRadius;
    }
    public bool IsCameraWithinCircleRadius(Vector3 position)
    {
        return IsCameraWithinCircleRadius(new Vector2(position.x, position.z));
    }
    public Vector2 GetClosestPositionWithinCircle(Vector2 currentPosition) // Used if the camera is outside the circle. Returns the closest point within the circle.
    {
        if (IsCameraWithinCircleRadius(currentPosition)) return currentPosition;
        Vector2 circleCenter = new(0.0f, 0.0f);
        Vector2 direction = currentPosition - circleCenter;
        direction.Normalize();
        return circleCenter + direction * circleMaxRadius;
    }

    public Vector3 GetClosestPositionWithinCircle(Vector3 currentPosition)
    {
        Vector2 vec2Position = GetClosestPositionWithinCircle(new Vector2(currentPosition.x, currentPosition.z));
        return new Vector3(vec2Position.x, currentPosition.y, vec2Position.y);
    }
    public float DetermineCircleRadiusForCurrentRoom()
    {
        if (currentRoom == null) return 0.0f; // If there's no currentRoom prefab return null
        if (PathNodeManager.Instance.IsNodesEmpty()) return 0.0f; // If there's no nodes return null as we can't determine this.
        Vector2 minPoint = PathNodeManager.Instance.GetPathfindingNodesMinimumCoordsAsVec2();
        Vector2 maxPoint = PathNodeManager.Instance.GetPathfindingNodesMaximumCoordsAsVec2();
        // We need the circle to enclose both the min and max points, then add an additional 3 units to act as a buffer.
        return (Vector2.Distance(minPoint, maxPoint) / 2.0f) + 3.0f;
        
    }
    public void SetCircleMaxRadius(float radius) { circleMaxRadius = radius; }
    /* *** MONOBEHAVIOUR FUNCTIONS *** */
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        worldPrefabs = new(Resources.LoadAll<GameObject>("Rooms")); // Get all the needed resources
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to the CombatManager's GenerateNewRoom event
        CombatManager.Instance.GenerateNewRoom.AddListener(GenerateNewRoom);
    }

    private void OnDestroy()
    {
        CombatManager.Instance.GenerateNewRoom.RemoveListener(GenerateNewRoom);
        Instance = null;
    }

    private void GenerateNewRoom() // Whenever the combat manager calls this
    {
        Debug.Log("Generating new room...");
        // If something already exists in the current room, destroy it
        if (currentRoom != null)
        {
            Debug.Log("Cleaning up old room");
            // Clear all occupants
            TurnOrderManager.Instance.ClearTurnList();
            foreach (PathfindingNode node in PathNodeManager.Instance.GetAllPathfindingNodes())
            {
                // Clear the occupant on the node
                PathNodeManager.Instance.ClearCombatantPosition(node.GetX(), node.GetZ());
                // Clear the combatant
                Destroy(node.gameObject);
            }
            PathNodeManager.Instance.ClearPathfindingNodes();
            Destroy(currentRoom);
        }
        // Get a prefab from the folder at Resources/Rooms
        if (worldPrefabs.Count == 0) // If for some reason they're not loaded then load them
        {
            UnityEngine.Object[] prefabs = Resources.LoadAll("Rooms", typeof(GameObject)); // Get all the needed resources
            foreach (UnityEngine.Object prefab in prefabs)
            {
                worldPrefabs.Add((GameObject)prefab);
            }
            // If the count is still 0 raise an error and return
            if (worldPrefabs.Count == 0)
            {
                Debug.LogError("No prefabs found in Resources/Rooms");
                return;
            }
        }
        GameObject roomPrefab = worldPrefabs[UnityEngine.Random.Range(0, worldPrefabs.Count)];  // Pick a random prefab
        currentRoom = Instantiate(roomPrefab, new Vector3(0, 0, 0), Quaternion.identity); // Instantiate the prefab at 0, 0, 0
        currentRoom.transform.parent = transform; // Set the parent of the room to this object
    }
}
