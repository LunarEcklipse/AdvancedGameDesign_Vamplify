using System;

public class PoisonStatusEffect : BaseStatusEffect
{
    Damage damagePerTurn;
    public PoisonStatusEffect(Combatant inVictim, string inEffectName, ModifierViability inViability, float inEffectChance, float inEffectIntensity, int inEffectDuration) : base(inVictim, inEffectName, inViability, inEffectChance, inEffectIntensity, inEffectDuration)
    {
        damagePerTurn = new Damage(victim, victim, new(), new System.Tuple<DamageType, float>(DamageType.Poison, inEffectIntensity), 0, false, false);
    }

    public override void OnTurnEndWithEffect()
    {
        victim.ApplyDamage(damagePerTurn);
        base.OnTurnEndWithEffect();
    }
}