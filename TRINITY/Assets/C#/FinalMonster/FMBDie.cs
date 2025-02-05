using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBDie : FinalMonsterBossState
{
    AFinalMonsterController FMBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;
    private Color originalColor = new Color(0x00 / 255f, 0x2D / 255f, 0x24 / 255f);

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
        FinalMonsterBossFSM.Animator.SetTrigger("Die");
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        StopAllParticleSystems(FMBController.gameObject);
        
        Renderer renderer = FMBController.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
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
        return false;
    }
}