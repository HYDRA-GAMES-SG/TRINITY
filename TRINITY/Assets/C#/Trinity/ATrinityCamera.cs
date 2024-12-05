using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ATrinityCamera : MonoBehaviour
{
    [HideInInspector]
    public Camera Camera;
    private ATrinityController Controller; // Reference to the player controller
    private APlayerInput InputReference;   // Reference to the input script

    public float HorizontalRotationSpeed = 5f;
    [SerializeField] public float VerticalRotationSpeed = 1.5f;
    public float VerticalClampMin = -30f; // Minimum vertical angle
    public float VerticalClampMax = 60f;  // Maximum vertical angle

    //private float VerticalRotationExtent = 0f;  // Track vertical rotation
    
    [HideInInspector] public CinemachineVirtualCamera CinemachineCamera;
    private CinemachineTransposer Transposer;
    public GameObject LookAtObject;


    //camera variables
    private float OriginalZDamping;
    public float BlinkZDamping = 5f;
    private float LerpTime = 1f;
    private float LerpClock = 0f;
    private float VerticalOffset = 0f;

    void Start()
    {
        Controller = transform.parent.GetComponent<ATrinityController>();
        InputReference = transform.root.Find("Brain").GetComponent<APlayerInput>();
        CinemachineCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        Camera = CinemachineCamera.GetComponent<Camera>();
        Transposer = CinemachineCamera.GetCinemachineComponent<CinemachineTransposer>();
        LerpClock = LerpTime;
        ABlink.OnBlink += Blink;
    }

    private void OnDestroy()
    {
        ABlink.OnBlink -= Blink;
    }

    void Update()
    {
        //DO NOT USE
    }
    

    void LateUpdate()
    {
        UpdateZDamping();
        //HandleTrackedAimOffset();
    }

    public void Blink()
    {
        SetZDamping(BlinkZDamping);
    }
    
    private void SetZDamping(float value)
    {
        // Set the target value to 3 (or any other value)
        LerpClock = 0f; // Reset the lerp timer when starting a new lerp
        Transposer.m_ZDamping = value;
    }

    private void UpdateZDamping()
    {
        if (LerpClock < LerpTime)
        {
            LerpClock += Time.deltaTime;
            float lerpedValue = Mathf.Lerp(BlinkZDamping, OriginalZDamping, LerpClock / LerpTime);
            Transposer.m_ZDamping = lerpedValue;
        }
    }
    
    // private void HandleTrackedAimOffset()
    // {
    //     if (Composer == null) return;
    //
    //     // Adjust the vertical offset based on input
    //     VerticalOffset -= InputReference.CameraInput.y * VerticalRotationSpeed * Time.deltaTime;
    //     VerticalOffset = Mathf.Clamp(VerticalOffset, VerticalClampMin, VerticalClampMax);
    //
    //     print(VerticalOffset);
    //     // Apply the new offset to the Cinemachine Composer
    //     Composer.m_TrackedObjectOffset.x = VerticalOffset;
    // }
    
    // private void HandleVerticalRotation()
    // {
    //     // Calculate the vertical rotation and clamp it
    //     VerticalRotationExtent -= InputReference.CameraInput.y * RotationSpeed * Time.deltaTime;
    //     VerticalRotationExtent = Mathf.Clamp(VerticalRotationExtent, VerticalClampMin, VerticalClampMax);
    //
    //     // Update the CameraBoom position and rotation
    //     Vector3 offset = Quaternion.Euler(VerticalRotationExtent, 0f, 0f) * CameraBoomOffset; // Rotate the offset
    //     CameraBoom.transform.position = LookTarget.position + offset; // Position the boom around the LookTarget
    //     CameraBoom.transform.LookAt(LookTarget); // Ensure the camera faces the LookTarget
    // }
}