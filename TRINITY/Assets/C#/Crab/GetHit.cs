using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHit : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is Pursue || fromState is NormalAttack)
        {
            if (CrabFSM.CrabController.CanGetHit)
            {
                return true;
            }
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if(CrabFSM.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
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
        if (toState is Death || toState is Pursue)
        {
            return true;
        }
        return false;
    }
}
