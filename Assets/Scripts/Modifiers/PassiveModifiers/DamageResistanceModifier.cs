using UnityEngine;

[CreateAssetMenu(fileName = "Damage Type Modifier", menuName = "Scriptable Objects/Passive Modifiers/Damage Type Modifier", order = 1)]
class DamageResistanceModifier : PassiveModifier
{
    public DamageType damageType;
    public float baseDamageTypeBonus = 0.25f;
    public float damageTypeBonusPerLevel = 0.05f;
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of " + damageType.ToString() + " Resistance";
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
                ModifierViability.Positive => "Increases resistance to " + damageType.ToString() + " damage by " + (DamageTypeModifierBonus * 100.0f) + "%",
                ModifierViability.Negative => "Decreases resistance to " + damageType.ToString() + " damage by " + -(DamageTypeModifierBonus * 100.0f) + "%",
                _ => "Alters resistance to " + damageType.ToString() + " damage by " + (DamageTypeModifierBonus * 100.0f) + "%",
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }
    public float DamageTypeModifierBonus
    {
        get
        {
            return baseDamageTypeBonus + (damageTypeBonusPerLevel * modifierLevel);
        }
    }
    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {

    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {

    }

    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }
}