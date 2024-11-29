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

    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float StrafeSpeed = 5f;
    [SerializeField] private float JumpVelocity = 10f;
    
    private bool bCanGlide = false;

    private string AnimKeyMove = "vForward";
    private string AnimKeyStrafe = "vStrafe";
    private string AnimKeyJump = "bJump";
    private string AnimKeyVertical = "vVertical";
    private string AnimKeyGlide = "bGlide";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        
        
        MovementState = ETrinityMovement.ETM_Grounded;

        if (fromState is GlideMovement)
        {
            MovementState = ETrinityMovement.ETM_Falling;
            TrinityFSM.Animator.SetBool(AnimKeyGlide, true);
            TrinityFSM.Animator.SetBool(AnimKeyJump, true);
        }
        else
        {
            TrinityFSM.Animator.SetBool(AnimKeyJump, false);
            TrinityFSM.Animator.SetBool(AnimKeyGlide, false);

        }
        
        bCanGlide = false;

    }

    public override void PreUpdateBehaviour(float dt)
    {
    }
    
    
    public override void UpdateBehaviour(float dt)
    {
        HandleMovement();
        HandleJump();
        HandleFalling();
        CheckCanGlide();
        Controller.Rigidbody.velocity = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
        UpdateAnimParams();
    }

    public override void PostUpdateBehaviour(float dt)
    {
       
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            return true;
        }
        
        return false;
    }


    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * MoveSpeed;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * StrafeSpeed;
        Controller.MoveDirection = moveZ;
        Controller.MoveDirection += moveX;
    }
    
    private void HandleJump()
    {
        if (InputReference.JumpInput)
        {
            if (MovementState == ETrinityMovement.ETM_Grounded)
            {
                Controller.VerticalVelocity += JumpVelocity;
                SetMovementState(ETrinityMovement.ETM_Jumping);
                TrinityFSM.Animator.SetBool(AnimKeyJump, true);
            }
            else
            {
                
            }
        }
    }

    private void HandleFalling()
    {
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            Controller.VerticalVelocity -= Controller.Gravity * Time.deltaTime;
            
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
                    Controller.VerticalVelocity = 0f;
                    TrinityFSM.Animator.SetBool(AnimKeyJump, false);
                }
            }
        }
    }
    
    

    private void CheckCanGlide()
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
                TrinityFSM.EnqueueTransition<GlideMovement>();
            }
        }
    }

    private void SetMovementState(ETrinityMovement newMovementState)
    {
        if (newMovementState != MovementState)
        {
            if (ENABLE_DEBUG)
            {
                print("Transition: " + MovementState + "->" + newMovementState);
            }
            MovementState = newMovementState;
        }
    }
    
    private void UpdateAnimParams()
    {
        // less damping if we are landing
        Vector3 playerSpaceVelocity = Controller.transform.InverseTransformVector(Controller.Rigidbody.velocity);
        
        TrinityFSM.Animator.SetFloat(AnimKeyMove, playerSpaceVelocity.z, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyStrafe, playerSpaceVelocity.x, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeyVertical, Controller.VerticalVelocity);
    }
    
    public ETrinityMovement GetMovementState()
    {
        return MovementState;
    }

}