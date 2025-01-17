using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GetHit : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return (fromState is Pursue || fromState is NormalAttack) && CrabFSM.CrabController.CanGetHit;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
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
        CrabFSM.CrabController.CanGetHit = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Death || toState is Pursue|| toState is IcePhaseRoar;
    }
}
