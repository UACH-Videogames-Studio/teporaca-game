using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenCameraMovement : MonoBehaviour
{
    [Header("Configuration")]
    public float lookSensitivity = 0.1f;
    public float maxYawAngle = 10f;
    public float returnSpeed = 2f;

    private float currentYaw = 0f;
    private Quaternion originalRotation;

    private Vector2 lookInput;
    private bool hasLookInput = false;

    [HideInInspector]
    protected InputActions _inputActions;
    public InputActions inputActions
    {
        get
        {
            return _inputActions;
        }
    }

    [HideInInspector]
    protected InputActions.PlayerActions _playerInput;
    public InputActions.PlayerActions playerInput
    {
        get
        {
            return _playerInput;
        }
    }

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
        originalRotation = transform.rotation;
    }

    void Update()
    {
        lookInput = _playerInput.Look.ReadValue<Vector2>();
        hasLookInput = lookInput.sqrMagnitude > 0.001f;

        if (hasLookInput)
        {
            currentYaw += lookInput.x * lookSensitivity;
            currentYaw = Mathf.Clamp(currentYaw, -maxYawAngle, maxYawAngle);
        }
        else
        {
            currentYaw = Mathf.Lerp(currentYaw, 0f, Time.deltaTime * returnSpeed);
        }

        Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);
        transform.rotation = yawRotation * originalRotation;
    }
}
