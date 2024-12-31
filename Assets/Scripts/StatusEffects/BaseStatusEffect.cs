using System;
using UnityEngine;

public class BaseStatusEffect : IEquatable<BaseStatusEffect>
{
    public string effectName = "Status Effect";
    public ModifierViability viability = ModifierViability.Neutral;
    public float effectChance = 0.1f;
    public float effectIntensity = 1.0f;
    public int effectDuration = 1;
    public int turnsRemaining;

    protected Combatant victim;

    /* *** OVERLOAD OPERATORS *** */
    public static bool operator ==(BaseStatusEffect a, BaseStatusEffect b) => System.Object.ReferenceEquals(a, b);
    public static bool operator !=(BaseStatusEffect a, BaseStatusEffect b) => !System.Object.ReferenceEquals(a, b);
    public bool Equals(BaseStatusEffect other) => this == other;
    public override bool Equals(object obj)
    {
        if (obj is BaseStatusEffect) return base.Equals(obj);
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(effectName, viability, effectChance, effectIntensity, effectDuration, turnsRemaining, victim);
    }
    public BaseStatusEffect(Combatant inVictim, string inEffectName, ModifierViability inViability, float inEffectChance, float inEffectIntensity, int inEffectDuration)
    {
        victim = inVictim;
        effectName = inEffectName;
        viability = inViability;
        effectChance = inEffectChance;
        effectIntensity = inEffectIntensity;
        effectDuration = inEffectDuration;
    }
    public virtual void OnEffectApply()
    {
        turnsRemaining = effectDuration;
    }

    public virtual void OnEffectExpire()
    {

    }

    public virtual void OnTurnStartWithEffect()
    {

    }

    public virtual void OnTurnEndWithEffect()
    {
        turnsRemaining--;
    }
    public static bool ShouldEffectBeApplied(float effectChance)
    {
        return UnityEngine.Random.value <= effectChance;
    }

    public virtual void ApplyEffect()
    {

    }
    public virtual void RemoveEffect()
    {

    }
    public bool ShouldBeDestroyed()
    {
        return turnsRemaining <= 0;
    }
}