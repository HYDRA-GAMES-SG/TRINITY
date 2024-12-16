using System;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityFSM : MonoBehaviour, IFSM
{
    public bool ENABLE_DEBUG = false;
    [Tooltip("The state used to start the state machine.")]
    public TrinityState InitialState;

    private readonly Dictionary<string, TrinityState> states = new Dictionary<string, TrinityState>();
    private Queue<TrinityState> transitionQueue = new Queue<TrinityState>();

    public TrinityState CurrentState { get; private set; }
    public TrinityState PreviousState { get; private set; }

    public event Action<TrinityState, TrinityState> OnStateChange;
    public ATrinityController Controller;
    public ATrinityBrain Brain;
    public ATrinityCamera Camera;
    public APlayerInput InputReference;
    public Animator Animator;
    private bool FSM_RUNNING = false;

    private void Awake()
    {
        InitializeStates();
    }

    private void FixedUpdate()
    {
        if (!FSM_RUNNING)
        {
            if (InitialState == null || !InitialState.isActiveAndEnabled)
            {
                Debug.LogError("FSM: Initial state is null or inactive. State machine will not start.");
                enabled = false;
                return;
            }

            StartStateMachine();
        }

        // Check for transitions and update the current state
        ProcessTransitions();

        if (!Controller || !InputReference || !Camera)
        {
            print("FSM:Null Camera Controller or Input");
            return;
        }
        
        if (CurrentState != null)
        {
            CurrentState.PreUpdateBehaviour(Time.deltaTime);
            CurrentState.UpdateBehaviour(Time.deltaTime);
            CurrentState.PostUpdateBehaviour(Time.deltaTime);
        }
    }

    private void StartStateMachine()
    {
        FSM_RUNNING = true;
        CurrentState = InitialState;
        CurrentState.EnterBehaviour(Time.deltaTime, null);
        Debug.Log("PLAYER: " + CurrentState);
    }

    private void ProcessTransitions()
    {
        if (transitionQueue.Count > 0)
        {
            while (transitionQueue.Count > 0)
            {
                TrinityState nextState = transitionQueue.Dequeue();

                if (nextState != null && nextState.isActiveAndEnabled)
                {
                    TransitionToState(nextState);
                    break;
                }
            }
        }
    }

    private void TransitionToState(TrinityState nextState)
    {
        if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState) || !nextState.CheckEnterTransition(CurrentState))
            return;

        PreviousState = CurrentState;
        CurrentState.ExitBehaviour(Time.deltaTime, nextState);

        OnStateChange?.Invoke(PreviousState, nextState);





        if (ENABLE_DEBUG)
        {
            Debug.Log("PLAYER: "  + CurrentState + "=>" + nextState);
        }
        
        CurrentState = nextState;
        Animator.enabled = false;
        Animator.runtimeAnimatorController = CurrentState.StateAnimController;
        CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
        Animator.enabled = true;
        
    }

    public TrinityState GetState(string stateName)
    {
        states.TryGetValue(stateName, out TrinityState state);
        return state;
    }

    public T GetState<T>() where T : TrinityState
    {
        string stateName = typeof(T).Name;
        return GetState(stateName) as T;
    }

    public void EnqueueTransition(string stateName)
    {
        TrinityState state = GetState(stateName);
        if (state != null)
        {
            transitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransition<T>() where T : TrinityState
    {
        TrinityState state = GetState<T>();
        if (state != null)
        {
            transitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransitionToPreviousState()
    {
        if (PreviousState != null)
        {
            transitionQueue.Enqueue(PreviousState);
        }
    }

    private void InitializeStates()
    {
        TrinityState[] statesArray = GetComponents<TrinityState>();

        foreach (var state in statesArray)
        {
            string stateName = state.GetType().Name;

            if (!states.ContainsKey(stateName))
            {
                states.Add(stateName, state);
                state.SetStateMachine(this);
                state.Controller = Controller;
                state.InputReference = InputReference;
                state.Brain = Brain;
            }
            else
            {
                Debug.LogWarning($"FSM: Duplicate state '{stateName}' found in {state.gameObject.name}. Skipping...");
            }
        }
    }


    public bool IsActionable()
    {

        return    !(Brain.GetAction() == ETrinityAction.ETA_Channeling
                || Brain.GetAction() == ETrinityAction.ETA_Stunned
                || Controller.HealthComponent.bDead);
    }
}

