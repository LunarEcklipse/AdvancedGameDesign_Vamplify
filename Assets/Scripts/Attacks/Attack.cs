using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System;

public abstract class Attack : ScriptableObject
{
    public abstract AttackType AttackType { get; }
    
    protected Ray CreateElevatedRayBetweenPoints(Vector3 origin, Vector3 target)
    {
        Vector3 direction = (new Vector3(target.x, 0.2f, target.z) - new Vector3(origin.x, 0.2f, origin.z)).normalized;
        return new Ray(new Vector3(origin.x, 0.2f, origin.z), direction);
    }
    protected Ray CreateRayBetweenPoints(Vector3 origin, Vector3 target)
    {
        Vector3 direction = (target - origin).normalized;
        return new Ray(origin, direction);
    }
    protected Ray CreateGroundedRayBetweenPoints(Vector3 origin, Vector3 target)
    {
        Vector3 direction = (new Vector3(target.x, 0.0f, target.z) - new Vector3(origin.x, 0.0f, origin.z)).normalized;
        return new Ray(new Vector3(origin.x, 0.0f, origin.z), direction);
    }

    public abstract List<PathfindingNode> GetAttackedNodes(Combatant caster, Limb limb, Vector3 targetPosition); // Used to beam out the attack but not actually confirm it
}