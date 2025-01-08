using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

public class FMBIdle : FinalMonsterBossState
{
    AFinalMonsterController FMBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FMBController = FinalMonsterBossFSM.FinalMonsterBossController;
        PlayerController = FinalMonsterBossFSM.PlayerController;

        AI = FMBController.AI;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        float randomValue = Random.Range(0f, 1f);

        if (FinalMonsterBossFSM.FinalMonsterBossController.CalculateGroundDistance() <= FinalMonsterBossFSM.FinalMonsterBossController.LongAttackRange)
        {
            if (randomValue < 0.5f)
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
            }
            else
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBLongRangeAttack>();
            }
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
