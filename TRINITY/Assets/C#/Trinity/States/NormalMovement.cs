using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum ETrinityMovement
{
    ETM_Grounded,
    ETM_Jumping,
    ETM_Falling,
    ETM_Gliding
}

public class NormalMovement : TrinityState
{
    public bool ENABLE_DEBUG = false;
    
    private ETrinityMovement MovementState;

    [Header("Normal Movement")] 
    [SerializeField] private float UnstableSlideAcceleration = 40f;
    [SerializeField] private float AirStrafeModifier = .5f;
    [SerializeField] private float AirMoveModifier = .5f;
    [SerializeField] private float MoveAirAcceleration = 5f;
    [SerializeField] private float StrafeAirAcceleration = 5f;
    [SerializeField] private float MoveAcceleration = 10f;
    [SerializeField] private float StrafeAcceleration = 10f;
    [SerializeField] private float MaxSpeed = 5f;
    [SerializeField] private float GlobalSpeedLimit = 7f;
    [SerializeField] private float JumpForce = 10f;
    
    [Header("Glide Movement")]
    [SerializeField] private float GlideMoveAcceleration = 40f;
    [SerializeField] private float GlideStrafeAcceleration = 40f;
    [SerializeField] private float GravityModifier = .4f;
    
    [HideInInspector] private int bMirror = 0;

    private bool bUnstable = false;
    private bool bCanGlide = false;
    private bool bFixedUpdate = false;

    private string AnimKeyMove = "vForward";
    private string AnimKeyStrafe = "vStrafe";
    private string AnimKeyJump = "bJump";
    private string AnimKeyVertical = "vVertical";
    private string AnimKeyGlide = "bGlide";
    private string AnimKeyBlink = "bBlink";
    private string AnimKeyMirror = "bMirror";
    private string AnimKeyDeath = "bDeath";
    private string AnimKeyStunned = "bStunned";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        SetMovementState(ETrinityMovement.ETM_Grounded);
        TrinityFSM.Animator.SetBool(AnimKeyJump, false);
        TrinityFSM.Animator.SetBool(AnimKeyGlide, false);
        
