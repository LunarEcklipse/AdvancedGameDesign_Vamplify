using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Combatant))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    [Header("Movement Speeds")]
    [SerializeField][Tooltip("The speed at which the player travels outside of water.")] private float nodeTravelSpeed = 6.0f;
    [SerializeField][Tooltip("The speed at which the player travels through water.")] private float waterNodeTravelSpeed = 4.0f;

    private bool isControlsLocked = false;
    private bool isCombat = false;
    private bool isTurn = false;
    private bool isMoving = false;
    private PathfindingNode occupyingNode;

    private bool nonCombatPlayerMovingNorth = false;
    private bool nonCombatPlayerMovingEast = false;
    private bool nonCombatPlayerMovingSouth = false;
    private bool nonCombatPlayerMovingWest = false;

    public int allowedMovesPerTurn;
    public int MovesRemaining { get; private set; }
    public int allowedAttacksPerTurn;
    public int attacksRemaining;

    private float normalGroundY = 0.5f;
    private float inWaterY = 0.1f;
    private float flyingY = 1.0f;

    public bool isInWater = false;
    public bool isFlying = false;

    [System.NonSerialized]public Combatant combatant;

    /* *** Player Input *** */
    private InputSystem_Actions inputActions;
    private InputAction moveNorth;
    private InputAction moveEast;
    private InputAction moveSouth;
    private InputAction moveWest;

    private InputAction selectLimb1;
    private InputAction selectLimb2;
    private InputAction selectLimb3;
    private InputAction selectLimb4;

    private InputAction engageAttackMode;
    private InputAction engageCamera;
    private InputAction endTurn;

    private Tuple<int, int> inputBufferDirection = new(0, 0);
    private bool isAttackModeBuffered = false;

    /* *** COROUTINES *** */
    private Coroutine movementCoroutine;
    private Coroutine movementBufferCoroutine;
    private Coroutine nonCombatMovementBufferCoroutine;

    /* *** EVENTS *** */
    [Header("Events")]
    public UnityEvent EngagePlayerAttackModeEvent = new();
    public UnityEvent DisengagePlayerAttackModeEvent = new();
    public UnityEvent RefreshLimbUI = new();
    public UnityEvent RefreshPlayerStats = new();

    // Move player along path
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
        moveNorth = inputActions.Player.MoveNorth;
        moveEast = inputActions.Player.MoveEast;
        moveSouth = inputActions.Player.MoveSouth;
        moveWest = inputActions.Player.MoveWest;

        selectLimb1 = inputActions.Player.SelectLimb1;
        selectLimb2 = inputActions.Player.SelectLimb2;
        selectLimb3 = inputActions.Player.SelectLimb3;
        selectLimb4 = inputActions.Player.SelectLimb4;

        engageAttackMode = inputActions.Player.EngageAttack;
        endTurn = inputActions.Player.EndTurn;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        moveNorth.performed += OnMoveNorth;
        moveEast.performed += OnMoveEast;
        moveSouth.performed += OnMoveSouth;
        moveWest.performed += OnMoveWest;
        moveNorth.canceled += OnMoveNorth;
        moveEast.canceled += OnMoveEast;
        moveSouth.canceled += OnMoveSouth;
        moveWest.canceled += OnMoveWest;

        selectLimb1.performed += OnSelectLimb1;
        selectLimb2.performed += OnSelectLimb2;
        selectLimb3.performed += OnSelectLimb3;
        selectLimb4.performed += OnSelectLimb4;

        engageAttackMode.performed += OnAttackModeInput;
        endTurn.performed += OnEndTurnInput;
    }


    private void OnDisable()
    {
        inputActions.Disable();
        moveNorth.performed -= OnMoveNorth;
        moveEast.performed -= OnMoveEast;
        moveSouth.performed -= OnMoveSouth;
        moveWest.performed -= OnMoveWest;
        moveNorth.canceled -= OnMoveNorth;
        moveEast.canceled -= OnMoveEast;
        moveSouth.canceled -= OnMoveSouth;
        moveWest.canceled -= OnMoveWest;

        selectLimb1.performed -= OnSelectLimb1;
        selectLimb2.performed -= OnSelectLimb2;
        selectLimb3.performed -= OnSelectLimb3;
        selectLimb4.performed -= OnSelectLimb4;

        engageAttackMode.performed -= OnAttackModeInput;
        endTurn.performed -= OnEndTurnInput;
    }
    void Start()
    {
        // Get the pathfinding component attached to this gameobject. If one doesn't exist, create it.
        
        // Subscribe to the OnNodeClicked event
        // Get the combatant on this object. If one doesn't exist create it
        if (!TryGetComponent<Combatant>(out combatant)) combatant = gameObject.AddComponent<Combatant>();
        combatant.isPlayer = true;

        // Subscribe the player to the SpawnActors event
        CombatManager.Instance.SpawnActors.AddListener(OnSpawnActors);
        CombatManager.Instance.CombatStart.AddListener(OnCombatStart);
        CombatManager.Instance.CombatEnded.AddListener(OnCombatEnd);
        CombatManager.Instance.CombatTurnStart.AddListener(OnCombatStartTurn);
        CameraTracker.Instance.ToggleFreecamEvent.AddListener(OnToggleFreecam);
        nonCombatMovementBufferCoroutine = StartCoroutine(NonCombatMovementBuffer());
    }
    private void OnDestroy()
    {
        CombatManager.Instance.SpawnActors.RemoveListener(OnSpawnActors);
        CombatManager.Instance.CombatTurnStart.RemoveListener(OnCombatStartTurn);
        EngagePlayerAttackModeEvent.RemoveAllListeners();
        DisengagePlayerAttackModeEvent.RemoveAllListeners();
        RefreshLimbUI.RemoveAllListeners();
        RefreshPlayerStats.RemoveAllListeners();
        StopCoroutine(nonCombatMovementBufferCoroutine);
    }
    private PathfindingNode GetCurrentNode()
    {
        // Get the X and Z coordinates of the player's current position
        int x = Mathf.RoundToInt(transform.position.x);
        int z = Mathf.RoundToInt(transform.position.z);
        // Get the node at the player's current position
        return PathNodeManager.Instance.GetPathfindingNode(x, z);
    }
    private IEnumerator RestartBufferedMovement()
    {
        // Get the target node based on the input buffer direction
        PathfindingNode targetNode = PathNodeManager.Instance.GetPathfindingNode(occupyingNode.GetX() + inputBufferDirection.Item1, occupyingNode.GetZ() + inputBufferDirection.Item2);
        // If the target node does not exist or is not traversible, set isMoving = false and return
        if (targetNode == null || !targetNode.IsTraversible(CreatureMovementType.All) || targetNode.IsNodeOccupied() || (MovesRemaining <= 0 && isCombat))
        {
            isMoving = false;
            yield break;
        }
        // TODO: Add a check here that checks if the player is allowed to keep moving or not
        // Start the movement coroutine
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MovePlayerToNode(targetNode));
        inputBufferDirection = new Tuple<int, int>(0, 0);
    }
    private IEnumerator MovePlayerToNode(PathfindingNode node)
    {
        isMoving = true;
        if (isCombat)
        {
            MovesRemaining -= 1;
            RefreshPlayerStats.Invoke();
        }
        var targetPosition = node.walkState switch
        {
            WalkState.Flyable => new Vector3(node.GetX(), flyingY, node.GetZ()),
            WalkState.Swimmable => new Vector3(node.GetX(), inWaterY, node.GetZ()),
            _ => new Vector3(node.GetX(), normalGroundY, node.GetZ()),
        };
        if (node.walkState == WalkState.Swimmable) isInWater = true;
        else isInWater = false;
        if (node.walkState == WalkState.Flyable) isFlying = true;
        else isFlying = false;
        RefreshLimbUI.Invoke();
        float speed = node.walkState switch
        {
            WalkState.Swimmable => waterNodeTravelSpeed,
            _ => nodeTravelSpeed,
        };
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        occupyingNode.ClearOccupant();
        occupyingNode = node;
        occupyingNode.SetOccupant(combatant);
        if (!isCombat && occupyingNode.isRoomExit)
        {
            Debug.Log("Transferring player to new room.");
            CombatManager.Instance.PlayerTouchedRoomTrigger.Invoke();
        }
        if (isAttackModeBuffered)
        {
            EngagePlayerAttackMode();
            isAttackModeBuffered = false;
            inputBufferDirection = new Tuple<int, int>(0, 0);
        }
        if (inputBufferDirection != new Tuple<int, int>(0, 0))
        {
            if (movementBufferCoroutine != null) StopCoroutine(movementBufferCoroutine);
            movementBufferCoroutine = StartCoroutine(RestartBufferedMovement());
        }
        else
        {
            isMoving = false;
        }
    }

    private IEnumerator NonCombatMovementBuffer()
    {
        while (true)
        {
            if (isCombat)
            {
                yield return null;
                continue;
            }
            int x = 0; // North/South
            int y = 0; // East/West
            if (nonCombatPlayerMovingNorth) x += 1;
            if (nonCombatPlayerMovingSouth) x -= 1;
            if (nonCombatPlayerMovingEast) y -= 1;
            if (nonCombatPlayerMovingWest) y += 1;
            Tuple<int, int> movementBuffer = isCombat switch
            {
                true => new Tuple<int, int>(0, 0),
                false => new Tuple<int, int>(x, y),
            };
            // Put the movementBuffer into the inputBufferDirection
            inputBufferDirection = movementBuffer;
            yield return null;
        }
    }

    /* *** PLAYER ATTACK MODE *** */
    private void EngagePlayerAttackMode()
    {
        if (attacksRemaining < 1)
        {
            Debug.Log("Player has no attacks remaining!");
            return;
        }
        Debug.Log("Player attack mode engaged!");
        EngagePlayerAttackModeEvent.Invoke();
        combatant.isInAttackMode = true;
        inputBufferDirection = new Tuple<int, int>(0, 0); // Clear the input buffer
    }
    public void DisengagePlayerAttackMode()
    {
        combatant.isInAttackMode = false;
        // Disengage all limbs
        combatant.ClearEngagedLimbs();
        DisengagePlayerAttackModeEvent.Invoke();
    }

    public Limb GetPrimaryLimb() { return combatant.GetPrimaryLimb(); } // Essentially acts as a shortcut to the primary limb for the mouse finder.

    /* *** Calculate Stats *** */
    public int GetPlayerMovementMax()
    {
        return combatant.Speed < 1 ? 1 : (int)combatant.Speed;
    }
    public int GetPlayerAttackMax()
    {
        return combatant.Speed < 5 ? 1 : (int)combatant.Speed / 5;
    }

    /* *** Player Movement Inputs *** */
    private void CombatMovePlayerToNode(int x, int z) // X and Z should be an adjacent node, but theoretically can work for any node.
    {
        PathfindingNode targetNode = PathNodeManager.Instance.GetPathfindingNode(x, z);
        // If the target node does not exist or is not traversible, return
        if (targetNode == null || !targetNode.IsTraversible(CreatureMovementType.All) || targetNode.IsNodeOccupied() || MovesRemaining <= 0) return;
        // If the player is already moving, return
        // Start the movement coroutine
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MovePlayerToNode(targetNode));
    }
    private void OnMoveNorth(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll || !isTurn || InspectHandler.Instance.isDisplaying || combatant.isInAttackMode || isControlsLocked) { return; }
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (context.performed)
        {
            if (MovesRemaining <= 0 && isCombat)
            {
                Debug.Log("Player has no moves remaining!");
                return;
            }
            if (!isCombat)
            {
                nonCombatPlayerMovingNorth = true;
            }
            if (isMoving)
            {
                inputBufferDirection = new Tuple<int, int>(1, 0);
                isAttackModeBuffered = false;
                return;
            }
            
            Debug.Log("Moving player North");
            CombatMovePlayerToNode(occupyingNode.GetX() + 1, occupyingNode.GetZ());
        }
        if (context.canceled)
        {
            if (!isCombat)
            {
                nonCombatPlayerMovingNorth = false;
            }
        }
    }

    private void OnMoveEast(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll || !isTurn || InspectHandler.Instance.isDisplaying || combatant.isInAttackMode || isControlsLocked) { return; }
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (context.performed)
        {
            if (MovesRemaining <= 0 && isCombat)
            {
                Debug.Log("Player has no moves remaining!");
                return;
            }
            if (!isCombat)
            {
                nonCombatPlayerMovingEast = true;
            }
            if (isMoving)
            {
                inputBufferDirection = new Tuple<int, int>(0, -1);
                isAttackModeBuffered = false;
                return;
            }
            
            Debug.Log("Moving player East");
            CombatMovePlayerToNode(occupyingNode.GetX(), occupyingNode.GetZ() - 1);
        }
        if (context.canceled)
        {
            if (!isCombat)
            {
                nonCombatPlayerMovingEast = false;
            }
        }
    }
    private void OnMoveSouth(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll || !isTurn || InspectHandler.Instance.isDisplaying || combatant.isInAttackMode || isControlsLocked) { return; }
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (context.performed)
        {
            if (MovesRemaining <= 0 && isCombat)
            {
                Debug.Log("Player has no moves remaining!");
                return;
            }
            if (!isCombat)
            {
                nonCombatPlayerMovingSouth = true;
            }
            if (isMoving)
            {
                inputBufferDirection = new Tuple<int, int>(-1, 0);
                isAttackModeBuffered = false;
                return;
            }
            
            Debug.Log("Moving player South");
            CombatMovePlayerToNode(occupyingNode.GetX() - 1, occupyingNode.GetZ());
        }
        if (context.canceled)
        {
            if (!isCombat)
            {
                nonCombatPlayerMovingSouth = false;
            }
        }
    }
    private void OnMoveWest(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll || !isTurn || InspectHandler.Instance.isDisplaying || combatant.isInAttackMode || isControlsLocked) { return; }
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (context.performed)
        {
            if (MovesRemaining <= 0 && isCombat)
            {
                Debug.Log("Player has no moves remaining!");
                return;
            }
            if (!isCombat)
            {
                nonCombatPlayerMovingWest = true;
            }
            if (isMoving)
            {
                inputBufferDirection = new Tuple<int, int>(0, 1);
                isAttackModeBuffered = false;
                return;
            }
            Debug.Log("Moving player West");
            CombatMovePlayerToNode(occupyingNode.GetX(), occupyingNode.GetZ() + 1);
        }
        if (context.canceled)
        {
            if (!isCombat)
            {
                nonCombatPlayerMovingWest = false;
            }
        }
    }

    private void UpdateLimbEngagement(int position)
    {
        if (position < 0 || position > combatant.maxLimbs)
        {
            Debug.LogError($"Player attempted to update limb engagement for invalid position: {position}.");
        }
        if (!combatant.isInAttackMode) EngagePlayerAttackMode();
        Limb engageLimb = combatant.GetLimbAtPosition(position);
        if (engageLimb == null)
        {
            Debug.Log($"There is no limb at position {position}");
            return;
        }
        if (combatant.IsLimbEngaged(engageLimb))
        {
            combatant.DisengageLimb(engageLimb);
            Debug.Log($"Disengaging Limb at position {position}");
            if (combatant.GetPrimaryLimb() == engageLimb)
            {
                Debug.Log("Primary limb disengaged.");
                combatant.SetPrimaryLimb(combatant.GetFirstEngagedLimb()); // GetFirstEngagedLimb returns null if there are no other engaged limbs, so we have no need to check if there are other engaged limbs first.
            }
            if (!combatant.IsAnyLimbsEngaged())
            {
                Debug.Log("No limbs engaged, disengaging attack mode");
                DisengagePlayerAttackMode();
            }
            return;
        }
        else
        {
            combatant.EngageLimb(engageLimb);
            Debug.Log($"Engaging Limb at position {position}");
            if (combatant.GetPrimaryLimb() == null)
            {
                combatant.SetPrimaryLimb(engageLimb);
                Debug.Log($"Set primary limb to Limb at position {position}");
            }
        }
    }

    private void OnSelectLimb1(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (attacksRemaining < 1) return;
        if (isMoving)
        {
            isAttackModeBuffered = true;
            return;
        }
        if (context.performed)
        {
            if (isFlying) return; // Can't use arms while flying
            UpdateLimbEngagement(0);
            RefreshLimbUI.Invoke();
        }
    }

    private void OnSelectLimb2(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (attacksRemaining < 1) return;
        if (isMoving)
        {
            isAttackModeBuffered = true;
            return;
        }
        if (context.performed)
        {
            if (isFlying) return; // Can't use arms while flying
            UpdateLimbEngagement(1);
            RefreshLimbUI.Invoke();
        }
    }
    private void OnSelectLimb3(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (attacksRemaining < 1) return;
        if (isMoving)
        {
            isAttackModeBuffered = true;
            return;
        }
        if (context.performed)
        {
            if (isInWater) return;
            UpdateLimbEngagement(2);
            RefreshLimbUI.Invoke();
        }
    }
    private void OnSelectLimb4(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (attacksRemaining < 1) return;
        if (isMoving)
        {
            isAttackModeBuffered = true;
            return;
        }
        if (context.performed)
        {
            if (isInWater) return;
            UpdateLimbEngagement(3);
            RefreshLimbUI.Invoke();
        }
    }
    private void OnAttackModeInput(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (isMoving)
        {
            isAttackModeBuffered = true;
            return;
        }
        if (context.performed)
        {
            if (combatant.isInAttackMode)
            {

                Debug.Log("Disengaging attack mode");
                DisengagePlayerAttackMode();
            }
            else
            {
                Debug.Log("Engaging attack mode");
                EngagePlayerAttackMode();
            }
        }
    }

    private void OnEndTurnInput(InputAction.CallbackContext context)
    {
        if (CombatManager.pauseAll) return;
        if (!isTurn) return;
        if (InspectHandler.Instance.isDisplaying) return;
        if (LootManager.Instance.isDisplaying) return;
        if (PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (context.performed)
        {
            CombatManager.Instance.CombatTurnEnd.Invoke(combatant);
        }
    }
        /* *** EVENT LISTENERS *** */
        private void OnSpawnActors()
    {
        // Get the player spawn node
        PathfindingNode playerSpawn = PathNodeManager.Instance.GetPlayerSpawnNode();
        // Set the player's position to the player spawn node.
        switch (playerSpawn.walkState)
        {
            case WalkState.Walkable:
                transform.position = new Vector3(playerSpawn.GetX(), normalGroundY, playerSpawn.GetZ());
                break;
            case WalkState.Unwalkable:
                Debug.LogError("Player spawn node is unwalkable!");
                transform.position = new Vector3(playerSpawn.GetX(), normalGroundY, playerSpawn.GetZ());
                break;
            case WalkState.Flyable:
                transform.position = new Vector3(playerSpawn.GetX(), flyingY, playerSpawn.GetZ());
                break;
            case WalkState.Swimmable:
                transform.position = new Vector3(playerSpawn.GetX(), inWaterY, playerSpawn.GetZ());
                break;
        }
        occupyingNode = playerSpawn;
        playerSpawn.SetOccupant(combatant);
        Debug.Log("Player spawned successfully!");
    }
    private void OnCombatStartTurn(Combatant turnStartCombatant)
    {
        inputBufferDirection = new Tuple<int, int>(0, 0);
        DisengagePlayerAttackMode();
        if (turnStartCombatant == combatant)
        {
            isTurn = true;
            allowedMovesPerTurn = GetPlayerMovementMax();
            MovesRemaining = allowedMovesPerTurn;
            allowedAttacksPerTurn = GetPlayerAttackMax();
            attacksRemaining = allowedAttacksPerTurn;
            RefreshPlayerStats.Invoke();
        }
        else
        {
            isTurn = false;
        }
    }
    private void OnCombatEndTurn(Combatant turnEndCombatant)
    {
        isTurn = false;
        DisengagePlayerAttackMode();
        inputBufferDirection = new Tuple<int, int>(0, 0);
        RefreshPlayerStats.Invoke();
    }

    private void OnCombatStart()
    {
        isCombat = true;
        nonCombatPlayerMovingEast = false;
        nonCombatPlayerMovingNorth = false;
        nonCombatPlayerMovingSouth = false;
        nonCombatPlayerMovingWest = false;
    }

    private IEnumerator DisengageAfterFramePass() // Prevents a NRE
    {
        yield return null;
        DisengagePlayerAttackMode();
    }
    private void OnCombatEnd()
    {
        isCombat = false;
        combatant.level += 1;
        RefreshPlayerStats.Invoke();
        if (combatant.IsAnyLimbsEngaged()) StartCoroutine(DisengageAfterFramePass());
    }

    private void OnToggleFreecam(bool freecamState)
    {
        isControlsLocked = freecamState;
    }
}
