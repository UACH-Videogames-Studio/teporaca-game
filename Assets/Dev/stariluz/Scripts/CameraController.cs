
using UnityEngine;

namespace Stariluz
{
    /*
        This file has a commented version with details about how each line works. 
        The commented version contains code that is easier and simpler to read. This file is minified.
    */

    /// <summary>
    /// Camera movement script for third person games.
    /// This Script should not be applied to the camera! It is attached to an empty object and inside
    /// it (as a child object) should be your game's MainCamera.
    /// <br/>
    /// <br/>
    /// 
    /// Based on EasyStart Third Person Controller from Conrado Saud
    /// <br/>
    ///  https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977
    /// </summary>
    /// 
    public class CameraController : MonoBehaviour
    {

        [Tooltip("Enable to move the camera by holding the right mouse button. Does not work with joysticks.")]
        public bool clickToMoveCamera = false;
        [Tooltip("Enable zoom in/out when scrolling the mouse wheel. Does not work with joysticks.")]
        public bool canZoom = true;
        [Space]
        [Tooltip("The higher it is, the faster the camera moves. It is recommended to increase this value for games that uses joystick.")]
        public float sensitivity = 5f;

        [Tooltip("Camera Y rotation limits. The X axis is the maximum it can go up and the Y axis is the maximum it can go down.")]
        public Vector2 cameraLimit = new Vector2(-45, 40);
        
        Transform player;
        protected float yOffset;
        protected Vector2 viewDirection;
        protected Vector2 lookInput;
        protected float CONSTANT_SENSIVITY = 5f;
        protected float ZOOM_CONSTANT_SENSIVITY = 1f;

        public float adjustedSensivity
        {
            get
            {
                return sensitivity*CONSTANT_SENSIVITY;
            }
        }
        
        public float adjustedZoomSensivity
        {
            get
            {
                return sensitivity*CONSTANT_SENSIVITY*ZOOM_CONSTANT_SENSIVITY;
            }
        }
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

            player = GameObject.FindWithTag("Player").transform;
            yOffset = transform.position.y;

            // Lock and hide cursor with option isn't checked
            if (!clickToMoveCamera)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }

        }

        void Update()
        {
            UpdatePosition();
            UpdateZoom();
            UpdateRotation();
        }

        private void UpdatePosition()
        {
            // Follow player - camera offset
            transform.position = player.position + new Vector3(0, yOffset, 0);
        }
        private void UpdateRotation()
        {
            lookInput = _playerInput.Look.ReadValue<Vector2>();
            Debug.Log(lookInput);
            viewDirection += adjustedSensivity * Time.deltaTime * lookInput;
            viewDirection.y = Mathf.Clamp(viewDirection.y, cameraLimit.x, cameraLimit.y);
            transform.rotation = Quaternion.Euler(-viewDirection.y, viewDirection.x, 0);
        }

        private void UpdateZoom()
        {
            float zoom = _playerInput.Zoom.ReadValue<float>();

            if (canZoom && zoom != 0)
                Camera.main.fieldOfView -= zoom * adjustedZoomSensivity * Time.deltaTime;
            // @todo Use Mathf.Clamp to set limits on the field of view
        }
        
    }
}