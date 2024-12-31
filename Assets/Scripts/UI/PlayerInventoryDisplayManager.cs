using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInventoryDisplayManager : MonoBehaviour
{
    public static PlayerInventoryDisplayManager Instance;
    public bool isInventoryOpen = false;
    public UnityEvent InventoryOpened = new();
    public UnityEvent InventoryClosed = new();

    public Image inventoryDisplayBackground;
    public TextMeshProUGUI leftArmHeader;
    public TextMeshProUGUI rightArmHeader;
    public TextMeshProUGUI leftLegHeader;
    public TextMeshProUGUI rightLegHeader;

    public TextMeshProUGUI leftArmAttackDetail;
    public TextMeshProUGUI rightArmAttackDetail;
    public TextMeshProUGUI leftLegAttackDetail;
    public TextMeshProUGUI rightLegAttackDetail;

    public TextMeshProUGUI leftArmDamage;
    public TextMeshProUGUI rightArmDamage;
    public TextMeshProUGUI leftLegDamage;
    public TextMeshProUGUI rightLegDamage;

    public TextMeshProUGUI leftArmModifier1;
    public TextMeshProUGUI rightArmModifier1;
    public TextMeshProUGUI leftLegModifier1;
    public TextMeshProUGUI rightLegModifier1;

    public TextMeshProUGUI leftArmModifier2;
    public TextMeshProUGUI rightArmModifier2;
    public TextMeshProUGUI leftLegModifier2;
    public TextMeshProUGUI rightLegModifier2;

    public TextMeshProUGUI leftArmModifier3;
    public TextMeshProUGUI rightArmModifier3;
    public TextMeshProUGUI leftLegModifier3;
    public TextMeshProUGUI rightLegModifier3;

    private InputSystem_Actions inputActions;
    private InputAction toggleInventoryAction;

    /* *** Open/Close *** */

    public void ToggleInventoryDisplay(bool isOpen)
    {
        isInventoryOpen = isOpen;
        inventoryDisplayBackground.gameObject.SetActive(isInventoryOpen);
        leftArmHeader.gameObject.SetActive(isInventoryOpen);
        rightArmHeader.gameObject.SetActive(isInventoryOpen);
        leftLegHeader.gameObject.SetActive(isInventoryOpen);
        rightLegHeader.gameObject.SetActive(isInventoryOpen);
        leftArmAttackDetail.gameObject.SetActive(isInventoryOpen);
        rightArmAttackDetail.gameObject.SetActive(isInventoryOpen);
        leftLegAttackDetail.gameObject.SetActive(isInventoryOpen);
        rightLegAttackDetail.gameObject.SetActive(isInventoryOpen);
        leftArmDamage.gameObject.SetActive(isInventoryOpen);
        rightArmDamage.gameObject.SetActive(isInventoryOpen);
        leftLegDamage.gameObject.SetActive(isInventoryOpen);
        rightLegDamage.gameObject.SetActive(isInventoryOpen);
        leftArmModifier1.gameObject.SetActive(isInventoryOpen);
        rightArmModifier1.gameObject.SetActive(isInventoryOpen);
        leftLegModifier1.gameObject.SetActive(isInventoryOpen);
        rightLegModifier1.gameObject.SetActive(isInventoryOpen);
        leftArmModifier2.gameObject.SetActive(isInventoryOpen);
        rightArmModifier2.gameObject.SetActive(isInventoryOpen);
        leftLegModifier2.gameObject.SetActive(isInventoryOpen);
        rightLegModifier2.gameObject.SetActive(isInventoryOpen);
        leftArmModifier3.gameObject.SetActive(isInventoryOpen);
        rightArmModifier3.gameObject.SetActive(isInventoryOpen);
        leftLegModifier3.gameObject.SetActive(isInventoryOpen);
        rightLegModifier3.gameObject.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            InventoryOpened.Invoke();
        }
        else
        {
            InventoryClosed.Invoke();
        }
    }

    private string GetLimbNameText(Limb limb) { return limb == null ? "-" : limb.limbName; }
    private string GetLimbAttackDetailText(Limb limb)
    {
        if (limb == null) return "";
        string attackDetail = limb.damageType switch
        {
            DamageType.Physical => "Physical ",
            DamageType.Fire => "Fire ",
            DamageType.Ice => "Ice ",
            DamageType.Lightning => "Lightning ",
            DamageType.Poison => "Poison ",
            DamageType.Blood => "Blood ",
            DamageType.Radiant => "Radiant ",
            DamageType.Void => "Void ",
            DamageType.True => "True ",
            _ => "Unknown ",
        };
        attackDetail += limb.attackData.AttackType switch
        {
            AttackType.Point => "Point ",
            AttackType.Line => "Line ",
            AttackType.Circle => "Circular ",
            AttackType.AroundSelf => "Self-Centered ",
            AttackType.Cone => "Cone ",
            AttackType.Undefined => "Unknown ",
            _ => "Unknown ",
        };
        attackDetail += "Attack";
        return attackDetail;
    }
    private string GetLimbDamageText(Limb limb) { return limb == null ? "" : "Damage: " + limb.Damage.ToString("0.00"); }   

    private List<Tuple<Color, string>> GetLimbModifierText(Limb limb)
    {
        List<Tuple<Color, string>> modifierTextList = new();
        if (limb == null) return modifierTextList;
        foreach (BaseModifier modifier in limb.modifiers)
        {
            if (modifier == null) continue;
            Color color = ModifierColor.neutralModifier;
            if (modifier.isUniqueLegendary) color = ModifierColor.legendaryModifier;
            else
            {
                color = modifier.modifierViability switch
                {
                    ModifierViability.Positive => ModifierColor.positiveModifier,
                    ModifierViability.Negative => ModifierColor.negativeModifier,
                    ModifierViability.Neutral => ModifierColor.neutralModifier,
                    _ => ModifierColor.neutralModifier,
                };
            }
            modifierTextList.Add(new Tuple<Color, string>(color, modifier.ModifierDescription));
        }
        return modifierTextList;
    }

    private void RefreshInventoryUI()
    {
        if (PlayerController.Instance == null || PlayerController.Instance.combatant == null)
        {
            Debug.LogError("PlayerController or PlayerController.combatant is null!");
            CloseInventory(); // Force close the inventory if we're here since we can't display it.
            return;
        }
        // Get all limbs from the player combatant instance
        Limb leftArm = PlayerController.Instance.combatant.GetLimbAtPosition(0);
        Limb rightArm = PlayerController.Instance.combatant.GetLimbAtPosition(1);
        Limb leftLeg = PlayerController.Instance.combatant.GetLimbAtPosition(2);
        Limb rightLeg = PlayerController.Instance.combatant.GetLimbAtPosition(3);

        // Set the header text for each limb
        leftArmHeader.text = GetLimbNameText(leftArm);
        rightArmHeader.text = GetLimbNameText(rightArm);
        leftLegHeader.text = GetLimbNameText(leftLeg);
        rightLegHeader.text = GetLimbNameText(rightLeg);

        leftArmAttackDetail.text = GetLimbAttackDetailText(leftArm);
        rightArmAttackDetail.text = GetLimbAttackDetailText(rightArm);
        leftLegAttackDetail.text = GetLimbAttackDetailText(leftLeg);
        rightLegAttackDetail.text = GetLimbAttackDetailText(rightLeg);

        leftArmDamage.text = GetLimbDamageText(leftArm);
        rightArmDamage.text = GetLimbDamageText(rightArm);
        leftLegDamage.text = GetLimbDamageText(leftLeg);
        rightLegDamage.text = GetLimbDamageText(rightLeg);

        List<Tuple<Color, string>> leftArmModifiers = GetLimbModifierText(leftArm);
        List<Tuple<Color, string>> rightArmModifiers = GetLimbModifierText(rightArm);
        List<Tuple<Color, string>> leftLegModifiers = GetLimbModifierText(leftLeg);
        List<Tuple<Color, string>> rightLegModifiers = GetLimbModifierText(rightLeg);

        leftArmModifier1.text = leftArmModifiers.Count > 0 ? leftArmModifiers[0].Item2 : "";
        leftArmModifier1.color = leftArmModifiers.Count > 0 ? leftArmModifiers[0].Item1 : ModifierColor.neutralModifier;
        rightArmModifier1.text = rightArmModifiers.Count > 0 ? rightArmModifiers[0].Item2 : "";
        rightArmModifier1.color = rightArmModifiers.Count > 0 ? rightArmModifiers[0].Item1 : ModifierColor.neutralModifier;
        leftLegModifier1.text = leftLegModifiers.Count > 0 ? leftLegModifiers[0].Item2 : "";
        leftLegModifier1.color = leftLegModifiers.Count > 0 ? leftLegModifiers[0].Item1 : ModifierColor.neutralModifier;
        rightLegModifier1.text = rightLegModifiers.Count > 0 ? rightLegModifiers[0].Item2 : "";
        rightLegModifier1.color = rightLegModifiers.Count > 0 ? rightLegModifiers[0].Item1 : ModifierColor.neutralModifier;

        leftArmModifier2.text = leftArmModifiers.Count > 1 ? leftArmModifiers[1].Item2 : "";
        leftArmModifier2.color = leftArmModifiers.Count > 1 ? leftArmModifiers[1].Item1 : ModifierColor.neutralModifier;
        rightArmModifier2.text = rightArmModifiers.Count > 1 ? rightArmModifiers[1].Item2 : "";
        rightArmModifier2.color = rightArmModifiers.Count > 1 ? rightArmModifiers[1].Item1 : ModifierColor.neutralModifier;
        leftLegModifier2.text = leftLegModifiers.Count > 1 ? leftLegModifiers[1].Item2 : "";
        leftLegModifier2.color = leftLegModifiers.Count > 1 ? leftLegModifiers[1].Item1 : ModifierColor.neutralModifier;
        rightLegModifier2.text = rightLegModifiers.Count > 1 ? rightLegModifiers[1].Item2 : "";
        rightLegModifier2.color = rightLegModifiers.Count > 1 ? rightLegModifiers[1].Item1 : ModifierColor.neutralModifier;

        leftArmModifier3.text = leftArmModifiers.Count > 2 ? leftArmModifiers[2].Item2 : "";
        leftArmModifier3.color = leftArmModifiers.Count > 2 ? leftArmModifiers[2].Item1 : ModifierColor.neutralModifier;
        rightArmModifier3.text = rightArmModifiers.Count > 2 ? rightArmModifiers[2].Item2 : "";
        rightArmModifier3.color = rightArmModifiers.Count > 2 ? rightArmModifiers[2].Item1 : ModifierColor.neutralModifier;
        leftLegModifier3.text = leftLegModifiers.Count > 2 ? leftLegModifiers[2].Item2 : "";
        leftLegModifier3.color = leftLegModifiers.Count > 2 ? leftLegModifiers[2].Item1 : ModifierColor.neutralModifier;
        rightLegModifier3.text = rightLegModifiers.Count > 2 ? rightLegModifiers[2].Item2 : "";
        rightLegModifier3.color = rightLegModifiers.Count > 2 ? rightLegModifiers[2].Item1 : ModifierColor.neutralModifier;
    }
    private void OpenInventory()
    {
        ToggleInventoryDisplay(true);
        RefreshInventoryUI();
    }
    private void CloseInventory()
    {
        ToggleInventoryDisplay(false);
    }

    /* *** Monobehaviour *** */

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
        inputActions = new InputSystem_Actions();
        toggleInventoryAction = inputActions.Player.ToggleInventory;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        toggleInventoryAction.performed += OnInventoryInput;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CloseInventory();
    }

    private void OnDestroy()
    {
        InventoryOpened.RemoveAllListeners();
        InventoryClosed.RemoveAllListeners();
        Instance = null;
    }

    private void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll || InspectHandler.Instance.isDisplaying) return;
        if (context.performed)
        {
            if (isInventoryOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
    }
}
