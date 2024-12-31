using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TurnOrderManager : MonoBehaviour
{
    public static TurnOrderManager Instance;
    [Tooltip("Whether or not combat is active.")]public bool isCombat = false;
    [Tooltip("The current turn list.")]public List<Combatant> turnList = new();
    [Tooltip("The combatant whose turn it is.")] public Combatant currentTurn;

    /* *** TURN LIST MANAGEMENT *** */
    public bool IsCombatantInTurnList(Combatant obj) { return turnList.Contains(obj); }
    public void SortTurnList()
    {
        turnList = turnList
            .OrderByDescending(x => x.Speed)
            .ThenByDescending(x => x.isPlayer)
            .ThenByDescending(x => x.combatTiebreakModifier)
            .ToList();
    }
    public void AddToTurnList(Combatant obj)
    {
        // If the combatant is already in the list, raise a warning and return.
        if (turnList.Contains(obj))
        {
            Debug.LogWarning("Attempted to add a combatant to the turn list that is already in the list.");
            return;
        }
        turnList.Add(obj);
        turnList = turnList.OrderByDescending(x => x.Speed).ToList(); // Sort the list by speed
    }
    public Combatant GetNextCombatant(Combatant combatantIndex)
    {
        if (!turnList.Contains(combatantIndex)) return null;
        int index = turnList.IndexOf(combatantIndex) + 1;
        if (index >= turnList.Count) index = 0;
        return turnList[index];
    }
    public Combatant GetNextLivingCombatant(Combatant combatantIndex)
    {
        if (turnList.Count == 0) return null;
        if (!turnList.Contains(combatantIndex)) return null;
        if (!DoesTurnListContainAnyLivingCombatants()) return null; // If there are no living combatants, return null.
        Combatant nextCombatant = GetNextCombatant(combatantIndex);
        if (nextCombatant == null) return null;
        if (nextCombatant.isDead) return GetNextLivingCombatant(nextCombatant); // If the next combatant is dead, get the next living combatant.
        return nextCombatant;
    }
    public Combatant GetNextLivingCombatant() { return GetNextLivingCombatant(currentTurn); }
    public void ClearTurnList() { turnList.Clear(); }

    /* *** *** TURN LIST CHECKS *** *** */
    public List<Combatant> GetLivingCombatants() { return turnList.FindAll(x => !x.isDead).ToList(); } // Returns a list of all living combatants
    public List<Combatant> GetDeadCombatants() { return turnList.FindAll(x => x.isDead).ToList(); } // Returns a list of all dead combatants
    public bool IsPlayerOnlyLivingCombatant()
    {
        List<Combatant> livingCombatants = GetLivingCombatants();
        return livingCombatants.Count == 1 && livingCombatants[0].isPlayer;
    }
    public bool DoesTurnListContainAnyLivingCombatants() { return GetLivingCombatants().Count > 0; }

    /* *** UI *** */
    public string GetCurrentTurnCombatantName()
    {
        if (currentTurn == null) return "None";
        if (currentTurn.isPlayer) return "Player";
        if (currentTurn.enemyData == null) return "Enemy";
        else return currentTurn.enemyData.enemyName;
    }
    public string GetNextTurnCombatantName()
    {
        Combatant nextCombatant = GetNextLivingCombatant();
        if (nextCombatant == null) return "None";
        if (nextCombatant.isPlayer) return "Player";
        if (nextCombatant.enemyData == null) return "Enemy";
        else return nextCombatant.enemyData.enemyName;
    }
    /* *** MONOBEHAVIOUR FUNCTIONS *** */
    private void Awake() // Singleton pattern
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
    private void Start()
    {
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        CombatManager.Instance.CombatantDied.AddListener(OnCombatantDeath);
        CombatManager.Instance.CombatTurnStart.AddListener(OnCombatTurnStart);
        CombatManager.Instance.CombatTurnEnd.AddListener(OnCombatTurnEnd);
    }
    private void OnDestroy()
    {
        CombatManager.Instance.CombatStart.RemoveListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.RemoveListener(OnCombatEnd);
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);
        CombatManager.Instance.CombatTurnStart.RemoveListener(OnCombatTurnStart);
        CombatManager.Instance.CombatTurnEnd.RemoveListener(OnCombatTurnEnd);
    }
    /* *** EVENTS *** */
    private IEnumerator OnCombatStart_AfterTurnOrderEstablished() // This runs after the turn order has been established by skipping a frame.
    {
        yield return null;
        currentTurn = turnList[0];
        CombatManager.Instance.CombatTurnStart.Invoke(currentTurn);
    }
    public void OnCombatStart()
    {
        Debug.Log("Combat has started!");
        Instance.isCombat = true;
        StartCoroutine(OnCombatStart_AfterTurnOrderEstablished());
    }
    public void OnCombatEnd()
    {
        Debug.Log("Combat has ended!");
        Instance.isCombat = false;
        // Get every combatant on the turn list who isn't the player
        List<Combatant> enemies = turnList.FindAll(x => !x.isPlayer);
        ClearTurnList();
        // TODO: Create the limb pool here
        // Free every game object on the enemies list
        foreach (Combatant enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
    public void OnCombatantDeath(Combatant combatant, Damage _)
    {
        combatant.isDead = true; // We double check that the combatant is in fact dead here.
        if (combatant.isPlayer)
        {
            Debug.Log("Player has died! Game over!");
            // Stop updates
            Time.timeScale = 0;
            CombatManager.Instance.PlayerDeath.Invoke();
        }
        else if (IsPlayerOnlyLivingCombatant())
        {
            Debug.Log("All enemies have been defeated. Combat end engaged here.");
            CombatManager.Instance.CombatEnded.Invoke();
        }
        if (currentTurn == combatant) CombatManager.Instance.CombatTurnEnd.Invoke(combatant); // If the current turn is the combatant that just died, then end their turn immediately.
    }
    public void OnCombatTurnStart(Combatant combatant)
    {
        Debug.Log("Starting turn for " + combatant.name + ".");
    }
    public void OnCombatTurnEnd(Combatant combatant)
    {
        Debug.Log("Ending turn for " + combatant.name + ".");
        // We get the next living combatant and set them as the current turn.
        currentTurn = GetNextLivingCombatant(combatant);
        if (currentTurn == null) // If there are no more living combatants, end combat.
        {
            CombatManager.Instance.CombatEnded.Invoke();
        }
        else
        {
            CombatManager.Instance.CombatTurnStart.Invoke(currentTurn);
        }
    }
}