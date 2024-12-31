using UnityEngine;
using TMPro;

public class NextTurnText : MonoBehaviour
{
    private TextMeshProUGUI nextTurnText;

    public string FormatNextTurnText(string combatantName)
    {
        return $"Next Turn: {combatantName}";
    }

    private void Awake()
    {
        if (!TryGetComponent(out nextTurnText))
        {
            nextTurnText = gameObject.AddComponent<TextMeshProUGUI>();
            nextTurnText.fontSize = 36;
            nextTurnText.color = Color.white;
        }
        nextTurnText.text = FormatNextTurnText("None");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update() // TODO: Make this only update when the player's HP is changed, probably via events
    {
        if (!TurnOrderManager.Instance.isCombat)
        {
            nextTurnText.text = FormatNextTurnText("None");
            return;
        }
        string txt = TurnOrderManager.Instance.GetNextTurnCombatantName();
        if (txt == null)
        {
            nextTurnText.text = FormatNextTurnText("None");
            return;
        }
        nextTurnText.text = FormatNextTurnText(txt);

    }
}
