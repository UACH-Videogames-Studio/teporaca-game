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
