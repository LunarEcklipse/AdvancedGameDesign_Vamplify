using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public abstract class Active : BaseModifier
{
    public readonly new ModifierType modifierType = ModifierType.Active;
    public UnityEvent<List<Combatant>, List<Limb>> modifierTrigger;

    public override abstract void LevelUpModifier(int newLevel); // Used to increase the stats of an individual modifier.

    public virtual void AdjustLimbStats(Limb adjustedLimb) // Adjusts the stats on a limb.
    {

    }

    public virtual void AdjustCombatantStats(Combatant adjustedCombatant) // Adjusts the stats on a combatant.
    {

    }
    public abstract void OnApplyModifier(Limb limb, Combatant combatant);
    public abstract void OnRemoveModifier(Limb limb, Combatant combatant);

    public virtual void OnEnable()
    {
        modifierTrigger.AddListener(TriggerActiveModifier);
    }
    public virtual void OnDisable()
    {
        modifierTrigger.RemoveListener(TriggerActiveModifier);
    }
    public virtual void OnDestroy()
    {
        modifierTrigger.RemoveListener(TriggerActiveModifier);
    }
    public abstract void TriggerActiveModifier(List<Combatant> affectedCombatants, List<Limb> affectedLimbs);
}