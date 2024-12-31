using UnityEngine;

[CreateAssetMenu(fileName = "Health Modifier", menuName = "Scriptable Objects/Passive Modifiers/Health Modifier", order = 1)]
public class HealthModifier : PassiveModifier
{
    public override string ModifierTitle
    { 
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Health";
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
            return modifierViability switch
            {
                ModifierViability.Positive => "Increases health by " + (HealthModifierBonus * 100.0f) + "%",
                ModifierViability.Negative => "Decreases health by " + -(HealthModifierBonus * 100.0f) + "%",
                _ => "Alters health by " + (HealthModifierBonus * 100.0f) + "%",
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public float baseHealthModifierBonus = 0.15f;
    public float healthModifierBonusPerLevel = 0.05f;
    public float HealthModifierBonus
    {
        get
        {
            return baseHealthModifierBonus + (healthModifierBonusPerLevel * modifierLevel);
        }
    }

    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {
        // Add the health to the combatant's health
        combatant.remainingHealth += baseHealthModifierBonus + (healthModifierBonusPerLevel * modifierLevel);
    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {
        combatant.remainingHealth -= baseHealthModifierBonus + (healthModifierBonusPerLevel * modifierLevel);
        if (combatant.remainingHealth <= 1.0f) combatant.remainingHealth = 1.0f;
    }

    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }
}