#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;

public class JumpSmash : CrabState
{
    [SerializeField, Range(0.10f, 1.00f)] float AnimationCheckExitTime;

    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue && CrabFSM.CrabController.CanJumpSmash && CrabFSM.CrabController.FacingTarget();
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;

        CrabFSM.CrabController.bCanChill = false;
        CrabFSM.CrabController.AI.enabled = false;
        CrabFSM.Animator.applyRootMotion = true;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        if (CrabFSM.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= AnimationCheckExitTime)
        {
            CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);
        }

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("3JumpSmashAttack_RM") && stateInfo.normalizedTime >= 0.95f)
        {
            AnimFinish=true;
            CrabFSM.EnqueueTransition<Pursue>();
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        CrabFSM.CrabController.CanJumpSmash = false;
        CrabFSM.CrabController.bCanChill = true;

    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Pursue || toState is Death || (toState is Idle && AnimFinish);
    }
}
