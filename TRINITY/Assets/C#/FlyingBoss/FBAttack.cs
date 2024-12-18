using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FBAttack : FlyingBossState
{
    AFlyingBossController FBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    public string SpikeAttack = "FlyClawsAttack_RM";
    public string ElectricAttack = "FlyElectroShot";
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FBController = FlyingBossFSM.FlyingBossController;
        PlayerController = FlyingBossFSM.PlayerController;

        AI = FBController.AI;

        if (FBController.bCanElectricChareAttacked)
        {
            FBController.Animator.SetTrigger(ElectricAttack);
            FBController.ElectricCharge.Play();
            FBController.bCanSpikeAttacked = false;
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
        else if (FBController.bCanSpikeAttacked)
        {
            FBController.Animator.SetTrigger(SpikeAttack);
            FBController.ElectricShot.Play();
            FBController.bCanSpikeAttacked = false;
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
        else if (FBController.CalculateDistance() >= FBController.LongAttackRange && FBController.CalculateDistance() <= FBController.HoverRange)
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
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
        FBController.ElectricCharge.Stop();
        FBController.ElectricShot.Stop();
    }

    public override bool CheckExitTransition(IState toState)
    {
        return false;
    }
}
