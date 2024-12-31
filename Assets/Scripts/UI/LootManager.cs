using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class LootManager : MonoBehaviour
{ 
    public static LootManager Instance;
    public bool isDisplaying = false;

    public Image uiBackground;
    public TextMeshProUGUI newItemHeader;
    public TextMeshProUGUI newItemName;
    public Image newItemImage;
    public TextMeshProUGUI attackTypeText;
    public TextMeshProUGUI attackDamageText;
    public TextMeshProUGUI attackRangeText;
    public TextMeshProUGUI attackElementText;
    public TextMeshProUGUI attackModifier1Text;
    public TextMeshProUGUI attackModifier2Text;
    public TextMeshProUGUI attackModifier3Text;

    public TextMeshProUGUI equipInstructText;
    public TextMeshProUGUI discardInstructText;

    private List<Limb> lootList = new();
    private Limb lootReward;

    public UnityEvent DistributeLoot = new();

    private InputSystem_Actions inputActions;
    private InputAction lootInsert1;
    private InputAction lootInsert2;
    private InputAction lootInsert3;
    private InputAction lootInsert4;
    private InputAction lootInsertCancel;

    public Limb GetRandomLimbFromLootList()
    {
        if (lootList.Count == 0) return null;
        int randomIndex = UnityEngine.Random.Range(0, lootList.Count);
        if (lootList[randomIndex] == null) return null;
        return Instantiate(lootList[randomIndex]);
    }
    public void AddLoot(Limb limb)
    {
        limb = Instantiate(limb);
        limb = Limb.RegenerateLimbForLoot(limb, (int)PlayerController.Instance.combatant.level);
        lootList.Add(limb);
    }
    public void RemoveLoot(Limb limb)
    {
        lootList.Remove(limb);
    }
    public void ClearLootList()
    {
        lootList.Clear();
    }
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

    private void RefreshLootDisplay()
    {
        newItemName.text = lootReward.limbName == null ? "Limb" : lootReward.limbName;
        newItemImage.sprite = lootReward.limbSprite;
        attackTypeText.text = lootReward.attackData.AttackType switch
        {
            AttackType.Point => "Attack Type: Point",
            AttackType.Circle => "Attack Type: Circle",
            AttackType.Line => "Attack Type: Line",
            AttackType.AroundSelf => "Attack Type: Around Self",
            AttackType.Cone => "Attack Type: Cone",
            AttackType.Undefined => "Attack Type: Unknown",
            _ => "Attack Type: Unknown"
        };
        attackDamageText.text = "Attack Damage: " + lootReward.Damage.ToString("0.0");
        attackRangeText.text = "Attack Range: " + lootReward.Range.ToString("0.0");
        attackElementText.text = lootReward.damageType switch
        {
            DamageType.Physical => "Attack Element: Physical",
            DamageType.Fire => "Attack Type: Fire",
            DamageType.Ice => "Attack Type: Ice",
            DamageType.Lightning => "Attack Type: Lightning",
            DamageType.Poison => "Attack Type: Poison",
            DamageType.Blood => "Attack Type: Blood",
            DamageType.Radiant => "Attack Type: Radiant",
            DamageType.Void => "Attack Type: Void",
            DamageType.True => "Attack Type: True",
            _ => "Attack Type: Unknown"
        };
        List<Tuple<Color, string>> modifierText = GetLimbModifierText(lootReward);
        attackModifier1Text.text = modifierText.Count > 0 ? modifierText[0].Item2 : "";
        attackModifier1Text.color = modifierText.Count > 0 ? modifierText[0].Item1 : ModifierColor.neutralModifier;
        attackModifier2Text.text = modifierText.Count > 1 ? modifierText[1].Item2 : "";
        attackModifier2Text.color = modifierText.Count > 1 ? modifierText[1].Item1 : ModifierColor.neutralModifier;
        attackModifier3Text.text = modifierText.Count > 2 ? modifierText[2].Item2 : "";
        attackModifier3Text.color = modifierText.Count > 2 ? modifierText[2].Item1 : ModifierColor.neutralModifier;
    }

    private void SetLootDisplay(bool toggle)
    {
        uiBackground.enabled = toggle;
        newItemHeader.enabled = toggle;
        newItemName.enabled = toggle;
        newItemImage.enabled = toggle;
        attackTypeText.enabled = toggle;
        attackDamageText.enabled = toggle;
        attackRangeText.enabled = toggle;
        attackElementText.enabled = toggle;
        attackModifier1Text.enabled = toggle;
        attackModifier2Text.enabled = toggle;
        attackModifier3Text.enabled = toggle;
        equipInstructText.enabled = toggle;
        discardInstructText.enabled = toggle;
        isDisplaying = toggle;
    }
    public void EnableLootDisplay()
    {
        SetLootDisplay(true);
    }
    public void DisableLootDisplay()
    {
        SetLootDisplay(false);
    }

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
        lootInsert1 = inputActions.Player.SelectLimb1;
        lootInsert2 = inputActions.Player.SelectLimb2;
        lootInsert3 = inputActions.Player.SelectLimb3;
        lootInsert4 = inputActions.Player.SelectLimb4;
        lootInsertCancel = inputActions.Player.CancelInventoryAdd;

    }
    private void OnEnable()
    {
        inputActions.Enable();
        lootInsert1.performed += ctx => InsertLootLimbIntoPosition(0);
        lootInsert2.performed += ctx => InsertLootLimbIntoPosition(1);
        lootInsert3.performed += ctx => InsertLootLimbIntoPosition(2);
        lootInsert4.performed += ctx => InsertLootLimbIntoPosition(3);
        lootInsertCancel.performed += ctx => CloseWindowWithoutLimbInsert();
    }
    private void OnDisable()
    {
        inputActions.Disable();
        lootInsert1.performed -= ctx => InsertLootLimbIntoPosition(0);
        lootInsert2.performed -= ctx => InsertLootLimbIntoPosition(1);
        lootInsert3.performed -= ctx => InsertLootLimbIntoPosition(2);
        lootInsert4.performed -= ctx => InsertLootLimbIntoPosition(3);
        lootInsertCancel.performed -= ctx => CloseWindowWithoutLimbInsert();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
    private void Start()
    {
        SetLootDisplay(false);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        Debug.Log("Loot Manager Initialized");

    }
    private void OnCombatEnd()
    {
        lootReward = GetRandomLimbFromLootList();
        ClearLootList();
        if (lootReward == null)
        {
            DisableLootDisplay();
            return;
        }
        EnableLootDisplay();
        RefreshLootDisplay();
    }

    private void CloseWindowWithoutLimbInsert()
    {
        lootReward = null;
        PlayerController.Instance.RefreshLimbUI.Invoke();
        PlayerController.Instance.RefreshPlayerStats.Invoke();
        DisableLootDisplay();
    }

    private void InsertLootLimbIntoPosition(int position)
    {
        if (lootReward == null) return;
        PlayerController.Instance.combatant.AddLimbToDict(lootReward, position);
        lootReward = null;
        PlayerController.Instance.RefreshLimbUI.Invoke();
        PlayerController.Instance.RefreshPlayerStats.Invoke();
        DisableLootDisplay();
    }
}