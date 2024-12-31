using UnityEngine;

class PermanentHealthRampModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    private Limb baseLimb;
    private Combatant baseCombatant;

    public float baseHealthIncreasePerKill = 10.0f;
    public float healthIncreaseBonusPerLevel = 5.0f;

    public float HealthIncreaseBonus
    {
        get
        {
            return baseHealthIncreasePerKill + (healthIncreaseBonusPerLevel * (this.modifierLevel - 1));
        }
        set
        {
            baseHealthIncreasePerKill = value;
        }
    }

    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Damage Empowerment";
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
            return "This limb grants a permanent health increase of " + HealthIncreaseBonus + " per slain foe";
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

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);
    }
    private void OnCombatantDeath(Combatant victim, Damage damage)
    {
        if (damage.attacker == baseCombatant && damage.usedLimbs.Contains(baseLimb))
        {
            baseCombatant.baseMaxHealth += HealthIncreaseBonus;
            baseCombatant.remainingHealth += HealthIncreaseBonus;
        }
    }
}