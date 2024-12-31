using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class DeadText : MonoBehaviour
{
    private bool canRestart = false;
    private TextMeshProUGUI deadText;
    private InputSystem_Actions inputActions;
    private InputAction restartAction;

    private void NullAllSingletons()
    {
        CameraTracker.Instance.ToggleFreecamEvent.RemoveAllListeners();
        CombatManager.Instance.GenerateNewRoom.RemoveAllListeners();
        CombatManager.Instance.SpawnActors.RemoveAllListeners();
        CombatManager.Instance.CombatStart.RemoveAllListeners();
        CombatManager.Instance.CombatTurnStart.RemoveAllListeners();
        CombatManager.Instance.CombatTurnEnd.RemoveAllListeners();
        CombatManager.Instance.CombatantAttacked.RemoveAllListeners();
        CombatManager.Instance.CombatantDied.RemoveAllListeners();
        CombatManager.Instance.PlayerDeath.RemoveAllListeners();
        CombatManager.Instance.ReportRemainingCombatants.RemoveAllListeners();
        CombatManager.Instance.CombatEnded.RemoveAllListeners();
        CombatManager.Instance.PlayerTouchedRoomTrigger.RemoveAllListeners();
        CombatManager.Instance.CombatReady.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToBlackStart.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToBlackEnd.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToBlackInstant.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToClearStart.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToClearEnd.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeToClearInstant.RemoveAllListeners();
        FadeToBlackPanel.Instance.fadeStateChange.RemoveAllListeners();
        MouseCast.OnNodeClicked.RemoveAllListeners();
        PlayerController.Instance.EngagePlayerAttackModeEvent.RemoveAllListeners();
        PlayerController.Instance.DisengagePlayerAttackModeEvent.RemoveAllListeners();
        PlayerController.Instance.RefreshLimbUI.RemoveAllListeners();
        PlayerController.Instance.RefreshPlayerStats.RemoveAllListeners();
        AudioManager.Instance = null;
        CameraTracker.Instance = null;
        EnemySpawningManager.Instance = null;
        FadeToBlackPanel.Instance = null;
        GameWorldSpawner.Instance = null;
        PlayerController.Instance = null;
        IntroVideoController.Instance = null;
        LoadPlaySceneFromTitle.Instance = null;
        TurnOrderManager.Instance = null;
        InspectHandler.Instance = null;
        LootManager.Instance = null;
        PlayerInventoryDisplayManager.Instance = null;
        UILimbColorController.Instance = null;
    }
    private void Awake()
    {
        inputActions = new();
        restartAction = inputActions.Player.EndTurn;
        canRestart = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        restartAction.performed += OnRestart;

    }

    private void OnDisable()
    {
        inputActions.Disable();
        restartAction.performed -= OnRestart;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<TextMeshProUGUI>(out deadText))
        {
            Debug.LogError("Could not find TextMeshProUGUI component on GameObject.");
        }
        CombatManager.Instance.PlayerDeath.AddListener(OnPlayerDeath);
        deadText.text = "";
        canRestart = false;
    }

    private void OnDestroy()
    {
        CombatManager.Instance.PlayerDeath.RemoveListener(OnPlayerDeath);
    }

    private void OnPlayerDeath()
    {
        canRestart = true;
        deadText.text = "Game Over!\nPress[Enter] to Quit.";
    }

    private void OnRestart(InputAction.CallbackContext ctx)
    {
        if (!canRestart) return;
        Application.Quit();
    }
}
