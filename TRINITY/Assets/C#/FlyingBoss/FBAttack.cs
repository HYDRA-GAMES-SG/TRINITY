using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBAttack : FlyingBossState
{
    public string SpikeAttack = "FlyClawsAttack_RM";
    public string ElectricAttack = "FlyElectroShot";

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
       
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        FlyingBossFSM.FlyingBossController.RotateTowardTarget(FlyingBossFSM.PlayerController.transform.position, RotateSpeed);

       
            Quaternion effectRotation = Quaternion.LookRotation(FlyingBossFSM.FlyingBossController.transform.forward) * Quaternion.Euler(0, 90, 0);
            Vector3 effectPosition = FlyingBossFSM.FlyingBossController.transform.position + FlyingBossFSM.FlyingBossController.transform.forward;
            FlyingBossFSM.FlyingBossController.Animator.SetTrigger(ElectricAttack);
            if (!FlyingBossFSM.FlyingBossController.bGOElectricChargeSpawned)
            {
                Instantiate(FlyingBossFSM.FlyingBossController.GOElectricCharge, effectPosition, effectRotation);
                FlyingBossFSM.FlyingBossController.bGOElectricChargeSpawned = true;
                ElectricChargeAttack();
            }
            else
            {
                ElectricChargeAttack();
            }
        
        //else if (FlyingBossFSM.FlyingBossController.bCanSpikeAttack)
        //{
        //    Quaternion effectRotation = Quaternion.LookRotation(FlyingBossFSM.FlyingBossController.transform.forward) * Quaternion.Euler(0, 90, 0);
        //    Vector3 effectPosition = FlyingBossFSM.FlyingBossController.transform.position + FlyingBossFSM.FlyingBossController.transform.forward;
        //    FlyingBossFSM.FlyingBossController.Animator.SetTrigger(SpikeAttack);
        //    if (!FlyingBossFSM.FlyingBossController.bGOElectricShotSpawned)
        //    {
        //        Instantiate(FlyingBossFSM.FlyingBossController.ElectricShot, effectPosition, effectRotation);
        //        FlyingBossFSM.FlyingBossController.bGOElectricShotSpawned = true;
        //        ElectricShotSpikeAttack();
        //    }
        //    else
        //    {
        //        ElectricShotSpikeAttack();
        //    }
        //}
        //else if (FlyingBossFSM.FlyingBossController.CalculateDistance() >= FlyingBossFSM.FlyingBossController.LongAttackRange || FlyingBossFSM.FlyingBossController.CalculateDistance() <= FlyingBossFSM.FlyingBossController.HoverRange)
        //{
        //    FlyingBossFSM.EnqueueTransition<FBHover>();
        //}
    }
    private void ElectricShotSpikeAttack()
    {
        FlyingBossFSM.FlyingBossController.ElectricShot.Play();
        FlyingBossFSM.FlyingBossController.bCanSpikeAttack = false;
        FlyingBossFSM.EnqueueTransition<FBHover>();
    }
    private void ElectricChargeAttack()
    {
        FlyingBossFSM.FlyingBossController.ElectricCharge.Play();
        FlyingBossFSM.FlyingBossController.bCanElectricChargeAttack = false;
        FlyingBossFSM.EnqueueTransition<FBHover>();
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        FlyingBossFSM.FlyingBossController.ElectricCharge.Stop();
        FlyingBossFSM.FlyingBossController.ElectricShot.Stop();

        //FlyingBossFSM.FlyingBossController.bCanElectricChargeAttack = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
