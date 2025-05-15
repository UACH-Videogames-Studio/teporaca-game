using UnityEngine;
using UnityEngine.InputSystem; // Importar el nuevo sistema de Input

public class PlayerControllerE : MonoBehaviour
{
    [Header("Move settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeedMultiplier = 1.8f; // Multiplicador para la velocidad de carrera
    [SerializeField] float rotationSpeed = 500f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 8f; // Fuerza del salto
    [SerializeField] float jumpStaminaCost = 15f; // Costo de estamina para saltar

    [Header("Dodge Settings")]
    [SerializeField] float dodgeSpeed = 8f; // Velocidad/distancia de la evasión
    [SerializeField] float dodgeDuration = 0.5f; // Duración de la evasión
    [SerializeField] float dodgeStaminaCost = 20f; // Costo de estamina para evadir
    [SerializeField] float dodgeCooldown = 0.2f; // Pequeño cooldown para no encadenar evasiones instantáneamente

    [Header("Stamina Costs")]
    [SerializeField] float runStaminaCostPerSecond = 10f; // Costo de estamina por segundo al correr

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    float ySpeed;
    Quaternion targetRotation;

    public Vector3 InputDirection { get; private set; }

    // Estados del jugador
    private bool isDodging = false;
    private bool isRunning = false;
    private bool isJumping = false; // Para evitar doble salto si no se resetea bien isGrounded

    private float currentDodgeCooldown = 0f;

    // Referencias a componentes
    CameraControllerE cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;
    CombatController combatController;
    public static PlayerControllerE Instance { get; private set; }

    // Input del jugador
    InputActions inputActions;
    Vector2 movementInput;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraControllerE>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
        combatController = GetComponent<CombatController>();
        Instance = this;
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump; // Suscribirse al evento de salto
        inputActions.Player.Dodge.performed += OnDodge; // Suscribirse al evento de evasión
        inputActions.Player.Run.performed += OnRunPerformed; // Para cuando se presiona correr
        inputActions.Player.Run.canceled += OnRunCanceled;   // Para cuando se suelta correr
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Dodge.performed -= OnDodge;
        inputActions.Player.Run.performed -= OnRunPerformed;
        inputActions.Player.Run.canceled -= OnRunCanceled;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    // ---- MANEJADORES DE INPUT ----
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && !meeleFighter.InAction && !isDodging && HealthCheck())
        {
            if (meeleFighter.ConsumeStamina(jumpStaminaCost))
            {
                isJumping = true; // Marcar que está en proceso de salto
                ySpeed = jumpForce;
                animator.SetTrigger("Jump"); // Asume que tienes un trigger "Jump" en el Animator
                // Considera desactivar isGrounded temporalmente aquí si es necesario para tu lógica de GroundCheck
            }
            else
            {
                Debug.Log("No hay suficiente estamina para saltar.");
                // Feedback visual/sonoro de falta de estamina
            }
        }
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && !meeleFighter.InAction && !isDodging && currentDodgeCooldown <= 0 && HealthCheck())
        {
            if (meeleFighter.ConsumeStamina(dodgeStaminaCost))
            {
                StartCoroutine(PerformDodge());
            }
            else
            {
                Debug.Log("No hay suficiente estamina para evadir.");
                // Feedback visual/sonoro de falta de estamina
            }
        }
    }
    
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (HealthCheck() && !meeleFighter.InAction && !isDodging)
        {
            isRunning = true;
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        isRunning = false;
        animator.SetBool("IsRunning", false); // Asegurar que la animación de correr se detenga
    }

    // ---- LÓGICA DE ACCIONES ----
    private System.Collections.IEnumerator PerformDodge()
    {
        isDodging = true;
        currentDodgeCooldown = dodgeCooldown; // Iniciar cooldown
        // meeleFighter.SetInvulnerable(true); // Opcional: si quieres invencibilidad

        animator.SetTrigger("Dodge"); // Asume que tienes un trigger "Dodge"

        Vector3 dodgeDirection = GetIntentDirection();
        if (movementInput.sqrMagnitude < 0.01f) // Si no hay input de movimiento, esquiva hacia atrás
        {
            dodgeDirection = -transform.forward;
        }


        float startTime = Time.time;
        animator.applyRootMotion = true; // Permitir que la animación controle el movimiento

        // Esperar un poco para que la animación de root motion comience a aplicar deltaPosition
        yield return new WaitForSeconds(0.05f); // Ajusta este valor si es necesario

        while (Time.time < startTime + dodgeDuration)
        {
            if (meeleFighter.Health <= 0) // Interrumpir si muere
            {
                isDodging = false;
                animator.applyRootMotion = false;
                // meeleFighter.SetInvulnerable(false);
                yield break;
            }
            // El movimiento es manejado por root motion de la animación "Dodge"
            // Si tu animación de Dodge no tiene root motion, deberás mover el CharacterController manualmente:
            // characterController.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            yield return null;
        }
        
        animator.applyRootMotion = false; // Devolver el control del movimiento al script
        isDodging = false;
        // meeleFighter.SetInvulnerable(false);
    }


    private void Update()
    {
        // Actualizar cooldown de evasión
        if (currentDodgeCooldown > 0)
        {
            currentDodgeCooldown -= Time.deltaTime;
        }

        // Si el jugador está muerto, en una acción (ataque, golpeado, esquivando) o saltando, no procesar movimiento normal.
        if (!HealthCheck() || meeleFighter.InAction || isDodging)
        {
            // Si está muerto, asegurarse que no haya velocidad de movimiento en el animador
            if (!HealthCheck())
            {
                animator.SetFloat("forwardSpeed", 0);
                animator.SetFloat("strafeSpeed", 0);
                animator.SetBool("IsRunning", false);
            }
            // Si está esquivando, el movimiento es manejado por PerformDodge y root motion
            // Si está en otra acción (InAction), el MeeleFighter o CombatController maneja la lógica.
            // Aplicar gravedad si no está esquivando (ya que el esquive podría ser en el aire)
            if (!isDodging) ApplyGravity();
            return;
        }
        
        HandleMovement();
        ApplyGravity(); // Aplicar gravedad después de calcular el movimiento horizontal
    }

    private void HandleMovement()
    {
        float h = movementInput.x;
        float v = movementInput.y;
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));
        var moveInput = new Vector3(h, 0, v).normalized;
        var moveDir = cameraController.PlanarRotation * moveInput;
        InputDirection = moveDir;

        float currentSpeed = moveSpeed;

        // Lógica de Correr
        if (isRunning && moveAmount > 0.1f && isGrounded) // Solo correr si hay input y está en el suelo
        {
            if (meeleFighter.ConsumeStamina(runStaminaCostPerSecond * Time.deltaTime))
            {
                currentSpeed *= runSpeedMultiplier;
                animator.SetBool("IsRunning", true);
            }
            else
            {
                isRunning = false; // No hay estamina para correr
                animator.SetBool("IsRunning", false);
                // Podrías añadir un sonido o efecto de "sin estamina para correr"
            }
        }
        else
        {
            isRunning = false; // Detener la carrera si no hay input, no está en el suelo o se suelta el botón
            animator.SetBool("IsRunning", false);
        }


        var velocity = moveDir * currentSpeed * moveAmount;

        if (combatController.CombatMode && combatController.TargetEnemy != null)
        {
            // En modo combate, la velocidad se reduce (ya no se divide por 4, se controla con moveSpeed y runSpeedMultiplier)
            // Rotar y estar de frente al enemigo
            var targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;

            if (moveAmount > 0.01f || combatController.TargetEnemy != null) // Rotar si hay input o si hay un objetivo fijado
            {
                targetRotation = Quaternion.LookRotation(targetVec.normalized);
            }
            // No rotar si no hay input y no hay objetivo (aunque en CombatMode debería haber TargetEnemy)

            // Calcular forwardSpeed y strafeSpeed relativo a la orientación del personaje
            Vector3 localVel = transform.InverseTransformDirection(velocity);
            animator.SetFloat("forwardSpeed", localVel.z / (isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed), 0.1f, Time.deltaTime);
            animator.SetFloat("strafeSpeed", localVel.x / (isRunning ? moveSpeed * runSpeedMultiplier : moveSpeed), 0.1f, Time.deltaTime);
        }
        else // Modo No Combate (Exploración)
        {
            if (moveAmount > 0.01f)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }
            animator.SetFloat("forwardSpeed", moveAmount * (isRunning ? runSpeedMultiplier : 1f), 0.1f, Time.deltaTime); // forwardSpeed ahora refleja la carrera
            animator.SetFloat("strafeSpeed", 0, 0.1f, Time.deltaTime); // No hay strafe en modo exploración
        }
        
        // Aplicar rotación
        if (moveAmount > 0.01f || (combatController.CombatMode && combatController.TargetEnemy != null))
        {
             transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplicar movimiento
        // La velocidad vertical (ySpeed) se añade en ApplyGravity()
        characterController.Move(velocity * Time.deltaTime);
    }


    private void ApplyGravity()
    {
        GroundCheck(); // Comprobar si está en el suelo

        if (isGrounded && ySpeed < 0) // Si está en el suelo y la velocidad Y es negativa (cayendo o en reposo)
        {
            ySpeed = -0.5f; // Mantener al personaje pegado al suelo
            isJumping = false; // Resetear el estado de salto
            animator.SetBool("IsGrounded", true);
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime; // Aplicar gravedad si está en el aire
             animator.SetBool("IsGrounded", false);
        }
        
        // Solo aplicar movimiento vertical si el CharacterController está habilitado
        if (characterController.enabled)
        {
            characterController.Move(new Vector3(0, ySpeed, 0) * Time.deltaTime);
        }
    }


    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
        animator.SetBool("IsGrounded", isGrounded);

        // Si aterriza después de un salto/caída
        if (isGrounded && isJumping && ySpeed < 0.1f) {
            isJumping = false;
            // animator.SetTrigger("Landed"); // Opcional: si tienes una animación de aterrizaje
        }
    }
    
    private bool HealthCheck()
    {
        // Comprueba si el meeleFighter existe y tiene salud.
        return meeleFighter != null && meeleFighter.Health > 0;
    }

    public Vector3 GetIntentDirection()
    {
        // Si hay input de movimiento, esa es la intención.
        // Si no, la dirección hacia donde mira el personaje.
        return (movementInput.sqrMagnitude > 0.01f) ? InputDirection : transform.forward;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        if (characterController != null) // Asegurarse que characterController no es null
        {
            Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
        }
    }
}
