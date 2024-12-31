using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UILimbColorController : MonoBehaviour
{
    public static UILimbColorController Instance;
    [SerializeField] private Image leftArm;
    [SerializeField] private Image rightArm;
    [SerializeField] private Image leftLeg;
    [SerializeField] private Image rightLeg;

    [SerializeField] private Color nullColor = Color.grey;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color primaryColor = Color.yellow;
    [SerializeField] private Color secondaryColor = Color.red;

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
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlayerController.Instance.RefreshLimbUI.AddListener(OnRefreshLimbUI);
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        if (leftArm == null || rightArm == null || leftLeg == null || rightLeg == null)
        {
            Debug.LogError("One or more limb images are not assigned in the inspector for the UI Limb Color Controller.");
        }
    }

    private void OnDestroy()
    {
        PlayerController.Instance.RefreshLimbUI.RemoveListener(OnRefreshLimbUI);
        CombatManager.Instance.CombatStart.RemoveListener(OnCombatStart);
        Instance = null;
    }
    /* *** EVENTS *** */
    private void RefreshLeftArm()
    {
        if (PlayerController.Instance.combatant.GetLimbAtPosition(0) == null || PlayerController.Instance.isFlying)
        {
            leftArm.color = nullColor;
            return;
        }
        if (PlayerController.Instance.combatant.GetPrimaryLimb() == PlayerController.Instance.combatant.GetLimbAtPosition(0))
        {
            leftArm.color = primaryColor;
        }
        else if (PlayerController.Instance.combatant.IsLimbEngaged(PlayerController.Instance.combatant.GetLimbAtPosition(0)))
        {
            leftArm.color = secondaryColor;
        }
        else
        {
            leftArm.color = defaultColor;
        }
    }
    private void RefreshRightArm()
    {
        if (PlayerController.Instance.combatant.GetLimbAtPosition(1) == null || PlayerController.Instance.isFlying)
        {
            rightArm.color = nullColor;
            return;
        }
        if (PlayerController.Instance.combatant.GetPrimaryLimb() == PlayerController.Instance.combatant.GetLimbAtPosition(1))
        {
            rightArm.color = primaryColor;
        }
        else if (PlayerController.Instance.combatant.IsLimbEngaged(PlayerController.Instance.combatant.GetLimbAtPosition(1)))
        {
            rightArm.color = secondaryColor;
        }
        else
        {
            rightArm.color = defaultColor;
        }
    }
    private void RefreshLeftLeg()
    {
        if (PlayerController.Instance.combatant.GetLimbAtPosition(2) == null || PlayerController.Instance.isInWater)
        {
            leftLeg.color = nullColor;
            return;
        }
        if (PlayerController.Instance.combatant.GetPrimaryLimb() == PlayerController.Instance.combatant.GetLimbAtPosition(2))
        {
            leftLeg.color = primaryColor;
        }
        else if (PlayerController.Instance.combatant.IsLimbEngaged(PlayerController.Instance.combatant.GetLimbAtPosition(2)))
        {
            leftLeg.color = secondaryColor;
        }
        else
        {
            leftLeg.color = defaultColor;
        }
    }
    private void RefreshRightLeg()
    {
        if (PlayerController.Instance.combatant.GetLimbAtPosition(3) == null || PlayerController.Instance.isInWater)
        {
            rightLeg.color = nullColor;
            return;
        }
        if (PlayerController.Instance.combatant.GetPrimaryLimb() == PlayerController.Instance.combatant.GetLimbAtPosition(3))
        {
            rightLeg.color = primaryColor;
        }
        else if (PlayerController.Instance.combatant.IsLimbEngaged(PlayerController.Instance.combatant.GetLimbAtPosition(3)))
        {
            rightLeg.color = secondaryColor;
        }
        else
        {
            rightLeg.color = defaultColor;
        }
    }
    private void OnCombatStart()
    {
        OnRefreshLimbUI();
    }
    private void OnRefreshLimbUI()
    {
        RefreshLeftArm();
        RefreshRightArm();
        RefreshLeftLeg();
        RefreshRightLeg();
    }
}
