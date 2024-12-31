using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class InspectHandler : MonoBehaviour
{
    public static InspectHandler Instance;

    public bool isDisplaying = false;

    public Image inspectBackground;
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI enemyHealth;
    public TextMeshProUGUI enemySpeed;
    public TextMeshProUGUI enemyCrit;
    public TextMeshProUGUI enemyDodge;
    public TextMeshProUGUI enemyResistancesHeader;
    public TextMeshProUGUI enemyResistancesLeft;
    public TextMeshProUGUI enemyResistancesRight;

    public UnityEvent<Combatant> DisplayInspect;
    public UnityEvent HideInspect;

    private string FormatResistanceNumber(float resistanceNum)
    {
        return resistanceNum.ToString("0.0") + "x";
    }

    private void UpdateEnemyResistancesLeft(DamageModifierTable combatantResistances) // Left contains physical, fire, ice, and lightning
    {
        enemyResistancesLeft.text = "Physical: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Physical)) + "\n" +
                                    "Fire: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Fire)) + "\n" +
                                    "Ice: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Ice)) + "\n" +
                                    "Lightning: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Lightning));
    }

    private void UpdateEnemyResistancesRight(DamageModifierTable combatantResistances)
    {
        enemyResistancesRight.text = "Poison: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Poison)) + "\n" +
                                     "Blood: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Blood)) + "\n" +
                                     "Radiant: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Radiant)) + "\n" +
                                     "Void: " + FormatResistanceNumber(combatantResistances.GetDamageModifierByType(DamageType.Void));
    }

    /* *** Monobehaviour Functions *** */

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        DisplayInspect = new UnityEvent<Combatant>();
        HideInspect = new UnityEvent();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (inspectBackground == null || enemyName == null || enemyHealth == null || enemySpeed == null || enemyCrit == null || enemyDodge == null || enemyResistancesHeader == null || enemyResistancesLeft == null || enemyResistancesRight == null)
        {
            Debug.LogError("InspectHandler: One or more fields are not assigned!");
        }
        DisableAll();
    }

    private void OnDestroy()
    {
        DisplayInspect.RemoveAllListeners();
        HideInspect.RemoveAllListeners();
        Instance = null;
    }

    /* *** Enable/Disable *** */
    private void EnableAll()
    {
        if (Instance == null)
        {
            Debug.LogError("InspectHandler: Instance is null!");
            return;
        }
        Instance.inspectBackground.enabled = true;
        Instance.enemyName.enabled = true;
        Instance.enemyHealth.enabled = true;
        Instance.enemySpeed.enabled = true;
        Instance.enemyCrit.enabled = true;
        Instance.enemyDodge.enabled = true;
        Instance.enemyResistancesHeader.enabled = true;
        Instance.enemyResistancesLeft.enabled = true;
        Instance.enemyResistancesRight.enabled = true;
    }

    private void DisableAll()
    {
        if (Instance == null)
        {
            Debug.LogError("InspectHandler: Instance is null!");
            return;
        }
        Instance.inspectBackground.enabled = false;
        Instance.enemyName.enabled = false;
        Instance.enemyHealth.enabled = false;
        Instance.enemySpeed.enabled = false;
        Instance.enemyCrit.enabled = false;
        Instance.enemyDodge.enabled = false;
        Instance.enemyResistancesHeader.enabled = false;
        Instance.enemyResistancesLeft.enabled = false;
        Instance.enemyResistancesRight.enabled = false;
    }

    public void DisplayOrUpdateInspect(Combatant combatant)
    {
        if (combatant == null)
        {
            Debug.LogError("InspectHandler: Combatant is null!");
            return;
        }
        if (!isDisplaying)
        {
            DisplayInspect.Invoke(combatant);
            isDisplaying = true;
            // Enable all of the components
            EnableAll();
        }
        if (combatant.isPlayer)
        {
            enemyName.text = "Player";
        }
        else
        {
            enemyName.text = combatant.enemyData.enemyName;
        }
        enemyHealth.text = "Health: " + combatant.remainingHealth.ToString("0.0") + "/" + (int)(combatant.MaxHealth);
        enemySpeed.text = "Speed: " + combatant.Speed;
        enemyCrit.text = "Crit: " + (combatant.CritChance * 100).ToString("0.0") + "%";
        enemyDodge.text = "Dodge: " + (combatant.DodgeChance * 100).ToString("0.0") + "%";
        UpdateEnemyResistancesLeft(combatant.GetDamageModifiers());
        UpdateEnemyResistancesRight(combatant.GetDamageModifiers());
    }

    public void HideInspectPanel()
    {
        HideInspect.Invoke();
        isDisplaying = false;
        DisableAll();
    }
}