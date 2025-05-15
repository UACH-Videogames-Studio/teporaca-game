using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Estado actual del ataque: reposo, preparación, impacto o recuperación
public enum AttackStates { Idle, Windup, Impact, Cooldown }

public class MeeleFighter : MonoBehaviour
{
    [Header("Stats")]
    [field: SerializeField] public float Health { get; private set; } = 100f; // Salud inicial del personaje
    [SerializeField] private float maxHealth = 100f; // Salud máxima del personaje
    [SerializeField] public float MaxStamina { get; private set; } = 100f; // Estamina máxima del personaje
    [SerializeField] public float CurrentStamina { get; private set; } // Estamina actual del personaje
    [SerializeField] private float staminaRegenRate = 15f; // Estamina regenerada por segundo
    [SerializeField] private float staminaRegenDelay = 1.5f; // Tiempo en segundos antes de que la estamina comience a regenerarse después de usarla
    [SerializeField] private float attackStaminaCost = 10f; // Costo de estamina por ataque

    [Header("Combat Settings")]
    [SerializeField] List<AttackData> attacks; // Lista de ataques posibles (definidos como ScriptableObjects)
    [SerializeField] List<AttackData> longRangeAttacks; // Lista de ataques a distancia (definidos como ScriptableObjects)
    [SerializeField] float longRangeAttackThreshold = 1.5f; // Distancia mínima para considerar un ataque a distancia
    [SerializeField] GameObject weapon; // Referencia al arma del personaje (si tiene una)
    [SerializeField] float rotationSpeed = 500f; // Velocidad de rotación del personaje al atacar

    public bool IsTakingHit { get; private set; } = false; // Indica si el personaje está siendo golpeado

    public event Action<MeeleFighter> OnGotHit; // Evento que se dispara cuando el personaje recibe un golpe
    public event Action OnHitComplete; // Evento que se dispara cuando el ataque impacta
    public event Action<float, float> OnStaminaChanged; // Evento para actualizar la UI de estamina (current, max)
    public event Action<float, float> OnHealthChanged; // Evento para actualizar la UI de vida (current, max)


    // Colliders
    BoxCollider weaponCollider;
    SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;

    Animator animator;
    CharacterController characterController; // Añadido para desactivarlo al morir

