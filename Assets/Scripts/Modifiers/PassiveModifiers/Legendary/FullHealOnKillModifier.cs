using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullHealOnKillModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    private Limb baseLimb;
    private Combatant baseCombatant;

    public float baseHealChancePerKill = 0.2f;
    public float healChanceBonusPerLevel = 0.05f;

    public float HealChancePerKill
    {
        get
        {
            return baseHealChancePerKill + (healChanceBonusPerLevel * (this.modifierLevel - 1));
        }
        set
        {
            baseHealChancePerKill = value;
        }
    }

    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Wellbeing";
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
            return "Attacks with this limb have a " + (int)(HealChancePerKill * 100) + "% chance to fully heal on kill";
        }
        set
        {
            customModifierDescription = value;
        }
    }

    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {
        baseLimb = limb;
        baseCombatant = combatant;
        CombatManager.Instance.CombatantDied.AddListener(OnCombatantDeath);
    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {
        baseLimb = null;
        baseCombatant = null;
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);
    }
    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }

    private IEnumerator FullHealOnKill(Combatant combatant, Damage healthToHeal)
    {
        yield return new WaitForSeconds(0.1f);
        combatant.ApplyDamage(healthToHeal);
    }
    private void OnCombatantDeath(Combatant combatant, Damage damage)
    {
        if (combatant == baseCombatant && damage.usedLimbs.Contains(baseLimb))
        {
            // Roll the die to see if we heal
            if (UnityEngine.Random.value < HealChancePerKill)
            {
                float healthDifference = baseCombatant.MaxHealth - baseCombatant.remainingHealth;
                Damage dmg = new(baseCombatant, baseCombatant, new List<Limb>(), new Tuple<DamageType, float>(DamageType.True, -(Mathf.Abs(healthDifference))), 0, false, false);
                baseCombatant.StartCoroutine(FullHealOnKill(baseCombatant, dmg));

            }
        }
    }
}
