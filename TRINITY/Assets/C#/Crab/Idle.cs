using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD;
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
        if (CrabFSM.PlayerController != null && ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD)
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
        return ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD;
    }
}
