using System;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityFSM : MonoBehaviour, IFSM
{
    // Public state list for assignment in the Inspector
    public List<TrinityState> States = new List<TrinityState>();

    // Current and previous states
    [HideInInspector]
    public TrinityState CurrentState { get; private set; }
    [HideInInspector]
    public TrinityState PreviousState { get; private set; }

    // Transition queue
    private Queue<Type> TransitionQueue;

    private void Awake()
    {
        TransitionQueue = new Queue<Type>();
        InitializeStates();
    }

    private void FixedUpdate()
    {
        // Call the current state's FixedUpdate function
        CurrentState?.UpdateBehaviour();

        // Handle transitions in the queue
        ProcessTransitionQueue();
    }

    /// <summary>
    /// Gets the state of the specified type.
    /// </summary>
    public T GetState<T>() where T : TrinityState
    {
        foreach (var state in States)
        {
            if (state is T matchedState)
            {
                return matchedState;
            }
        }

        Debug.LogWarning($"State of type {typeof(T).Name} not found.", this);
        return null;
    }

    /// <summary>
    /// Queues a transition to the specified state.
    /// </summary>
    public void EnqueueTransition<T>() where T : TrinityState
    {
        TransitionQueue.Enqueue(typeof(T));
    }

    /// <summary>
    /// Queues a transition back to the previous state.
    /// </summary>
    public void EnqueuePrevious()
    {
        if (PreviousState != null)
        {
            TransitionQueue.Enqueue(PreviousState.GetType());
        }
    }

    /// <summary>
    /// Immediately transitions to the specified state.
    /// </summary>
    public void ForceTransition<T>() where T : TrinityState
    {
        TransitionToState(typeof(T));
    }

    /// <summary>
    /// Initializes all states. Override to add specific states manually or via Inspector.
    /// </summary>
    protected virtual void InitializeStates()
    {
        States = new List<TrinityState>(GetComponents<TrinityState>());
        
        // Ensure states are aware of this state machine
        foreach (var state in States)
        {
            state.SetStateMachine(this);
        }
    }

    /// <summary>
    /// Processes the next transition in the queue.
    /// </summary>
    private void ProcessTransitionQueue()
    {
        if (TransitionQueue.Count > 0)
        {
            var nextStateType = TransitionQueue.Dequeue();
            TransitionToState(nextStateType);
        }
    }

    /// <summary>
    /// Handles the transition to a specific state.
    /// </summary>
    private void TransitionToState(Type stateType)
    {
        var nextState = FindStateByType(stateType);

        if (nextState != null)
        {
            // Exit the current state
            CurrentState?.OnExit();

            // Update state tracking
            PreviousState = CurrentState;
            CurrentState = nextState;

            // Enter the new state
            CurrentState.OnEnter();
        }
        else
        {
            Debug.LogWarning($"State of type {stateType.Name} not found in StateMachine.", this);
        }
    }

    /// <summary>
    /// Finds a state by its type.
    /// </summary>
    private TrinityState FindStateByType(Type stateType)
    {
        foreach (var state in States)
        {
            if (state.GetType() == stateType)
            {
                return state;
            }
        }

        return null;
    }
}
