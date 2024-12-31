using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    // Singleton instance
    public static CombatManager Instance { get; private set; }
    [System.NonSerialized] public static GameObject healthbarPrefab;
    public static BigInteger totalScore = 0;
    public static bool isCombatActive = false;
    public static bool pauseAll = false;
    public static float scalingPerRoom = 1.3f;
    public static float currentRoomScale = 1.0f;

    /* *** EVENTS *** */
    public UnityEvent GenerateNewRoom;
    public UnityEvent SpawnActors; // Used when the game is ready to spawn things into the level
    public UnityEvent CombatStart;
    public UnityEvent<Combatant> CombatTurnStart;
    public UnityEvent<Combatant, Damage> CombatantAttacked;
    public UnityEvent<Combatant, Damage> CombatantDied;
    public UnityEvent<Combatant> CombatTurnEnd;
    public UnityEvent PlayerDeath;
    public UnityEvent ReportRemainingCombatants;
    public UnityEvent CombatEnded;
    public UnityEvent PlayerTouchedRoomTrigger;
    public UnityEvent CombatReady;

    /* Coroutines */
    private IEnumerator PrepareAndStartCombat() // To be ran after a room is generated.
    {
        pauseAll = true;
        // First we wait one frame so everything's start function runs
        yield return null;
        GenerateNewRoom.Invoke(); // TODO: Remove this and place in a more suitable location once testing is done
        yield return null; // Pass a frame for the room to generate.
        // Calculate the camera radius for the current room
        GameWorldSpawner.Instance.SetCircleMaxRadius(GameWorldSpawner.Instance.DetermineCircleRadiusForCurrentRoom());
        Debug.Log("Circle Radius: " + GameWorldSpawner.Instance.circleMaxRadius);
        Debug.Log("Room generation complete.");
        // Count the number of nodes with enemy spawns.
        int maxEnemyCount = 0;
        foreach (PathfindingNode node in PathNodeManager.Instance.GetEnemySpawnNodes())
        {
            if (node.isEnemySpawn) maxEnemyCount++;
        }
        if (maxEnemyCount == 0) Debug.LogError("No enemy spawn nodes found in the room!");
        Debug.Log("Spawning actors");
        SpawnActors.Invoke();
        Debug.Log("Actor spawning done");
        yield return null;
        Debug.Log("Combat Ready Invoking");
        CombatReady.Invoke(); // Combat ready but awaiting start
        Debug.Log("Fade Clear Invoking");
        FadeToBlackPanel.Instance.FadeToClear();
        Debug.Log("Waiting for fade to finish.");
        yield return new WaitForEvent(FadeToBlackPanel.Instance.fadeToClearEnd);
        Debug.Log("Starting combat");
        CombatStart.Invoke();
        yield return null;
        pauseAll = false;
    }

    /* *** MONOBEHAVIOR FUNCTIONS *** */
    private void Awake()
    {
        // Check if instance already exists. If so, destroy self
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        CombatManager.healthbarPrefab = Resources.Load("Healthbar") as GameObject;

        // Initialize events 
        GenerateNewRoom = new UnityEvent();
        CombatReady = new UnityEvent();
        CombatStart = new UnityEvent();
        CombatTurnStart = new UnityEvent<Combatant>();
        CombatantAttacked = new UnityEvent<Combatant, Damage>();
        CombatantDied = new UnityEvent<Combatant, Damage>();
        CombatTurnEnd = new UnityEvent<Combatant>();
        PlayerDeath = new();
        ReportRemainingCombatants = new();
        CombatEnded = new();
        PlayerTouchedRoomTrigger = new();


        // Subscribe to combat end
        CombatEnded.AddListener(OnCombatEnd);
        PlayerTouchedRoomTrigger.AddListener(OnPlayerTouchRoomTrigger);
    }

    private void OnDestroy()
    {
        GenerateNewRoom.RemoveAllListeners();
        SpawnActors.RemoveAllListeners();
        CombatStart.RemoveAllListeners();
        CombatTurnStart.RemoveAllListeners();
        CombatantDied.RemoveAllListeners();
        CombatTurnEnd.RemoveAllListeners();
        PlayerDeath.RemoveAllListeners();
        ReportRemainingCombatants.RemoveAllListeners();
        CombatEnded.RemoveAllListeners();
        PlayerTouchedRoomTrigger.RemoveAllListeners();
        CombatReady.RemoveAllListeners();
        Instance = null;
    }
    private void Start() // Start is called once before the first execution of Update after the MonoBehaviour is created
    {
        StartCoroutine(PrepareAndStartCombat()); // For first combat occurrence.
    }

    private void OnCombatEnd()
    {

    }

    private IEnumerator FadeOutLoadCombatFadeIn()
    {
        FadeToBlackPanel.Instance.FadeToBlack();
        yield return new WaitForEvent(FadeToBlackPanel.Instance.fadeToBlackEnd);
        StartCoroutine(PrepareAndStartCombat());
    }

    private void OnPlayerTouchRoomTrigger()
    {
        Debug.Log("Touched room change trigger.");
        StartCoroutine(FadeOutLoadCombatFadeIn());
    }

    private void UpdateScore(int scoreAdd)
    {
        totalScore += Mathf.Abs(scoreAdd);
    }
}
