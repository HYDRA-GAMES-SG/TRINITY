using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class APlayerInput : MonoBehaviour, IAA_TrinityControls.IPLAYERActions
{
    public Camera CameraReference;
    public IAA_TrinityControls InputActions;

    private PlayerInput InputReference;

    // Public accessor variables for input values
    public Vector2 MoveInput { get; private set; }
    public Vector2 CameraInput { get; private set; }
    public bool JumpInput { get; private set; }
    public InputAction.CallbackContext PrimaryInput;

    void Awake()
    {
        InputActions = new IAA_TrinityControls();
        InputActions.PLAYER.SetCallbacks(this);
        InputActions.Enable();    
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
            InputReference.actions["Jump"].performed -= OnJumpGlide;
            InputReference.actions["Jump"].canceled -= OnJumpGlide;
        }
    }
    
    // Input handling functions
    public void OnJumpGlide(InputAction.CallbackContext context)
    {
        JumpInput = context.ReadValue<float>() > 0f;
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
        PrimaryInput = context;
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
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        CameraInput = context.ReadValue<Vector2>();
    }
}