using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class TrinityState : MonoBehaviour, IState
{
    public AnimatorController StateAnimController;
    protected ATrinityFSM StateMachine;
    [HideInInspector]
    public ATrinityController Controller;
    [HideInInspector]
    public APlayerInput InputReference;
    
    public void Awake()
    {
        

    }
    
    public void Start()
    {
        
    }

    public void Update()
    {
        if (Controller == null && StateMachine.Controller != null)
        {
            Controller = StateMachine.Controller;
        }

        if (InputReference == null && StateMachine.InputReference != null)
        {
            InputReference = StateMachine.InputReference;
        }
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

    public virtual bool CheckExitTransition(IState toState)
    {
        return false;
    }

    
    public virtual void ExitBehaviour(float dt, IState toState)
    {
        
    }
    
    public virtual void OnExit()
    {
        
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void SetStateMachine(ATrinityFSM aTrinityStateMachine)
    {
        StateMachine = aTrinityStateMachine;
    }
}