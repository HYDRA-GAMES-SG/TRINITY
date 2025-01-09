using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class TrinityState : MonoBehaviour, IState
{
    public RuntimeAnimatorController StateAnimController;
    protected ATrinityFSM TrinityFSM;
    
    public void Awake()
    {
        

    }

    public void Update()
    {

    }
    public virtual bool CheckEnterTransition(IState fromState)
    {
        return false;
    }
    
    public virtual void EnterBehaviour(float dt, IState fromState)
    {
        
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
    
    public virtual void SetStateMachine(ATrinityFSM aTrinityStateMachine)
    {
        TrinityFSM = aTrinityStateMachine;
    }
}