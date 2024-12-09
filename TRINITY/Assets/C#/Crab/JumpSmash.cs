using UnityEditor;
using UnityEngine;

public class JumpSmash : CrabState
{
    [SerializeField, Range(0.10f, 1.00f)] float AnimationCheckExitTime;


    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is Pursue)
        {
            if (CrabFSM.CrabController.CanJumpSmash)
            {
                return true;
            }
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
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
            RotateTowardTarget(faceDirection);
        }

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("3JumpSmashAttack_RM") && stateInfo.normalizedTime >= 0.9f)
        {
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

    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }
    private void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, 10 * Time.deltaTime);
    }
}
