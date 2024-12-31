using UnityEngine;

[CreateAssetMenu(fileName = "Dodge Modifier", menuName = "Scriptable Objects/Passive Modifiers/Dodge Modifier", order = 1)]
public class DodgeChanceModifier : PassiveModifier
{
    public float baseDodgeModifierBonus = 0.15f;
    public float dodgeModifierBonusPerLevel = 0.05f;
    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Evasion";
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
                ModifierViability.Positive => "Increases dodge chance by " + (DodgeModifierBonus * 100.0f) + "%",
                ModifierViability.Negative => "Decreases dodge chance by " + -(DodgeModifierBonus * 100.0f) + "%",
                _ => "Alters dodge chance by " + (DodgeModifierBonus * 100.0f) + "%",
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public float DodgeModifierBonus { get
        {
            return baseDodgeModifierBonus + (dodgeModifierBonusPerLevel * modifierLevel);
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