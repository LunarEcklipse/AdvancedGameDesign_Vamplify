using UnityEngine;

class PermanentDamageRampModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    private Limb baseLimb;
    private Combatant baseCombatant;

    public float baseDamageIncreasePerKill = 3.0f;
    public float damageIncreaseBonusPerLevel = 1.0f;

    public float DamageIncreasePerKill
    {
        get
        {
            return baseDamageIncreasePerKill + (damageIncreaseBonusPerLevel * (this.modifierLevel - 1));
        }
        set
        {
            baseDamageIncreasePerKill = value;
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
            return "This limb gains a permanent damage increase of " + (int)DamageIncreasePerKill + " per slain foe";
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
            baseLimb.baseDamage += DamageIncreasePerKill;
        }
    }
}