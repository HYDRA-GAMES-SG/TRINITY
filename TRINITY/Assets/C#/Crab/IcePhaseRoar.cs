using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePhaseRoar : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue || fromState is NormalAttack || fromState is GetHit;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabFSM.CrabController.bCanChill = false;


        CrabFSM.CrabController.bElementPhase = true;
        CrabFSM.CrabController.AI.ResetPath();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roar2") && stateInfo.normalizedTime >= 0.95f)
        {
            Debug.Log("Straight go pursue");
            CrabFSM.EnqueueTransition<Pursue>();
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.CrabController.bCanChill = true;

    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
