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
    public Transform LookTarget;              // The target the camera follows (usually the player)

    private float VerticalRotationExtent = 0f;  // Track vertical rotation
    private GameObject CameraBoom;

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
    }

    void LateUpdate()
    {
        if (InputReference == null || LookTarget == null) return;


        // Calculate vertical rotation and clamp it
        VerticalRotationExtent -= InputReference.CameraInput.y * RotationSpeed * Time.deltaTime;
        VerticalRotationExtent = Mathf.Clamp(VerticalRotationExtent, VerticalClampMin, VerticalClampMax);

        // Apply vertical rotation by adjusting the camera's local rotation
        CameraBoom.transform.localRotation = Quaternion.Euler(VerticalRotationExtent, 0f, 0f);
    }
    
}