using UnityEngine;

class AllDamageTrueModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of True Damage";
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
            return "All damage dealt by this limb is considered True damage.";
        }
        set
        {
            customModifierDescription = value;
        }
    }
    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {

    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {

    }
    private void OnEnable()
    {
        // Check if we are in playmode or a proper build
        if (!Application.isPlaying) return;
    }
    private void OnDisable()
    {
        if (!Application.isPlaying) return;
    }
    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
    }

    public override void LevelUpModifier(int newLevel) // This modifier has no need to level so we leave it blank
    {
        
    }
}