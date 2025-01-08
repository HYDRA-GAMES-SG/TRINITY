using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBWalk : FinalMonsterBossState
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
        AI = FinalMonsterBossFSM.GetComponent<NavMeshAgent>();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        float randomValue = Random.Range(0f, 1f);

        FMBController.RotateTowardTarget(FMBController.transform.position,15f);
        FMBController.rb.AddForce(FMBController.rb.transform.forward, ForceMode.Force);

        if (FMBController.CalculateGroundDistance() <= FMBController.CloseAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBCloseAttack>();
        }
        else if (FMBController.CalculateGroundDistance() <= FMBController.LongAttackRange)
        {
            if (randomValue < 0.5f)
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBLongRangeAttack>();
            }
            else
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
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