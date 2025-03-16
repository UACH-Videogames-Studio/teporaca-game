using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Stariluz
{

    public class PlayerMovement : MonoBehaviour
    {
        public InputActionReference moveAction;


        [SerializeField] protected float movementSpeed = 5.0f;

        [SerializeField] protected float rotationSpeed = 150.0f;

        [SerializeField] protected Animator animator;

        private float2 movement;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            movement = moveAction.action.ReadValue<Vector2>();
            transform.Rotate(0, movement.x * Time.deltaTime * rotationSpeed, 0);
            transform.Translate(0, 0, movement.y * Time.deltaTime * movementSpeed);
            animator.SetFloat("xMovement", movement.x);
            animator.SetFloat("yMovement", movement.y);
        }
    }

}