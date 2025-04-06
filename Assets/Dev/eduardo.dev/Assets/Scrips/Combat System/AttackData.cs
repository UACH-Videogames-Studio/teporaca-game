// using UnityEngine;

// [CreateAssetMenu(menuName = "Combat System/Create a new attack")]
// public class AttackData : ScriptableObject
// {
//     [field: SerializeField] public string AnimName { get; private set;}
//     [field: SerializeField] public AttackHitbox HitboxToUse { get; private set;}

//     [field: SerializeField] public float ImpactStartTime { get; private set;}
//     [field: SerializeField] public float ImpactEndTime { get; private set;}

// }

// public enum AttackHitbox {LeftHand, RightHand, LeftFoot, RightFoot, Axe, Arrow}

using UnityEngine;

// Este atributo permite crear una nueva instancia de este ScriptableObject desde el menú de Unity.
// Se crea un nuevo menú en: Assets > Create > Combat System > Create a new attack
[CreateAssetMenu(menuName = "Combat System/Create a new attack")]
public class AttackData : ScriptableObject
{
    // Este campo representa el nombre de la animación del ataque.
    // Por ejemplo, puede coincidir con el nombre de una animación en el Animator.
    [field: SerializeField] 
    public string AnimName { get; private set; }

    // Este campo indica qué parte del cuerpo (o arma) se usará como "hitbox" para detectar el impacto.
    // Usa un enumerador llamado AttackHitbox definido más abajo.
    [field: SerializeField] 
    public AttackHitbox HitboxToUse { get; private set; }

    // Tiempo (en segundos) desde que inicia la animación hasta que el impacto del ataque comienza a ser efectivo.
    // Se usa para sincronizar la lógica del daño con la animación.
    [field: SerializeField] 
    public float ImpactStartTime { get; private set; }

    // Tiempo (en segundos) hasta el cual el impacto sigue siendo válido.
    // Después de este tiempo, el ataque ya no tiene efecto aunque la animación continúe.
    [field: SerializeField] 
    public float ImpactEndTime { get; private set; }
}

// Enumerador que define las posibles zonas de impacto o armas que se pueden usar en un ataque.
// Esto ayuda a identificar qué collider o parte del cuerpo debe activarse para detectar colisiones.
public enum AttackHitbox 
{
    LeftHand,   // Mano izquierda
    RightHand,  // Mano derecha
    LeftFoot,   // Pie izquierdo
    RightFoot,  // Pie derecho
    Axe,        // Hacha
    Arrow       // Flecha
}
