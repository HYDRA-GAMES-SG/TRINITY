using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionMonsterIdleState : ExplosionMonsterState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override bool CheckExitTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (explosionMonsterFSM.explosionMonsterController.CalculateDistance() <= explosionMonsterFSM.explosionMonsterController.PursueRange)
        {
            explosionMonsterFSM.EnqueueTransition<ExplosionMonsterPursueState>();
        }
    }
}
