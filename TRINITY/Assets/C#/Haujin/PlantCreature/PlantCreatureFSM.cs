using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantCreatureFSM : MonoBehaviour, IFSM
{
    [Tooltip("The state used to start the state machine.")]
    public PlantCreatureState InitialState;

    private readonly Dictionary<string, PlantCreatureState> states = new Dictionary<string, PlantCreatureState>();
    private Queue<PlantCreatureState> TransitionQueue = new Queue<PlantCreatureState>();

    public PlantCreatureState CurrentState { get; private set; }
    public PlantCreatureState PreviousState { get; private set; }

    public event Action<PlantCreatureState, PlantCreatureState> OnStateChange;
    public PlantCreatureController PlantCreatureController;
    public ATrinityController PlayerController;
    public Animator Animator;
    private bool FSM_RUNNING = false;

    public bool ShowDebugLog = false;

    private void Awake()
    {
        InitializeStates();
    }

    private void FixedUpdate()
    {
        if (ShowDebugLog)
        {
            //Debug.Log(TransitionQueue.Count);
            Debug.Log(Vector3.Distance(PlayerController.transform.position, PlantCreatureController.transform.position));

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
            PlayerController = FindObjectOfType<ATrinityController>();

            if (!PlayerController)
            {
                Debug.LogError("PlantCreatureFSM: No Player Controller Reference found.");
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
                PlantCreatureState nextState = TransitionQueue.Dequeue();

                if (nextState != null && nextState.isActiveAndEnabled)
                {
                    TransitionToState(nextState);
                    break;
                }
            }
        }
    }

    private void TransitionToState(PlantCreatureState nextState)
    {
        if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState) || !nextState.CheckEnterTransition(CurrentState))
            return;

        PreviousState = CurrentState;
        CurrentState.ExitBehaviour(Time.deltaTime, nextState);

        OnStateChange?.Invoke(PreviousState, nextState);

        //Debug.Log("AI: " + CurrentState + "=>" + nextState);

        CurrentState = nextState;
        CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
    }

    public PlantCreatureState GetState(string stateName)
    {
        states.TryGetValue(stateName, out PlantCreatureState state);
        return state;
    }

    public T GetState<T>() where T : PlantCreatureState
    {
        string stateName = typeof(T).Name;
        return GetState(stateName) as T;
    }

    public void EnqueueTransition(string stateName)
    {
        PlantCreatureState state = GetState(stateName);
        if (state != null)
        {
            TransitionQueue.Enqueue(state);
        }
    }

    public void EnqueueTransition<T>() where T : PlantCreatureState
    {
        PlantCreatureState state = GetState<T>();
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
        PlantCreatureState[] statesArray = GetComponents<PlantCreatureState>();

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

    public void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        PlantCreatureController.transform.rotation = Quaternion.Slerp(PlantCreatureController.transform.rotation, targetRotation, 10 * Time.deltaTime);
    }
}
