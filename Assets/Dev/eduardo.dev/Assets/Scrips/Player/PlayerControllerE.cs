using UnityEngine;
using UnityEngine.InputSystem; // Importar el nuevo sistema de Input

public class PlayerControllerE : MonoBehaviour
{
    [Header("Move settings")] // Configuración de movimiento
    [SerializeField] float moveSpeed = 5f; // Velocidad de movimiento
    [SerializeField] float rotationSpeed = 500f; // Velocidad de rotación del personaje

    [Header("Ground Check Settings")] // Configuración para la detección del suelo
    [SerializeField] float groundCheckRadius = 0.2f; // Radio de la esfera que detecta el suelo
    [SerializeField] Vector3 groundCheckOffset; // Offset desde el centro del jugador para la esfera
    [SerializeField] LayerMask groundLayer; // Capa que representa el suelo

    bool isGrounded; // Si el personaje está tocando el suelo
    float ySpeed; // Velocidad vertical (afectada por la gravedad)

    Quaternion targetRotation; // Rotación deseada hacia la que debe girar el personaje

    public Vector3 InputDirection { get; private set; } // Dirección de movimiento del jugador (Vector3)

    // Referencias a componentes
    CameraControllerE cameraController; // Controlador de cámara
    Animator animator; // Componente Animator del personaje
    CharacterController characterController; // Componente CharacterController del personaje
    MeeleFighter meeleFighter; // Controlador de combate cuerpo a cuerpo
    CombatController combatController; // Referencia al controlador de combate
    public static PlayerControllerE Instance { get; private set; } // Instancia estática del controlador de jugador (Singleton)

    // Input del jugador
    InputActions inputActions; // Referencia al mapa de input generado
    Vector2 movementInput; // Almacena el input del joystick o teclado (Vector2)

    private void Awake()
    {
        // Obtener referencias a componentes del objeto o la escena
        cameraController = Camera.main.GetComponent<CameraControllerE>(); // Obtener el controlador de cámara
        animator = GetComponent<Animator>(); // Obtener el Animator del personaje
        characterController = GetComponent<CharacterController>();  // Obtener el CharacterController del personaje
        meeleFighter = GetComponent<MeeleFighter>(); // Obtener el controlador de combate cuerpo a cuerpo
        combatController = GetComponent<CombatController>(); // Obtener el controlador de combate

        Instance = this; // Asignar la instancia estática a este objeto

        // Inicializar input actions
        inputActions = new InputActions(); // Crear una nueva instancia de InputActions
    }

    private void OnEnable()
    {
        // Activar y suscribirse a los eventos de input
        inputActions.Player.Enable(); // Activar el mapa de input del jugador
        inputActions.Player.Move.performed += OnMove; // Suscribirse al evento de movimiento
        inputActions.Player.Move.canceled += OnMove; // Suscribirse al evento de movimiento cancelado
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos y desactivar input
        inputActions.Player.Move.performed -= OnMove; // Desuscribirse del evento de movimiento
        inputActions.Player.Move.canceled -= OnMove; // Desuscribirse del evento de movimiento cancelado
        inputActions.Player.Disable(); // Desactivar el mapa de input del jugador
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
            targetRotation = transform.rotation; // Mantener la rotación actual
            animator.SetFloat("forwardSpeed", 0); // No enviar velocidad a la animación
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

        InputDirection = moveDir; // Almacenar la dirección de input

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

        // Calcular velocidad total
        var velocity = moveDir * moveSpeed * moveAmount;

        if (combatController.CombatMode)
        {
            velocity /= 4; // Reducir velocidad al entrar en modo combate
            
            // Rotar y estar de frente al enemigo
            var targetVec = combatController.TargetEnemy.transform.position - transform.position; // Vector hacia el enemigo
            targetVec.y = 0; // Ignorar la altura

            // Si hay movimiento, cambiar la rotación hacia la dirección del movimiento
            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec); // Rotación hacia el enemigo
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // Rotar suavemente
            }

            //Dividir la velocidad en sus componentes y asiganarlas a forwardSpeed y strafeSpeed
            float forwardSpeed = Vector3.Dot(velocity, transform.forward); // Velocidad hacia adelante
            animator.SetFloat("forwardSpeed", forwardSpeed / moveSpeed, 0.2f, Time.deltaTime); // Enviar valor a la animación

            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up); // Calcular el ángulo entre la dirección del personaje y la dirección de movimiento
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad); // Calcular la velocidad lateral
            animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime); // Enviar valor a la animación
        }
        else
        {
            // Si hay movimiento, cambiar la rotación hacia la dirección del movimiento
            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(moveDir); // Rotación hacia la dirección de movimiento
            }

            // Rotación suave hacia la dirección deseada
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Enviar valor a la animación
            animator.SetFloat("forwardSpeed", moveAmount, 0.2f, Time.deltaTime);
        }

        velocity.y = ySpeed; // Asignar la velocidad vertical a la velocidad total

        // Mover al personaje
        characterController.Move(velocity * Time.deltaTime);

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

