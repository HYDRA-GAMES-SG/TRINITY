using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBIdle : InvincibleBossState
{
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.AI.ResetPath();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (InvincibleBossFSM.PlayerController != null)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
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
