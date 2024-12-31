using UnityEngine;
public class PermanentCritChanceRampModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    private Limb baseLimb;
    private Combatant baseCombatant;

    public float baseCritChanceIncreasePerKill = 0.1f;
    public float critChanceIncreaseBonusPerLevel = 0.05f;
    
    public float CritChanceIncreasePerKill
    {
        get
        {
            return baseCritChanceIncreasePerKill + (critChanceIncreaseBonusPerLevel * (this.modifierLevel - 1));
        }
        set
        {
            baseCritChanceIncreasePerKill = value;
        }
    }
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Critical Strike Empowerment";
        }
        set
        {
            base.customModifierTitle = value;
        }
    }
    public override string ModifierDescription
    {
        get
        {
            if (customModifierDescription != null) return customModifierDescription;
            return "This limb gains a permanent critical strike chance increase of " + (CritChanceIncreasePerKill * 100) + "% per slain foe";
        }
        set
        {
            customModifierDescription = value;
        }
    }
    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {
        baseLimb = limb;
        baseCombatant = combatant;
        CombatManager.Instance.CombatantDied.AddListener(OnCombatantDeath);
    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {
        baseLimb = null;
        baseCombatant = null;
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);
    }
    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }
    private void OnCombatantDeath(Combatant combatant, Damage damage)
    {
        if (combatant == baseCombatant && damage.usedLimbs.Contains(baseLimb))
        {
            baseCombatant.baseCritChance += CritChanceIncreasePerKill;
        }
    }
}