using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMovement : TrinityState
{
    private bool bStunned => Brain.GetAction() == ETrinityAction.ETA_Stunned;
    [SerializeField] private float DesiredVelocityControlModifier = .5f;
    [SerializeField] private Vector3 DesiredVelocity = Vector3.zero;
    [SerializeField] private float AirMoveSpeed = 5f;
    [SerializeField] private float AirStrafeSpeed = 5f;
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
        HandleFalling();
        
        
        DesiredVelocity = new Vector3(Controller.MoveDirection.x, Controller.VerticalVelocity, Controller.MoveDirection.z);
        Vector3 externalVelocity = Controller.RB.velocity;
        Controller.RB.velocity = Vector3.Lerp(externalVelocity, DesiredVelocity, DesiredVelocityControlModifier);
    }

    private void HandleFalling()
    {
            Controller.VerticalVelocity -= Controller.Gravity * GravityModifier * Time.deltaTime;
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
        Vector3 moveZ = Controller.Forward * InputReference.MoveInput.y * AirMoveSpeed;
        Vector3 moveX = Controller.Right * InputReference.MoveInput.x * AirStrafeSpeed;
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