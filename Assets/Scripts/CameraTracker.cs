using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance;
    public static bool isFreecam = false;
    public float moveSpeed = 1.0f;

    /* Events */
    public UnityEvent<bool> ToggleFreecamEvent;

    /* Input */
    private InputSystem_Actions inputActions;
    private InputAction toggleFreecam;
    private InputAction moveCamera;

    private Vector2 cameraInput = Vector2.zero;
    
    public void EnableFreecam() // Enables the freecam.
    {
        isFreecam = true;
        ToggleFreecamEvent.Invoke(isFreecam);
    }
    public void DisableFreecam() // Disables the freecam.
    {
        isFreecam = false;
        ToggleFreecamEvent.Invoke(isFreecam);
    }
    public void ToggleFreecamState() // Toggles the freecam state.
    {
        isFreecam = !isFreecam;
        ToggleFreecamEvent.Invoke(isFreecam);
    }
    public void SetCameraTarget(Transform target)
    {
        DisableFreecam();
        this.transform.parent = target;
        this.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }
    public void SetCameraTarget(GameObject target) => SetCameraTarget(target.transform);
    public void SetCameraTargetToPlayer() => SetCameraTarget(PlayerController.Instance.transform);

    /* *** MONOBEHAVIOUR FUNCTIONS *** */
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
        ToggleFreecamEvent = new UnityEvent<bool>();
        inputActions = new InputSystem_Actions();
        toggleFreecam = inputActions.Camera.ToggleFreecam;
        moveCamera = inputActions.Camera.MoveCamera;

    }
    private void OnEnable()
    {
        inputActions.Enable();
        toggleFreecam.started += ToggleFreecam; // We use started so it only counts on key down.
    }
    private void OnDisable()
    {
        inputActions.Disable();
        toggleFreecam.started -= ToggleFreecam;
    }
    private void Start()
    {
        SetCameraTarget(PlayerController.Instance.transform);
    }
    private void OnDestroy()
    {
        ToggleFreecamEvent.RemoveAllListeners();
        Instance = null;
    }

    private void Update()
    {
        if (CombatManager.pauseAll)
        {
            cameraInput = Vector2.zero;
            return;
        }
        else if (!isFreecam)
        {
            cameraInput = Vector2.zero; // To prevent hanging inputs
            // Set the position to the combatant whose turn it currently is.
            if (TurnOrderManager.Instance.isCombat) transform.position = new Vector3(TurnOrderManager.Instance.currentTurn.transform.position.x, transform.position.y, TurnOrderManager.Instance.currentTurn.transform.position.z);
            else if (PlayerController.Instance != null) transform.position = new Vector3(PlayerController.Instance.transform.position.x, transform.position.y, PlayerController.Instance.transform.position.z);
            return;
        } 
        cameraInput = moveCamera.ReadValue<Vector2>();

        Vector3 move = new(cameraInput.x, 0.0f, cameraInput.y);
        // Rotate the movement by 45 degrees on the Y axis.
        move = Quaternion.Euler(0.0f, 45.0f, 0.0f) * move;
        transform.position += moveSpeed * Time.deltaTime * move;
        if (!GameWorldSpawner.Instance.IsCameraWithinCircleRadius(transform.position))
        {
            transform.position = GameWorldSpawner.Instance.GetClosestPositionWithinCircle(transform.position);
        }
    }

    private void ToggleFreecam(InputAction.CallbackContext context)
    {
        Debug.Log("Freecam toggled!");
        if (context.started)
        {
            ToggleFreecamState();
            ToggleFreecamEvent.Invoke(isFreecam);
        }

    }
}
