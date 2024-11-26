using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float wheelAdjustFactor = 0.2f;

    [SerializeField] private float movementSmoothing = 0.9f;
    [SerializeField] private float rotationSmoothing = 0.9f;

    [SerializeField] private float movementDamping = 0.1f;

    private float speedMultiplier = 1f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    [SerializeField] private Camera cam;
    [SerializeField] private Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        //fallbacks if unset in inspector
        if (cam == null) cam = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void FixedUpdate() // fixedupdate for physics
    {
        Vector3 rotation = cam.transform.eulerAngles;

        Vector3 deltaPosition = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            deltaPosition += cam.transform.forward;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            deltaPosition += cam.transform.right * -1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            deltaPosition += cam.transform.forward * -1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            deltaPosition += cam.transform.right;
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
        {
            deltaPosition += cam.transform.up * 0.8f;
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
        {
            deltaPosition += cam.transform.up * -0.8f;
        }

        deltaPosition *= baseSpeed * speedMultiplier;
        rb.AddForce(deltaPosition);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity * (1-movementDamping), baseSpeed * speedMultiplier);
        

        if(Input.GetAxis("Mouse ScrollWheel") != 0f) speedMultiplier += speedMultiplier * wheelAdjustFactor * Input.GetAxis("Mouse ScrollWheel") * 10f;
    }

    private void Update()
    {
        Vector3 deltaRotationEuler = Vector3.zero;

        deltaRotationEuler.x += Input.GetAxis("Mouse Y") * -1f;
        deltaRotationEuler.y += Input.GetAxis("Mouse X");

        deltaRotationEuler *= lookSensitivity;
        if (deltaRotationEuler != Vector3.zero) targetRotation = Quaternion.Euler(targetRotation.eulerAngles + deltaRotationEuler);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetRotation, 1 - rotationSmoothing);
    }
}
