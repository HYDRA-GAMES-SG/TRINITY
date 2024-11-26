using System;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityFSM : MonoBehaviour, IFSM
{
    [Tooltip("The state used to start the state machine.")]
    public TrinityState InitialState;

    private readonly Dictionary<string, TrinityState> states = new Dictionary<string, TrinityState>();
    private Queue<TrinityState> transitionQueue = new Queue<TrinityState>();

    public TrinityState CurrentState { get; private set; }
    public TrinityState PreviousState { get; private set; }

    public event Action<TrinityState, TrinityState> OnStateChange;
    public ATrinityController Controller;
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
        if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState))
            return;

        PreviousState = CurrentState;
        CurrentState.ExitBehaviour(Time.deltaTime, nextState);

        OnStateChange?.Invoke(PreviousState, nextState);

        
        
        Debug.Log("PLAYER: "  + CurrentState + "=>" + nextState);
        
        CurrentState = nextState;
        Animator.runtimeAnimatorController = CurrentState.StateAnimController;
        CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
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
            }
            else
            {
                Debug.LogWarning($"FSM: Duplicate state '{stateName}' found in {state.gameObject.name}. Skipping...");
            }
        }
    }
}

