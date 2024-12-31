using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class MouseCast : MonoBehaviour
{
    public bool isUpdating = true;
    private bool isPlayerAttacking = false;
    // Creat a layer mask for the raycast- it should only hit the "TraversibleGeometry" layer
    private LayerMask floorLayerMask;
    private LayerMask pathNodeMask;
    private LayerMask wallLayerMask;
    private Collider[] hitColliders;

    /* *** INPUT *** */

    private InputSystem_Actions inputActions;
    private InputAction mouseClickAction;
    private InputAction inspectAction;

    [Header("Materials")]
    public Material walkableMaterial;
    public Material unwalkableMaterial;
    private List<PathfindingNode> previousAttackHits = new();
    private List<GameObject> attackIndicatorNodes = new();
    private List<GameObject> activeAttackIndicatorNodes = new();

    public static UnityEvent<PathfindingNode> OnNodeClicked = new();

    [SerializeField] private GameObject gridHighlightObject;
    private MeshRenderer gridHighlightObjectMeshRenderer;
    [SerializeField] private GameObject attackIndicatorHighlight;
    public PathfindingNode GetClosestNodeToPoint(Vector3 worldPoint)
    {
        int numHits = Physics.OverlapSphereNonAlloc(worldPoint, 0.5f, hitColliders, pathNodeMask);
        if (numHits <= 0) { return null; }
        Collider closestNode = hitColliders[0];
        for (int i = 1; i < numHits; i++)
        {
            if (Vector3.Distance(worldPoint, hitColliders[i].transform.position) < Vector3.Distance(worldPoint, closestNode.transform.position)) { closestNode = hitColliders[i]; }
        }
        if (closestNode == null) { return null; }
        if (closestNode.TryGetComponent(out PathfindingNode node)) { return node; }
        return null;
    }

    private Vector3? GetMouseHitPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayerMask))
        {
            return hit.point;
        }
        return null;
    }

    private void SetGameCursorToMousePosition()
    {
        // TODO: add a check if the player is in combat or not. If not, return early.
        Vector3? hitPoint = GetMouseHitPosition();
        if (hitPoint.HasValue)
        {
            PathfindingNode closestNode = GetClosestNodeToPoint(hitPoint.Value);
            if (closestNode != null)
            {
                gridHighlightObject.transform.position = new Vector3(closestNode.transform.position.x, closestNode.transform.position.y + 0.001f, closestNode.transform.position.z);
                gridHighlightObjectMeshRenderer.enabled = true;
                gridHighlightObjectMeshRenderer.material = closestNode.walkState switch
                {
                    WalkState.Unwalkable => unwalkableMaterial,
                    _ => walkableMaterial
                };
            }
            else
            {
                gridHighlightObjectMeshRenderer.enabled = false;
            }
        }
    }

    private void DrawAttackPrediction()
    {
        Limb limb = PlayerController.Instance.GetPrimaryLimb(); // Get the primary limb to determine which attack cast to use.
        if (limb == null) return; // Safety check.
        Vector3? hitPoint = GetMouseHitPosition();
        if (!hitPoint.HasValue)
        {
            // Clear all attack indicators
            foreach (GameObject node in activeAttackIndicatorNodes)
            {
                node.SetActive(false);
            }
            activeAttackIndicatorNodes.Clear();
            return;
        }
        if (limb.attackData == null)
        {
            Debug.LogError("Primary limb did not have an attack data object while drawing attack.");
            return;
        }; // Safety check.
        List<PathfindingNode> attackedNodes = limb.attackData.GetAttackedNodes(PlayerController.Instance.combatant, limb, hitPoint.Value);
        // Compare the length of the previous attack hits to the current attack hits. Enable or disable objects as necessary.
        if (activeAttackIndicatorNodes.Count > attackedNodes.Count) // There are too many active indicator nodes and some need to be disabled.
        {
            Debug.Log("Need to cull attack indicators.");
            // Disable the excess attack indicators
            int numExcess = activeAttackIndicatorNodes.Count - attackedNodes.Count;
            for (int i = 0; i < numExcess; i++)
            {
                activeAttackIndicatorNodes[^1].SetActive(false);
                activeAttackIndicatorNodes.RemoveAt(activeAttackIndicatorNodes.Count - 1);
            }
        }
        else if (activeAttackIndicatorNodes.Count < attackedNodes.Count) // There are not enough active attack indicator nodes and some need to be enabled.
        {
            Debug.Log("Need to add attack indicators");
            if (attackedNodes.Count > attackIndicatorNodes.Count) // If true, there is not enough nodes and new ones need to be instantiated.
            {
                int indicatorNodeOriginalCount = attackIndicatorNodes.Count;
                int numNeedingInstancing = attackedNodes.Count - attackIndicatorNodes.Count;
                for (int i = 0; i < numNeedingInstancing; i++)
                {
                    GameObject attackIndicatorNode = Instantiate(attackIndicatorHighlight, Vector3.zero, Quaternion.identity);
                    attackIndicatorNode.transform.parent = transform;
                    attackIndicatorNode.name = "AttackIndicatorNode_" + (i + indicatorNodeOriginalCount).ToString();
                    attackIndicatorNode.SetActive(false);
                    attackIndicatorNodes.Add(attackIndicatorNode);
                }
            }
            int numDeficit = attackedNodes.Count - activeAttackIndicatorNodes.Count;
            List<GameObject> newInitializedNodes = new();
            foreach (GameObject node in attackIndicatorNodes)
            {
                // Check if the node is in the active list. If not, add it to the list of nodes to initialize.
                if (!activeAttackIndicatorNodes.Contains(node))
                {
                    // Enable the node
                    node.SetActive(true);
                    newInitializedNodes.Add(node);
                    if (newInitializedNodes.Count == numDeficit) break;
                }
            }
            // Add the new nodes to the active list
            activeAttackIndicatorNodes.AddRange(newInitializedNodes);
        }
        // Iterate through the attacked nodes and set the positions of the active attack indicator nodes.
        // Clear the previous attack
        previousAttackHits.Clear();
        for (int i = 0; i < attackedNodes.Count; i++)
        {
            activeAttackIndicatorNodes[i].transform.position = new Vector3(attackedNodes[i].transform.position.x, attackedNodes[i].transform.position.y + 0.002f, attackedNodes[i].transform.position.z);
            previousAttackHits.Add(attackedNodes[i]);
        }
    }

    /* *** MONOBEHAVIOUR FUNCTIONS *** */
    private void Awake()
    {
        hitColliders = new Collider[9];
        floorLayerMask = LayerMask.GetMask("TraversibleGeometry");
        pathNodeMask = LayerMask.GetMask("PathNodes");
        wallLayerMask = LayerMask.GetMask("WallBlocker");
        inputActions = new InputSystem_Actions();
        mouseClickAction = inputActions.Player.Click;
        inspectAction = inputActions.Player.InspectTile;
        // Instantiate the prefab into attackNodes array
        for (int i = 0; i < 100; i++)
        {
            GameObject obj = Instantiate(attackIndicatorHighlight, Vector3.zero, Quaternion.identity);
            obj.transform.parent = transform;
            obj.name = "AttackIndicatorNode_" + i.ToString();
            obj.SetActive(false);
            attackIndicatorNodes.Add(obj);
        }
    }
    private void OnEnable()
    {
        inputActions.Enable();
        mouseClickAction.performed += OnMouseClick;
        inspectAction.performed += OnInspectPress;

    }
    private void OnDisable()
    {
        inputActions.Disable();
        mouseClickAction.performed -= OnMouseClick;
        inspectAction.performed -= OnInspectPress;

    }
    private void Start()
    {
        gridHighlightObjectMeshRenderer = gridHighlightObject.GetComponent<MeshRenderer>();
        PlayerController.Instance.EngagePlayerAttackModeEvent.AddListener(EngagePlayerAttackMode);
        PlayerController.Instance.DisengagePlayerAttackModeEvent.AddListener(DisengagePlayerAttackMode);
    }

    private void OnDestroy()
    {
        // Destroy all the attack indicator nodes
        for (int i = 0; i < attackIndicatorNodes.Count; i++)
        {
            Destroy(attackIndicatorNodes[i]);
        }
        PlayerController.Instance.EngagePlayerAttackModeEvent.RemoveListener(EngagePlayerAttackMode);
        PlayerController.Instance.DisengagePlayerAttackModeEvent.RemoveListener(DisengagePlayerAttackMode);
    }

    void Update()
    {
        SetGameCursorToMousePosition();
        if (isPlayerAttacking) DrawAttackPrediction();
    }
    
    /* *** INPUT ACTIONS *** */
    private void OnMouseClick(InputAction.CallbackContext ctx)
    {
        if (isUpdating && isPlayerAttacking && PlayerController.Instance.combatant.IsAnyLimbsEngaged() && PlayerController.Instance.attacksRemaining > 0 && !InspectHandler.Instance.isDisplaying)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayerMask))
            {
                
                PlayerController.Instance.RefreshPlayerStats.Invoke();
                bool hasHitTarget = false;
                foreach (PathfindingNode node in previousAttackHits)
                {
                    // Get the combatant at the node
                    Combatant target = PathNodeManager.Instance.GetCombatantOnNode(node);
                    if (target == null) continue;
                    // Calculate the damage
                    hasHitTarget = true;
                    Damage damage = Damage.CalculateDamageAgainstTarget(PlayerController.Instance.combatant, target);
                    // Apply the damage
                    target.ApplyDamage(damage);
                }
                if (!hasHitTarget)
                {
                    AudioManager.Instance.PlayHitSoundAtLocation(hit.point, DamageType.Physical, true, false);
                }
                else
                {
                    PlayerController.Instance.attacksRemaining--;
                    PlayerController.Instance.RefreshLimbUI.Invoke();
                    PlayerController.Instance.RefreshPlayerStats.Invoke();
                }
            }
            if (PlayerController.Instance.attacksRemaining <= 0)
            {
                PlayerController.Instance.DisengagePlayerAttackMode();
            }
        }
    }
    public void EngagePlayerAttackMode()
    {
        isPlayerAttacking = true;
        previousAttackHits.Clear(); // Safety check
        PlayerController.Instance.RefreshLimbUI.Invoke();
        PlayerController.Instance.RefreshPlayerStats.Invoke();
    }
    public void DisengagePlayerAttackMode()
    {
        isPlayerAttacking = false;
        previousAttackHits.Clear();
        foreach (GameObject node in activeAttackIndicatorNodes)
        {
            node.SetActive(false);
        }
        activeAttackIndicatorNodes.Clear();
        PlayerController.Instance.RefreshLimbUI.Invoke();
        PlayerController.Instance.RefreshPlayerStats.Invoke();
    }
    private void OnInspectPress(InputAction.CallbackContext ctx)
    {
        if (!isUpdating || PlayerInventoryDisplayManager.Instance.isInventoryOpen) return;
        if (InspectHandler.Instance.isDisplaying)
        {
            InspectHandler.Instance.HideInspectPanel();
            return;
        }
        Vector3? hitPoint = GetMouseHitPosition();
        if (!hitPoint.HasValue) return;
        PathfindingNode closestNode = GetClosestNodeToPoint(hitPoint.Value);
        if (closestNode == null) return;
        // Get the combatant at the node, if any.
        Combatant target = PathNodeManager.Instance.GetCombatantOnNode(closestNode);
        if (target == null) return;
        InspectHandler.Instance.DisplayOrUpdateInspect(target);
    }
}
