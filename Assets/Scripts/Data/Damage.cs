using System;
using System.Collections.Generic;
using System.Linq;
public class Damage
{
    public Combatant attacker;
    public Combatant defender;
    public List<Limb> usedLimbs;
    public Dictionary<DamageType, float> damages;
    
    public float TotalDamage
    {
        get
        {
            return damages.Values.Sum();
        }
    }
    public int critCount;
    public bool dodgedAttack;
    public List<BaseStatusEffect> statusEffects;
    public bool isDirectAttack = true; // If this is false we know this was created by another attack and to not process onattack effects.
    public Damage(Combatant inAttacker, Combatant inDefender, List<Limb> usedLimbs, Dictionary<DamageType, float> inDamage, int inCritCount = 0, bool inDodged = false, bool isDirectAttack = true)
    {
        attacker = inAttacker;
        defender = inDefender;
        this.usedLimbs = usedLimbs;
        damages = inDamage;
        critCount = inCritCount;
        dodgedAttack = inDodged;
        this.isDirectAttack = isDirectAttack;
    }

    public Damage(Combatant inAttacker, Combatant inDefender, List<Limb> usedLimbs, Tuple<DamageType, float> inDamage, int inCritCount = 0, bool inDodged = false, bool isDirectAttack = true)
    {
        attacker = inAttacker;
        defender = inDefender;
        this.usedLimbs = usedLimbs;
        damages = new()
        {
            { inDamage.Item1, inDamage.Item2 }
        };
        critCount = inCritCount;
        dodgedAttack = inDodged;
        this.isDirectAttack = isDirectAttack;
    }
    public bool IsBeneficial()
    {
        return TotalDamage < 0.0f;
    }

    /* *** DAMAGE CALCULATORS *** */

    public static int CalculateCritCount(Combatant attacker, Combatant defender)
    {
        int critCount = 0;
        if (attacker.CritChance > 1.0f)
        {
            critCount = (int)attacker.CritChance;
            float tempCritChance = attacker.CritChance - critCount;
            if (UnityEngine.Random.value < tempCritChance) critCount++;
        }
        else if (attacker.CritChance < -1.0f)
        {
            critCount = (int)attacker.CritChance;
            float tempCritChance = attacker.CritChance - critCount;
            if (UnityEngine.Random.value < tempCritChance) critCount--;
        }
        else if (UnityEngine.Random.value < attacker.CritChance) critCount = 1;
        else if (UnityEngine.Random.value < -attacker.CritChance) critCount = -1;
        return critCount;
    }
    public DamageType GetLargestDamageType() // Used for audio primarily
    {
        DamageType largestType = DamageType.Physical;
        float largestValue = 0.0f;
        foreach (DamageType key in damages.Keys)
        {
            if (damages[key] > largestValue)
            {
                largestValue = damages[key];
                largestType = key;
            }
        }
        return largestType;
    }

    public static Damage CalculateDamageAgainstTarget(Combatant attacker, Combatant defender)
    {
        Dictionary<DamageType, float> baseDamage = attacker.GetAllDamageOfEngagedLimbs();
        float accuracy = 1.0f;
        float enemyDodgeChance = defender.DodgeChance;
        if (UnityEngine.Random.value * accuracy < enemyDodgeChance) return new Damage(attacker, defender, attacker.GetEngagedLimbs(), baseDamage, 0, true); // The enemy dodged the attack.
        // TODO: Add weakness/resistance calculations here.
        int critCount = CalculateCritCount(attacker, defender);
        // Multiply the damage by 2 for each crit, or by 0.5 for each negative crit.
        if (critCount > 0)
        {
            Dictionary<DamageType, float> dictCopy = new(baseDamage);
            foreach (int i in System.Linq.Enumerable.Range(0, critCount))
            {
                foreach (DamageType key in baseDamage.Keys)
                {
                    dictCopy[key] = baseDamage[key] * 2.0f;
                }
            }
            baseDamage = dictCopy;
        }
        else if (critCount < 0)
        {
            Dictionary<DamageType, float> dictCopy = new(baseDamage);
            foreach (int i in System.Linq.Enumerable.Range(0, -critCount))
            {
                foreach (DamageType key in baseDamage.Keys)
                {
                    dictCopy[key] = baseDamage[key] * 0.5f;
                }
            }
            baseDamage = dictCopy;
        }
        return new Damage(attacker, defender, attacker.GetEngagedLimbs(), baseDamage, critCount, false);
    }
}