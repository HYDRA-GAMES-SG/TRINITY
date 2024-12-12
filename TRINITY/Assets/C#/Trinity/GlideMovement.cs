using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : TrinityState
{
    private bool bStunned => Brain.GetAction() == ETrinityAction.ETA_Stunned;
    
    [SerializeField] private float AirMoveAcceleration = 10f;
    [SerializeField] private float AirStrafeAcceleration = 10f;
    [SerializeField] private float AirMaxMoveSpeed = 5f;
    [SerializeField] private float AirMaxStrafeSpeed = 5f;
    [SerializeField] private float GravityModifier = .5f;
    [HideInInspector]
    public bool bMirror = false;
    [HideInInspector]
    public string AnimKeyMirror = "bMirror";
    [HideInInspector]
    public string AnimKeyDeath = "bDeath";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        
        if (fromState is NormalMovement)
        {
            TrinityFSM.Animator.SetBool(AnimKeyMirror, !bMirror);
        }

    }

    public override void PreUpdateBehaviour(float dt)
    {
        
    }
    
    
    public override void UpdateBehaviour(float dt)
    {

        // if (Controller.HealthComponent.bDead)
        // {
        //     return;
        // }
        
        if (Controller.CheckGround().transform || !TrinityFSM.InputReference.JumpInput)
        {
            TrinityFSM.EnqueueTransition<NormalMovement>();
            return;
        }

        if (bStunned)
        {
            return;
        }
        
        HandleMovement();
        HandleGravity();

        Vector3 moveVec = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
        Controller.RB.AddForce(moveVec * Controller.RB.mass);
    }

    private void HandleGravity()
    {
        Controller.RB.AddForce(-Controller.Up * Controller.Gravity * GravityModifier * Controller.RB.mass);
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        Controller.HealthComponent.OnDeath -= HandleDeath;

    }


    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    
    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * AirMoveAcceleration;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * AirStrafeAcceleration;
        Controller.MoveDirection = Vector3.zero;
        Controller.MoveDirection += moveZ;
        Controller.MoveDirection += moveX;
    }
    
    
    private void HandleDeath()
    {
        // if (Controller.CheckGround().transform)
        // {
            TrinityFSM.Animator.SetBool(AnimKeyDeath, true);
        // }
        // else
        // {
        //     Controller.EnableRagdoll();
        // }
    }
    
    
}