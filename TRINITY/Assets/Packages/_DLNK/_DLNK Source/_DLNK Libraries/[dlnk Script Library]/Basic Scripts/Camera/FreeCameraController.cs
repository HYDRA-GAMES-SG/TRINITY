using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;
    public float minFOV = 10f;
    public float maxFOV = 100f;
    public float fastMovementSpeedMultiplier = 2f;

    private Camera cam;
    private float rotationX = 0f;
    private Vector3 startPosition;
    private bool isFastMovementEnabled = false;

    private void Start()
    {
        cam = GetComponent<Camera>();
        startPosition = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Camera movement
        float horizontalMovement = Input.GetAxis("Horizontal") * GetMovementSpeed() * Time.deltaTime;
        float verticalMovement = Input.GetAxis("Vertical") * GetMovementSpeed() * Time.deltaTime;
        float riseMovement = 0f;

        if (Input.GetKey(KeyCode.E))
        {
            riseMovement = GetMovementSpeed() * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            riseMovement = -GetMovementSpeed() * Time.deltaTime;
        }

        transform.Translate(new Vector3(horizontalMovement, riseMovement, verticalMovement));

        // Camera rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        rotationX -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationX, transform.eulerAngles.y + mouseX, 0f);

        // Camera zoom
        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        float newFOV = cam.fieldOfView - zoom;
        newFOV = Mathf.Clamp(newFOV, minFOV, maxFOV);
        cam.fieldOfView = newFOV;

        // Toggle fast movement speed
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isFastMovementEnabled = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isFastMovementEnabled = false;
        }
    }

    private float GetMovementSpeed()
    {
        return movementSpeed * (isFastMovementEnabled ? fastMovementSpeedMultiplier : 1f);
    }

    public void ResetCameraPosition()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        rotationX = 0f;
    }
}
