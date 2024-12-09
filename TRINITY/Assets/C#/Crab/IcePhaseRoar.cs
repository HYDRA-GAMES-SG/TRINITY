using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePhaseRoar : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
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

    private void RotateTowardTarget(Vector3 directionToTarget)
    {
    }
}
