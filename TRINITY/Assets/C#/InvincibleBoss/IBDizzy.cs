using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBDizzy : InvincibleBossState
{
    [SerializeField] ParticleSystem LightningEffect;

    [SerializeField] float DizzyTime;
    float Timer = 0f;

    AInvincibleBossController IBController;
    UHealthComponent Heath;
    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.Health.Current <= 0;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        IBController = InvincibleBossFSM.InvincibleBossController;
        Heath = InvincibleBossFSM.InvincibleBossController.Health;

        InvincibleBossFSM.InvincibleBossController.AI.ResetPath();
        LightningEffect.Stop();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Timer += Time.deltaTime;

        if (Timer >= DizzyTime)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();

        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        Timer = 0f;
        Heath.Current = Heath.MAX;
        Heath.bDead = false;
        InvincibleBossFSM.InvincibleBossController.bIsDead = false;
        LightningEffect.Play();

    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
