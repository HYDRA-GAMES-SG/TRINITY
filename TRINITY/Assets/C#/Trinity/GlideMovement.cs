using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : TrinityState
{
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float StrafeSpeed = 5f;
    [HideInInspector]
    public bool bMirror = false;
    [HideInInspector]
    public string AnimKeyMirror = "bMirror";

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

        //Controller.HealthComponent.OnDeath += HandleDeath;
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
        
        HandleMovement();

        Controller.RB.velocity = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        //Controller.HealthComponent.OnDeath -= HandleDeath;

    }


    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    
    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * MoveSpeed;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * StrafeSpeed;
        Controller.MoveDirection = Vector3.zero;
        Controller.MoveDirection += moveZ;
        Controller.MoveDirection += moveX;
    }
    
    
    // private void HandleDeath()
    // {
    //     if (Controller.CheckGround().transform)
    //     {
    //         TrinityFSM.Animator.SetBool(AnimKeyDeath, true);
    //     }
    //     else
    //     {
    //         Controller.EnableRagdoll();
    //     }
    // }
    
    
}