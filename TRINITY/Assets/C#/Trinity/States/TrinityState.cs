using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class TrinityState : MonoBehaviour, IState
{
    public RuntimeAnimatorController StateAnimController;
    protected ATrinityFSM TrinityFSM;
    
    [HideInInspector]
    public ATrinityController Controller;
    [HideInInspector]
    public APlayerInput InputReference;
    [HideInInspector] 
    public ATrinityBrain Brain;
    
    public void Awake()
    {
        

    }

    public void Update()
    {
        if (Controller == null && TrinityFSM.Controller != null)
        {
            Controller = TrinityFSM.Controller;
        }

        if (InputReference == null && TrinityFSM.InputReference != null)
        {
            InputReference = TrinityFSM.InputReference;
        }
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