using UnityEngine;

[CreateAssetMenu(fileName = "Speed Modifier", menuName = "Scriptable Objects/Passive Modifiers/Speed Modifier", order = 1)]
class SpeedModifier : PassiveModifier
{
    // Common: 2
    // Uncommon: 5
    // Rare: 8
    // Legendary: 12
    public float BaseSpeedModifierBonus
    {
        get
        {
            float modifierVal = base.modifierRarity switch
            {
                ModifierRarity.Common => 2.0f,
                ModifierRarity.Uncommon => 5.0f,
                ModifierRarity.Rare => 8.0f,
                ModifierRarity.Legendary => 12.0f,
                _ => 2.0f
            };
            if (base.modifierViability == ModifierViability.Negative)
            {
                modifierVal = -(Mathf.Abs(modifierVal));
            }
            return modifierVal;
        }
    }
    // Common: 0.4
    // Uncommon: 1.0
    // Rare: 1.6
    // Legendary: 3.0

    public float SpeedModifierBonusPerLevel
    {
        get
        {
            float modifierVal = base.modifierRarity switch
            {
                ModifierRarity.Common => 0.4f,
                ModifierRarity.Uncommon => 1.0f,
                ModifierRarity.Rare => 1.6f,
                ModifierRarity.Legendary => 3.0f,
                _ => 0.4f
            };
            if (base.modifierViability == ModifierViability.Negative)
            {
                modifierVal = -(Mathf.Abs(modifierVal));
            }
return modifierVal;
        }
    }
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Swiftness";
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
                ModifierViability.Positive => "Increases speed by " + (int)SpeedModifierBonus,
                ModifierViability.Negative => "Decreases speed by " + (int)SpeedModifierBonus,
                _ => "Alters speed by " + (int)SpeedModifierBonus,
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public float SpeedModifierBonus
    {
        get
        {
            return BaseSpeedModifierBonus + (SpeedModifierBonusPerLevel * modifierLevel);
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