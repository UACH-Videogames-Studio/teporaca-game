using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenCameraMovement : MonoBehaviour
{
    [Header("Configuration")]
    public Transform target;
    public float lookSensitivity = 0.1f;
    public float maxYawAngle = 10f;
    public float returnSpeed = 2f;
    public float movementDelay = 3f; // ⏳ Tiempo antes de comenzar el movimiento

    private float currentYaw = 0f;
    private float targetYaw = 0f;

    private Vector3 initialOffset;
    private float initialHeight;

    private Vector2 lookInput;
    private bool hasLookInput = false;

    private float elapsedTime = 0f;
    private bool canMove = false;

    [HideInInspector]
    protected InputActions _inputActions;
    public InputActions inputActions => _inputActions;

    [HideInInspector]
    protected InputActions.PlayerActions _playerInput;
    public InputActions.PlayerActions playerInput => _playerInput;

    private void Awake()
    {
        _inputActions = new InputActions();
        _playerInput = inputActions.Player;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        if (!target)
        {
            Debug.LogError("TitleScreenCameraMovement: No se asignó un 'target'.");
            enabled = false;
            return;
        }

        initialOffset = transform.position - target.position;
        initialHeight = transform.position.y;
    }

    void Update()
    {
        if (!target) return;

        // Esperar antes de activar movimiento
        if (!canMove)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= movementDelay)
                canMove = true;
            else
                return; // No hacer nada hasta que pase el delay
        }

        lookInput = _playerInput.Look.ReadValue<Vector2>();
        hasLookInput = lookInput.sqrMagnitude > 0.001f;

        if (hasLookInput)
        {
            targetYaw += lookInput.x * lookSensitivity;
            targetYaw = Mathf.Clamp(targetYaw, -maxYawAngle, maxYawAngle);
        }
        else
        {
            targetYaw = Mathf.Lerp(targetYaw, 0f, Time.deltaTime * returnSpeed);
        }

        currentYaw = targetYaw;

        Vector3 horizontalOffset = new Vector3(initialOffset.x, 0f, initialOffset.z);
        Quaternion rotation = Quaternion.Euler(0f, currentYaw, 0f);
        Vector3 rotatedOffset = rotation * horizontalOffset;

        Vector3 newPosition = target.position + rotatedOffset;
        newPosition.y = initialHeight;

        transform.position = newPosition;
        transform.LookAt(new Vector3(target.position.x, initialHeight, target.position.z));
    }
}
