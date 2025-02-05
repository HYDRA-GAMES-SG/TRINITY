using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ATrinityInput : MonoBehaviour, IAA_TrinityControls.IPLAYERActions
{
    public IAA_TrinityControls InputActions;

    // Public accessor variables for input values
    public bool MenuInput { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public Vector2 CameraInput { get; private set; }
    public bool BlinkInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool ForcefieldInput { get; private set; }
    public bool ElementalUtililyInput { get; private set; }
    public bool ElementalPrimaryInput { get; private set; }
    public bool ElementalSecondaryInput { get; private set; }
    public bool NextElementInput { get; private set; }
    public bool PreviousElementInput { get; private set; }
    public bool FireElementInput { get; private set; }
    public bool ColdElementInput { get; private set; }
    public bool LightningElementInput { get; private set; }

    public void NullifyInputs()
    {
        JumpInput = false;
        MenuInput = false;
        ForcefieldInput = false;
        ElementalPrimaryInput = false;
    }
    private float ElementCooldownTimer = 0f;
    private float ElementCooldown = .2f;
    // System.Actions for events
    public event Action OnJumpGlidePressed;
    public event Action OnJumpGlideReleased;

    public event Action OnBlinkPressed;
    public event Action OnBlinkReleased;

    public event Action OnForcefieldPressed;
    public event Action OnForcefieldReleased;

    public event Action OnElementalUtiltiyPressed;
    public event Action OnElementalUtilityReleased;

    public event Action OnElementalPrimaryPressed;
    public event Action OnElementalPrimaryReleased;

    public event Action OnElementalSecondaryPressed;
    public event Action OnElementalSecondaryReleased;

    public event Action OnNextElementPressed;
    public event Action OnNextElementReleased;

    public event Action OnPreviousElementPressed;
    public event Action OnPreviousElementReleased;

    public event Action OnMovePressed;
    public event Action OnMoveReleased;

    public event Action OnCameraPressed;
    public event Action OnCameraReleased;

    public event Action OnMenuPressed;
    public event Action OnMenuReleased;

    public event Action<ETrinityElement> OnElementPressed;
    public event Action<ETrinityElement> OnElementReleased;

    void Awake()
    {
        ATrinityGameManager.SetInput(this);
        
        InputActions = new IAA_TrinityControls();
        InputActions.PLAYER.SetCallbacks(this);
        InputActions.Enable();
        
        // Subscribe inputs
        InputActions.PLAYER.JumpGlide.started += OnJumpGlide;
        InputActions.PLAYER.JumpGlide.canceled += OnJumpGlide;

        InputActions.PLAYER.Blink.started += OnBlink;
        InputActions.PLAYER.Blink.canceled += OnBlink;

        InputActions.PLAYER.Forcefield.started += OnForcefield;
        InputActions.PLAYER.Forcefield.canceled += OnForcefield;

        InputActions.PLAYER.ElementalUtility.started += OnElementalUtility;
        InputActions.PLAYER.ElementalUtility.canceled += OnElementalUtility;

        InputActions.PLAYER.ElementalPrimary.started += OnElementalPrimary;
        InputActions.PLAYER.ElementalPrimary.canceled += OnElementalPrimary;

        InputActions.PLAYER.ElementalSecondary.started += OnElementalSecondary;
        InputActions.PLAYER.ElementalSecondary.canceled += OnElementalSecondary;

        InputActions.PLAYER.NextElement.started += OnNextElement;
        InputActions.PLAYER.NextElement.canceled += OnNextElement;

        InputActions.PLAYER.PreviousElement.started += OnPreviousElement;
        InputActions.PLAYER.PreviousElement.canceled += OnPreviousElement;

        InputActions.PLAYER.Move.started += OnMove;
        InputActions.PLAYER.Move.canceled += OnMove;

        InputActions.PLAYER.Camera.started += OnCamera;
        InputActions.PLAYER.Camera.canceled += OnCamera;

        InputActions.PLAYER.FireElement.started += OnFireElement;
        InputActions.PLAYER.FireElement.canceled += OnFireElement;
        
        
        InputActions.PLAYER.ColdElement.started += OnColdElement;
        InputActions.PLAYER.ColdElement.canceled += OnColdElement;
        
        InputActions.PLAYER.LightningElement.started += OnLightningElement;
        InputActions.PLAYER.LightningElement.canceled += OnLightningElement;

        InputActions.PLAYER.Menu.started += OnMenu;
        InputActions.PLAYER.Menu.canceled += OnMenu;

    }

    void OnDestroy()
    {
        // Unsubscribe inputs
        InputActions.PLAYER.JumpGlide.started -= OnJumpGlide;
        InputActions.PLAYER.JumpGlide.canceled -= OnJumpGlide;

        InputActions.PLAYER.Blink.started -= OnBlink;
        InputActions.PLAYER.Blink.canceled -= OnBlink;

        InputActions.PLAYER.Forcefield.started -= OnForcefield;
        InputActions.PLAYER.Forcefield.canceled -= OnForcefield;

        InputActions.PLAYER.ElementalUtility.started -= OnElementalUtility;
        InputActions.PLAYER.ElementalUtility.canceled -= OnElementalUtility;

        InputActions.PLAYER.ElementalPrimary.started -= OnElementalPrimary;
        InputActions.PLAYER.ElementalPrimary.canceled -= OnElementalPrimary;

        InputActions.PLAYER.ElementalSecondary.started -= OnElementalSecondary;
        InputActions.PLAYER.ElementalSecondary.canceled -= OnElementalSecondary;

        InputActions.PLAYER.NextElement.started -= OnNextElement;
        InputActions.PLAYER.NextElement.canceled -= OnNextElement;

        InputActions.PLAYER.PreviousElement.started -= OnPreviousElement;
        InputActions.PLAYER.PreviousElement.canceled -= OnPreviousElement;

        InputActions.PLAYER.Move.started -= OnMove;
        InputActions.PLAYER.Move.canceled -= OnMove;

        InputActions.PLAYER.Camera.started -= OnCamera;
        InputActions.PLAYER.Camera.canceled -= OnCamera;

        InputActions.PLAYER.FireElement.started -= OnFireElement;
        InputActions.PLAYER.FireElement.canceled -= OnFireElement;
        
        
        InputActions.PLAYER.ColdElement.started -= OnColdElement;
        InputActions.PLAYER.ColdElement.canceled -= OnColdElement;
        
        InputActions.PLAYER.LightningElement.started -= OnLightningElement;
        InputActions.PLAYER.LightningElement.canceled -= OnLightningElement;
        
        InputActions.PLAYER.Menu.started += OnMenu;
        InputActions.PLAYER.Menu.canceled += OnMenu;
    }

    public void Update()
    {
        ElementCooldownTimer -= Time.deltaTime;
    }
    
    public void OnJumpGlide(InputAction.CallbackContext context)
    {
        JumpInput = context.ReadValue<float>() > 0f;
        if (context.started)
        {
            OnJumpGlidePressed?.Invoke();
        }

        if (context.canceled)
        {
            OnJumpGlideReleased?.Invoke();
        }
    }

    public void OnBlink(InputAction.CallbackContext context)
    { 
        BlinkInput = context.ReadValue<float>() > 0f;
        if (context.started)
        {
            OnBlinkPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnBlinkReleased?.Invoke();
        }
    }

    public void OnForcefield(InputAction.CallbackContext context)
    {
        ForcefieldInput = context.ReadValue<float>() > 0f;
        if (context.started)
        {
            OnForcefieldPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnForcefieldReleased?.Invoke();
        }
        
    }

    public void OnElementalUtility(InputAction.CallbackContext context)
    {
        ElementalUtililyInput = context.ReadValue<float>() > 0f;
        if (context.started)
        {
            OnElementalUtiltiyPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnElementalUtilityReleased?.Invoke();
        }

    }

    public void OnElementalPrimary(InputAction.CallbackContext context)
    {
        ElementalPrimaryInput = context.ReadValue<float>() > 0f;
        if (context.started)
        {
            OnElementalPrimaryPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnElementalPrimaryReleased?.Invoke();
        }

    }

    public void OnElementalSecondary(InputAction.CallbackContext context)
    {
        ElementalSecondaryInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnElementalSecondaryPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnElementalSecondaryReleased?.Invoke();
        }

    }

    public void OnNextElement(InputAction.CallbackContext context)
    {
        if (ElementCooldownTimer > 0)
        {
            return;
        }

        ElementCooldownTimer = ElementCooldown;
        
        NextElementInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnNextElementPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnNextElementReleased?.Invoke();
        }

    }

    public void OnPreviousElement(InputAction.CallbackContext context)
    {
        if (ElementCooldownTimer > 0)
        {
            return;
        }

        ElementCooldownTimer = ElementCooldown;
        
        
        PreviousElementInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnPreviousElementPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnPreviousElementReleased?.Invoke();
        }

    }

    public void OnFireElement(InputAction.CallbackContext context)
    {
        if (ElementCooldownTimer > 0)
        {
            return;
        }

        ElementCooldownTimer = ElementCooldown;
        
        FireElementInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnElementPressed?.Invoke(ETrinityElement.ETE_Fire);
        }

        if (context.canceled)
        {
            OnElementReleased?.Invoke(ETrinityElement.ETE_Fire);
        }

    }

    public void OnColdElement(InputAction.CallbackContext context)
    {
        if (ElementCooldownTimer > 0)
        {
            return;
        }

        ElementCooldownTimer = ElementCooldown;
        
        ColdElementInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnElementPressed?.Invoke(ETrinityElement.ETE_Cold);
        }

        if (context.canceled)
        {
            OnElementReleased?.Invoke(ETrinityElement.ETE_Cold);
        }

    }

    public void OnLightningElement(InputAction.CallbackContext context)
    {
        if (ElementCooldownTimer > 0)
        {
            return;
        }

        ElementCooldownTimer = ElementCooldown;
        
        LightningElementInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnElementPressed?.Invoke(ETrinityElement.ETE_Lightning);
        }

        if (context.canceled)
        {
            OnElementReleased?.Invoke(ETrinityElement.ETE_Lightning);
        }

    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        
        MenuInput = context.ReadValue<float>() > 0f;
        if(context.started)
        {
            OnMenuPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnMenuReleased?.Invoke();
        }

    }
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        if(context.started)
        {        
            OnMovePressed?.Invoke();
        }

        if (context.canceled)
        {
            OnMoveReleased?.Invoke();
        }
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        CameraInput = context.ReadValue<Vector2>();
        if(context.started)
        {
            OnCameraPressed?.Invoke();
        }

        if (context.canceled)
        {
            OnCameraReleased?.Invoke();
        }
        
    }
}