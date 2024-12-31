using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    public CreatureMovementType movementType = CreatureMovementType.Ground;
    public Combatant combatant;
    protected Coroutine turnTimeoutCoroutine;
    [System.NonSerialized] protected bool isMoving = false;
    /* *** TARGETING AND PATHFINDING *** */
    protected Ray CreateElevatedRayBetweenPoints(Vector3 origin, Vector3 target)
    {
        Vector3 direction = (new Vector3(target.x, 0.2f, target.z) - new Vector3(origin.x, 0.2f, origin.z)).normalized;
        return new Ray(new Vector3(origin.x, 0.2f, origin.z), direction);
    }
    public PathfindingNode FindNodePlayerIsOn() { return PathNodeManager.Instance.GetCombatantNode(PlayerController.Instance.combatant); }
    public bool CanTargetBeAttackedFromHere(Combatant target, Limb attackingLimb) // Determines whether or not the current tile the enemy is standing on is able to attack the player.
    {
        return DetermineValidAttackPositions(attackingLimb, FindNodePlayerIsOn(), movementType).Contains(PathNodeManager.Instance.GetCombatantNode(target));
    }
    public Limb PickAttackLimb(Dictionary<int, Limb> limbsList) // Pick a random limb to attack with
    {
        if (limbsList.Count == 0) return null;
        // Get a list of keys from the dict
        List<int> keys = new(limbsList.Keys);
        // Get a random key
        int randomKey = keys[Random.Range(0, keys.Count)];
        // Return the limb at that key
        if (limbsList[randomKey] == null) Debug.LogError("Limb is null");
        return limbsList[randomKey];
    }
    public List<PathfindingNode> DetermineValidAttackPositions(Limb attackLimb, PathfindingNode attackingPosition, CreatureMovementType movementType) // Determines valid positions the enemy can attack from
    {
        LayerMask pathNodesMask = LayerMask.GetMask("PathNodes");
        LayerMask wallMask = LayerMask.GetMask("WallBlocker");
        List<PathfindingNode> nodes = new();
        Collider[] hitColliders;
        // Get all valid traversible nodes within the attack range by spherecasting the radius.
        hitColliders = Physics.OverlapSphere(attackingPosition.GetVector3Position(), attackLimb.Range, pathNodesMask);
        foreach (Collider hit in hitColliders)
        {
            if (!hit.TryGetComponent<PathfindingNode>(out PathfindingNode node)) continue;
            if (node == null) continue;
            // Check if the node is traversible and does not have a combatant on it.
            if (!node.IsTraversible(movementType) || node.IsNodeOccupied()) continue;
            // Determine if there is a clear path between that node and the player node via raycast
            PathfindingNode playerNode = FindNodePlayerIsOn();
            if (playerNode == null) continue;
            Ray checkRay = CreateElevatedRayBetweenPoints(node.GetVector3Position(), playerNode.GetVector3Position());
            // Check if there is a wall or other obstacle in the way
            if (Physics.Raycast(checkRay, out RaycastHit _, attackLimb.Range, wallMask))
            {
                continue;
            }
            // Determine if the node is actually reachable regardless of how far the enemy may have to travel to get there.
            List<PathfindingNode> path = Pathfinding.FindPath(attackingPosition, node, movementType);
            if (path == null) continue;
            nodes.Add(node);
        }
        return nodes;
    }
    public List<PathfindingNode> GetReachablePositions(List<PathfindingNode> nodes) // Find all positions the pathfinder can get this combatant to based on their speed
    {
        List<PathfindingNode> reachableNodes = new();
        PathfindingNode currentNode = PathNodeManager.Instance.GetCombatantNode(combatant);
        foreach (PathfindingNode n in nodes)
        {
            List<PathfindingNode> path = Pathfinding.FindPath(currentNode, n, movementType);
            if (path == null) continue; // Can't path to this node
            if (path.Count <= combatant.Speed) reachableNodes.Add(n);
        }
        return reachableNodes;
    }
    public void EndTurn()
    {
        if (turnTimeoutCoroutine != null) StopCoroutine(turnTimeoutCoroutine);
        isMoving = false; // Safety
        CombatManager.Instance.CombatTurnEnd.Invoke(combatant);
    }

    public void Attack(PathfindingNode position, Limb attackingLimb)
    {
        combatant.SetPrimaryLimb(attackingLimb); // Just to make sure
        if (DetermineValidAttackPositions(attackingLimb, FindNodePlayerIsOn(), movementType).Contains(PathNodeManager.Instance.GetCombatantNode(combatant)))
        {
            return; // We can't attack here
        }
        if (FindNodePlayerIsOn() == null)
        {
            return; // Blocks a NRE
        }
        List<PathfindingNode> attackedNodes = combatant.GetPrimaryLimb().attackData.GetAttackedNodes(combatant, combatant.GetPrimaryLimb(), FindNodePlayerIsOn().transform.position);
        foreach (PathfindingNode node in attackedNodes)
        {
            Combatant victim = PathNodeManager.Instance.GetCombatantOnNode(node);
            if (victim == null) continue;
            Damage dmg = Damage.CalculateDamageAgainstTarget(combatant, victim);
            victim.ApplyDamage(dmg);
        }
    }
    protected IEnumerator EndTurnAfterTimeout()
    {
        yield return new WaitForSeconds(7.0f);
        // Check if it's still this enemy's turn
        if (TurnOrderManager.Instance.currentTurn == combatant)
        {
            if (isMoving)
            {
                // Find the nearest unoccupied node
                PathfindingNode closestNode = PathNodeManager.Instance.GetClosestUnoccupiedNode(transform.position, movementType);
                // Place the combatant there
                PathNodeManager.Instance.SetCombatantPosition(combatant, closestNode.GetX(), closestNode.GetZ());
                // Put the gameobject on the node
                transform.position = new Vector3(closestNode.GetX(), transform.position.y, closestNode.GetZ());
                isMoving = false;
            }
            EndTurn();
        }
    }
    public virtual void TakeTurn()
    {
        // Start a coroutine that ends the turn after a certain amount of time as a failsafe.
        turnTimeoutCoroutine = StartCoroutine(EndTurnAfterTimeout());
    }

    private void Start()
    {
        if (!TryGetComponent<Combatant>(out combatant))
        {
            Debug.LogError("This enemy AI does not have a combatant component.");
        }
        CombatManager.Instance.CombatTurnStart.AddListener(OnTurnStart);
    }

    private void OnDestroy()
    {
        CombatManager.Instance.CombatTurnStart.RemoveListener(OnTurnStart);
    }

    private void OnTurnStart(Combatant combatant)
    {
        if (turnTimeoutCoroutine != null) StopCoroutine(turnTimeoutCoroutine);

        if (combatant == this.combatant) TakeTurn();
    }
}