    private float lastStaminaUseTime; // Para el retraso de regeneración de estamina

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>(); // Obtener CharacterController
        CurrentStamina = MaxStamina; // Iniciar con estamina al máximo
        Health = maxHealth; // Iniciar con salud al máximo
    }

    private void Start()
    {
        if (weapon != null)
        {
            weaponCollider = weapon.GetComponent<BoxCollider>();
            if (animator.isHuman) // Asegurarse que es un rig humanoide antes de buscar huesos
            {
                Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                if (leftFoot != null) leftFootCollider = leftFoot.GetComponent<SphereCollider>();

                Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (rightFoot != null) rightFootCollider = rightFoot.GetComponent<SphereCollider>();
                
                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if (leftHand != null) leftHandCollider = leftHand.GetComponent<SphereCollider>();

                Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                if (rightHand != null) rightHandCollider = rightHand.GetComponent<SphereCollider>();
            }
            DisableHitboxes();
        }
        // Invocar eventos iniciales para la UI
        OnHealthChanged?.Invoke(Health, maxHealth);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    private void Update()
    {
        // Regeneración de Estamina
        if (CurrentStamina < MaxStamina && Time.time > lastStaminaUseTime + staminaRegenDelay && Health > 0 && !InAction) // No regenerar si está en acción o muerto
        {
            CurrentStamina += staminaRegenRate * Time.deltaTime;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }

    public AttackStates AttackStates { get; private set; }
    bool doCombo;
    int comboCount = 0;
    public bool InAction { get; private set; } = false;
    public bool InCounter { get; set; } = false;

    // Método para consumir estamina
    public bool ConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            lastStaminaUseTime = Time.time; // Actualizar el tiempo del último uso de estamina
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
            return true;
        }
        return false; // No hay suficiente estamina
    }

    public void TryToAttack(MeeleFighter target = null)
    {
        if (Health <= 0) return; // No atacar si está muerto

        // Verificar si hay suficiente estamina para el ataque
        if (!ConsumeStamina(attackStaminaCost))
        {
            Debug.Log("No hay suficiente estamina para atacar.");
            // Aquí podrías reproducir un sonido o feedback visual de "sin estamina"
            return;
        }

        if (!InAction)
        {
            StartCoroutine(Attack(target));
        }
        else if (AttackStates == AttackStates.Impact || AttackStates == AttackStates.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack(MeeleFighter target = null)
    {
        InAction = true;
        AttackStates = AttackStates.Windup;

        var attack = attacks[comboCount];

        var attackDir = transform.forward;
        Vector3 startPos = transform.position;
        Vector3 targetPos = Vector3.zero;

        if (target != null)
        {
            var vecToTarget = target.transform.position - transform.position;
            attackDir = vecToTarget.normalized;
            float distance = vecToTarget.magnitude - attack.DistanceFromTarget;

            if (distance > longRangeAttackThreshold && longRangeAttacks.Count > 0)
                attack = longRangeAttacks[0];

            if (attack.MoveToTarget)
            {
                if (distance <= attack.MaxMoveDistance)
                    targetPos = target.transform.position - attackDir * attack.DistanceFromTarget;
                else
                    targetPos = startPos + attackDir * attack.MaxMoveDistance;
            }
        }

        animator.CrossFade(attack.AnimName, 0.2f, 1); // Usar capa 1 para ataques
        yield return null;

        // Esperar a que la animación de ataque comience realmente en la capa 1
        // Esto es importante si tienes una transición con tiempo de salida en el Animator
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(1).IsName(attack.AnimName));
        var animState = animator.GetCurrentAnimatorStateInfo(1); // Ahora obtener el estado actual

        float timer = 0f;

        while (timer <= animState.length)
        {
            if (IsTakingHit || Health <= 0) // Interrumpir si es golpeado o muere
            {
                InAction = false; // Asegurar que InAction se resetee
                AttackStates = AttackStates.Idle;
                DisableHitboxes();
                yield break;
            }
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (target != null && attack.MoveToTarget)
            {
                // Asegurarse que MoveStartTime no sea igual o mayor que MoveEndTime para evitar división por cero
                if (attack.MoveEndTime > attack.MoveStartTime) {
                    float percTime = Mathf.Clamp01((normalizedTime - attack.MoveStartTime) / (attack.MoveEndTime - attack.MoveStartTime));
                    transform.position = Vector3.Lerp(startPos, targetPos, percTime);
                }
            }

            if (attackDir != Vector3.zero) // Evitar LookRotation con vector cero
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(attackDir), rotationSpeed * Time.deltaTime);
            }

            if (AttackStates == AttackStates.Windup)
            {
                if (InCounter) break;
                if (normalizedTime >= attack.ImpactStartTime)
                {
                    AttackStates = AttackStates.Impact;
                    EnableHitBox(attack);
                }
            }
            else if (AttackStates == AttackStates.Impact)
            {
                if (normalizedTime >= attack.ImpactEndTime)
                {
                    AttackStates = AttackStates.Cooldown;
                    DisableHitboxes();
                }
            }
            else if (AttackStates == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    comboCount = (comboCount + 1) % attacks.Count;
                    
                    // Verificar estamina para el siguiente ataque del combo
                    if (ConsumeStamina(attackStaminaCost))
                    {
                        StartCoroutine(Attack(target)); // Pasar el target al siguiente ataque del combo
                    }
                    else
                    {
                        Debug.Log("No hay suficiente estamina para el combo.");
                        // Si no hay estamina para el combo, terminar la acción
                        AttackStates = AttackStates.Idle;
                        comboCount = 0;
                        InAction = false;
                        yield break; 
                    }
                    yield break;
                }
            }
            yield return null;
        }

        AttackStates = AttackStates.Idle;
        comboCount = 0;
        InAction = false;
        DisableHitboxes(); // Asegurarse de desactivar hitboxes al final
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Health <= 0) return; // No procesar si ya está muerto

        if (other.CompareTag("Hitbox") && !IsTakingHit && !InCounter) // Usar CompareTag para eficiencia
        {
            var attacker = other.GetComponentInParent<MeeleFighter>();
            if (attacker == this) return; // No golpearse a sí mismo

            TakeDamage(5f, attacker); // Pasar el atacante
            OnGotHit?.Invoke(attacker);

            if (Health > 0)
                StartCoroutine(PlayHitReaction(attacker));
            else
                PlayDeathAnimation(attacker);
        }
    }

    public void TakeDamage(float damage, MeeleFighter attacker = null) // Añadir parámetro attacker
    {
        if (Health <= 0) return; // Ya está muerto

        Health = Mathf.Clamp(Health - damage, 0, maxHealth);
        OnHealthChanged?.Invoke(Health, maxHealth);
        Debug.Log(gameObject.name + " recibió daño. Salud restante: " + Health);

        if (Health <= 0)
        {
            PlayDeathAnimation(attacker);
        }
    }

    IEnumerator PlayHitReaction(MeeleFighter attacker)
    {
        if (Health <= 0) yield break; // No reaccionar si muere

        InAction = true;
        IsTakingHit = true;

        if (attacker != null)
        {
            var dispVec = attacker.transform.position - transform.position;
            dispVec.y = 0;
            if (dispVec != Vector3.zero) // Evitar LookRotation con vector cero
            {
                transform.rotation = Quaternion.LookRotation(dispVec);
            }
        }
        
        animator.CrossFade("Impact", 0.2f, 1); // Usar capa 1 para reacciones
        yield return null;

        // Esperar a que la animación de impacto comience realmente en la capa 1
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(1).IsName("Impact"));
        var animState = animator.GetCurrentAnimatorStateInfo(1);

        yield return new WaitForSeconds(animState.length * 0.8f);

        OnHitComplete?.Invoke();

        InAction = false;
        IsTakingHit = false;
    }

    void PlayDeathAnimation(MeeleFighter attacker)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("DeathBackward01")) return; // Evitar reproducir múltiples veces

        Debug.Log(gameObject.name + " ha muerto.");
        InAction = true; // Evitar otras acciones
        IsTakingHit = false; // Ya no está "tomando un golpe", está muerto

        // Desactivar CharacterController para que no interfiera con la animación de muerte (ragdoll si lo hubiera)
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Detener cualquier movimiento del NavMeshAgent si es un enemigo
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Orientarse hacia el atacante si existe
        if (attacker != null)
        {
            var dispVec = attacker.transform.position - transform.position;
            dispVec.y = 0;
            if (dispVec != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dispVec);
            }
        }

        animator.CrossFade("DeathBackward01", 0.2f, 0); // Usar capa base (0) para muerte
        // Aquí podrías añadir lógica para desactivar el GameObject después de un tiempo,
        // o notificar a un GameManager, etc.
    }

    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        if (Health <= 0 || opponent == null || opponent.Fighter == null || opponent.Fighter.Health <= 0) yield break; // Chequeos de seguridad

        InAction = true;
        InCounter = true;
        opponent.Fighter.InCounter = true; // El oponente también está en un contraataque (recibiéndolo)
        
        // No cambiar el estado del oponente a Dead aquí, la animación de víctima lo manejará o se hará después.
        // opponent.ChangeState(EnemyStates.Dead); // Esto podría ser prematuro

        var dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0;
        if (dispVec != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dispVec);
            opponent.transform.rotation = Quaternion.LookRotation(-dispVec);
        }

        var targetPos = opponent.transform.position - dispVec.normalized * 1f;

        animator.CrossFade("CounterAttackA", 0.2f, 1);
        opponent.Animator.CrossFade("CounterAttackVictimA", 0.2f, 1); // Asumiendo capa 1 para animaciones de combate
        yield return null;

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(1).IsName("CounterAttackA"));
        var animState = animator.GetCurrentAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            // Mover al personaje hacia la posición del oponente durante el contraataque
            // Solo si el character controller está activo
            if (characterController != null && characterController.enabled)
            {
                 transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime);
            }
            yield return null;
            timer += Time.deltaTime;
        }
        
        // El daño y la muerte del oponente deberían manejarse como resultado del contraataque,
        // por ejemplo, activando un hitbox especial o llamando a TakeDamage en el oponente.
        // Por ahora, asumimos que la animación "CounterAttackVictimA" implica la derrota.
        // Si no, aquí deberías aplicar el daño:
        opponent.Fighter.TakeDamage(100f, this); // Ejemplo de daño masivo por contraataque

        InCounter = false;
        opponent.Fighter.InCounter = false;
        InAction = false;
    }

    void EnableHitBox(AttackData attack)
    {
        // Verificar que los colliders no sean null antes de usarlos
        switch (attack.HitboxToUse)
        {
            case AttackHitbox.LeftHand:
                if (leftHandCollider != null) leftHandCollider.enabled = true;
                break;
            case AttackHitbox.RightHand:
                if (rightHandCollider != null) rightHandCollider.enabled = true;
                break;
            case AttackHitbox.LeftFoot:
                if (leftFootCollider != null) leftFootCollider.enabled = true;
                break;
            case AttackHitbox.RightFoot:
                if (rightFootCollider != null) rightFootCollider.enabled = true;
                break;
            case AttackHitbox.Axe:
            case AttackHitbox.Sword: // Agrupado ya que ambos usan weaponCollider
                if (weaponCollider != null) weaponCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    void DisableHitboxes()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
        if (leftFootCollider != null) leftFootCollider.enabled = false;
        if (rightFootCollider != null) rightFootCollider.enabled = false;
        if (leftHandCollider != null) leftHandCollider.enabled = false;
        if (rightHandCollider != null) rightHandCollider.enabled = false;
    }

    public List<AttackData> Attacks => attacks;
    public bool IsCounterable => AttackStates == AttackStates.Windup && comboCount == 0 && Health > 0; // Solo contraatacable si está vivo
}
