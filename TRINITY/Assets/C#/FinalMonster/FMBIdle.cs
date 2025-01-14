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

        if (FMBController.rb != null)
        {
            FMBController.rb.isKinematic = true;
        }
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {

        float distance = FMBController.CalculateGroundDistance();


        if (distance <= FMBController.LongAttackRange && distance > FMBController.CloseAttackRange)
        {
            float randomValue = Random.Range(0f, 1f);
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
        return true;
    }
}
