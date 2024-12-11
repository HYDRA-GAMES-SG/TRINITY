using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : CrabState
{
    public Boss Graphics;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabFSM.CrabController.bCrabDie = true;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (CrabFSM.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
        {
            Graphics.ActivateRagdoll();
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