        bCanGlide = false;
        ABlink.OnBlink += OnBlink;
        Controller.HealthComponent.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        // if (Controller.CheckGround().transform)
        // {
        TrinityFSM.Animator.SetTrigger(AnimKeyDeath);
        // }
        // else
        // {
        //     Controller.EnableRagdoll();
        // }
    }


    public override void PreUpdateBehaviour(float dt)
    {
    }
    
    
    public override void UpdateBehaviour(float dt)
    {
        bFixedUpdate = true;
        
        HandleGliding();
        HandleFalling();
        HandleUnstableGround();

        if (!TrinityFSM.IsActionable() || bUnstable)
        {
            UpdateAnimParams();
            return;
        }
        
        HandleMovement();
        HandleJumping();
        HandleBlink();
        TryEnterGlide();


        switch (MovementState)
        {
            case ETrinityMovement.ETM_Grounded:
                Controller.MoveDirection = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
                break;
            case ETrinityMovement.ETM_Gliding:
                Controller.MoveDirection = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
                break;
            default:
                Controller.MoveDirection = new Vector3(Controller.MoveDirection.x * AirMoveModifier, Controller.VerticalVelocity, Controller.MoveDirection.z * AirStrafeModifier);
                break;
        }
        
        // if speed in the direction of the movedirection is not faster than max speed
        if (Vector3.Project(Controller.RB.velocity, Controller.MoveDirection).magnitude < MaxSpeed) 
        {
            Controller.RB.AddForce(Controller.MoveDirection);
        }
        
        HandleAirStrafing();
        UpdateAnimParams();
    }


    public override void PostUpdateBehaviour(float dt)
    {
       
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        ABlink.OnBlink -= OnBlink;
        Controller.HealthComponent.OnDeath -= HandleDeath;

    }

    public override bool CheckExitTransition(IState toState)
    {
        
        return false;
    }


    private void HandleGliding()
    {
        if (MovementState != ETrinityMovement.ETM_Gliding)
        {
            return;
        }
        
        if (!InputReference.JumpInput || Controller.CheckGround().transform) // let HandleFalling() handle groundedness
        {
            SetMovementState(ETrinityMovement.ETM_Falling);
            Controller.Gravity = ATrinityController.GRAVITY_CONSTANT;
            return;
        }
        
        //Handle Glide
        Controller.Gravity = ATrinityController.GRAVITY_CONSTANT * GravityModifier;
    }
    
    private void HandleAirStrafing()
    {
        Vector3 planarVelocity = Controller.PlanarVelocity;
        if (planarVelocity.magnitude > GlobalSpeedLimit)
        {
            planarVelocity = planarVelocity.normalized * GlobalSpeedLimit;
        }
        
        
        Controller.RB.velocity = planarVelocity + new Vector3(0f, Controller.VerticalVelocity, 0f);

    }

    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x;

        switch (MovementState)
        {
            case ETrinityMovement.ETM_Grounded:
                moveZ *= MoveAcceleration;
                moveX *= StrafeAcceleration;
                break;
            case ETrinityMovement.ETM_Gliding:
                moveZ *= GlideMoveAcceleration;
                moveX *= GlideStrafeAcceleration;
                break;
            default: //jumping or falling
                moveZ *= MoveAirAcceleration;
                moveX *= StrafeAirAcceleration;
                break;
        }
        
        Controller.MoveDirection = moveZ;
        Controller.MoveDirection += moveX;
    }
    
    private void HandleJumping()
    {  
        if (InputReference.JumpInput)
        {
            if (MovementState == ETrinityMovement.ETM_Grounded)
            {
                SetMovementState(ETrinityMovement.ETM_Jumping);
                
                if (MovementState == ETrinityMovement.ETM_Jumping) //need to check this
                {
                    Controller.RB.AddForce(Controller.Up * JumpForce / Controller.RB.mass, ForceMode.Impulse);
                    TrinityFSM.Animator.SetBool(AnimKeyJump, true);
                    bMirror++; //increment counter
                    TrinityFSM.Animator.SetBool(AnimKeyMirror, bMirror % 2 == 1); //flip flop counter
                }
            }
        }
    }

    private void HandleFalling()
    {
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            if (MovementState == ETrinityMovement.ETM_Jumping && Controller.VerticalVelocity < 0f)
            {
                SetMovementState(ETrinityMovement.ETM_Falling);
            }
            
            // Perform raycast to check for ground
            if (MovementState != ETrinityMovement.ETM_Jumping)
            {
                if (Controller.CheckGround().transform)
                {
                    // Ground detected, ensure movement state remains grounded
                    SetMovementState(ETrinityMovement.ETM_Grounded);
                    bCanGlide = false;
                    TrinityFSM.Animator.SetBool(AnimKeyJump, false);
                    TrinityFSM.Animator.SetBool(AnimKeyBlink, false);
                    TrinityFSM.Animator.SetBool(AnimKeyGlide, false);
                }
            }
        }
    }
    
    
    private void HandleBlink()
    {
        if (TrinityFSM.Animator.GetBool(AnimKeyBlink) && Controller.CheckGround().transform)
        {
            // Ground detected, ensure movement state remains grounded
            SetMovementState(ETrinityMovement.ETM_Grounded);
            bCanGlide = false;
            TrinityFSM.Animator.SetBool(AnimKeyJump, false);
            TrinityFSM.Animator.SetBool(AnimKeyBlink, false);
        }
    }
    
    private void HandleUnstableGround()
    {
        // Vector3 groundNormal = Controller.FindUnstableGround().normal;
        //
        // Vector3 slideDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;
        //
        // Controller.MoveDirection += slideDirection * UnstableSlideAcceleration * Time.deltaTime;
        //
        // //check if the angle is steep enough to transition into falling
        // float groundAngle = Vector3.Angle(Vector3.up, groundNormal);
        //
        // if (groundAngle > Controller.MaxStableAngle)
        // {
        //     SetMovementState(ETrinityMovement.ETM_Falling);
        //     Controller.MoveDirection = slideDirection.normalized * UnstableSlideAcceleration;
        //     bCanGlide = false;
        //     bUnstable = true;
        // }
        // else
        // {
        //     bUnstable = false;
        // }
    }
    
    

    private void TryEnterGlide()
    {
        if (MovementState == ETrinityMovement.ETM_Falling || MovementState == ETrinityMovement.ETM_Jumping)
        {
            if (!InputReference.JumpInput)
            {
                bCanGlide = true;
            }
        }

        if (bCanGlide)
        {
            if (InputReference.JumpInput && MovementState == ETrinityMovement.ETM_Falling)
            {
                SetMovementState(ETrinityMovement.ETM_Gliding);
                TrinityFSM.Animator.SetBool(AnimKeyGlide, true);
            }
        }
    }

    private void SetMovementState(ETrinityMovement newMovementState)
    {
        if (!bFixedUpdate)
        {
            return;
        }
        
        if (newMovementState != MovementState)
        {
            if (ENABLE_DEBUG)
            {
                print("Transition: " + MovementState + "->" + newMovementState);
            }

            bFixedUpdate = false; //flag to prevent new movement if physics hasnt been updated
            MovementState = newMovementState;
        }
    }
    
    private void UpdateAnimParams()
    {
        // less damping if we are landing
        float maxSpeedThreshold = MaxSpeed * .6f;
        Vector3 playerSpaceVelocity = Controller.transform.InverseTransformVector(Controller.RB.velocity) / maxSpeedThreshold;
        
        TrinityFSM.Animator.SetFloat(AnimKeyMove, playerSpaceVelocity.z, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyStrafe, playerSpaceVelocity.x, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyVertical, Controller.VerticalVelocity);
        TrinityFSM.Animator.SetBool(AnimKeyStunned, Brain.bIsStunned);
    }
    
    public ETrinityMovement GetMovementState()
    {
        return MovementState;
    }
    
    
    private void OnBlink()
    {
        TrinityFSM.Animator.SetBool(AnimKeyBlink, true);
    }


    //

}