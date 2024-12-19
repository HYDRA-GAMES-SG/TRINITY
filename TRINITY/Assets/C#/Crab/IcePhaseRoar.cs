using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePhaseRoar : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue || fromState is NormalAttack;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabFSM.CrabController.bElementPhase = true;
        CrabFSM.CrabController.AI.ResetPath();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        if (CrabFSM.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f)
        {
            CrabFSM.EnqueueTransition<Pursue>();
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
        return true;
    }

    private void RotateTowardTarget(Vector3 directionToTarget)
    {
    }
}
