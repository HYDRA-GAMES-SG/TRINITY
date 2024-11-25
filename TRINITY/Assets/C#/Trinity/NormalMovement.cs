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
    [SerializeField, ReadOnly] private ETrinityMovement MovementState;
    [SerializeField]
    private float MoveSpeed = 100f;
    [SerializeField]
    private float StrafeSpeed = 100f;

    private string moveAnim = "vForward";
    private string strafeAnim = "vStrafe";
    
    public override void CheckEnterTransition()
    {
        //base.CheckEnterTransition();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // Custom behavior when entering the state
        Debug.Log("Entering Normal Movement State");
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        //DO NOT DELETE
        base.EnterBehaviour(dt, fromState); //sets animator controller
        //DO NOT DELETE
        MovementState = ETrinityMovement.ETM_Grounded;

    }

    public override void PreUpdateBehaviour(float dt)
    {
        //base.PreUpdateBehaviour(dt);
    }
    
    
    public override void UpdateBehaviour(float dt)
    {
        //base.UpdateBehaviour(dt);
        if (!Controller || !InputReference)
        {
            return;
        }
        
        
        Controller.Forward = transform.forward;
        Controller.Right = transform.right;

        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * MoveSpeed;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * StrafeSpeed;
        Controller.MoveDirection = Vector3.zero;
        Controller.MoveDirection += moveZ;
        Controller.MoveDirection += moveX;

        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            Controller.VerticalVelocity -= Controller.Gravity * Time.deltaTime;
        }

        // Apply movement to Rigidbody
        Controller.Rigidbody.velocity = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);

        StateMachine.Animator.SetFloat(moveAnim, Controller.Rigidbody.velocity.z);
        StateMachine.Animator.SetFloat(strafeAnim, Controller.Rigidbody.velocity.x);

    }


    public override void PostUpdateBehaviour(float dt)
    {
        //base.PostUpdateBehaviour(dt);
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        //base.ExitBehaviour(dt, toState);
    }

    public override void CheckExitTransition()
    {
        //base.CheckExitTransition();
    }

    public override void OnExit()
    {
        //base.OnExit();
    }

    public override void FixedUpdate()
    {
        //base.FixedUpdate();
    }

    public override void SetStateMachine(ATrinityFSM aTrinityStateMachine)
    {
        //DO NOT DELETE
        base.SetStateMachine(aTrinityStateMachine); // Set state machine variable
        //DO NOT DELETE

    }
}