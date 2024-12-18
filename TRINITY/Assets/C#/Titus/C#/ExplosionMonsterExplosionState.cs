using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

public class ExplosionMonsterExplosionState : ExplosionMonsterState
{
    NavMeshAgent AI;
    ExplosionMonsterController EMController;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override bool CheckExitTransition(IState fromState)
    {
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        EMController = explosionMonsterFSM.explosionMonsterController;
        EMController.ExplodeTimer = EMController.ExplodeDelay;
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        EMController.ExplodeTimer -= Time.deltaTime;
        if (EMController.ExplodeTimer < 0)
        {
            if (EMController.bExploded)
            {
                return;
            }

            EMController.bExploded = true;

            ParticleSystem ExplosionInstance = Instantiate(EMController.ExplosionEffect, EMController.rb.transform.position, Quaternion.identity);
            ExplosionInstance.transform.localScale *= EMController.ExplosionSizeScale;

            ExplosionInstance.Play();

            if (EMController.rb != null)
            {
                EMController.rb.AddForce(transform.up * EMController.JumpForce, ForceMode.Force);
            }
            Destroy(EMController.ExplosionMonster, 0.3f);
        }
    }
}
