using System;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    [Header("Is Player")]
    public bool isPlayer = false;
    [Header("Base Stats")]
    public float baseMaxHealth = 100.0f;
    public float MaxHealth
    {
        get
        {
            float editableBase = baseMaxHealth;
            editableBase = (editableBase * (Mathf.Pow(1.3f, (int)level - 1)));
            float totalModifier = 1.0f;
            foreach (var limb in limbs.Values)
            {
                foreach (var modifier in limb.GetModifiers())
                {
                    // Check if the modifier is of the type HealthBonus
                    if (modifier.GetType() == typeof(HealthModifier))
                    {
                        HealthModifier bns = (HealthModifier)modifier;
                        totalModifier += bns.HealthModifierBonus;
                    }
                }
            }
            if ((editableBase * totalModifier) < 1.0f) return 1.0f;
            return editableBase * totalModifier;
        }
        set
        {
            if (value < 1.0f) baseMaxHealth = 1.0f;
            else baseMaxHealth = value;
            if (remainingHealth > MaxHealth) remainingHealth = MaxHealth;
        }
    }
    [System.NonSerialized] public float remainingHealth;
    public float level = 1.0f;
    public float baseSpeed = 5.0f;
    public float Speed
    {
        get
        {
            float totalSpeed = baseSpeed;
            foreach (var limb in limbsOnStart)
            {
                foreach (var modifier in limb.GetModifiers())
                {
                    if (modifier.GetType() == typeof(SpeedModifier))
                    {
                        SpeedModifier bns = (SpeedModifier)modifier;
                        totalSpeed += bns.SpeedModifierBonus;
                    }
                }
            }
            if (totalSpeed < 1.0f) return 1.0f;
            return totalSpeed;
        }
        set { baseSpeed = value; }
    }
    [Tooltip("The chance for an enemy to dodge an attack. 1.0 = 100%. Can exceed 100% or drop below 0%.")] public float baseDodgeChance = 0.05f;
    public float DodgeChance
    {
        get
        {
            float totalModifier = baseDodgeChance;
            foreach (var limb in limbs.Values)
            {
                foreach (var modifier in limb.GetModifiers())
                {
                    // Check if the modifier is of the type DodgeModifier
                    if (modifier.GetType() == typeof(DodgeChanceModifier))
                    {
                        DodgeChanceModifier bns = (DodgeChanceModifier)modifier;
                        totalModifier += bns.DodgeModifierBonus;
                    }
                }
            }
            if (totalModifier < 0.0f) return 0.0f;
            else if (totalModifier > 1.0f) return 1.0f;
            return totalModifier;
        }
        set
        {
            baseDodgeChance = value;
        }
    }
    public float baseCritChance = 0.05f;
    public float CritChance
    {
        get
        {
            float totalModifier = baseCritChance;
            foreach (var limb in limbs.Values)
            {
                foreach (var modifier in limb.GetModifiers())
                {
                    // Check if the modifier is of the type CritChanceModifier
                    if (modifier.GetType() == typeof(CritChanceModifier))
                    {
                        CritChanceModifier bns = (CritChanceModifier)modifier;
                        totalModifier += bns.CritChanceModifierBonus;
                    }
                }
            }
            return totalModifier;
        }
        set
        {
            baseCritChance = value;
        }
    }
    [Tooltip("Whether or not the combatant is currently dead.")] public bool isDead = false;
    [Tooltip("Whether or not it is currently this combatant's turn.")] public bool isTurn = false;
    [Header("Limbs")]
    [Tooltip("The maximum number of limbs the Combatant can have.")]public int maxLimbs = 4;
    [Tooltip("The limbs to populate this combatant with when spawned. This list is only referenced upon spawn, after which it is no longer updated.")]public List<Limb> limbsOnStart = new();
    public Dictionary<int, Limb> limbs = new();
    private List<Limb> engagedLimbs = new();
    private Limb primaryLimb;
    [NonSerialized]public bool isInAttackMode = false;
    [Tooltip("The enemy data if this is not a player.")] public EnemyData enemyData;
    [Tooltip("The weaknesses and resistances downloaded from the enemydata if any.")] public DamageModifierTable damageMultipliers;
    [NonSerialized] public float combatTiebreakModifier = 0.0f;

    public List<BaseStatusEffect> statusEffects = new();

    public bool CanThisCombatantAttack() { return !isDead && limbs.Count != 0; }
    /* *** LIMBS *** */
    public void SetLimbsInBulk(List<Limb> limbList) // This function is exposed so that controllers can assign limbs to combatants in an easy manner.
    {
        // If the limb list passed in is longer than the maximum number of limbs, raise a warning and only add limbs up to the maximum.
        // First clear the list if it's not empty.
        if (limbs.Count > 0) ClearLimbs();
        if (limbList.Count > maxLimbs) Debug.LogWarning("Attempted to assign more limbs to a combatant than the maximum number of limbs. Only the first " + maxLimbs + " limbs will be assigned.");
        for (int i = 0; i < Mathf.Min(limbList.Count, maxLimbs); i++)
        {
            if (limbList[i] == null)
            {
                Debug.LogError("LimbList at position " + i + " is null on " + name);
                continue;
            }
            Limb newLimb = Instantiate(limbList[i]);
            newLimb.SetLevel((int)PlayerController.Instance.combatant.level);
            if (!isPlayer)
            {
                newLimb = Limb.RandomizeBaseLimb(newLimb);
            }
            AddLimbToDict(newLimb, i);
        }
    }
    public int GetLimbCount() { return limbs.Count; }
    private int GetFirstAvailableLimbPosition() // Gets the first available position in the dictionary without a limb. Returns -1 if there's no space for a limb.
    {
        for (int i = 0; i < maxLimbs; i++)
        {
            if (!limbs.ContainsKey(i)) return i;
        }
        return -1;
    }
    public bool IsRoomForLimb() { return GetFirstAvailableLimbPosition() != -1; }
    public bool IsLimbInDict(Limb limb) { return limbs.ContainsValue(limb); }
    public int GetLimbPosition(Limb limb)
    {
        foreach (KeyValuePair<int, Limb> pair in limbs)
        {
            if (pair.Value == limb) return pair.Key;
        }
        return -1;
    }
    public Limb GetLimbAtPosition(int position)
    {
        if (limbs.ContainsKey(position)) return limbs[position];
        else return null;
    }
    public void AddLimbToDict(Limb limb, int position = -1)
    {
        if (limb == null)
        {
            Debug.LogError("Limb is null. Cannot add a null limb to a combatant.");
            return;
        }
        limb = Instantiate(limb);
        // If position is -1, add it to the first available position starting from 0.
        if (position >= maxLimbs) Debug.LogError("Tried to add a limb to a position that's outside of the maximum limb count.");
        if (position == -1) position = GetFirstAvailableLimbPosition();
        if (position != -1)
        {
            // If the limb is at this position already, replace. Otherwise, add.
            if (GetLimbAtPosition(position) != null)
            {
                limbs.Remove(position);
            }
            limbs.Add(position, limb);
            foreach (BaseModifier modifier in limb.GetModifiers())
            {
                // Check if the modifier is a passive modifier
                if (modifier.GetType().IsSubclassOf(typeof(PassiveModifier))) // Is passive modifier
                {
                    PassiveModifier passiveModifier = (PassiveModifier)modifier;
                    passiveModifier.OnApplyModifier(limb, this);
                }
            }
        } 
        else Debug.LogError("Attempted to add a limb to a combatant that already has the maximum number of limbs.");
    }
    public void RemoveLimbFromDict(Limb limb)
    {
        if (IsLimbInDict(limb))
        {
            int position = GetLimbPosition(limb);
            limbs.Remove(position);
            foreach (BaseModifier modifier in limb.GetModifiers())
            {
                // Check if the modifier is a passive modifier
                if (modifier.GetType().IsSubclassOf(typeof(PassiveModifier))) // Is passive modifier
                {
                    PassiveModifier passiveModifier = (PassiveModifier)modifier;
                    passiveModifier.OnRemoveModifier(limb, this);
                    Destroy(limb);
                }
            }
        }
        else Debug.LogError("Attempted to remove a limb from a combatant that does not have that limb.");
    }
    public void ReplaceLimb(Limb oldLimb, Limb newLimb)
    {
        if (!IsLimbInDict(oldLimb))
        {
            Debug.LogError("Attempted to replace a limb that this combatant does not have.");
            return;
        }
        int position = GetLimbPosition(oldLimb);
        limbs[position] = newLimb;
        Destroy(oldLimb);
    }
    public void ClearLimbs()
    {
        foreach (Limb limb in limbs.Values)
        {
            Destroy(limb);
        }
        limbs.Clear();
    }
    public bool IsLimbEngaged(Limb limb) { return engagedLimbs.Contains(limb); }
    public void EngageLimb(Limb limb)
    {
        if (IsLimbInDict(limb))
        {
            if (engagedLimbs.Contains(limb)) return; // This limb is already engaged.
            if (engagedLimbs.Count == 0) SetPrimaryLimb(limb); // Engage this limb and set it as the primary.
            engagedLimbs.Add(limb);
            SetPrimaryLimb(engagedLimbs[0]);
        }
        else
        {
            Debug.LogError("Attempted to engage a limb that this character does not have.");
        }
    }
    public void DisengageLimb(Limb limb)
    {
        if (IsLimbInDict(limb))
        {
            if (engagedLimbs.Contains(limb)) engagedLimbs.Remove(limb);
            else return; // This limb is not engaged.
            if (primaryLimb == limb) // If this limb was the primary, clear it and set the next limb as primary.
            {
                if (engagedLimbs.Count > 0) primaryLimb = engagedLimbs[0];
                else primaryLimb = null;
            }
        }
        else
        {
            Debug.LogError("Attempted to disengage a limb that this character does not have.");
        }
    }
    public List<Limb> GetEngagedLimbs() { return engagedLimbs; }
    public Limb GetFirstEngagedLimb() { return engagedLimbs.Count > 0 ? engagedLimbs[0] : null; }
    public bool IsAnyLimbsEngaged() { return engagedLimbs.Count > 0; }
    public void ClearEngagedLimbs()
    {
        engagedLimbs.Clear();
        ClearPrimaryLimb();
    }
    public Dictionary<DamageType, float> GetAllDamageOfEngagedLimbs() // TODO: Make this way better
    {
        Dictionary<DamageType, float> totalDamage = new();
        List<Limb> validLimbs = new();
        foreach (Limb limb in engagedLimbs)
        {
            if (limb != null) validLimbs.Add(limb);
        }
        int totalLimbs = validLimbs.Count;
        if (totalLimbs == 0) return totalDamage; // This avoids a divide by zero error.
        float damageMultiplier = 1.0f / (float)totalLimbs;
        foreach (Limb limb in validLimbs)
        {
            // Check if the limb's damage type already exists in the dictionary.
            if (totalDamage.ContainsKey(limb.damageType))
            {
                totalDamage[limb.damageType] += limb.Damage * damageMultiplier;
            }
            else
            {
                totalDamage.Add(limb.damageType, limb.Damage * damageMultiplier);
            }
        }
        return totalDamage;
    }
    /* *** *** PRIMARY LIMB *** *** */
    public Limb GetPrimaryLimb()
    {
        if (primaryLimb != null)
        {
            if (IsAnyLimbsEngaged()) primaryLimb = GetFirstEngagedLimb();
        }
        return primaryLimb;
    }
    public void SetPrimaryLimb(Limb limb)
    {
        // Check if the limb is in the dictionary.
        if (IsLimbInDict(limb))
        {
            primaryLimb = limb;
        }
        else
        {
            Debug.LogError("Attempted to set a primary limb that this character does not have.");
        }
    }
    public void ClearPrimaryLimb() { primaryLimb = null; }

    /* *** DATA FUNCTIONS *** */
    private string GetCombatantName() { return isPlayer ? "Player" : enemyData.enemyName; }
    public Vector3 GetCombatantPositionNoY() { return new Vector3(transform.position.x, 0.0f, transform.position.z); }

    /* *** ATTAC *** */
    public Damage CalculateDamage(Combatant target)
    {
        return Damage.CalculateDamageAgainstTarget(this, target);
    }

    public void ApplyDamage(Damage damage)
    {
        if (damage == null) return;
        if (isDead) return;
        DamageModifierTable effectiveDamageModifiers = GetDamageModifiers();
        float effectiveDamage = 0.0f;
        bool isAllDamageTrue = false;
        if (damage.dodgedAttack)
        {
            CombatManager.Instance.CombatantAttacked.Invoke(this, damage);
            return; // If the attack was dodged, don't apply damage.
        }
        foreach (Limb limb in damage.attacker.GetEngagedLimbs())
        {
            foreach (BaseModifier modifier in limb.GetModifiers())
            {
                if (modifier.GetType() == typeof(AllDamageTrueModifier))
                {
                    isAllDamageTrue = true;
                    break;
                }
            }
            if (isAllDamageTrue) break;
        }
        foreach (DamageType damageType in damage.damages.Keys)
        {
            if (damageType == DamageType.True || isAllDamageTrue) effectiveDamage += damage.damages[damageType]; // True damage does not get modulated in this way.
            else
            {
                // Get the resistance multiplier for this damage type
                effectiveDamage += damage.damages[damageType] * effectiveDamageModifiers.GetDamageModifierByType(damageType);
            }
        }
        remainingHealth -= effectiveDamage;
        CombatManager.Instance.CombatantAttacked.Invoke(this, damage);
        AudioManager.Instance.PlayHitSoundAtLocation(transform.position, damage.GetLargestDamageType(), damage.dodgedAttack, effectiveDamage < 0.0f);
        if (isPlayer) PlayerController.Instance.RefreshPlayerStats.Invoke();
        if (remainingHealth <= 0)
        {
            remainingHealth = 0;
            CombatManager.Instance.CombatantDied.Invoke(this, damage);
            if (isPlayer) CombatManager.Instance.PlayerDeath.Invoke();
            isDead = true;
        }
        else if (remainingHealth > MaxHealth) remainingHealth = MaxHealth; // For any situation where a heal occurs.
    }

    public DamageModifierTable GetDamageModifiers()
    {
        
        if (!isPlayer)
        {
            if(enemyData != null)
            {
                return enemyData.GetDamageModifierTable();
            }
        }
        DamageModifierTable table = new();
        // Iterate through limbs and search for damage type modifiers
        foreach (Limb limb in limbs.Values)
        {
            foreach(BaseModifier modifier in limb.GetModifiers())
            {
                if (modifier.GetType() == typeof(DamageResistanceModifier))
                {
                    DamageResistanceModifier dmgMod = (DamageResistanceModifier)modifier;
                    table.SetDamageModifier(dmgMod.damageType, (table.GetDamageModifierByType(dmgMod.damageType) - dmgMod.DamageTypeModifierBonus));
                }
            }
        }
        return table;
    }

    /* *** MONOBEHAVIOUR FUNCTIONS *** */
    private void Awake()
    {
        if (!isPlayer) { if (!TryGetComponent<EnemyData>(out enemyData)) { Debug.LogError($"Non-player combatant {this.name} does not have an EnemyData component."); } }
        limbs = new();
        gameObject.tag = "Combatant";
    }
    private void Start()
    {
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        CombatManager.Instance.CombatTurnStart.AddListener(OnCombatTurnStart);
        CombatManager.Instance.CombatTurnEnd.AddListener(OnCombatTurnEnd);
        CombatManager.Instance.CombatantDied.AddListener(OnCombatantDeath);
        if (!isPlayer) // Add self to position dictionary
        {
            PathNodeManager.Instance.SetCombatantPosition(this, (int)transform.position.x, (int)transform.position.z);
        }
        if (limbsOnStart.Count > 0) SetLimbsInBulk(limbsOnStart);
        remainingHealth = MaxHealth;
        if (!isPlayer && enemyData != null)
        {
            damageMultipliers = enemyData.TranslateEnemyDataToDamageModifierTable();
        }
    }

    private void OnDestroy()
    {
        CombatManager.Instance.CombatStart.RemoveListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.RemoveListener(OnCombatEnd);
        CombatManager.Instance.CombatTurnStart.RemoveListener(OnCombatTurnStart);
        CombatManager.Instance.CombatTurnEnd.RemoveListener(OnCombatTurnEnd);
        CombatManager.Instance.CombatantDied.RemoveListener(OnCombatantDeath);

        // Get the node this enemy is on if any
        PathfindingNode node = PathNodeManager.Instance.GetCombatantNode(this);
        if (node != null)
        {
            PathNodeManager.Instance.ClearCombatantPosition(node.GetX(), node.GetZ());
        }
        // Clear all limbs
        ClearLimbs();

    }
    /* *** EVENTS *** */
    public void OnCombatStart()
    {
        combatTiebreakModifier = UnityEngine.Random.value;
        // Add self to turn order
        TurnOrderManager.Instance.AddToTurnList(this);
        FadeObjectsObstructingCamera.AddObjectToFadeFor(gameObject);
        // Get a random limb from the limbs dictionary
        if (!isPlayer)
        {
            Limb lootLimb = GetLimbAtPosition(UnityEngine.Random.Range(0, limbs.Count));
            if (lootLimb != null)
            {
                LootManager.Instance.AddLoot(lootLimb);
            }
            baseMaxHealth = (float)(int)(baseMaxHealth + (baseMaxHealth * Mathf.Pow(1.3f, level - 1)));
            remainingHealth = baseMaxHealth;
        }
    }

    public void OnCombatEnd()
    {
        if (isPlayer && !isDead)
        {
            level += 1;
            foreach(Limb limb in limbs.Values)
            {
                limb.SetLevel((int)level);
            }
            PlayerController.Instance.RefreshPlayerStats.Invoke();
            PlayerController.Instance.RefreshLimbUI.Invoke();
            remainingHealth += (MaxHealth * 0.5f); // Give the player some health back
            if (remainingHealth > MaxHealth)
            {
                remainingHealth = MaxHealth;
            }
        }
    }

    private void OnCombatTurnStart(Combatant combatant)
    {
        if (combatant == this)
        {
            ClearEngagedLimbs();
            isTurn = true;
            if (statusEffects.Count > 0)
            {
                foreach (BaseStatusEffect effect in statusEffects)
                {
                    effect.OnTurnStartWithEffect();
                }
                // Remove the effect if it's expired
                statusEffects.RemoveAll(effect => effect.ShouldBeDestroyed());
            }
            // Enemies can calculate their AI coroutine here.
        }
        else
        {
            isTurn = false; // Safety guard to make sure that if the turn order updates irregularly this combatant doesn't get stuck in a turn that's not theirs.
        }
    }
    private void OnCombatTurnEnd(Combatant combatant)
    {
        isTurn = false;
        if (combatant == this)
        {
            foreach (BaseStatusEffect effect in statusEffects)
            {
                effect.OnTurnEndWithEffect();
            }
            // Remove expired effects
            statusEffects.RemoveAll(effect => effect.ShouldBeDestroyed());
        }
    }
    private void OnCombatantDeath(Combatant combatant, Damage _)
    {
        if (combatant == this)
        {
            isDead = true;
            ClearEngagedLimbs();
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z); // TODO: Replace with death pose/anim
            // Remove all status effects
            statusEffects.Clear();

            PathfindingNode node = PathNodeManager.Instance.GetCombatantNode(this);
            if (node != null)
            {
                PathNodeManager.Instance.ClearCombatantPosition(node.GetX(), node.GetZ());
            }
        }
    }
}
