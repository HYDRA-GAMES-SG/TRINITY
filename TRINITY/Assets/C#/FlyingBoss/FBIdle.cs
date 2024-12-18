using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FBIdle : FlyingBossState
{
    AFlyingBossController FBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FBController = FlyingBossFSM.FlyingBossController;
        PlayerController = FlyingBossFSM.PlayerController;

        AI = FBController.AI;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (FlyingBossFSM.FlyingBossController.CalculateDistance() <= FlyingBossFSM.FlyingBossController.HoverRange)
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
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
