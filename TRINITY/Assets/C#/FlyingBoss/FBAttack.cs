using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        FBController.RotateTowardTarget(FlyingBossFSM.PlayerController.transform.position);
        if (FBController.bCanElectricChargeAttack)
        {
            Quaternion effectRotation = Quaternion.LookRotation(FBController.transform.forward) * Quaternion.Euler(0,90,0);
            Vector3 effectPosition = FBController.transform.position + FBController.transform.forward;
            FBController.Animator.SetTrigger(ElectricAttack);
            if (!FBController.bGOElectricChargeSpawned)
            {
                Instantiate(FBController.GOElectricCharge, effectPosition, effectRotation);
                FBController.bGOElectricChargeSpawned = true;
                ElectricChargeAttack();
            }
            else
            {
                ElectricChargeAttack();
            }
        }
        else if (FBController.bCanSpikeAttack)
        {
            Quaternion effectRotation = Quaternion.LookRotation(FBController.transform.forward) * Quaternion.Euler(0, 90, 0);
            Vector3 effectPosition = FBController.transform.position + FBController.transform.forward;
            FBController.Animator.SetTrigger(SpikeAttack);
            if (!FBController.bGOElectricShotSpawned)
            {
                Instantiate(FBController.ElectricShot, effectPosition, effectRotation);
                FBController.bGOElectricShotSpawned = true;
                ElectricShotSpikeAttack();
            }
            else
            {
                ElectricShotSpikeAttack();
            }
        }
        else if (FBController.CalculateDistance() >= FBController.LongAttackRange || FBController.CalculateDistance() <= FBController.HoverRange)
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
    }
    private void ElectricShotSpikeAttack()
    {
        FBController.ElectricShot.Play();
        FBController.bCanSpikeAttack = false;
        FlyingBossFSM.EnqueueTransition<FBHover>();
    }
    private void ElectricChargeAttack()
    {
        FBController.ElectricCharge.Play();
        FBController.bCanElectricChargeAttack = false;
        FlyingBossFSM.EnqueueTransition<FBHover>();
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
        return true;
    }
}
