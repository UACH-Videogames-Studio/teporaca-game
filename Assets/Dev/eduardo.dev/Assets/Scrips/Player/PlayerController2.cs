using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    float ySpeed;


    Quaternion targetRotation;

    CameraController1 cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;

    private void Awake()
    {
       cameraController = Camera.main.GetComponent<CameraController1>();
       animator = GetComponent<Animator>();
       characterController = GetComponent<CharacterController>();
       meeleFighter = GetComponent<MeeleFighter>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (meeleFighter.InAction == true)
        {
            animator.SetFloat("moveAmount", 0);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = new Vector3(h, 0, v).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;

        GroundCheck();
    
        if (isGrounded)
        {
            ySpeed = -0.5f;
        } 
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
    }

    public void GroundCheck ()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,1,0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
