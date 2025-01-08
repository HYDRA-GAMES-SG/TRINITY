using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBCloseAttack : FinalMonsterBossState
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

        FMBController.RotateTowardTarget(FMBController.transform.position, 15f);

        if (randomValue < (1f / 3f) && FMBController.bHeavyAttack)
        {
            FMBController.PlayParticleSystem(
                "HeavyAttack",
                FMBController.FrozenPulsePrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FMBController.bHeavyAttack = false;
        }
        else if (randomValue < (2f / 3f) && FMBController.bDash)
        {
            FMBController.PlayParticleSystem(
                "Dash",
                FMBController.WaveFrostCastPrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FMBController.bDash = false;
        }
        else if (randomValue < 1f && FMBController.bHitTheGround)
        {
            FMBController.PlayParticleSystem(
                "HitTheGround",
                FMBController.OrbFrostPrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FMBController.bHitTheGround = false;
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
