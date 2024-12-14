using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFlyingBossFSM : MonoBehaviour
{
    [Tooltip("The state used to start the state machine.")]
    public FlyingBossState InitialState;

    private readonly Dictionary<string, FlyingBossState> states = new Dictionary<string, FlyingBossState>();
    private Queue<FlyingBossState> TransitionQueue = new Queue<FlyingBossState>();

    public FlyingBossState CurrentState { get; private set; }
    public FlyingBossState PreviousState { get; private set; }

    public event Action<FlyingBossState, FlyingBossState> OnStateChange;
    public AFlyingBossController AFlyingBossController;
    public ATrinityController PlayerController;
    public Animator Animator;
    private bool FSM_RUNNING = false;

    private void Awake()
    {
        InitializeStates();
    }

    private void FixedUpdate()
    {
        TryInitialize();

        // Check for transitions and update the current state
        ProcessTransitions();

        if (CurrentState != null)
        {
            CurrentState.PreUpdateBehaviour(Time.deltaTime);
            CurrentState.UpdateBehaviour(Time.deltaTime);
            CurrentState.PostUpdateBehaviour(Time.deltaTime);
        }
    }

    private void TryInitialize()
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

        if (!PlayerController)
        {
            PlayerController = FindObjectOfType<ATrinityController>();

            if (!PlayerController)
            {
                Debug.LogError("AFlyingBossFSM: No Player Controller Reference found.");
                return;
            }
        }
    }

    private void StartStateMachine()
    {
        FSM_RUNNING = true;
        CurrentState = InitialState;
        CurrentState.EnterBehaviour(0f, null);
    }

    private void ProcessTransitions()
    {
        if (TransitionQueue.Count > 0)
        {
            while (TransitionQueue.Count > 0)
            {
                FlyingBossState nextState = TransitionQueue.Dequeue();

                if (nextState != null && nextState.isActiveAndEnabled)
                {
                    TransitionToState(nextState);
                    break;
                }
            }
        }
    }

    private void TransitionToState(FlyingBossState nextState)
    {
        if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState) || !nextState.CheckEnterTransition(CurrentState))
            return;

        PreviousState = CurrentState;
        CurrentState.ExitBehaviour(Time.deltaTime, nextState);

        OnStateChange?.Invoke(PreviousState, nextState);

        Debug.Log("AI: " + CurrentState + "=>" + nextState);

        CurrentState = nextState;
        CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
    }

    public FlyingBossState GetState(string stateName)
    {
        states.TryGetValue(stateName, out FlyingBossState state);
        return state;
    }

    public T GetState<T>() where T : FlyingBossState
    {
        string stateName = typeof(T).Name;
        return GetState(stateName) as T;
    }

    public void EnqueueTransition(string stateName)
    {
        FlyingBossState state = GetState(stateName);
        if (state != null)
        {
            TransitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransition<T>() where T : FlyingBossState
    {
        FlyingBossState state = GetState<T>();
        if (state != null)
        {
            TransitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransitionToPreviousState()
    {
        if (PreviousState != null)
        {
            TransitionQueue.Enqueue(PreviousState);
        }
    }

    private void InitializeStates()
    {
        FlyingBossState[] statesArray = GetComponents<FlyingBossState>();

        foreach (var state in statesArray)
        {
            string stateName = state.GetType().Name;

            if (!states.ContainsKey(stateName))
            {
                states.Add(stateName, state);
                state.SetStateMachine(this);
            }
            else
            {
                Debug.LogWarning($"FSM: Duplicate state '{stateName}' found in {state.gameObject.name}. Skipping...");
            }
        }
    }
}
