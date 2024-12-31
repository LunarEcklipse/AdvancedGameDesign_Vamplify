using UnityEngine;
using System.Collections.Generic;
using System;

public class DamageModifierTable
{
    public Dictionary<DamageType, float> damageMultipliers = new();
    public DamageModifierTable()
    {
        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            damageMultipliers.Add(type, 1.0f);
        }
    }
    public DamageModifierTable(Dictionary<DamageType, float> multipliers)
    {
        damageMultipliers = multipliers;
    }

    public DamageModifierTable GetCopy()
    {
        return new DamageModifierTable(new Dictionary<DamageType, float>(damageMultipliers));
    }

    public bool IsImmuneToDamageType(DamageType type)
    {
        return damageMultipliers[type] == 0.0f;
    }
    public float GetDamageModifierByType(DamageType type)
    {
        if (!damageMultipliers.ContainsKey(type))
        {
            return 1.0f;
        }
        return damageMultipliers[type];
    }

    public void SetDamageModifier(DamageType type, float modifier)
    {
        if (damageMultipliers.ContainsKey(type))
        {
            damageMultipliers[type] = modifier;
        }
        else
        {
            damageMultipliers.Add(type, modifier);
        }
    }
}