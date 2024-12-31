using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Limb", menuName = "Scriptable Objects/Limb")]
public class Limb : ScriptableObject
{
    public string limbName = "Limb";
    public Attack attackData;
    public DamageType damageType = DamageType.Physical;
    public List<BaseModifier> modifiers;
    public Sprite limbSprite;
    public int level = 1;
    public float baseRange = 5.0f;
    public float Range
    {
        get
        {
            float range = baseRange;
            if (range < 0) range = 0;
            // TODO: Range mods here
            return range;
        }
        set
        {
            baseRange = value;
        }
    }
    public float baseRadius = 1.0f;
    public float Radius
    {
        get
        {
            float radius = baseRadius;
            if (radius < 0) radius = 0;
            return radius;
        }
        set
        {
            baseRadius = value;
        }
    }

    public float baseDamage = 10.0f;
    public float Damage
    {
        get
        {
            float damage = (baseDamage * (Mathf.Pow(1.3f, level - 1)));
            float damageMultiplier = 1.0f;
            foreach (BaseModifier modifier in modifiers)
            {
                // If the modifier is of type AttackDamageModifier, apply it.
                if (modifier.GetType() == typeof(AttackDamageModifier))
                {
                    AttackDamageModifier adModifier = (AttackDamageModifier)modifier;
                    damageMultiplier += adModifier.DamageModifierBonus;
                }
            }
            damage *= damageMultiplier;
            return damage;
        }
        set { baseDamage = value; }
    }
    public Color originCreatureBloodColor = new(0.47059f, 0.02353f, 0.02353f);

    public void LevelUp()
    {
        SetLevel(level + 1);
        // TODO: Roll for modifiers and stuff here
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
        List<BaseModifier> modifiers = GetModifiers();
        foreach(BaseModifier modifier in modifiers)
        {
            modifier.LevelUpModifier(newLevel);
        }
    }
    public List<BaseModifier> GetModifiers()
    {
        return modifiers;
    }

    public static int GetRandomLimbModifierCount(int level) // Determines how many modifiers the limb should have
    {
        int modifierCount = 0;

        // Base modifier chance
        if (Random.value < 0.5f) modifierCount++; // Base 50% chance of a modifier
        if (level > 5) // 6+
        {
            //
            float chance = Mathf.Min(((float)(level - 4) * 0.1f), 0.5f);
            if (Random.value < chance) modifierCount++;
        }
        if (level > 10) // 11+
        {
            float chance = Mathf.Min(((float)(level - 9) * 0.05f), 0.5f);
            if (Random.value < chance) modifierCount++;
        }
        
        return modifierCount;
    }
    public static BaseModifier GenerateRandomModifier(int level)
    {
        // Randomize the modifier rarity. The higher the level, the better the chance of a high rarity. 

        ModifierRarity rarity = ModifierRarity.Common;
        int common_weight = Mathf.Max(1000 - (level * 50), 100);
        int uncommon_weight = Mathf.Max(Mathf.Min((level - 3) * 100, 300), 0);
        int rare_weight = Mathf.Max(Mathf.Min((level - 6) * 100, 300), 0);
        int legendary_weight = Mathf.Max(Mathf.Min((level - 9) * 100, 300), 0);

        int rarityRoll = Random.Range(0, common_weight + uncommon_weight + rare_weight + legendary_weight);
        if (rarityRoll < common_weight) rarity = ModifierRarity.Common;
        else if (rarityRoll < common_weight + uncommon_weight) rarity = ModifierRarity.Uncommon;
        else if (rarityRoll < common_weight + uncommon_weight + rare_weight) rarity = ModifierRarity.Rare;
        else rarity = ModifierRarity.Legendary;

        // Determine if the modifier is negative or not. You can't get negative modifiers before level 3, but have a 10% chance after.
        ModifierViability viability = ModifierViability.Positive;
        if (level > 3)
        {
            int viabilityRoll = Random.Range(0, 10);
            if (viabilityRoll == 0) viability = ModifierViability.Negative;
        }

        // Determine which modifier we're actually getting
        // If legendary, flip a coin for if it's a legendary tier normal modifier or a unique legendary modifier. It can't be legendary if it's negative
        bool isUniqueLegendary = (rarity == ModifierRarity.Legendary && viability != ModifierViability.Negative && Random.value < 0.5f);
        BaseModifier newModifier = null;
        if (isUniqueLegendary)
        {
            newModifier = Random.Range(0, 6) switch
            {
                0 => ScriptableObject.CreateInstance<AllDamageTrueModifier>(),
                1 => ScriptableObject.CreateInstance<PermanentDamageRampModifier>(),
                2 => ScriptableObject.CreateInstance<PermanentHealthRampModifier>(),
                3 => ScriptableObject.CreateInstance<PermanentCritChanceRampModifier>(),
                4 => ScriptableObject.CreateInstance<InstantKillChanceModifier>(),
                5 => ScriptableObject.CreateInstance<FullHealOnKillModifier>(),
                _ => ScriptableObject.CreateInstance<AllDamageTrueModifier>(),
            };
        }
        else
        {
            newModifier = Random.Range(0,6) switch
            {
                0 => ScriptableObject.CreateInstance<AttackDamageModifier>(),
                1 => ScriptableObject.CreateInstance<CritChanceModifier>(),
                2 => ScriptableObject.CreateInstance<DamageResistanceModifier>(),
                3 => ScriptableObject.CreateInstance<HealthModifier>(),
                4 => ScriptableObject.CreateInstance<SpeedModifier>(),
                5 => ScriptableObject.CreateInstance<DodgeChanceModifier>(),
                _ => ScriptableObject.CreateInstance<HealthModifier>(),
            };
        }
        newModifier.modifierViability = viability;
        newModifier.modifierRarity = rarity;

        newModifier.LevelUpModifier(level);
        
        
        return newModifier;
    }
    public static Limb RandomizeBaseLimb(Limb inLimb)
    {
        // Level up the limb to the player's level
        inLimb.level = (int)PlayerController.Instance.combatant.level;
        // Determine the number of modifiers the limb should have. It should be between 0 and 3.
        int modifierCount = Limb.GetRandomLimbModifierCount(inLimb.level);
        List<BaseModifier> newModifiers = new();
        for (int i = 0; i < modifierCount; i++)
        {
            newModifiers.Add(Limb.GenerateRandomModifier(inLimb.level));
        }
        // Remove nulls from newModifiers
        newModifiers.RemoveAll(mod => mod == null);
        inLimb.modifiers = newModifiers;
        return inLimb;
    }

