using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBDie : FlyingBossState
{
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] ParticleSystem ElectricParticle;
    [SerializeField] GameObject shieldRope;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FlyingBossFSM.FlyingBossController.ActivateRagdoll();
        ElectricParticle.Stop();
        Collider.enabled = true;
        FlyingBossFSM.FlyingBossController.RB.useGravity = true;
        shieldRope.SetActive(false);
        FlyingBossFSM.FlyingBossController.FlyingAudio.PlayDeath();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (Physics.Raycast(FlyingBossFSM.FlyingBossController.transform.position, Vector3.down, 0.3f, GroundLayer, QueryTriggerInteraction.Ignore))
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
