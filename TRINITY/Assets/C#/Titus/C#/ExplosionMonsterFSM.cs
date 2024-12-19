using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TitusAssignment
{
    public class ExplosionMonsterFSM : MonoBehaviour, IFSM
    {
        [Tooltip("The state used to start the state machine.")]
        public ExplosionMonsterState InitialState;

        private readonly Dictionary<string, ExplosionMonsterState> states = new Dictionary<string, ExplosionMonsterState>();
        private Queue<ExplosionMonsterState> TransitionQueue = new Queue<ExplosionMonsterState>();

        public ExplosionMonsterState CurrentState { get; private set; }
        public ExplosionMonsterState PreviousState { get; private set; }

        public event Action<ExplosionMonsterState, ExplosionMonsterState> OnStateChange;
        public ExplosionMonsterController explosionMonsterController;
        public ATrinityController PlayerController;
        public Animator Animator;
        private bool FSM_RUNNING = false;

        private Coroutine layerWeightCoroutine;

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
                    Debug.LogError("ExplosionMonsterFSM: No Player Controller Reference found.");
                    return;
                }
            }
        }

        private void StartStateMachine()
        {
            FSM_RUNNING = true;
            CurrentState = InitialState;
            UpdateAnimatorLayer(CurrentState.GetType().Name);
            CurrentState.EnterBehaviour(0f, null);
        }

        private void ProcessTransitions()
        {
            if (TransitionQueue.Count > 0)
            {
                while (TransitionQueue.Count > 0)
                {
                    ExplosionMonsterState nextState = TransitionQueue.Dequeue();

                    if (nextState != null && nextState.isActiveAndEnabled)
                    {
                        TransitionToState(nextState);
                        break;
                    }
                }
            }
        }

        private void TransitionToState(ExplosionMonsterState nextState)
        {
            if (CurrentState == nextState || !CurrentState.CheckExitTransition(nextState) || !nextState.CheckEnterTransition(CurrentState))
                return;

            PreviousState = CurrentState;
            CurrentState.ExitBehaviour(Time.deltaTime, nextState);

            OnStateChange?.Invoke(PreviousState, nextState);

            Debug.Log("AI: " + CurrentState + "=>" + nextState);

            CurrentState = nextState;
            UpdateAnimatorLayer(CurrentState.GetType().Name);
            CurrentState.EnterBehaviour(Time.deltaTime, PreviousState);
        }

        public ExplosionMonsterState GetState(string stateName)
        {
            states.TryGetValue(stateName, out ExplosionMonsterState state);
            return state;
        }

        public T GetState<T>() where T : ExplosionMonsterState
        {
            string stateName = typeof(T).Name;
            return GetState(stateName) as T;
        }

        public void EnqueueTransition(string stateName)
        {
            ExplosionMonsterState state = GetState(stateName);
            if (state != null)
            {
                TransitionQueue.Enqueue(state);
            }
        }

        public void EnqueueTransition<T>() where T : ExplosionMonsterState
        {
            ExplosionMonsterState state = GetState<T>();
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
            ExplosionMonsterState[] statesArray = GetComponents<ExplosionMonsterState>();

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

        //To use this the state C# script & Animator Layer name have to be same
        //private void UpdateAnimatorLayer(string stateName)
        //{
        //    for (int i = 0; i < Animator.layerCount; i++)
        //    {
        //        Animator.SetLayerWeight(i, 0); // Reset all layers to 0
        //    }

        //    int layerIndex = Animator.GetLayerIndex(stateName);
        //    if (layerIndex >= 0)
        //    {
        //        Animator.SetLayerWeight(layerIndex, 1); // Activate the current state's layer
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"Animator layer for state '{stateName}' not found.");
        //    }
        //}

        private void UpdateAnimatorLayer(string stateName, float transitionDuration = 0.5f)
        {
            if (Animator == null)
            {
                Debug.LogWarning("Animator is not assigned.");
                return;
            }

            int targetLayerIndex = Animator.GetLayerIndex(stateName);

            if (targetLayerIndex < 0)
            {
                Debug.LogWarning($"Animator layer for state '{stateName}' not found.");
                return;
            }

            // Smoothly reset all layers to 0 except the target layer
            for (int i = 0; i < Animator.layerCount; i++)
            {
                float targetWeight = i == targetLayerIndex ? 1f : 0f;
                StartCoroutine(SmoothSetLayerWeight(i, targetWeight, transitionDuration));
            }
        }

        private IEnumerator SmoothSetLayerWeight(int layerIndex, float targetWeight, float duration)
        {
            if (layerIndex < 0 || layerIndex >= Animator.layerCount)
            {
                Debug.LogWarning($"Invalid layer index: {layerIndex}");
                yield break;
            }

            float initialWeight = Animator.GetLayerWeight(layerIndex);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newWeight = Mathf.Lerp(initialWeight, targetWeight, elapsedTime / duration);
                Animator.SetLayerWeight(layerIndex, newWeight);
                yield return null;
            }

            Animator.SetLayerWeight(layerIndex, targetWeight); // Ensure exact target weight is set
        }
    }
}
