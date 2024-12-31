using UnityEngine;

public abstract class PassiveModifier : BaseModifier
{
    public readonly new ModifierType modifierType = ModifierType.Passive;

    public override abstract void LevelUpModifier(int newLevel); // Used to increase the stats of an individual modifier.

    public virtual void AdjustLimbStats(Limb adjustedLimb) // Adjusts the stats on a limb.
    {

    }

    public virtual void AdjustCombatantStats(Combatant adjustedCombatant) // Adjusts the stats on a combatant.
    {

    }
    public abstract void OnApplyModifier(Limb limb, Combatant combatant);
    public abstract void OnRemoveModifier(Limb limb, Combatant combatant);

}