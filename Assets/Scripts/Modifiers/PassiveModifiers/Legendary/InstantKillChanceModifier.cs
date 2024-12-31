using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillChanceModifier : PassiveModifier
{
    public new bool isUniqueLegendary = true;
    private Limb baseLimb;
    private Combatant baseCombatant;

    public float baseKillChancePerAttack = 0.05f;
    public float killChancePerAttackPerLevel = 0.025f;

    public float KillChancePerAttack
    {
        get
        {
            return baseKillChancePerAttack + (killChancePerAttackPerLevel * (this.modifierLevel - 1));
        }
        set
        {
            baseKillChancePerAttack = value;
        }
    }

    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Smiting";
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
            return "Attacks with this limb have a " + (int)(KillChancePerAttack * 100) + "% chance to instantly kill a target.";
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
        CombatManager.Instance.CombatantAttacked.AddListener(OnCombatantAttacked);
    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {
        baseLimb = null;
        baseCombatant = null;
        CombatManager.Instance.CombatantAttacked.RemoveListener(OnCombatantAttacked);
    }
    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }

    private IEnumerator KillTarget(Combatant combatant, Damage lethalDamage)
    {
        yield return new WaitForSeconds(0.1f);
        combatant.ApplyDamage(lethalDamage);
    }

    private void OnCombatantAttacked(Combatant victim, Damage damage)
    {
        if (damage.attacker == baseCombatant && damage.usedLimbs.Contains(baseLimb))
        {
            if (UnityEngine.Random.value < KillChancePerAttack)
            {
                Damage lethalDamage = new(baseCombatant, victim, new List<Limb>(), new Dictionary<DamageType, float> { { DamageType.True, victim.MaxHealth } }, 0, false, false);
                victim.StartCoroutine(KillTarget(victim, lethalDamage));
            }
        }
    }
}