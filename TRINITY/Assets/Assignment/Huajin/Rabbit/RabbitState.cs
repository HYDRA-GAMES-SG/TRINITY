using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RabbitState : MonoBehaviour, IState
{
    protected ARabbitFSM RabbitFSM;
    public float RotateSpeed;
    public abstract void Awake();
    public abstract bool CheckEnterTransition(IState fromState);
    public abstract bool CheckExitTransition(IState fromState);
    public abstract void EnterBehaviour(float dt, IState fromState);
    public abstract void ExitBehaviour(float dt, IState toState);
    public abstract void PostUpdateBehaviour(float dt);
    public abstract void PreUpdateBehaviour(float dt);
    public abstract void UpdateBehaviour(float dt);
    public virtual void SetStateMachine(ARabbitFSM rabbitFSM)
    {
        RabbitFSM = rabbitFSM;
    }
}
