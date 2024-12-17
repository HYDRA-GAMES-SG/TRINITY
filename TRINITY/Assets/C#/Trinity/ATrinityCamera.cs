using System;
using ThirdPersonCamera;
using UnityEngine;

public class ATrinityCamera : MonoBehaviour
{
    private OverTheShoulder OverTheShoulderCameraComponent;
    public Camera Camera;
    private ATrinityController Controller; // Reference to the player controller
    private APlayerInput InputReference;   // Reference to the input script
    public GameObject LookAtObject;
    public float BlinkCameraLerpTime = 1f;
    public float BlinkCameraDistance = 5f;

    private float OriginalCameraDistance; // To store the original value of ReleaseDistance
    private float CurrentLerpTime;         // To track lerp progress
    private bool bBlinkLerp;               // To track if a blink event is active

    void Start()
    {
        Cursor.visible = false;
        Controller = transform.parent.GetComponent<ATrinityController>();
        InputReference = transform.root.Find("Brain").GetComponent<APlayerInput>();
        Camera = GetComponent<Camera>();

        OverTheShoulderCameraComponent = GetComponent<OverTheShoulder>();
        if (OverTheShoulderCameraComponent != null)
        {
            OriginalCameraDistance = OverTheShoulderCameraComponent.ReleaseDistance;
        }

        ABlink.BlinkCamera += HandleBlink;
    }

    private void HandleBlink()
    {
        if (OverTheShoulderCameraComponent != null)
        {
            OverTheShoulderCameraComponent.ReleaseDistance = BlinkCameraDistance;
            CurrentLerpTime = 0f; // Reset lerp time
            bBlinkLerp = true;    // Start lerp process
        }
    }

    private void OnDestroy()
    {
        ABlink.OnBlink -= HandleBlink; // Unsubscribe from the event to avoid memory leaks
    }

    void Update()
    {
        if (bBlinkLerp && OverTheShoulderCameraComponent != null)
        {
            // Increment the lerp time
            CurrentLerpTime += Time.deltaTime;
            float t = CurrentLerpTime / BlinkCameraLerpTime;

            // Lerp the ReleaseDistance value back to its original value
            OverTheShoulderCameraComponent.ReleaseDistance = Mathf.Lerp(BlinkCameraDistance, OriginalCameraDistance, t);

            // Stop lerping once the time is exceeded
            if (CurrentLerpTime >= BlinkCameraLerpTime)
            {
                OverTheShoulderCameraComponent.ReleaseDistance = OriginalCameraDistance;
                bBlinkLerp = false;
            }
        }
    }

    void LateUpdate()
    {
        // Additional LateUpdate logic if needed
    }
}
