// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework.Internal.Execution;
// using UnityEngine;

// public enum AttackStates { Idle, Windup, Impact, Cooldown }

// public class MeeleFighter : MonoBehaviour
// {
//     [SerializeField] List<AttackData> attacks;
//     [SerializeField] GameObject axe;

//     BoxCollider axeCollider;
//     SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;

//     Animator animator;

//     private void Awake()
//     {
//         animator = GetComponent<Animator>();
//     }

//     private void Start()
//     {
//         if (axe != null)
//         {
//             axeCollider = axe.GetComponent<BoxCollider>();
//             leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
//             rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
//             leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
//             rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();

//             DisableHitboxes();
//         }
//     } 

//     AttackStates AttackStates;
//     bool doCombo;
//     int comboCount = 0;
//     public bool InAction {get; private set;} = false;
//     public void TryToAttack()
//     {
//         if (!InAction)
//         {
//             StartCoroutine(Attack());
//         }
//         else if (AttackStates == AttackStates.Impact || AttackStates == AttackStates.Cooldown)
//         {
//             doCombo = true;
//         }
//     }

//     IEnumerator Attack()
//     {
//         InAction = true;
//         AttackStates = AttackStates.Windup;

//         animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
//         yield return null;

//         var animState = animator.GetNextAnimatorStateInfo(1);

//         float timer = 0f;
//         while (timer <= animState.length)
//         {
//             timer += Time.deltaTime;
//             float normalizedTime = timer / animState.length;

//             if (AttackStates == AttackStates.Windup)
//             {
//                 if (normalizedTime >= attacks[comboCount].ImpactStartTime)
//                 {
//                     AttackStates = AttackStates.Impact;
//                     EnableHitBox(attacks[comboCount]);
//                 }
//             }
//             else if (AttackStates == AttackStates.Impact)
//             {
//                 if (normalizedTime >= attacks[comboCount].ImpactEndTime)
//                 {
//                     AttackStates = AttackStates.Cooldown;
//                     DisableHitboxes();
//                 }
//             }
//             else if (AttackStates == AttackStates.Cooldown)
//             {
//                 if (doCombo)
//                 {
//                     doCombo = false;
//                     comboCount = (comboCount + 1) % attacks.Count;

//                     StartCoroutine(Attack());
//                     yield break;
//                 }
//             }

//             yield return null;
//         }

//         AttackStates = AttackStates.Idle;
//         comboCount = 0;
//         InAction = false;
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.tag == "Hitbox" && !InAction)
//         {
//             StartCoroutine(PlayHitReaction());
//         }
//     }

//     IEnumerator PlayHitReaction()
//     {
//         InAction = true;
//         animator.CrossFade("Impact", 0.2f);
//         yield return null;

//         var animState = animator.GetNextAnimatorStateInfo(1);

//         yield return new WaitForSeconds(animState.length * 0.8f);

//         InAction = false;
//     }

//     void EnableHitBox (AttackData attack)
//     {
//         switch (attack.HitboxToUse)
//         {
//             case AttackHitbox.LeftHand:
//                 leftHandCollider.enabled = true;
//                 break;
//             case AttackHitbox.RightHand:
//                 rightHandCollider.enabled = true;
//                 break;
//             case AttackHitbox.LeftFoot:
//                 leftFootCollider.enabled = true;
//                 break;
//             case AttackHitbox.RightFoot:
//                 rightFootCollider.enabled = true;
//                 break;
//             case AttackHitbox.Axe:
//                 axeCollider.enabled = true;
//                 break;
//             default:
//                 break;
//         }
//     }

//     void DisableHitboxes()
//     {
//         axeCollider.enabled = false;
//         leftFootCollider.enabled = false;
//         rightFootCollider.enabled = false;
//         leftHandCollider.enabled = false;
//         rightHandCollider.enabled = false;
//     }

// }

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Execution; // Esta línea no se usa aquí y puede eliminarse
using UnityEngine;

// Estado actual del ataque: reposo, preparación, impacto o recuperación
public enum AttackStates { Idle, Windup, Impact, Cooldown }

