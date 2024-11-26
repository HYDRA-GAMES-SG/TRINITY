using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class CrabState : MonoBehaviour, IState
{
    public AnimatorController StateAnimController;
    protected ACrabFSM CrabFSM;

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

    public virtual bool CheckEnterTransition(IState fromState)
    {
        return false;
    }
    

    public virtual void EnterBehaviour(float dt, IState fromState)
    {
        if (CrabFSM)
        {
            CrabFSM.Animator.runtimeAnimatorController = StateAnimController;
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

    public virtual bool CheckExitTransition(IState toState)
    {
        return false;
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
        CrabFSM = aCrabStateMachine;
    }
}