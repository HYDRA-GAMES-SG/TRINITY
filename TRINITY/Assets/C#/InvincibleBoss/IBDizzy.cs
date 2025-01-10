using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBDizzy : InvincibleBossState
{
    [SerializeField] float DizzyTime;
    float Timer = 0f;

    AInvincibleBossController IBController;
    UHealthComponent Heath;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        IBController = InvincibleBossFSM.InvincibleBossController;
        Heath = InvincibleBossFSM.InvincibleBossController.Health;

        InvincibleBossFSM.InvincibleBossController.AI.ResetPath();
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
        Heath.Current = Heath.MAX;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
