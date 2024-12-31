using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawningManager : MonoBehaviour
{
    public static EnemySpawningManager Instance;
    private List<GameObject> masterEnemyList = new();

    private List<GameObject> LoadEnemiesFromResources() { return new List<GameObject>(Resources.LoadAll<GameObject>("Enemies")); }

    /* *** ENEMY SPAWNING ALGORITHMS *** */

    private Dictionary<PathfindingNode, GameObject> GenerateRoomEnemiesPrioritizeCount() // Generates a list of enemies to spawn in the room. Prioritizes filling as many nodes as possible, but cannot exceed the player's level.
    {
        Dictionary<PathfindingNode, GameObject> enemySpawnList = new();
        List<PathfindingNode> validEnemySpawns = PathNodeManager.Instance.GetEnemySpawnNodes();
        int numSpawnNodes = validEnemySpawns.Count;
        List<GameObject> gameObjects = GetValidEnemyList();
        // If there are no enemy spawn nodes, return an empty list.
        if (validEnemySpawns.Count == 0) return enemySpawnList;
        // If there are no enemies to spawn, return an empty list.
        if (gameObjects.Count == 0) return enemySpawnList;
        float remainingLevel = PlayerController.Instance.combatant.level;
        List<Tuple<GameObject, Combatant, EnemyData>> enemyDataList = new();
        foreach (GameObject enemy in gameObjects)
        {
            if (enemy.TryGetComponent(out Combatant combatant))
            {
                if (enemy.TryGetComponent(out EnemyData enemyData))
                {
                    enemyDataList.Add(new Tuple<GameObject, Combatant, EnemyData>(enemy, combatant, enemyData));
                }
                else
                {
                    Debug.LogError("Enemy prefab " + enemy.name + " does not have an EnemyData component attached to it!");
                    continue;
                }
            }
            else
            {
                Debug.LogError("Enemy prefab " + enemy.name + " does not have a Combatant component attached to it!");
                continue;
            }
        }
        // Sort the list by the enemy's level
        enemyDataList = enemyDataList.OrderBy(x => x.Item2.level).ToList();
        int randomSeed = DateTime.Now.Millisecond; // Initial random seed
        while (numSpawnNodes > 0 && remainingLevel > 0.0f)
        {
            UnityEngine.Random.InitState(randomSeed);
            List<Tuple<GameObject, Combatant, EnemyData>> validEnemies = enemyDataList
                .Where(x => x.Item2.level <= remainingLevel)
                .OrderBy(x => UnityEngine.Random.value)
                .ToList();
            if (validEnemies.Count == 0) break; // Break if there are no enemies that can spawn by these requirements left
            
            Tuple<GameObject, Combatant, EnemyData> enemyData = validEnemies[0]; // Get the first enemy in the list
            PathfindingNode spawnNode = validEnemySpawns[UnityEngine.Random.Range(0, validEnemySpawns.Count)];
            enemySpawnList.Add(spawnNode, enemyData.Item1);
            validEnemySpawns.Remove(spawnNode);
            numSpawnNodes--;
            remainingLevel -= enemyData.Item2.level;
            randomSeed += (spawnNode.GetX() + spawnNode.GetZ());
        }

        return enemySpawnList;
    }

    private Dictionary<PathfindingNode, GameObject> GenerateRoomEnemiesPrioritizeLevel() // Generates a list of enemies to spawn in the room. Prioritizes the highest level enemies first.
    {
        Dictionary<PathfindingNode, GameObject> enemySpawnList = new();
        List<PathfindingNode> validEnemySpawns = PathNodeManager.Instance.GetEnemySpawnNodes();
        int numSpawnNodes = validEnemySpawns.Count;
        List<GameObject> gameObjects = GetValidEnemyList();
        if (validEnemySpawns.Count == 0) return enemySpawnList;
        if (gameObjects.Count == 0) return enemySpawnList;
        float remainingLevel = PlayerController.Instance.combatant.level;
        List<Tuple<GameObject, Combatant, EnemyData>> enemyDataList = new();
        foreach (GameObject enemy in gameObjects)
        {
            if (enemy.TryGetComponent(out Combatant combatant))
            {
                if (enemy.TryGetComponent(out EnemyData enemyData))
                {
                    enemyDataList.Add(new Tuple<GameObject, Combatant, EnemyData>(enemy, combatant, enemyData));
                }
                else
                {
                    Debug.LogError("Enemy prefab " + enemy.name + " does not have an EnemyData component attached to it!");
                    continue;
                }
            }
            else
            {
                Debug.LogError("Enemy prefab " + enemy.name + " does not have a Combatant component attached to it!");
                continue;
            }
        }
        // Sort the list by enemy level, ties broken by random value
        enemyDataList = enemyDataList
            .OrderByDescending(x => x.Item2.level)
            .ThenBy(x => UnityEngine.Random.value)
            .ToList();
        int randomSeed = DateTime.Now.Millisecond; // Initial random seed
        while (numSpawnNodes > 0 && remainingLevel > 0.0f)
        {
            UnityEngine.Random.InitState(randomSeed);
            List<Tuple<GameObject, Combatant, EnemyData>> validList = enemyDataList
                .Where(x => x.Item2.level <= remainingLevel)
                .OrderByDescending(x => x.Item2.level)
                .ThenBy(x => UnityEngine.Random.value)
                .ToList();
            if (validList.Count == 0) break;
            // Divide the list in half and take the first half
            int half = validList.Count / 2;
            // Pick a random enemy from the list
            Tuple<GameObject, Combatant, EnemyData> enemyData = validList[UnityEngine.Random.Range(0, half)];
            // Pick a random pathfinding node to assign it to
            PathfindingNode spawnNode = validEnemySpawns[UnityEngine.Random.Range(0, validEnemySpawns.Count)];
            enemySpawnList.Add(spawnNode, enemyData.Item1);
            validEnemySpawns.Remove(spawnNode);
            numSpawnNodes--;
            remainingLevel -= enemyData.Item2.level;
            randomSeed += (spawnNode.GetX() + spawnNode.GetZ());
        }
        return enemySpawnList;
    }

    /* *** ENEMY LIST GENERATION *** */
    private List<GameObject> GetValidEnemyList(float playerLevel) // Gets a list of enemies that can spawn in the room.
    {
        List<GameObject> validList = new();
        foreach (GameObject enemy in masterEnemyList)
        {
            if (enemy.TryGetComponent(out Combatant combatant))
            {
                if (enemy.TryGetComponent(out EnemyData enemyData))
                {
                    if (combatant.level <= playerLevel && !enemyData.HasEnemyDisappeared(playerLevel)) // If the enemy's disappear level is below 0, then it should never disappear.
                    {
                        validList.Add(enemy);
                        continue;
                    }
                }
                else
                {
                    Debug.LogError("Enemy prefab " + enemy.name + " does not have an EnemyData component attached to it!");
                    continue;
                }
            }
            else
            {
                Debug.LogError("Enemy prefab " + enemy.name + " does not have a Combatant component attached to it!");
                continue;
            }
        }
        return validList;
    }

    private List<GameObject> GetValidEnemyList() // Automatically gets the player's level.
    {
        return GetValidEnemyList(PlayerController.Instance.combatant.level);
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
        masterEnemyList = LoadEnemiesFromResources();
        Debug.Log("Loaded " + masterEnemyList.Count + " enemies.");
    }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to the CombatManager's SpawnActors event
        CombatManager.Instance.SpawnActors.AddListener(SpawnActors);
    }

    private void OnDestroy()
    {
        CombatManager.Instance.SpawnActors.RemoveListener(SpawnActors);
        Instance = null;
    }
    void SpawnActors()
    {
        Debug.Log("Spawning enemies...");
        List<PathfindingNode> validEnemySpawns = PathNodeManager.Instance.GetEnemySpawnNodes();
        Debug.Log("Detected " + validEnemySpawns.Count + " enemy spawn nodes.");
        List<GameObject> gameObjects = GetValidEnemyList();
        Debug.Log("Detected " + gameObjects.Count + " valid enemies.");
        // Equal chance between prioritize enemy count and prioritize level
        if (UnityEngine.Random.value > 0.5f)
        {
            Debug.Log("Prioritizing count");
            int enemyNum = 0;
            Dictionary<PathfindingNode, GameObject> enemySpawnList = GenerateRoomEnemiesPrioritizeCount();
            foreach (KeyValuePair<PathfindingNode, GameObject> pair in enemySpawnList)
            {
                GameObject enemy = Instantiate(pair.Value, new Vector3(pair.Key.transform.position.x, pair.Key.transform.position.y + 0.5f, pair.Key.transform.position.z), Quaternion.identity);
                enemy.transform.parent = this.transform;
                enemy.name = $"Enemy_{enemyNum.ToString()}_{pair.Value.name}";
                enemyNum++;
                Debug.Log("Spawned " + enemy.name);
            }
        }
        else
        {
            Debug.Log("Prioritizing level");
            Dictionary<PathfindingNode, GameObject> enemySpawnList = GenerateRoomEnemiesPrioritizeLevel();
            int enemyNum = 0;
            foreach (KeyValuePair<PathfindingNode, GameObject> pair in enemySpawnList)
            {
                GameObject enemy = Instantiate(pair.Value, new Vector3(pair.Key.transform.position.x, pair.Key.transform.position.y + 0.5f, pair.Key.transform.position.z), Quaternion.identity);
                enemy.transform.parent = this.transform;
                enemy.name = $"Enemy_{enemyNum.ToString()}_{pair.Value.name}";
            }
        }
        Debug.Log("Done spawning enemies");
    }
}
