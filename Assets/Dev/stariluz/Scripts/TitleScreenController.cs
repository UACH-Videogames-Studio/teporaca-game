using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenController : MonoBehaviour
{

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
    protected InputActions.UIActions _uiActions;
    public InputActions.UIActions uiActions
    {
        get
        {
            return _uiActions;
        }
    }

    [SerializeField] private string sceneToLoad = "Narrative1"; // Cambia esto por el nombre real de tu escena

    
    private void Awake()
    {
        _inputActions = new InputActions();
        _uiActions = inputActions.UI;
        _uiActions.FastNext.performed += StartGame;
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    public SceneTransitionManager manager;
    private void StartGame(InputAction.CallbackContext context)
    {
        manager.LoadScene(sceneToLoad);
    }
}
