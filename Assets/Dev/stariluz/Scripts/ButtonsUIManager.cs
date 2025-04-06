using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonsUIManager : MonoBehaviour
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
    protected InputActions.PlayerActions _playerInput;
    public InputActions.PlayerActions playerInput
    {
        get
        {
            return _playerInput;
        }
    }

    private void Awake()
    {
        _inputActions = new InputActions();
        _playerInput = inputActions.Player;
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    string deviceLayout;
    string controlPath;

    void ChangeDisplay()
    {
        // Call GetBindingDisplayString() such that it also returns information about the
        // name of the device layout and path of the control on the device. This information
        // is useful for reliably associating imagery with individual controls.
        // NOTE: The first argument is the index of the binding within InputAction.bindings.
        var bindingString = playerInput.Move.GetBindingDisplayString(0, out deviceLayout, out controlPath);
        Debug.Log(controlPath);

        // // If it's a gamepad, look up an icon for the control.
        // Sprite icon = null;
        // if (!string.IsNullOrEmpty(deviceLayout)
        //     && !string.IsNullOrEmpty(controlPath)
        //     && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, "Gamepad"))
        // {
        //     switch (controlPath)
        //     {
        //         case "buttonSouth": icon = aButtonIcon; break;
        //         case "dpad/up": icon = dpadUpIcon; break;
        //             //...
        //     }
        // }

        // // If you have an icon, display it instead of the text.
        // var text = m_RebindButton.GetComponentInChildren<Text>();
        // var image = m_RebindButton.GetComponentInChildren<Image>();
        // if (icon != null)
        // {
        //     // Display icon.
        //     text.gameObject.SetActive(false);
        //     image.gameObject.SetActive(true);
        //     image.sprite = icon;
        // }
        // else
        // {
        //     // Display text.
        //     text.gameObject.SetActive(true);
        //     image.gameObject.SetActive(false);
        //     text.text = bindingString;
        // }
    }
}
