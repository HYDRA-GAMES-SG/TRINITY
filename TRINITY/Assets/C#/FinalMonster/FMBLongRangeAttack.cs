using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBLongRangeAttack : FinalMonsterBossState
{
    [SerializeField] string LRAtk1 = "InvokeSnowFall";
    [SerializeField] string LRAtk2 = "InvokeSpike";
    [SerializeField] string LRAtk3 = "FrostRay";
    [SerializeField] string LRAtk4 = "FrostWave";

    AFinalMonsterController FMBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    public override bool CheckEnterTransition(IState fromState)
    {
        return (fromState is FMBIdle or FMBWalk) &&
           FMBController.CalculateGroundDistance() <= FMBController.LongAttackRange;
        /*return (fromState is FMBWalk && !FMBController.bInvokeSnowFall) ||
            (fromState is FMBWalk && !FMBController.bInvokeSpike) ||
            (fromState is FMBWalk && !FMBController.bFrostRay) ||
            (fromState is FMBWalk && !FMBController.bFrostWave);*/
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
        if (FMBController.CalculateGroundDistance() > FMBController.LongAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
        }

        float randomValue = Random.Range(0f, 1f);

        if (FMBController.bPhase2)
        {
            if (randomValue < 0.25f && FMBController.bInvokeSnowFall)
            {
                FMBController.PlayParticleSystem(
                    "SnowFall",
                    FMBController.SnowFallPrefab,
                    FMBController.transform.up,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "BlackHole",
                    FMBController.BlackHolePrefab,
                    FMBController.transform.up,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            else if (randomValue < 0.5f && FMBController.bInvokeSpike)
            {
                FMBController.PlayParticleSystem(
                    "PilarFrost",
                    FMBController.PilarFrostPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "FrostAura",
                    FMBController.FrostAuraPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSpike = false;
            }
            else if (randomValue < 0.75f && FMBController.bFrostRay)
            {
                FMBController.PlayParticleSystem(
                    "FrostRay",
                    FMBController.FrostRayPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "FrostAura",
                    FMBController.FrostAuraPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bFrostRay = false;
            }
            else if (randomValue < 1.0f && FMBController.bFrostWave)
            {
                FMBController.PlayParticleSystem(
                    "WaveFrost",
                    FMBController.WaveFrostPrefab,
                    FMBController.transform.forward,
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
                    FMBController.transform.up,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "BlackHole",
                    FMBController.BlackHolePrefab,
                    FMBController.transform.up,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            else if (randomValue < 1.0f && FMBController.bInvokeSpike)
            {
                FMBController.PlayParticleSystem(
                    "PilarFrost",
                    FMBController.PilarFrostPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "FrostAura",
                    FMBController.FrostAuraPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
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
        StopAllParticleSystems(FMBController.gameObject);
    }
    private void StopAllParticleSystems(GameObject target)
    {
        ParticleSystem[] particleSystems = target.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
            ps.Clear();
        }
    }
    public override bool CheckExitTransition(IState toState)
    {
        return toState is FMBIdle;
    }
}