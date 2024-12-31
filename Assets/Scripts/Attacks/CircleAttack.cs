using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CircleAttack", menuName = "Scriptable Objects/Attacks/CircleAttack")]
public class CircleAttack : Attack
{
    public override AttackType AttackType
    {
        get
        {
            return AttackType.Circle;
        }
    }
    public override List<PathfindingNode> GetAttackedNodes(Combatant caster, Limb limb, Vector3 targetPosition)
    {
        // Raycast between the caster and the target position to get the range
        Vector3 casterPosition = caster.transform.position;
        Vector3 direction = targetPosition - casterPosition;
        LayerMask wallLayerMask = LayerMask.GetMask("WallBlocker");
        LayerMask pathNodeMask = LayerMask.GetMask("PathNodes");
        float range = limb.Range;
        // Send a raycast out to see if it hits a wall. If it does, set the range to the distance of the wall.
        Ray ray = CreateElevatedRayBetweenPoints(casterPosition, targetPosition);
        if (Physics.Raycast(ray, out RaycastHit wallHit, range, wallLayerMask))
        {
            range = Vector3.Distance(casterPosition, wallHit.point);
        }
        // Check if the target position is out of range of the attack. If so, bring it back to the closest allowable point within range
        if (Vector3.Distance(casterPosition, targetPosition) > range)
        {
            targetPosition = casterPosition + direction.normalized * range;
        }
        // Get all nodes within the range of the attack
        Collider[] hits = Physics.OverlapSphere(targetPosition, limb.Radius, pathNodeMask);
        List<PathfindingNode> attackNodes = new();
        foreach (Collider hit in hits)
        {
            // Get the PathfindingNode component from the collider
            if (!hit.TryGetComponent<PathfindingNode>(out var node)) continue;
            if (node.walkState == WalkState.Unwalkable) continue;
            // Shoot a raycast to each node to see if it hits a wall.
            Ray testRay = CreateElevatedRayBetweenPoints(casterPosition, node.GetVector3Position());
            float distanceBetweenCenterAndNode = Vector3.Distance(targetPosition, node.GetVector3Position());
            if (Physics.Raycast(testRay, out RaycastHit _, distanceBetweenCenterAndNode, wallLayerMask))
            {
                continue;
            }
            attackNodes.Add(node);
        }
        return attackNodes;
    }
}