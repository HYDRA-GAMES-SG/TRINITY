using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatRegen : RatState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return !(fromState is Death);
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {

    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        if (RatFSM.PlayerController == null)
        {
            return;
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
}
