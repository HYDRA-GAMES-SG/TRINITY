using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InvincibleBossState : MonoBehaviour, IState
{
    protected AInvincibleBossFSM InvincibleBossFSM;
    public float RotateSpeed;
    public void Awake()
    {
    }

    public virtual bool CheckEnterTransition(IState fromState)
    {
        return false;
    }

    public virtual bool CheckExitTransition(IState fromState)
    {
        return false;
    }

    public virtual void EnterBehaviour(float dt, IState fromState)
    {
    }

    public virtual void ExitBehaviour(float dt, IState toState)
    {
    }

    public virtual void PostUpdateBehaviour(float dt)
    {
    }

    public virtual void PreUpdateBehaviour(float dt)
    {
    }

    public virtual void UpdateBehaviour(float dt)
    {
    }

    public virtual void SetStateMachine(AInvincibleBossFSM invincibleBossFSM)
    {
        InvincibleBossFSM = invincibleBossFSM;
    }
}
