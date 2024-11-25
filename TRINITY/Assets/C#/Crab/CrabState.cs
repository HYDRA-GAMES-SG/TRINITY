using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class CrabState : MonoBehaviour, IState
{
    public AnimatorController StateAnimController;
    protected ACrabFSM StateMachine;

    public void Awake()
    {
    }

    public void Start()
    {

    }

    public void Update()
    {
    }

    public virtual void OnEnter()
    {
    }

    public virtual void CheckEnterTransition()
    {
    }


    public virtual void EnterBehaviour(float dt, IState fromState)
    {
        if (StateMachine)
        {
            StateMachine.Animator.runtimeAnimatorController = StateAnimController;
        }
    }

    public virtual void PreUpdateBehaviour(float dt)
    {
    }

    public virtual void UpdateBehaviour(float dt)
    {
    }

    public virtual void PostUpdateBehaviour(float dt)
    {

    }

    public virtual void ExitBehaviour()
    {
    }

    public virtual bool CheckEnterTransition(IState fromState)
    {
        return false;
    }

    public virtual void CheckExitTransition()
    {
    }

    public virtual void OnExit()
    {

    }

    public virtual void ExitBehaviour(float dt, IState toState)
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void SetStateMachine(ACrabFSM aCrabStateMachine)
    {
        StateMachine = aCrabStateMachine;
    }
}