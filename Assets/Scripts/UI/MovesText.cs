using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MovesText : MonoBehaviour
{
    private bool isCombat = false;
    TextMeshProUGUI movesText;

    public string FormatMovesText(int moves, int maxMoves)
    {
        return $"Moves Left: {moves}/{maxMoves}";
    }

    private void Awake()
    {
        if (!TryGetComponent(out movesText))
        { 
            movesText = gameObject.AddComponent<TextMeshProUGUI>();
            movesText.fontSize = 36;
            movesText.color = Color.white;
        }
        movesText.text = "";
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
        if (isCombat)
        {
            movesText.text = FormatMovesText(PlayerController.Instance.MovesRemaining, PlayerController.Instance.allowedMovesPerTurn);
        }
        else
        {
            movesText.text = "";
        }
    }
}
