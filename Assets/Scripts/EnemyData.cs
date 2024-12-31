using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Combatant))]
public class EnemyData : MonoBehaviour
{
    [Tooltip("The name that appears for the enemy in the UI.")]public string enemyName = "Unnamed Enemy";
    [Tooltip("When the player reaches this level, the enemy can no longer spawn. If a negative number, the enemy will never disappear.")]public float disappearLevel = -1.0f; // If this is negative, the enemy will never disappear.
    [Tooltip("The type of movement the creature is able to use. This also determines which tiles the enemy is allowed to traverse over.")]public CreatureMovementType movementType = CreatureMovementType.Ground;
    [Tooltip("The number of tiles that the enemy can move per turn. The enemy is not guaranteed to use all these tiles for movement. If 0, the enemy is incapable of moving.")]public uint movesPerTurn = 5;
    private Combatant combatant;
    [Header("Blood Splatter")]
    [Tooltip("The color of the blood splatter that appears when the enemy is hit or dies.")] public Color bloodColor = Color.red;
    [Tooltip("The multiplier that controls how much blood the enemy will splatter upon hit or death.")] public float bloodSplatterMultiplier = 1.0f;
    [Header("Weaknesses and Resistances")]
    public float physicalMultiplier = 1.0f;
    public float fireMultiplier = 1.0f;
    public float iceMultiplier = 1.0f;
    public float lightningMultiplier = 1.0f;
    public float poisonMultiplier = 1.0f;
    public float bloodMultiplier = 1.0f;
    public float radiantMultiplier = 1.0f;
    public float voidMultiplier = 1.0f;

    [System.NonSerialized] public DamageModifierTable damageModifiers = new();

    /* *** Health Bar *** */
    private Healthbar healthbar;

    /* *** Monobehaviour Functions *** */
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Try to get the combatant off this object. If it doesn't exist raise an error and destroy self.
        if (!TryGetComponent<Combatant>(out combatant))
        {
            Debug.LogError("No Combatant component found on " + gameObject.name + ". Destroying self.");
            Destroy(gameObject);
        }
        damageModifiers = TranslateEnemyDataToDamageModifierTable();
        // Instantiate a healthbar from the prefab on combatmanager
        GameObject healthbarObject = Instantiate(CombatManager.healthbarPrefab);
        healthbarObject.transform.SetParent(transform);
        if (!healthbarObject.TryGetComponent<Healthbar>(out healthbar))
        {
            Debug.LogError("Healthbar failed to correctly instantiate.");
            Destroy(healthbarObject);
        }
        else healthbar.SyncToCombatant(combatant);
    }
    private void OnDestroy()
    {
        if (healthbar != null) Destroy(healthbar);
    }

    /* *** Helpers *** */

    public bool HasEnemyDisappeared(float level)
    {
        if (disappearLevel < 0.0f) return false;
        return level >= disappearLevel;
    }

    public DamageModifierTable GetDamageModifierTable()
    {
        return damageModifiers.GetCopy();
    }

    public DamageModifierTable TranslateEnemyDataToDamageModifierTable()
    {
        Dictionary<DamageType, float> multipliers = new()
        {
            { DamageType.Physical, physicalMultiplier },
            { DamageType.Fire, fireMultiplier },
            { DamageType.Ice, iceMultiplier },
            { DamageType.Lightning, lightningMultiplier },
            { DamageType.Poison, poisonMultiplier },
            { DamageType.Blood, bloodMultiplier },
            { DamageType.Radiant, radiantMultiplier },
            { DamageType.Void, voidMultiplier }
        };
        return new DamageModifierTable(multipliers);
    }


}