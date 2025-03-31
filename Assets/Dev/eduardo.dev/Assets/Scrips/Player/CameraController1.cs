/*using Unity.Mathematics;
using UnityEngine;

public class CameraController1 : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float rotationSpeed = 2f;    
    [SerializeField] float distance = 5f;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = 45;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    public float rotationX;
    public float rotationY;
    public float invertXVal;
    public float invertYVal;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;
        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        transform.position = focusPosition - targetRotation * new Vector3(0,0,distance);
        transform.rotation = targetRotation;
    }
}
*/

using UnityEngine;
using UnityEngine.InputSystem; // Importar el nuevo Input System

public class CameraController1 : MonoBehaviour
{
    [SerializeField] Transform followTarget; // Referencia al objeto que la cámara seguirá
    [SerializeField] float rotationSpeed = 2f; // Velocidad de rotación de la cámara  
    [SerializeField] float distance = 5f; // Distancia entre la cámara y el objetivo
    [SerializeField] float minVerticalAngle = -45; // Ángulo mínimo de rotación en el eje X (vertical)
    [SerializeField] float maxVerticalAngle = 45; // Ángulo máximo de rotación en el eje X
    [SerializeField] Vector2 framingOffset; // Desplazamiento de la cámara respecto al objetivo
    [SerializeField] bool invertX; // Invertir el eje horizontal
    [SerializeField] bool invertY; // Invertir el eje vertical

    private float rotationX;
    private float rotationY;
    private Vector2 lookInput; // Variable para almacenar la entrada de movimiento de la cámara

    private void Start()
    {
        Cursor.visible = false; // Oculta el cursor
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla
    }

    private void Update()
    {
        // Aplica la inversión de controles si está activada
        float invertXVal = (invertX) ? -1 : 1;
        float invertYVal = (invertY) ? -1 : 1;

        // Modifica los valores de rotación basados en la entrada del usuario
        rotationX += lookInput.y * invertYVal * rotationSpeed; 
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle); // Limita la rotación vertical
        rotationY += lookInput.x * invertXVal * rotationSpeed; 

        // Calcula la nueva rotación de la cámara
        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Calcula la posición de la cámara en función del objetivo y la distancia
        Vector3 focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        // Aplica la nueva posición y rotación
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }

    // Método llamado por el nuevo Input System para recibir la entrada del mouse o joystick
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>(); // Captura la entrada de la cámara
    }

    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}
