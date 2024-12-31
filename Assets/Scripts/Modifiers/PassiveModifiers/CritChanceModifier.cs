using UnityEngine;

[CreateAssetMenu(fileName = "Crit Chance Modifier", menuName = "Scriptable Objects/Passive Modifiers/Crit Chance Modifier", order = 1)]
public class CritChanceModifier : PassiveModifier
{
    public float baseCritChanceModifierBonus = 0.15f;
    public float critChanceModifierBonusPerLevel = 0.05f;
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Precision";
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
                ModifierViability.Positive => "Increases critical hit chance by " + (CritChanceModifierBonus * 100.0f),
                ModifierViability.Negative => "Decreases critical hit chance by " + -(CritChanceModifierBonus * 100.0f),
                _ => "Alters critical hit chance by " + (CritChanceModifierBonus * 100.0f),
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public float CritChanceModifierBonus
    {
        get
        {
            return baseCritChanceModifierBonus + (critChanceModifierBonusPerLevel * modifierLevel);
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

    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }
}