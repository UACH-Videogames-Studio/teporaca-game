using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stariluz
{
    /// <summary>
    /// Based on EasyStart Third Person Controller from Conrado Saud
    /// <br/>
    ///  https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {

        [Space]
        [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
        public float velocity = 1f;

        [Tooltip("This value is added to the speed value while the character is sprinting.")]
        public float sprintAdittion = 1.5f;

        [Tooltip("Animation smoth transition constant")]
        public float smoothTime = 0.25f;

        [Space]
        [Tooltip("The higher the value, the higher the character will jump.")]
        public float jumpForce = 10f;
        [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
        public float jumpTime = 0.85f;

        [Space]
        [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
        public float gravity = 9.8f;

        protected float jumpElapsedTime = 0;

        // Player states
        protected bool isJumping = false;
        protected bool isSprinting = false;
        protected bool isCrouching = false;

        protected bool inputJump;
        protected bool inputCrouch;
        protected bool inputSprint;

        protected CharacterController characterController;
        protected int ACzMovementHash;

        protected Vector2 moveInput;
        protected float currentVelocity;


        public delegate void UpdateFunction();

        public UpdateFunction ExecuteCurrentUpdate;

        protected int ACchopHash;
        private bool isChopping = false;
        [SerializeField] private Collider axeCollider;
        [SerializeField] private float frameRate = 24f; // Asume que el juego corre a 60 fps

        [HideInInspector]
        protected AudioSource _audioSource;
        public AudioSource audioSource
        {
            get
            {
                return _audioSource;
            }
        }
        public AudioClip[] steps;
        private int _lastStepIndex = -1;

        [HideInInspector]
        protected Animator _animator;
        public Animator animator
        {
            get
            {
                return _animator;
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
            ExecuteCurrentUpdate = UpdateOnGame;
            inputActions.Enable();
            playerInput.Attack.started += Chop;
            GameManager.Instance.OnGame += HandleOnGame;
            GameManager.Instance.OnUI += HandleOnUI;
        }
        private void OnDisable()
        {
            inputActions.Disable();
            playerInput.Attack.started -= Chop;
            GameManager.Instance.OnGame -= HandleOnGame;
            GameManager.Instance.OnUI -= HandleOnUI;
        }

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            ACzMovementHash = Animator.StringToHash("zMovement");
            ACchopHash = Animator.StringToHash("chop");

            // Message informing the user that they forgot to add an animator
            if (animator == null)
                Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
        }

        void Update()
        {
            ExecuteCurrentUpdate();
        }

        // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
        private void FixedUpdate()
        {
            // Direction movement
            float directionY = -gravity * Time.deltaTime;

            // Jump handler
            if (isJumping)
            {
                directionY += JumpUpdate();
            }

            Vector3 verticalDirection = Vector3.up * directionY;
            Vector3 horizontalDirection = CalcHoriziontalMovement();

            Vector3 movement = verticalDirection + horizontalDirection;
            characterController.Move(movement);
            UpdateAnimation();
        }

        void UpdateOnGame()
        {
            // Input checkers
            moveInput = playerInput.Move.ReadValue<Vector2>();
            inputJump = playerInput.Jump.IsPressed();
            inputSprint = playerInput.Sprint.IsPressed();
            // inputCrouch = playerInput.Crouch.IsPressed();

            // Check if you pressed the crouch input key and change the player's state
            // if (inputCrouch)
            //     isCrouching = !isCrouching;

            if (characterController.isGrounded)
            {
                // Run
                float minimumSpeed = 0.9f;

                // Sprint
                isSprinting = characterController.velocity.magnitude > minimumSpeed && inputSprint;
            }

            if (CanJump())
            {
                isJumping = true;
            }

            HeadHittingDetect();
        }
        void UpdateOnUI() { }

        void HandleOnGame()
        {
            playerInput.Enable();
            ExecuteCurrentUpdate = UpdateOnGame;
        }

        void HandleOnUI()
        {
            playerInput.Disable();
            ExecuteCurrentUpdate = UpdateOnUI;
        }

        protected void UpdateAnimation()
        {
            // Vector3 velocity = characterController.velocity;

            // animator.SetFloat(ACzMovementHash, new Vector2(velocity.x, velocity.z).magnitude);

            Vector3 velocity = characterController.velocity;
            float targetSpeed = new Vector2(velocity.x, velocity.z).magnitude;

            // Suaviza la transición entre valores de velocidad
            float smoothedSpeed = Mathf.SmoothDamp(animator.GetFloat(ACzMovementHash), targetSpeed, ref currentVelocity, Time.deltaTime * smoothTime);

            animator.SetFloat(ACzMovementHash, smoothedSpeed);
        }

        protected bool CanJump()
        {
            return inputJump && characterController.isGrounded;
        }

        protected float JumpUpdate()
        {
            // Apply inertia and smoothness when climbing the jump
            // It is not necessary when descending, as gravity itself will gradually pulls
            float directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            // Jump timer
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
            return directionY;
        }

        protected Vector3 CalcHoriziontalMovement()
        {
            float velocityAdittion = CalcVelocittyAdition();
            float directionX = moveInput.x * (velocity + velocityAdittion) * Time.deltaTime;
            float directionZ = moveInput.y * (velocity + velocityAdittion) * Time.deltaTime;

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            // Relate the front with the Z direction (depth) and right with X (lateral movement)
            forward *= directionZ;
            right *= directionX;

            if (directionX != 0 || directionZ != 0)
            {
                float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, angle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
            }

            return forward + right;
        }

        protected float CalcVelocittyAdition()
        {
            float velocityAdittion = 0;
            if (isSprinting)
                velocityAdittion = sprintAdittion;
            if (isCrouching)
                velocityAdittion = -(velocity * 0.50f); // -50% velocity
            return velocityAdittion;
        }

        //This function makes the character end his jump if he hits his head on something
        void HeadHittingDetect()
        {
            float headHitDistance = 1.1f;
            Vector3 ccCenter = transform.TransformPoint(characterController.center);
            float hitCalc = characterController.height / 2f * headHitDistance;

            // Uncomment this line to see the Ray drawed in your characters head
            // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

            if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
            {
                jumpElapsedTime = 0;
                isJumping = false;
            }
        }


        private void Chop(InputAction.CallbackContext context)
        {
            if (!isChopping)
            {
                isChopping = true;
                animator.SetTrigger(ACchopHash);
                StartCoroutine(EnableAxeColliderAtFrame(5, 16));
            }
        }
        public void EndChoping()
        {
            isChopping = false;
        }
        private IEnumerator EnableAxeColliderAtFrame(int startFrame, int endFrame)
        {
            // Esperar hasta el frame 10
            float startTime = startFrame / frameRate;
            yield return new WaitForSeconds(startTime);

            axeCollider.enabled = true;

            // Esperar hasta el frame 20 (desde el frame 10 ya estamos esperando)
            float duration = (endFrame - startFrame) / frameRate;
            yield return new WaitForSeconds(duration);

            axeCollider.enabled = false;
        }
        public void PlayStepSound()
        {
            if (steps == null || steps.Length == 0 || _audioSource == null)
                return;

            int index;
            do
            {
                index = Random.Range(0, steps.Length);
            } while (index == _lastStepIndex && steps.Length > 1);

            _lastStepIndex = index;
            AudioClip clip = steps[index];

            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.volume = Random.Range(0.8f, 1.0f); // volumen aleatorio
            
            _audioSource.PlayOneShot(clip);
        }
    }

}