using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBDie : FlyingBossState
{
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] LayerMask GroundLayer;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        Collider.enabled = true;
        FlyingBossFSM.FlyingBossController.RB.useGravity = true;
        Ragdoll rd = FlyingBossFSM.FlyingBossController.GetComponentInChildren<Ragdoll>();
        rd.ActivateRagdoll();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (Physics.Raycast(FlyingBossFSM.FlyingBossController.transform.position, Vector3.down, 0.3f, GroundLayer))
        {
            FlyingBossFSM.Animator.SetTrigger("OnGround");
        }
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
}
