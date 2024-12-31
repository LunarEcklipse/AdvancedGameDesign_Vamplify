using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConeAttack", menuName = "Scriptable Objects/Attacks/ConeAttack")]
public class ConeAttack : Attack
{
    public override AttackType AttackType { get { return AttackType.Cone; } }

    public override List<PathfindingNode> GetAttackedNodes(Combatant caster, Limb limb, Vector3 targetPosition)
    {
        Vector3 casterPosition = caster.transform.position;
        Vector3 direction = targetPosition - casterPosition;
        LayerMask wallLayerMask = LayerMask.GetMask("WallBlocker");
        LayerMask pathNodeMask = LayerMask.GetMask("PathNodes");
        float range = limb.Range;
        Ray ray = CreateElevatedRayBetweenPoints(casterPosition, targetPosition);
        if (Physics.Raycast(ray, out RaycastHit wallHit, range, wallLayerMask))
        {
            range = Vector3.Distance(casterPosition, wallHit.point);
        }
        Collider[] hits = Physics.OverlapSphere(casterPosition, range, pathNodeMask);
        List<PathfindingNode> attackNodes = new();
        foreach(Collider hit in hits)
        {
            Vector3 directionToObject = (hit.transform.position - casterPosition);
            directionToObject.Normalize();

            float angleToObject = Vector3.Angle(direction, directionToObject);
            if (angleToObject <= limb.Radius / 2f)
            {
                if (hit.TryGetComponent<PathfindingNode>(out PathfindingNode node))
                {
                    if (node.walkState == WalkState.Unwalkable) continue;
                    attackNodes.Add(node);
                }
            }
        }
        return attackNodes;
    }
}
