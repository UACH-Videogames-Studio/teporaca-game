/*using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    float ySpeed;


    Quaternion targetRotation;

    CameraControllerE cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;

    private void Awake()
    {
       cameraController = Camera.main.GetComponent<CameraControllerE>();
       animator = GetComponent<Animator>();
       characterController = GetComponent<CharacterController>();
       meeleFighter = GetComponent<MeeleFighter>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (meeleFighter.InAction == true)
        {
            animator.SetFloat("moveAmount", 0);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = new Vector3(h, 0, v).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;

        GroundCheck();
    
        if (isGrounded)
        {
            ySpeed = -0.5f;
        } 
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
    }

    public void GroundCheck ()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,1,0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}*/

using UnityEngine;
using UnityEngine.InputSystem; // Importar el nuevo sistema de Input

public class PlayerControllerE : MonoBehaviour
{
    [Header("Move settings")]
    [SerializeField] float moveSpeed = 5f; // Velocidad de movimiento
    [SerializeField] float rotationSpeed = 500f; // Velocidad de rotación del personaje

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f; // Radio de la esfera que detecta el suelo
    [SerializeField] Vector3 groundCheckOffset; // Offset desde el centro del jugador para la esfera
    [SerializeField] LayerMask groundLayer; // Capa que representa el suelo

    bool isGrounded; // Si el personaje está tocando el suelo
    float ySpeed; // Velocidad vertical (afectada por la gravedad)

    Quaternion targetRotation; // Rotación deseada hacia la que debe girar el personaje

    // Referencias a componentes
    CameraControllerE cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;

    // Input del jugador
    InputActions inputActions; // Referencia al mapa de input generado
    Vector2 movementInput; // Almacena el input del joystick o teclado (Vector2)

    private void Awake()
    {
        // Obtener referencias a componentes del objeto o la escena
        cameraController = Camera.main.GetComponent<CameraControllerE>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();

        // Inicializar input actions
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        // Activar y suscribirse a los eventos de input
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos y desactivar input
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Disable();
    }

    // Evento cuando se recibe input de movimiento
    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>(); // Leer input (Vector2)
    }

    private void Update()
    {
        // Si el jugador está atacando, no se mueve
        if (meeleFighter.InAction)
        {
            animator.SetFloat("forwardSpeed", 0);
            return;
        }

        // Extraer valores del input (Horizontal y Vertical)
        float h = movementInput.x;
        float v = movementInput.y;

        // Calcular la cantidad de movimiento, limitándola a 1
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        // Crear vector de movimiento en el plano XZ
        var moveInput = new Vector3(h, 0, v).normalized;

        // Ajustar dirección según la cámara
        var moveDir = cameraController.PlanarRotation * moveInput;

        // Comprobar si está en el suelo
        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f; // Mantener al personaje pegado al suelo
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime; // Aplicar gravedad si está en el aire
        }

        // Calcular velocidad total (horizontal y vertical)
        var velocity = moveDir * moveSpeed * moveAmount;
        velocity.y = ySpeed;

        // Mover al personaje
        characterController.Move(velocity * Time.deltaTime);

        // Si hay movimiento, cambiar la rotación hacia la dirección del movimiento
        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        // Rotación suave hacia la dirección deseada
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Enviar valor a la animación
        animator.SetFloat("forwardSpeed", moveAmount, 0.2f, Time.deltaTime);
    }

    // Comprobar si el jugador está tocando el suelo usando una esfera
    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
    }

    // Dibuja una esfera en el editor para visualizar la detección del suelo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}

