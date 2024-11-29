using UnityEngine;

public class RoarStun : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is PursueAttack)
        {
            if (CrabFSM.CrabController.CanRoarStun)
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
        Vector3 faceDirection =
            (CrabFSM.PlayerController.transform.position
            - CrabFSM.CrabController.transform.position).normalized;
        CrabFSM.CrabController.transform.rotation = Quaternion.LookRotation(faceDirection);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roar") && stateInfo.normalizedTime >= 1f)
        {
            CrabFSM.EnqueueTransition<PursueAttack>();
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        CrabFSM.CrabController.CanRoarStun = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is PursueAttack)
        {
            return true;
        }
        return false;
    }
}