public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks; // Lista de ataques posibles (definidos como ScriptableObjects)
    [SerializeField] GameObject axe; // Referencia al hacha del personaje (si tiene una)

    // Colliders que se usarán como hitboxes para detectar impactos en distintas partes del cuerpo
    BoxCollider axeCollider;
    SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;

    Animator animator; // Referencia al Animator del personaje para controlar animaciones

    private void Awake()
    {
        // Obtenemos el Animator del personaje
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Si el personaje tiene un hacha asignada
        if (axe != null)
        {
            // Se obtienen los colliders correspondientes a cada parte del cuerpo desde el Animator
            axeCollider = axe.GetComponent<BoxCollider>();

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

    // Método llamado por el sistema de input cuando se presiona el botón de ataque
    public void TryToAttack()
    {
        // Si no está haciendo otra acción, comienza el ataque
        if (!InAction)
        {
            StartCoroutine(Attack());
        }
        // Si ya está atacando pero en la fase correcta (impacto o recuperación), se activa el combo
        else if (AttackStates == AttackStates.Impact || AttackStates == AttackStates.Cooldown)
        {
            doCombo = true;
        }
    }

    // Corrutina que maneja todo el proceso del ataque: animación, tiempos y combos
    IEnumerator Attack()
    {
        InAction = true; // El personaje está ocupado
        AttackStates = AttackStates.Windup; // Empieza en fase de preparación

        // Se inicia la animación del ataque actual según el combo
        animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
        yield return null; // Espera un frame

        // Se obtiene el estado siguiente del Animator en la capa 1
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;

        // Se ejecuta mientras la animación esté activa
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            // Transición de preparación a impacto
            if (AttackStates == AttackStates.Windup)
            {
                if (normalizedTime >= attacks[comboCount].ImpactStartTime)
                {
                    AttackStates = AttackStates.Impact;
                    EnableHitBox(attacks[comboCount]); // Activar hitbox correspondiente
                }
            }
            // Transición de impacto a recuperación
            else if (AttackStates == AttackStates.Impact)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
                {
                    AttackStates = AttackStates.Cooldown;
                    DisableHitboxes(); // Desactiva hitboxes después del golpe
                }
            }
            // Durante la recuperación, verifica si hay combo
            else if (AttackStates == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    comboCount = (comboCount + 1) % attacks.Count; // Avanza al siguiente ataque en la lista (ciclo)

                    StartCoroutine(Attack()); // Inicia el siguiente ataque del combo
                    yield break; // Termina la corrutina actual
                }
            }

            yield return null;
        }

        // Cuando termina la animación del ataque
        AttackStates = AttackStates.Idle;
        comboCount = 0; // Reinicia combo
        InAction = false; // Personaje libre para otras acciones
    }

    // Detecta si este personaje es golpeado por otro (usando colliders con tag "Hitbox")
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hitbox" && !InAction)
        {
            StartCoroutine(PlayHitReaction()); // Ejecuta animación de impacto
        }
    }

    // Reproduce una animación cuando el personaje recibe un golpe
    IEnumerator PlayHitReaction()
    {
        InAction = true;
        animator.CrossFade("Impact", 0.2f); // Reproduce la animación de impacto
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        yield return new WaitForSeconds(animState.length * 0.8f); // Espera hasta casi terminar la animación

        InAction = false; // El personaje puede volver a actuar
    }

    // Activa el collider correspondiente al ataque actual
    void EnableHitBox(AttackData attack)
    {
        switch (attack.HitboxToUse)
        {
            case AttackHitbox.LeftHand:
                leftHandCollider.enabled = true;
                break;
            case AttackHitbox.RightHand:
                rightHandCollider.enabled = true;
                break;
            case AttackHitbox.LeftFoot:
                leftFootCollider.enabled = true;
                break;
            case AttackHitbox.RightFoot:
                rightFootCollider.enabled = true;
                break;
            case AttackHitbox.Axe:
                axeCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    // Desactiva todos los colliders de ataque
    void DisableHitboxes()
    {
        if (axeCollider != null)
            axeCollider.enabled = false;
        if (leftFootCollider != null)
            leftFootCollider.enabled = false;
        if (rightFootCollider != null)
            rightFootCollider.enabled = false;
        if (leftHandCollider != null)
            leftHandCollider.enabled = false;
        if (rightHandCollider != null)
            rightHandCollider.enabled = false;
    }
}
