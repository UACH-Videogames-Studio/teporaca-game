using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stariluz
{
    public class PlayerMovement : MonoBehaviour
    {
        public InputActionReference moveAction;
        [SerializeField] protected float aceleration = 2f;
        [SerializeField] protected float deaceleration = 3f;
        [SerializeField] protected float maxVelocity = 5f;
        [SerializeField] protected float rotationSpeed = 20.0f;

        private Vector2 _velocity;
        protected Vector2 velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }
        private float _xVelocity = 0f;
        protected float xVelocity
        {
            get
            {
                return _xVelocity;
            }
            set
            {
                _xVelocity = value;
            }
        }
        private float _yVelocity = 0f;
        protected float yVelocity
        {
            get
            {
                return _yVelocity;
            }
            set
            {
                _yVelocity = value;
            }
        }


        protected Animator animator;
        protected new Rigidbody rigidbody;


        private Vector2 movement;
        private int ACxMovementHash, ACyMovementHash;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            ACxMovementHash = Animator.StringToHash("xMovement");
            ACyMovementHash = Animator.StringToHash("yMovement");
        }

        // Update is called once per frame
        void Update()
        {
            movement = moveAction.action.ReadValue<Vector2>();
            // float xDelta = movement.x * Time.deltaTime * rotationSpeed;
            // float yDelta = movement.y * Time.deltaTime * aceleration;
            if (movement.magnitude > 0)
            {
                velocity = new Vector2(
                    velocity.x + movement.x * Time.deltaTime * aceleration,
                    velocity.y + movement.y * Time.deltaTime * aceleration
                );
            }
            else if (velocity.magnitude > 0)
            {
                velocity = new Vector2(
                    Math.Sign(velocity.x) * Math.Max(Math.Abs(velocity.x) - Time.deltaTime * deaceleration, 0),
                    Math.Sign(velocity.y) * Math.Max(Math.Abs(velocity.y) - Time.deltaTime * deaceleration, 0)
                );
            }
            else
            {
                velocity = new Vector2(0, 0);
            }
            velocity = Vector2.ClampMagnitude(velocity, maxVelocity);
            transform.Rotate(0, velocity.x, 0);
            rigidbody.linearVelocity = new Vector3(transform.forward.y*velocity.x, 0, transform.forward.y*velocity.y);
            // Debug.Log((rigidbody.linearVelocity));
            animator.SetFloat(ACxMovementHash, velocity.x);
            animator.SetFloat(ACyMovementHash, velocity.y);
        }
    }

}