using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Estado actual del ataque: reposo, preparación, impacto o recuperación
public enum AttackStates { Idle, Windup, Impact, Cooldown }

public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks; // Lista de ataques posibles (definidos como ScriptableObjects)
    [SerializeField] List<AttackData> longRangeAttacks; // Lista de ataques a distancia (definidos como ScriptableObjects)
    [SerializeField] float longRangeAttackThreshold = 1.5f; // Distancia mínima para considerar un ataque a distancia

    [SerializeField] GameObject weapon; // Referencia al arma del personaje (si tiene una)
    [SerializeField] float rotationSpeed = 500f; // Velocidad de rotación del personaje al atacar

    public event Action OnGotHit; // Evento que se dispara cuando el personaje recibe un golpe (para efectos visuales o de sonido)
    public event Action OnHitComplete; // Evento que se dispara cuando el ataque impacta (para efectos visuales o de sonido)

    // Colliders que se usarán como hitboxes para detectar impactos en distintas partes del cuerpo
    BoxCollider weaponCollider; // Collider del arma (hacha o espada)
    SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider; // Colliders de las manos y pies

    Animator animator; // Referencia al Animator del personaje para controlar animaciones

    private void Awake()
    {
        // Obtenemos el Animator del personaje
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Si el personaje tiene un hacha asignada
        if (weapon != null)
        {
            // Se obtienen los colliders correspondientes desde el Animator
            weaponCollider = weapon.GetComponent<BoxCollider>(); // Collider del arma

            // Se accede a los huesos humanos del rig para obtener las manos y pies, y sus colliders
            leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
            rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
            leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
            rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();

            // Se desactivan todos los colliders al iniciar (por seguridad)
            DisableHitboxes();
        }
    } 

    // Estado actual del ataque (Idle, Windup, Impact o Cooldown)
    public AttackStates AttackStates { get; private set; }

    bool doCombo; // Indica si el jugador presionó para hacer un combo
    int comboCount = 0; // Contador del combo actual

    public bool InAction { get; private set; } = false; // Indica si el personaje está actualmente atacando o siendo golpeado
    public bool InCounter { get; set; } = false; // Indica si el personaje está en medio de un contraataque

    // Método llamado por el sistema de input cuando se presiona el botón de ataque
    public void TryToAttack(MeeleFighter target = null)
    {
        // Si no está haciendo otra acción, comienza el ataque
        if (!InAction)
        {
            StartCoroutine(Attack(target)); // Inicia la corrutina de ataque
        }
        // Si ya está atacando pero en la fase correcta (impacto o recuperación), se activa el combo
        else if (AttackStates == AttackStates.Impact || AttackStates == AttackStates.Cooldown)
        {
            doCombo = true; // Permite hacer un combo
        }
    }

    // Corrutina que maneja todo el proceso del ataque: animación, tiempos y combos
    IEnumerator Attack(MeeleFighter target = null)
    {
        // Si el personaje está en medio de un ataque, no puede iniciar otro
        InAction = true; // El personaje está ocupado
        AttackStates = AttackStates.Windup; // Empieza en fase de preparación

        var attack = attacks[comboCount]; // Obtiene el ataque actual según el contador de combo

        var attackDir = transform.forward; // Dirección del ataque (por defecto hacia adelante)
        Vector3 startPos = transform.position; // Posición inicial del personaje
        Vector3 targetPos = Vector3.zero; // Posición del objetivo (inicialmente vacía)

        if (target != null)
        {
            // Si hay un objetivo, se calcula la dirección del ataque hacia él
            var vecToTarget = target.transform.position - transform.position; // Vector de desplazamiento hacia el objetivo
            //vecToTarget.y = 0; // Mantiene la dirección horizontal

            attackDir = vecToTarget.normalized; // Normaliza el vector para obtener la dirección
            float distance = vecToTarget.magnitude - attack.DistanceFromTarget; // Calcula la distancia al objetivo menos la distancia de ataque

            if (distance > longRangeAttackThreshold) // Si la distancia es mayor que el umbral
                attack = longRangeAttacks[0]; // Cambia al ataque a distancia

            if (attack.MoveToTarget) 
            {
                if (distance <= attack.MaxMoveDistance) // Si la distancia es menor o igual a la distancia máxima de movimiento
                    targetPos = target.transform.position - attackDir * attack.DistanceFromTarget; // Calcula la posición de destino
                else
                    targetPos = startPos + attackDir * attack.MaxMoveDistance; // Si no, se usa la distancia máxima de movimiento             
            }
            
        }
        
        // Se inicia la animación del ataque actual según el combo
        animator.CrossFade(attack.AnimName, 0.2f);
        yield return null; // Espera un frame

        // Se obtiene el estado siguiente del Animator en la capa 1
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f; // Temporizador para controlar el tiempo de la animación

        // Se ejecuta mientras la animación esté activa
        while (timer <= animState.length)
        {
            timer += Time.deltaTime; // Incrementa el temporizador
            float normalizedTime = timer / animState.length; // Normaliza el tiempo de la animación (0 a 1)

            // Mover al atacante hacia el objetivo mientras ataca

            if (target != null && attack.MoveToTarget) 
            {
                float percTime = (normalizedTime - attack.MoveStartTime) / (attack.MoveEndTime - attack.MoveStartTime); // Calcula el porcentaje de movimiento
                transform.position = Vector3.Lerp(startPos, targetPos, percTime); // Interpola la posición entre el inicio y el objetivo
            }

            //Rotar a la dirección del ataque
            if (attackDir != null)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(attackDir), rotationSpeed * Time.deltaTime); // Rotar hacia la dirección del ataque
            }

            // Transición de preparación a impacto
            if (AttackStates == AttackStates.Windup)
            {
                if (InCounter) break; // Si está en contraataque, no se activa el impacto
                if (normalizedTime >= attack.ImpactStartTime) // Si el tiempo normalizado supera el inicio del impacto
                {
                    AttackStates = AttackStates.Impact; // Cambia el estado a impacto
                    EnableHitBox(attack); // Activar hitbox correspondiente
                }
            }
            // Transición de impacto a recuperación
            else if (AttackStates == AttackStates.Impact)
            {
                if (normalizedTime >= attack.ImpactEndTime) 
                {
                    AttackStates = AttackStates.Cooldown; // Cambia el estado a recuperación
                    DisableHitboxes(); // Desactiva hitboxes después del golpe
                }
            }
            // Durante la recuperación, verifica si hay combo
            else if (AttackStates == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false; // Reinicia el combo
                    comboCount = (comboCount + 1) % attacks.Count; // Avanza al siguiente ataque en la lista (ciclo)

                    StartCoroutine(Attack()); // Inicia el siguiente ataque del combo
                    yield break; // Termina la corrutina actual
                }
            }

            yield return null;
        }

        // Cuando termina la animación del ataque
        AttackStates = AttackStates.Idle; // Cambia el estado a reposo
        comboCount = 0; // Reinicia combo
        InAction = false; // Personaje libre para otras acciones
    }

    // Detecta si este personaje es golpeado por otro (usando colliders con tag "Hitbox")
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hitbox" && !InAction)
        {
            StartCoroutine(PlayHitReaction(other.GetComponentInParent<MeeleFighter>().transform)); // Ejecuta animación de impacto
        }
    }

    // Reproduce una animación cuando el personaje recibe un golpe
    IEnumerator PlayHitReaction(Transform attacker)
    {
        InAction = true; // El personaje está ocupado

        var dispVec = attacker.position - transform.position; // Calcula la dirección del golpe
        dispVec.y = 0; // Mantiene la dirección horizontal
        transform.rotation = Quaternion.LookRotation(dispVec); // Orienta el personaje hacia el atacante

        OnGotHit?.Invoke(); // Dispara el evento de golpe recibido (para efectos visuales o de sonido)

        animator.CrossFade("Impact", 0.2f); // Reproduce la animación de impacto
        yield return null; // Espera un frame

        var animState = animator.GetNextAnimatorStateInfo(1); // Obtiene el estado de la animación actual

        yield return new WaitForSeconds(animState.length * 0.8f); // Espera hasta casi terminar la animación

        OnHitComplete?.Invoke(); // Dispara el evento de golpe completado (para efectos visuales o de sonido)

        InAction = false; // El personaje puede volver a actuar
    }

    // Reproduce una animación cuando el personaje recibe un golpe
    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        InAction = true; // El personaje está ocupado

        InCounter = true; // Indica que el personaje está en medio de un contraataque
        opponent.Fighter.InCounter = true; // Indica que el oponente está en medio de un contraataque
        opponent.ChangeState(EnemyStates.Dead); // Cambia el estado del oponente a muerto

        var dispVec = opponent.transform.position - transform.position; // Calcula la dirección del golpe
        dispVec.y = 0; // Mantiene la dirección horizontal
        transform.rotation = Quaternion.LookRotation(dispVec); // Orienta el personaje hacia el oponente
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec); // Orienta el oponente hacia el personaje

        var targetPos = opponent.transform.position - dispVec.normalized * 1f; // Calcula la posición de destino para el personaje

        animator.CrossFade("CounterAttackA", 0.2f); // Reproduce la animación de impacto
        opponent.Animator.CrossFade("CounterAttackVictimA", 0.2f); // Reproduce la animación de impacto del oponente
        yield return null; // Espera un frame

        var animState = animator.GetNextAnimatorStateInfo(1); // Obtiene el estado de la animación actual

        float timer = 0f; // Temporizador para controlar el tiempo de la animación
        while (timer <= animState.length) // Se ejecuta mientras la animación esté activa
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime); // Mueve el personaje hacia la posición del oponente

            yield return null; // Espera un frame
            timer += Time.deltaTime; // Incrementa el temporizador
        }

        InCounter = false; // Indica que el personaje ya no está en medio de un contraataque
        opponent.Fighter.InCounter = false; // Indica que el oponente ya no está en medio de un contraataque

        InAction = false; // El personaje puede volver a actuar
    }

    // Activa el collider correspondiente al ataque actual
    void EnableHitBox(AttackData attack) 
    {
        switch (attack.HitboxToUse)
        {
            case AttackHitbox.LeftHand: 
                leftHandCollider.enabled = true; // Activa el collider de la mano izquierda
                break;
            case AttackHitbox.RightHand:
                rightHandCollider.enabled = true; // Activa el collider de la mano derecha
                break;
            case AttackHitbox.LeftFoot:
                leftFootCollider.enabled = true; // Activa el collider del pie izquierdo
                break;
            case AttackHitbox.RightFoot:
                rightFootCollider.enabled = true; // Activa el collider del pie derecho
                break;
            case AttackHitbox.Axe:
                weaponCollider.enabled = true; // Activa el collider del hacha
                break;
            case AttackHitbox.Sword:
                weaponCollider.enabled = true; // Activa el collider de la espada
                break;
            default:
                break;
        }
    }

    // Desactiva todos los colliders de ataque
    void DisableHitboxes()
    {
        weaponCollider.enabled = false; // Desactiva el collider del arma

        if (leftFootCollider != null)
            leftFootCollider.enabled = false; // Desactiva el collider del pie izquierdo
        if (rightFootCollider != null)
            rightFootCollider.enabled = false; // Desactiva el collider del pie derecho
        if (leftHandCollider != null)
            leftHandCollider.enabled = false;  // Desactiva el collider de la mano izquierda
        if (rightHandCollider != null)
            rightHandCollider.enabled = false; // Desactiva el collider de la mano derecha
    }

    public List<AttackData> Attacks => attacks; // Propiedad para acceder

    public bool IsCounterable  => AttackStates == AttackStates.Windup && comboCount == 0; // Indica si el ataque puede ser contrarrestado (en fase de preparación y sin combo activo)
}
