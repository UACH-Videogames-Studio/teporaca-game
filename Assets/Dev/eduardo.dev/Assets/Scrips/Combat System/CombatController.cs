using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CombatController : MonoBehaviour
{
    MeeleFighter meeleFighter;

    private void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
    }

    public void OnAttack (InputAction.CallbackContext context)
    {
        if (context.started)
        {
            meeleFighter.TryToAttack();
        }
    }
    
}

using UnityEditor.Rendering.LookDev; // (Parece que esta línea no se usa aquí; puede eliminarse si no es necesaria)
using UnityEngine; // Importa las funciones básicas de Unity
using UnityEngine.InputSystem; // Importa el nuevo sistema de entrada (Input System)
using UnityEngine.UI; // Se usa para elementos UI, aunque no se utiliza en este script directamente

// Este script controla el sistema de combate (golpes/ataques) del personaje
public class CombatController : MonoBehaviour
{
    // Referencia al componente MeeleFighter, encargado de gestionar los ataques cuerpo a cuerpo
    MeeleFighter meeleFighter;

    // Awake se llama antes de Start, ideal para obtener referencias a componentes en el mismo GameObject
    private void Awake()
    {
        // Obtiene el componente MeeleFighter del mismo GameObject donde esté este script
        meeleFighter = GetComponent<MeeleFighter>();
    }

    // Esta función se conecta al nuevo sistema de entrada (Input System)
    // Se debe enlazar en el Input Action con el evento "performed" o "started"
    public void OnAttack(InputAction.CallbackContext context)
    {
        // Verifica si el botón de ataque acaba de ser presionado (fase "started")
        if (context.started)
        {
            // Llama al método TryToAttack() del componente MeeleFighter
            // Este método intentará realizar un ataque si las condiciones lo permiten
            meeleFighter.TryToAttack();
        }
    }
}