    public static Limb RegenerateLimbForLoot(Limb limb, int level) // TODO: Flesh this out
    {
        Limb newLimb = Instantiate(limb);
        newLimb.baseDamage = Random.Range(10.0f, 15.0f);
        // Randomize the attack type and damage type
        switch (Random.Range(0, 4))
        {
            case 0: // Point
                newLimb.attackData = ScriptableObject.CreateInstance<PointAttack>();
                newLimb.baseDamage *= 1.1f;
                newLimb.baseRange = Random.Range(1.0f, 13.0f);
                newLimb.baseRadius = 1.0f;
                break;
            case 1: // Sphere
                newLimb.attackData = ScriptableObject.CreateInstance<CircleAttack>();
                newLimb.baseDamage *= 0.85f;
                newLimb.baseRange = Random.Range(5.0f, 8.0f);
                newLimb.baseRadius = Random.Range(1.5f, 3.5f);
                break;
            case 2: // Line
                newLimb.attackData = ScriptableObject.CreateInstance<LineAttack>();
                newLimb.baseDamage *= 0.95f;
                newLimb.baseRange = Random.Range(3.0f, 10.0f);
                newLimb.baseRadius = 1.0f;
                break;
            case 3: // Cone
                newLimb.attackData = ScriptableObject.CreateInstance<ConeAttack>();
                newLimb.baseDamage *= 0.9f;
                newLimb.baseRange = Random.Range(3.0f, 6.0f);
                newLimb.baseRadius = Random.Range(45.0f, 120.0f); // Remember that cones use radius as an angle, so the multiplier needs to be much higher (aiming for 45 degrees minimum).
                break;
            default: // Do point anyways
                newLimb.attackData = ScriptableObject.CreateInstance<PointAttack>();
                newLimb.baseDamage *= 1.1f;
                newLimb.baseRange = Random.Range(1.0f, 13.0f);
                newLimb.baseRadius = 1.0f;
                break;
        }
        /* Randomize the damage type with the following weights:
         * Physical: 20 
         * Fire: 15
         * Ice: 15
         * Lightning: 10 
         * Poison: 8
         * Blood: 5
         * Radiant: 2
         * Void: 2
         * */
        /*
        int damageTypeRoll = Random.Range(0, 77);
        if (damageTypeRoll < 20) newLimb.damageType = DamageType.Physical;
        else if (damageTypeRoll < 35) newLimb.damageType = DamageType.Fire;
        else if (damageTypeRoll < 50) newLimb.damageType = DamageType.Ice;
        else if (damageTypeRoll < 60) newLimb.damageType = DamageType.Lightning;
        else if (damageTypeRoll < 68) newLimb.damageType = DamageType.Poison;
        else if (damageTypeRoll < 73) newLimb.damageType = DamageType.Blood;
        else if (damageTypeRoll < 75) newLimb.damageType = DamageType.Radiant;
        else newLimb.damageType = DamageType.Void;
        */
        // Determine the number of modifiers the limb should have. It should be between 0 and 3.
        int modifierCount = Limb.GetRandomLimbModifierCount(level);
        List<BaseModifier> newModifiers = new();
        for (int i = 0; i < modifierCount; i++)
        {
            newModifiers.Add(Limb.GenerateRandomModifier(level));
        }
        // Remove any modifiers from newModifiers that are null
        newModifiers.RemoveAll(mod => mod == null);
        newLimb.modifiers = newModifiers;

        return newLimb;
    }

    private void Awake()
    {
        // Instantiate copies of all modifiers on me if in playmode
        if (Application.isPlaying)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i] = Instantiate(modifiers[i]);
            }
        }
    }
    private void OnDestroy()
    {
        // Destroy all modifiers on me if in playmode
        if (Application.isPlaying)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                Destroy(modifiers[i]);
            }
        }
    }
}