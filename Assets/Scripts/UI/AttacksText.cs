using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AttacksText : MonoBehaviour
{
    private bool isCombat = false;
    TextMeshProUGUI attacksText;
    public string FormatAttacksText(int moves, int maxMoves)
    {
        return $"Attacks Left: {moves}/{maxMoves}";
    }

    private void Awake()
    {
        if (!TryGetComponent(out attacksText))
        { 
            attacksText = gameObject.AddComponent<TextMeshProUGUI>();
            attacksText.fontSize = 36;
            attacksText.color = Color.white;
        }
        attacksText.text = "";
    }
    private void Start()
    {
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        PlayerController.Instance.RefreshPlayerStats.AddListener(OnPlayerUIUpdate);
    }
    private void OnDestroy()
    {
        CombatManager.Instance.CombatStart.RemoveListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.RemoveListener(OnCombatEnd);
        PlayerController.Instance.RefreshPlayerStats.RemoveListener(OnPlayerUIUpdate);
    }

    /* *** EVENTS *** */
    private void OnCombatStart()
    {
        isCombat = true;
        OnPlayerUIUpdate();
    }
    private void OnCombatEnd()
    {
        isCombat = false;
        OnPlayerUIUpdate();
    }
    private void OnPlayerUIUpdate()
    {
        if (isCombat) attacksText.text = FormatAttacksText(PlayerController.Instance.attacksRemaining, PlayerController.Instance.allowedAttacksPerTurn);
        else attacksText.text = "";
    }
}