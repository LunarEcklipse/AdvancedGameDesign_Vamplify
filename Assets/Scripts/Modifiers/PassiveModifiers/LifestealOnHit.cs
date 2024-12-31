using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LifestealOnHit", menuName = "Scriptable Objects/Passive Modifiers/Lifesteal On Hit", order = 1)]
public class LifestealOnHit : PassiveModifier
{
    public float baseLifestealPercentage = 0.2f;
    private float EffectiveLifestealPercentage { get { return baseLifestealPercentage + (lifestealScalingPerLevel * modifierLevel); } }
    public float lifestealScalingPerLevel = 0.2f;
    private Limb parentLimb;
    private Combatant parentCombatant;

    public override string ModifierTitle
    {
        get
        {
            if (customModifierTitle != null) return customModifierTitle;
            return BaseModifier.GetModifierRarityTitle(modifierRarity) + " " + BaseModifier.GetModifierViabilityTitle(modifierViability) + " of Lifesteal";
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
                ModifierViability.Positive => "Heals for " + (EffectiveLifestealPercentage * 100.0f) + "% of the damage dealt",
                ModifierViability.Negative => "Drains for " + -(EffectiveLifestealPercentage * 100.0f) + "% of the damage dealt",
                _ => "Alters lifesteal by " + (EffectiveLifestealPercentage * 100.0f) + "%",
            };
        }
        set
        {
            customModifierDescription = value;
        }
    }
    public override void OnApplyModifier(Limb limb, Combatant combatant)
    {
        parentLimb = limb;
        parentCombatant = combatant;
    }
    public override void OnRemoveModifier(Limb limb, Combatant combatant)
    {
        parentLimb = null;
        parentCombatant = null;
    }
    private void OnEnable()
    {
        // Check if we are in playmode or a proper build
        if (!Application.isPlaying) return;
        CombatManager.Instance.CombatantAttacked.AddListener(OnHitEnemy);
    }
    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        CombatManager.Instance.CombatantAttacked.RemoveListener(OnHitEnemy);
    }
    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        CombatManager.Instance.CombatantAttacked.RemoveListener(OnHitEnemy);
    }
    private IEnumerator HealAfterDelay(Damage damage)
    {
        yield return new WaitForSeconds(0.5f);
        // Determine the total damage dealt to the victim by the attack
        float totalDamageDealt = damage.TotalDamage;
        float lifestealAmount = -((float)totalDamageDealt * EffectiveLifestealPercentage);
        lifestealAmount = Mathf.Min(lifestealAmount, 0.0f); // Ensures that we don't damage the attacker
                                                            // Deal that as negative true damage to the attacker
        Tuple<DamageType, float> lifestealDamage = new(DamageType.True, lifestealAmount);
        parentCombatant.ApplyDamage(new Damage(damage.attacker, damage.attacker, new(), lifestealDamage, 0, false));
    }
    private void OnHitEnemy(Combatant victim, Damage damage)
    {
        if (damage.attacker == damage.defender) return; // If the attacker is the defender, we don't want to lifesteal. This prevents exploits and infinite loops.
        // If the attacker is the parent combatant, we activate.
        if (damage.attacker == parentCombatant && parentCombatant.IsLimbEngaged(parentLimb))
        {
            damage.attacker.StartCoroutine(HealAfterDelay(damage));
        }
    }
    public override void LevelUpModifier(int newLevel)
    {
        base.modifierLevel = newLevel;
    }
}