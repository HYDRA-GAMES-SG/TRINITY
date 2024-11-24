using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class APlayerInput : MonoBehaviour, IAA_TrinityControls.IPLAYERActions
{
    public Camera CameraReference;
    public InputActionAsset InputActions;

    private PlayerInput InputReference;

    // Public accessor variables for input values
    public Vector2 MoveInput { get; private set; }
    public Vector2 CameraInput { get; private set; }

    void Start()
    {
        InputReference = GetComponent<PlayerInput>();

        if (!InputReference)
        {
            Debug.LogError("APlayerInput: PlayerInput component null or missing.");
        }
        else
        {
            InputReference.camera = CameraReference;

            if (InputActions)
            {
                InputReference.actions = InputActions;
                InputReference.defaultActionMap = "PLAYER";
                InputReference.defaultControlScheme = "GENERIC";
                InputReference.actions.FindActionMap("PLAYER").Enable();


                // Subscribe to input actions
                InputReference.actions["Move"].performed += OnMove;
                InputReference.actions["Move"].canceled += OnMove;
                InputReference.actions["Camera"].performed += OnCamera;
                InputReference.actions["Camera"].canceled += OnCamera;
            }
            else
            {
                Debug.LogError("APlayerInput: InputActions unassigned.");
            }
        }
    }

    void OnDestroy()
    {
        if (InputReference && InputReference.actions != null)
        {
            // Unsubscribe from input actions to avoid memory leaks
            InputReference.actions["Move"].performed -= OnMove;
            InputReference.actions["Move"].canceled -= OnMove;
            InputReference.actions["Camera"].performed -= OnCamera;
            InputReference.actions["Camera"].canceled -= OnCamera;
        }
    }
    
    // Input handling functions
    public void OnJumpGlide(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnForcefield(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnElementalUtility(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnElementalPrimary(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnElementalSecondary(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnNextElement(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnPreviousElement(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        print("on move");
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        CameraInput = context.ReadValue<Vector2>();
    }

    // Optional: Add a debug log for testing
    void Update()
    {
    }
}