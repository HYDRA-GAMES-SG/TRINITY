using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBDead : InvincibleBossState
{
    [SerializeField] ParticleSystem LightningEffect;
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.EnemyStatus.Health.Current <= 0;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {

        InvincibleBossFSM.InvincibleBossController.AI.ResetPath();
        LightningEffect.Stop();
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
        
    }

    public override bool CheckExitTransition(IState toState)
    {
        return false;
    }
}
