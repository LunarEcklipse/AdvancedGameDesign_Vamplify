using UnityEngine;

[CreateAssetMenu(fileName = "Attack Damage Modifier", menuName = "Scriptable Objects/Passive Modifiers/Attack Damage Modifier", order = 1)]
public class AttackDamageModifier : PassiveModifier
{
    // Common: 10%
    // Uncommon: 25%
    // Rare: 50%
    // Legendary: 100%
    public float BaseAttackDamageBonus
    {
        get
        {
            float modifierVal = base.modifierRarity switch
            {
                ModifierRarity.Common => 0.1f,
                ModifierRarity.Uncommon => 0.25f,
                ModifierRarity.Rare => 0.5f,
                ModifierRarity.Legendary => 0.8f,
                _ => 0.1f
            };
            if (base.modifierViability == ModifierViability.Negative)
            {
                modifierVal = -(Mathf.Abs(modifierVal));
            }
            return modifierVal;
        }
    }
    // Common: 0.01f
    // Uncommon: 0.05f,
    // Rare: 0.1f,
    // Legendary: 0.2f
    public float AttackDamageBonusPerLevel
    {
        get
        {
            float modifierVal = base.modifierRarity switch
            {
                ModifierRarity.Common => 0.01f,
                ModifierRarity.Uncommon => 0.05f,
                ModifierRarity.Rare => 0.1f,
                ModifierRarity.Legendary => 0.2f,
                _ => 0.01f
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
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Damage";
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
                ModifierViability.Positive => "Increases damage this limb deals by " + (DamageModifierBonus * 100.0f),
                ModifierViability.Negative => "Decreases damage this limb deals by " + -(DamageModifierBonus * 100.0f),
                _ => "Alters damage this limb deals by " + (DamageModifierBonus * 100.0f),
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public float DamageModifierBonus
    {
        get
        {
            return BaseAttackDamageBonus + (AttackDamageBonusPerLevel * modifierLevel);
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