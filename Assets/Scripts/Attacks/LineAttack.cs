using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LineAttack", menuName = "Scriptable Objects/Attacks/LineAttack")]
public class LineAttack : Attack
{
    public override AttackType AttackType { get { return AttackType.Line; } }
    public override List<PathfindingNode> GetAttackedNodes(Combatant caster, Limb limb, Vector3 targetPosition) // Used to beam out the attack but not actually confirm it
    {
        // Box cast from the player to the mouse position
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
        RaycastHit[] hits = Physics.BoxCastAll(casterPosition, new Vector3(0.5f, 0.5f, 0.5f), direction, Quaternion.LookRotation(direction), range, pathNodeMask);
        List<PathfindingNode> attackNodes = new();
        // Get the node the caster is on
        PathfindingNode casterNode = PathNodeManager.Instance.GetCombatantNode(caster);
        // Remove the caster node from the list of nodes to attack
        foreach (RaycastHit hit in hits)
        {
            PathfindingNode node = hit.collider.GetComponent<PathfindingNode>();
            if (node == casterNode || node.walkState == WalkState.Unwalkable) continue;
            attackNodes.Add(node);
        }
        return attackNodes;
    }
}