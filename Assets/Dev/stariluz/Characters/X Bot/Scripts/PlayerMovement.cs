using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputAction moveAction;
    private float _movementSpeed = 0.0f;
    [SerializeField]
    public float movementSpeed
    {
        get
        {
            return _movementSpeed;
        }
        set
        {
            _movementSpeed = value;
        }
    }

    private float _rotationSpeed = 0.0f;
    [SerializeField]
    public float rotationSpeed
    {
        get
        {
            return _rotationSpeed;
        }
        set
        {
            _rotationSpeed = value;
        }
    }

    [SerializeField]
    public Animator animator;


    private float2 movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        animator=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();
        transform.Rotate(0,movement.x*Time.deltaTime,0);
        transform.Translate(0,0,movement.y*Time.deltaTime*movementSpeed);
    }
}
