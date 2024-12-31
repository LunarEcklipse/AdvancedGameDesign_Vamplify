using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PointAttack", menuName = "Scriptable Objects/Attacks/PointAttack")]
public class PointAttack : Attack
{
    public override AttackType AttackType { get { return AttackType.Point; } }
    
    public override List<PathfindingNode> GetAttackedNodes(Combatant caster, Limb limb, Vector3 targetPosition) // Used to beam out the attack but not actually confirm it
    {
        List<PathfindingNode> nodes = new();
        LayerMask pathNodeMask = LayerMask.GetMask("PathNodes");
        LayerMask blockerMask = LayerMask.GetMask("WallBlockers");
        // Do a spherecast at the target position and get the closest node caught within that cast
        Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.5f, pathNodeMask);
        PathfindingNode closestNode = null;
        foreach (Collider collider in colliders)
        {
            if (!collider.gameObject.TryGetComponent<PathfindingNode>(out PathfindingNode node)) continue;
            if (closestNode == null)
            {
                closestNode = node;
                continue;
            }
            if (Vector3.Distance(node.GetVector3Position(), targetPosition) < Vector3.Distance(closestNode.GetVector3Position(), targetPosition))
            {
                closestNode = node;
            }
        }

        // Fire a ray between the caster and the target position and see if it hits any wallblockers
        if (closestNode == null) return nodes; // Prevents a NRE
        Ray ray = CreateElevatedRayBetweenPoints(caster.transform.position, closestNode.transform.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, blockerMask)) // There's something blocking line of sight.
        {
            closestNode = null;
            Vector3 castPoint = new(hit.point.x, 0.0f, hit.point.z);
            colliders = Physics.OverlapSphere(castPoint, 1.0f, pathNodeMask);
            foreach (Collider collider in colliders) // Find the closest node to the hit point
            {
                if (!collider.gameObject.TryGetComponent<PathfindingNode>(out PathfindingNode node)) continue;
                if (closestNode == null && node.IsTraversible(CreatureMovementType.All))
                {
                    closestNode = node;
                    continue;
                }
                if (Vector3.Distance(node.GetVector3Position(), castPoint) < Vector3.Distance(closestNode.GetVector3Position(), castPoint) && node.IsTraversible(CreatureMovementType.All)) // We can't have non-traversible nodes for this target point.
                {
                    closestNode = node;
                }
            }
        }
        // Determine the distance between the caster and the target node. If it's less than the range of the limb, return the node.
        if (closestNode != null && Vector3.Distance(new Vector3(caster.transform.position.x, 0.0f, caster.transform.position.z), closestNode.GetVector3Position()) <= limb.Range)
        {
            nodes.Add(closestNode);
            return nodes;
        }
        else // We need to do a distance check along the ray and determine the most suitable node
        {
            closestNode = null;
            ray = CreateRayBetweenPoints(caster.transform.position, targetPosition);
            Vector3 maxRangePoint = ray.GetPoint(limb.Range);
            colliders = Physics.OverlapSphere(maxRangePoint, 1.0f, pathNodeMask);
            foreach(Collider collider in colliders)
            {
                if (!collider.gameObject.TryGetComponent<PathfindingNode>(out PathfindingNode node)) continue;
                if (closestNode == null && node.IsTraversible(CreatureMovementType.All))
                {
                    closestNode = node;
                    continue;
                }
                if (node == null || closestNode == null) continue; // Safety check that should hopefully fix a NRE
                if (Vector3.Distance(node.GetVector3Position(), maxRangePoint) < Vector3.Distance(closestNode.GetVector3Position(), maxRangePoint) && node.IsTraversible(CreatureMovementType.All)) // We can't have non-traversible nodes for this target point.
                {
                    closestNode = node;
                }
            }
        }

        if (closestNode != null) nodes.Add(closestNode);
        return nodes;
    }
}