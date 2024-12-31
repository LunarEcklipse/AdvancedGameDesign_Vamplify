using UnityEngine;

public enum ModifierType
{
    Unset, // Default value
    Passive, // Changes stats
    Active // Changes the way an item works
}
public enum ModifierViability
{
    Positive, // Buffs, green text
    Neutral, // Double-edged or situational which change the way the item works, white text
    Negative // Debuffs, red text
}

public enum  ModifierRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public abstract class BaseModifier : ScriptableObject
{
    protected string customModifierTitle;
    public virtual string ModifierTitle { get; set; }
    protected string customModifierDescription;
    public virtual string ModifierDescription { get; set; }
    public readonly ModifierType modifierType = ModifierType.Unset;
    public ModifierViability modifierViability = ModifierViability.Neutral;
    public ModifierRarity modifierRarity = ModifierRarity.Common;
    public int modifierLevel = 1;
    public bool isUniqueLegendary = false;
    public abstract void LevelUpModifier(int newLevel); // Used to increase the stats of an individual modifier.
    public void LevelUpModifier() // Shorthand for += 1
    {
        LevelUpModifier(modifierLevel + 1);
    }
    public static string GetModifierRarityTitle(ModifierRarity rarity)
    {
        return rarity switch
        {
            ModifierRarity.Common => "Trivial",
            ModifierRarity.Uncommon => "Modest",
            ModifierRarity.Rare => "Impressive",
            ModifierRarity.Legendary => "Extraordinary",
            _ => "Uncertain"
        };
    }

    public static string GetModifierViabilityTitle(ModifierViability viability)
    {
        return viability switch
        {
            ModifierViability.Positive => "Boon",
            ModifierViability.Neutral => "Effect",
            ModifierViability.Negative => "Curse",
            _ => "Uncertain"
        };
    }
}
