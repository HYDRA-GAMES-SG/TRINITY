using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum ETrinityMovement
{
    ETM_Grounded,
    ETM_Jumping,
    ETM_Falling
}

public class NormalMovement : TrinityState
{
    public bool ENABLE_DEBUG = false;
    private ETrinityMovement MovementState;

    private bool bStunned => Brain.GetAction() == ETrinityAction.ETA_Stunned;

    [SerializeField] private float MoveAirAcceleration = 5f;
    [SerializeField] private float StrafeAirAcceleration = 5f;
    [SerializeField] private float MoveAcceleration = 10f;
    [SerializeField] private float StrafeAcceleration = 10f;
    [SerializeField] private float MaxSpeed = 5f;
    [SerializeField] private float GlobalSpeedLimit = 7f;
    [SerializeField] private float JumpForce = 10f;
    
    [HideInInspector] private int bMirror = 0;
    
    private bool bCanGlide = false;
    private bool bFixedUpdate = false;

    private string AnimKeyMove = "vForward";
    private string AnimKeyStrafe = "vStrafe";
    private string AnimKeyJump = "bJump";
    private string AnimKeyVertical = "vVertical";
    private string AnimKeyGlide = "bGlide";
    private string AnimKeyBlink = "bBlink";
    private string AnimKeyMirrorJump = "bMirror";
    private string AnimKeyDeath = "Death";
    private string AnimKeyStunned = "bStunned";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        if (fromState is GlideMovement)
        {
            MovementState = ETrinityMovement.ETM_Falling;
            TrinityFSM.Animator.SetBool(AnimKeyGlide, true);
            TrinityFSM.Animator.SetBool(AnimKeyMirrorJump, bMirror % 2 == 1);
        }
        else
        {
            MovementState = ETrinityMovement.ETM_Grounded;
            TrinityFSM.Animator.SetBool(AnimKeyJump, false);
            TrinityFSM.Animator.SetBool(AnimKeyGlide, false);
        }
        
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

        if (Controller.HealthComponent.bDead)
        {
            return;
        }

        HandleMovement();
        HandleJump();
        HandleFalling();
        HandleBlink();
        TryEnterGlide();
        HandleUnstableGround();
        
        Controller.MoveDirection = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
        
        // if speed in the direction of the movedirection is not faster than max speed
        if (Vector3.Project(Controller.RB.velocity, Controller.MoveDirection).magnitude < MaxSpeed) 
        {
            Controller.RB.AddForce(Controller.MoveDirection);
        }
        
        HandleAirStrafing();
        HandleGravity();
        
        UpdateAnimParams();
    }

    private void HandleGravity()
    {
        Controller.RB.AddForce(-Controller.Up * Controller.Gravity);
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
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            if (toState is GlideMovement)
            {
                GlideMovement newGlideState = (GlideMovement)toState;
                newGlideState.bMirror = bMirror % 2 == 1;
            }
            
            return true;
        }
        
        return false;
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
        if (bStunned)
        {
            return;
        }

        Vector3 moveZ = Vector3.zero;
        Vector3 moveX = Vector3.zero;
        
        if (MovementState == ETrinityMovement.ETM_Grounded)
        {
            
            moveZ = Controller.Forward * InputReference.MoveInput.y * MoveAcceleration;
            moveX = Controller.Right * InputReference.MoveInput.x * StrafeAcceleration;
        }
        else
        {
            moveZ = Controller.Forward * InputReference.MoveInput.y * MoveAirAcceleration;
            moveX = Controller.Right * InputReference.MoveInput.x * StrafeAirAcceleration;
        }
        
        Controller.MoveDirection = moveZ;
        Controller.MoveDirection += moveX;
    }
    
    private void HandleJump()
    {
        if (bStunned)
        {
            return;
        }
        
        if (InputReference.JumpInput)
        {
            if (MovementState == ETrinityMovement.ETM_Grounded)
            {
                Controller.RB.AddForce(Controller.Up * JumpForce / Controller.RB.mass, ForceMode.Impulse);
                SetMovementState(ETrinityMovement.ETM_Jumping);
                TrinityFSM.Animator.SetBool(AnimKeyJump, true);
                bMirror++; //increment counter
                TrinityFSM.Animator.SetBool(AnimKeyMirrorJump, bMirror % 2 == 1); //flip flop counter

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
        else
        {
            // if (Controller.CheckGround().transform)
            // {
            //     return;
            // }
            //
            // if (MovementState != ETrinityMovement.ETM_Jumping)
            // {
            //     MovementState = ETrinityMovement.ETM_Falling;
            // }
        }
    }
    
    
    private void HandleBlink()
    {
        if (bStunned)
        {
            return;
        }
        
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
        Vector3 groundNormal = Controller.FindUnstableGround().normal;
        
        Vector3 slideDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;

        Controller.MoveDirection += slideDirection * MaxSpeed * Time.deltaTime;

        // Check if the angle is steep enough to transition into falling
        float groundAngle = Vector3.Angle(Vector3.up, groundNormal);
        
        if (groundAngle > Controller.MaxStableAngle)
        {
            SetMovementState(ETrinityMovement.ETM_Falling);
            Controller.MoveDirection = slideDirection * MaxSpeed * Time.deltaTime;
            bCanGlide = false;
        }
    }
    
    

    private void TryEnterGlide()
    {
        if (bStunned)
        {
            return;
        }
        
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
                TrinityFSM.EnqueueTransition<GlideMovement>();
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
        Vector3 playerSpaceVelocity = Controller.transform.InverseTransformVector(Controller.RB.velocity);
        
        TrinityFSM.Animator.SetFloat(AnimKeyMove, playerSpaceVelocity.z, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyStrafe, playerSpaceVelocity.x, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyVertical, Controller.VerticalVelocity);
        TrinityFSM.Animator.SetBool(AnimKeyStunned, bStunned);
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