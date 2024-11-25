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

    private string AnimKeyMove = "vForward";
    private string AnimKeyStrafe = "vStrafe";
    private string AnimKeyJump = "bJump";
    private string AnimKeyVertical = "vVertical";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return false;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        
        MovementState = ETrinityMovement.ETM_Grounded;

    }

    public override void PreUpdateBehaviour(float dt)
    {
    }
    
    
    public override void UpdateBehaviour(float dt)
    {
        
        HandleMovement();
        HandleJump();
        HandleFalling();
        
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
        return false;
    }

    public override void OnExit()
    {
    }

    public override void FixedUpdate()
    {
    }

    public ETrinityMovement GetMovementState()
    {
        return MovementState;
    }

    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * MoveSpeed;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * StrafeSpeed;
        Controller.MoveDirection = Vector3.zero;
        Controller.MoveDirection += moveZ;
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
                StateMachine.Animator.SetBool(AnimKeyJump, true);
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
            RaycastHit hit;
            if (MovementState != ETrinityMovement.ETM_Jumping) 
            {
                if (Physics.Raycast(Controller.transform.position, Vector3.down, out hit, Controller.GroundDistance, Controller.GroundLayer))
                {
                    // Ground detected, ensure movement state remains grounded
                    SetMovementState(ETrinityMovement.ETM_Grounded);
                    Controller.VerticalVelocity = 0f;
                    StateMachine.Animator.SetBool(AnimKeyJump, false);
                }
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

        StateMachine.Animator.SetFloat(AnimKeyMove, Controller.Rigidbody.velocity.z, .05f, Time.deltaTime);
        StateMachine.Animator.SetFloat(AnimKeyStrafe, Controller.Rigidbody.velocity.x, .05f, Time.deltaTime);
        StateMachine.Animator.SetFloat(AnimKeyVertical, Controller.VerticalVelocity);
    }

}