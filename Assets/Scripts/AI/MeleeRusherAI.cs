using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRusherAI : EnemyAI
{
    private IEnumerator TakeTurnCoroutine()
    {
        Debug.Log(gameObject.name + "is AI and is taking its turn.");
        float speed = 6;
        Limb attackingLimb = PickAttackLimb(combatant.limbs);
        combatant.EngageLimb(attackingLimb);
        if (!CanTargetBeAttackedFromHere(PlayerController.Instance.combatant, attackingLimb))
        {
            List<PathfindingNode> validAttackPositions = DetermineValidAttackPositions(attackingLimb, FindNodePlayerIsOn(), movementType);
            if (validAttackPositions.Count == 0) // There's nowhere this attack can hit the player.
            {
                Debug.Log("No valid attack positions.");
                EndTurn();
                yield break;
            }
            if (!validAttackPositions.Contains(PathNodeManager.Instance.GetCombatantNode(combatant))) // We cannot attack from where we are
            {
                List<PathfindingNode> reachableAttackPositions = GetReachablePositions(validAttackPositions);
                if (reachableAttackPositions.Count == 0) // We can't attack the player this turn, so we get as close as possible and then end our turn.
                {
                    // Get the closest unoccupied node to the player
                    PathfindingNode closestNode = PathNodeManager.Instance.GetClosestUnoccupiedNode(FindNodePlayerIsOn());
                    if (closestNode == null) // Couldn't find one
                    {
                        yield return new WaitForSeconds(0.5f); // Wait for half a second
                        EndTurn();
                        yield break;
                    }
                    List<PathfindingNode> path = Pathfinding.FindPath(PathNodeManager.Instance.GetCombatantNode(combatant), closestNode, movementType);
                    if (path == null) // Couldn't find a path
                    {
                        yield return new WaitForSeconds(0.5f); // Wait for half a second
                        EndTurn();
                        yield break;
                    }
                    isMoving = true;
                    // Traverse along that path as far as we can
                    for (int i = 0; i < combatant.Speed; i++)
                    {

                        if (i >= path.Count) break;

                        Vector3 targetPosition = new((float)path[i].GetX(), transform.position.y, (float)path[i].GetZ());
                        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                            yield return null;
                        }
                        PathNodeManager.Instance.GetCombatantNode(combatant).ClearOccupant();
                        combatant.transform.position = targetPosition;
                        path[i].SetOccupant(combatant);
                    }
                    isMoving = false;
                    EndTurn();
                    yield break;
                }
                // We can attack the player from a different position
                isMoving = true;
                PathfindingNode targetNode = validAttackPositions[Random.Range(0, validAttackPositions.Count)];
                List<PathfindingNode> pathToTarget = Pathfinding.FindPath(PathNodeManager.Instance.GetCombatantNode(combatant), targetNode, movementType);
                if (pathToTarget == null) // Couldn't find a path
                {
                    isMoving = false;
                    EndTurn();
                    yield break;
                }
                for (int i = 0; i < combatant.Speed; i++) // Traverse the path
                {
                    if (i >= pathToTarget.Count) break;
                    Vector3 targetPosition = new((float)pathToTarget[i].GetX(), transform.position.y, (float)pathToTarget[i].GetZ());
                    while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                        yield return null;
                    }
                    PathNodeManager.Instance.GetCombatantNode(combatant).ClearOccupant();
                    combatant.transform.position = targetPosition;
                    pathToTarget[i].SetOccupant(combatant);
                }
                // Wait half a second
                yield return new WaitForSeconds(0.5f);
                isMoving = false;
            }
        }
        Attack(FindNodePlayerIsOn(), attackingLimb);
        EndTurn();
        yield return new WaitForSeconds(0.5f); // Wait for half a second
        yield break;
    }

    public override void TakeTurn()
    {
        base.TakeTurn(); // Start the timeout
        StartCoroutine(TakeTurnCoroutine());
    }
}
