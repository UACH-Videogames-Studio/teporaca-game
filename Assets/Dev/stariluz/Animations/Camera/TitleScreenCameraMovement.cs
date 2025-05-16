using UnityEngine;
using UnityEngine.InputSystem;

namespace Stariluz
{
    // [ExecuteInEditMode]
    public class TitleScreenCameraMovement : MonoBehaviour
    {
        public Transform target;
        public float lookSensitivity = 0.1f;
        public float maxYawAngle = 10f;
        public float returnSpeed = 2f;
        public float movementDelay = 3f;

        private float currentYaw = 0f;
        private Quaternion initialRotation;
        private Vector3 initialOffset;
        private float initialHeight;
        private bool movementStarted = false;
        private float elapsedTime = 0f;

        private Vector2 lookInput;
        private bool hasLookInput = false;

        protected InputActions _inputActions;
        public InputActions inputActions => _inputActions;
        protected InputActions.PlayerActions _playerInput;
        public InputActions.PlayerActions playerInput => _playerInput;

        private void Awake()
        {
            _inputActions = new InputActions();
            _playerInput = inputActions.Player;
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        void Start()
        {
            if (!target)
            {
                Debug.LogError("Target not assigned to camera.");
                enabled = false;
                return;
            }

            // Guardamos offset y altura inicial
            initialOffset = transform.position - target.position;
            initialHeight = transform.position.y;

            // Hacemos que mire al target y guardamos la rotación
            transform.LookAt(new Vector3(target.position.x, initialHeight, target.position.z));
            initialRotation = transform.rotation;
        }

        void Update()
        {
            if (!target) return;

            elapsedTime += Time.deltaTime;

            if (!movementStarted)
            {
                if (elapsedTime < movementDelay)
                {
                    // Mantener posición y rotación originales durante el delay
                    transform.position = target.position + initialOffset;
                    transform.rotation = initialRotation;
                    return;
                }

                movementStarted = true;
            }

            lookInput = playerInput.Look.ReadValue<Vector2>();
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

            // Calcular rotación horizontal
            Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);
            Vector3 rotatedOffset = yawRotation * initialOffset;

            // Mantener altura original
            Vector3 targetPosition = target.position + rotatedOffset;
            targetPosition.y = initialHeight;
            transform.position = targetPosition;

            // Mirar al target sin cambiar inclinación vertical
            Vector3 lookAtTarget = new Vector3(target.position.x, initialHeight, target.position.z);
            transform.LookAt(lookAtTarget);
        }
    }
}
