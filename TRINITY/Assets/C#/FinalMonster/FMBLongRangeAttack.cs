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
        return (fromState is FMBIdle or FMBCloseAttack);
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
        FMBController.RotateTowardTarget(PlayerController.Position, 1f);
        if (FMBController.CalculateGroundDistance() > FMBController.LongAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBIdle>();
        }
        if (FMBController.CalculateGroundDistance() <= FMBController.CloseAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBCloseAttack>();
        }
        float randomValue = Random.Range(0f, 1f);

        if (FMBController.bPhase2)
        {
            if (randomValue <= 0.25f && FMBController.bInvokeSnowFall)
            {
                FMBController.Animator.SetTrigger(LRAtk1);
                FMBController.PlayParticleSystem(
                    "SnowFall",
                    FMBController.SnowFallPrefab,
                    FMBController.transform.position + FMBController.transform.up * 2,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "BlackHole",
                    FMBController.BlackHolePrefab,
                    FMBController.transform.position + FMBController.transform.up * 2,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            if (randomValue > 0.25f && randomValue <= 0.5f && FMBController.bInvokeSpike)
            {
                FMBController.Animator.SetTrigger(LRAtk2);
                FMBController.PlayParticleSystem(
                    "PilarFrost",
                    FMBController.PilarFrostPrefab,
                    FinalMonsterBossFSM.PlayerController.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "FrostAura",
                    FMBController.FrostAuraPrefab,
                    FinalMonsterBossFSM.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSpike = false;
            }
            if (randomValue > 0.5f && randomValue <= 0.75f && FMBController.bFrostRay)
            {
                FMBController.Animator.SetTrigger(LRAtk3);
                FMBController.PlayParticleSystem(
                    "FrostRay",
                    FMBController.FrostRayPrefab,
                    FMBController.transform.position + FMBController.transform.right,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "FrostAura",
                    FMBController.FrostAuraPrefab,
                    FinalMonsterBossFSM.transform.position,
                    FMBController.transform.rotation
                );
                FMBController.bFrostRay = false;
            }
            if (randomValue > 0.75f && FMBController.bFrostWave)
            {
                FMBController.Animator.SetTrigger(LRAtk4);
                FMBController.PlayParticleSystem(
                    "WaveFrost",
                    FMBController.WaveFrostPrefab,
                    -FMBController.transform.position + FMBController.transform.forward * 2,
                    FMBController.transform.rotation
                );
                FMBController.bFrostWave = false;
            }
        }
        else
        {
            if (randomValue <= 0.5f && FMBController.bInvokeSnowFall)
            {
                FMBController.Animator.SetTrigger(LRAtk1);
                FMBController.PlayParticleSystem(
                    "SnowFall",
                    FMBController.SnowFallPrefab,
                    FMBController.transform.position + FMBController.transform.up * 2,
                    FMBController.transform.rotation
                );
                FMBController.PlayParticleSystem(
                    "BlackHole",
                    FMBController.BlackHolePrefab,
                    FMBController.transform.position + FMBController.transform.up * 2,
                    FMBController.transform.rotation
                );
                FMBController.bInvokeSnowFall = false;
            }
            if (randomValue > 0.5f && FMBController.bInvokeSpike)
            {
                FMBController.Animator.SetTrigger(LRAtk2);
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
        return toState is FMBIdle or FMBCloseAttack or FMBDie;
    }
}