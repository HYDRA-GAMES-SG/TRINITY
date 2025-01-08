using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBLongRangeAttack : FinalMonsterBossState
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

        if (FMBController.bPhase2)
        {
            if (randomValue < 0.25f && FMBController.bInvokeSnowFall)
            {
                FMBController.PlayParticleSystem(
                    "SnowFall",
                    FMBController.SnowFallPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            else if (randomValue < 0.5f && FMBController.bInvokeSpike)
            {
                FMBController.PlayParticleSystem(
                    "PilarFrost",
                    FMBController.PilarFrostPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSpike = false;
            }
            else if (randomValue < 0.75f && FMBController.bFrostRay)
            {
                FMBController.PlayParticleSystem(
                    "FrostRay",
                    FMBController.FrostRayPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bFrostRay = false;
            }
            else if (randomValue < 1.0f && FMBController.bFrostWave)
            {
                FMBController.PlayParticleSystem(
                    "WaveFrost",
                    FMBController.WaveFrostPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bFrostWave = false;
            }
        }
        else
        {
            if (randomValue < 0.5f && FMBController.bInvokeSnowFall)
            {
                FMBController.PlayParticleSystem(
                    "SnowFall",
                    FMBController.SnowFallPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            else if (randomValue < 1.0f && FMBController.bInvokeSpike)
            {
                FMBController.PlayParticleSystem(
                    "PilarFrost",
                    FMBController.PilarFrostPrefab,
                    FMBController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSpike = false;
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