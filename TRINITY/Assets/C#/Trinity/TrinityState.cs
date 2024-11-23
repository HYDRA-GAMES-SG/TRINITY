using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TrinityState : MonoBehaviour, IState
{
    public AnimatorController Animation;
    protected ATrinityFSM StateMachine;
    
    // Implement the interface methods
    public void Start()
    {
    }

    public void Update()
    {
    }

    public void UpdateBehaviour()
    {
        
    }

    public void Initialize(IFSM stateMachine, Animator animator)
    {
    }

    void IState.Awake()
    {
    }

    public void OnEnter()
    {
    }

    public void CheckEnterTransition()
    {
    }

    public void EnterBehaviour()
    {
    }

    public void PreUpdateBehaviour()
    {
    }

    public void PostUpdateBehaviour()
    {
    }

    public void ExitBehaviour()
    {
    }

    public void CheckExitTransition()
    {
    }

    public void OnExit()
    {
    }

    public void FixedUpdate()
    {
    }

    public void SetStateMachine(ATrinityFSM aTrinityStateMachine)
    {
    }
}