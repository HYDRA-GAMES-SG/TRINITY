using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ATrinityCamera : MonoBehaviour
{
    public Camera Camera;
    public ATrinityController Controller; // Reference to the player controller
    public APlayerInput InputReference;   // Reference to the input script
    public ATrinityFSM CharacterState;    // Reference to the character state machine

    public float RotationSpeed = 5f;
    public float VerticalClampMin = -30f; // Minimum vertical angle
    public float VerticalClampMax = 60f;  // Maximum vertical angle
    public Transform LookTarget;          // The target the camera follows (usually the player)

    private float VerticalRotationExtent = 0f;  // Track vertical rotation
    public Vector3 CameraBoomOffset = new Vector3(0f, 1.2f, -3f); // Default offset from LookTarget
    private GameObject CameraBoom;
    
    public float ReturnSpeed = 2f; // Speed multiplier for exponential return
    public float CollisionCheckRadius = 0.2f; // Radius for collision detection
    public LayerMask CollisionMask; // Mask to specify which layers to detect collisions with

    private Vector3 initialBoomPosition; // To store the original position
    private float currentDistance;       // Current distance of the camera from the boom origin

    void Start()
    {
        if (Camera == null)
        {
            Camera = GetComponent<Camera>();
            CameraBoom = Camera.transform.parent.gameObject;
        }

        if (LookTarget == null && Controller != null)
        {
            LookTarget = Controller.transform; // Default to the controller's transform
        }

        // Store the original position of the camera boom
        if (CameraBoom != null)
        {
            initialBoomPosition = CameraBoom.transform.localPosition;
        }

        currentDistance = CameraBoomOffset.z; // Initialize with the default offset
    }

    void Update()
    {
        LerpCameraBoom();
        HandleCollisionAdjustment();
    }

    void LateUpdate()
    {
        if (InputReference == null || LookTarget == null)
        {
            print("ATrinityCamera: InputRef or LookTarget null.");
            return;
        }
        HandleVerticalRotation();
    }

    private void HandleVerticalRotation()
    {
        // Calculate vertical rotation and clamp it
        VerticalRotationExtent -= InputReference.CameraInput.y * RotationSpeed * Time.deltaTime;
        VerticalRotationExtent = Mathf.Clamp(VerticalRotationExtent, VerticalClampMin, VerticalClampMax);

        // Apply vertical rotation by adjusting the camera's local rotation
        CameraBoom.transform.localRotation = Quaternion.Euler(VerticalRotationExtent, 0f, 0f);
    }

    private void LerpCameraBoom()
    {
        if (CameraBoom == null) return;

        // Exponentially interpolate the camera boom's position back to the original position
        CameraBoom.transform.localPosition = Vector3.Lerp(
            CameraBoom.transform.localPosition,
            initialBoomPosition,
            ReturnSpeed * Time.deltaTime
        );
    }

    private void HandleCollisionAdjustment()
    {
        if (CameraBoom == null)
        {
            return;
        }

        Vector3 boomOrigin = LookTarget.position + CameraBoomOffset;
        Vector3 cameraPosition = boomOrigin + CameraBoom.transform.forward * currentDistance;

        // Check for collision along the boom's direction
        RaycastHit hit;
        if (Physics.SphereCast(
                boomOrigin,
                CollisionCheckRadius,
                CameraBoom.transform.forward,
                out hit,
                Mathf.Abs(CameraBoomOffset.z),
                CollisionMask))
        {
            // Adjust the distance to the hit point
            currentDistance = Mathf.Clamp(hit.distance, 0.5f, Mathf.Abs(CameraBoomOffset.z));
        }
        else
        {
            // Smoothly return to the default distance
            currentDistance = Mathf.Lerp(currentDistance, Mathf.Abs(CameraBoomOffset.z), ReturnSpeed * Time.deltaTime);
        }

        // Update the camera position
        CameraBoom.transform.localPosition = CameraBoomOffset + CameraBoom.transform.forward * -currentDistance;
    }
}