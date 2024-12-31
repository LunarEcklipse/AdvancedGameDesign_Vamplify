using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public GameObject damageTextPrefab;

    private void CreateDamageTextOverCombatant(Combatant combatant, Damage damage)
    {
        GameObject damageText = Instantiate(damageTextPrefab, combatant.transform);
        DamageText damageTextComponent = damageText.GetComponent<DamageText>();
        damageTextComponent.damage = damage;
        damageTextComponent.combatant = combatant;
        damageText.name = "DamageText_" + combatant.name;
        damageText.transform.SetParent(transform);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CombatManager.Instance.CombatantAttacked.AddListener(OnCombatantAttacked);
    }

    private void OnDestroy()
    {
        CombatManager.Instance.CombatantAttacked.RemoveListener(OnCombatantAttacked);
    }

    /* *** EVENT LISTENERS *** */
    private void OnCombatantAttacked(Combatant combatant, Damage damage)
    {
        Debug.Log("Spawning text!");
        CreateDamageTextOverCombatant(combatant, damage);
    }
}