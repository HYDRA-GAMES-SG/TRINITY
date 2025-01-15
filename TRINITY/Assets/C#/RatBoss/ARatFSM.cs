using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARatFSM : MonoBehaviour, IFSM
{
    [HideInInspector]
    public ARatController RatController;
    [Tooltip("The state used to start the state machine.")]
    public RatState InitialState;

    private readonly Dictionary<string, RatState> states = new Dictionary<string, RatState>();
    private Queue<RatState> TransitionQueue = new Queue<RatState>();

    public RatState CurrentState { get; private set; }
    public RatState PreviousState { get; private set; }

    public event Action<RatState, RatState> OnStateChange;
    
    [HideInInspector]
    public ATrinityController PlayerController;
    public Animator Animator;
    private bool FSM_RUNNING = false;

    private void Awake()
    {
        RatController = transform.root.GetComponent<ARatController>();
        PlayerController = ATrinityGameManager.GetPlayerController();
        InitializeStates();
    }

    private void FixedUpdate()
    {
        if (PlayerController == null)
        {
            PlayerController = ATrinityGameManager.GetPlayerController();
        }
        
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
            PlayerController = ATrinityGameManager.GetPlayerController();

            if (!PlayerController)
            {
                Debug.LogError("RatFSM: No Player Controller Reference found.");
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
                RatState nextState = TransitionQueue.Dequeue();

                if (nextState != null && nextState.isActiveAndEnabled)
                {
                    TransitionToState(nextState);
                    break;
                }
            }
        }
    }

    private void TransitionToState(RatState nextState)
    {
        if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState) || !nextState.CheckEnterTransition(CurrentState))
            return;

        PreviousState = CurrentState;
        CurrentState.ExitBehaviour(Time.deltaTime, nextState);

        OnStateChange?.Invoke(PreviousState, nextState);

        //Debug.Log("AI: " + CurrentState + "=>" + nextState);

        CurrentState = nextState;
        Animator.runtimeAnimatorController = CurrentState.StateAnimController;
        CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
    }

    public RatState GetState(string stateName)
    {
        states.TryGetValue(stateName, out RatState state);
        return state;
    }

    public T GetState<T>() where T : RatState
    {
        string stateName = typeof(T).Name;
        return GetState(stateName) as T;
    }

    public void EnqueueTransition(string stateName)
    {
        RatState state = GetState(stateName);
        if (state != null)
        {
            TransitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransition<T>() where T : RatState
    {
        RatState state = GetState<T>();
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
        RatState[] statesArray = GetComponents<RatState>();

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
