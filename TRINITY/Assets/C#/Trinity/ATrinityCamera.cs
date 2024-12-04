using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityCamera : MonoBehaviour
{
    public Camera Camera;
    public ATrinityController Controller; // Reference to the player controller
    public APlayerInput InputReference;   // Reference to the input script

    public float RotationSpeed = 5f;
    public float VerticalClampMin = -30f; // Minimum vertical angle
    public float VerticalClampMax = 60f;  // Maximum vertical angle
    public Transform LookTarget;          // The target the camera follows (usually the player)

    private float VerticalRotationExtent = 0f;  // Track vertical rotation
    public Vector3 CameraBoomOffset = new Vector3(0f, 1.2f, -3f); // Default offset from LookTarget
    private GameObject CameraBoom;
    

    private Vector3 initialBoomPosition; // To store the original position
    private float currentDistance;       // Current distance of the camera from the boom origin

    void Start()
    {
        CameraBoom = Camera.transform.parent.gameObject;

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

}