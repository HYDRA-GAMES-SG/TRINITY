using UnityEditor.Animations;
using UnityEngine;

public interface IState
{
    /// <summary>
    /// Assign the StateMachine and AnimatorComponent references.
    /// </summary>
    public void Initialize(IFSM stateMachine, Animator animator)
    {
        Awake();
    }

    /// <summary>
    /// Called when the state is first initialized. Use this for setup.
    /// </summary>
    protected void Awake();

    /// <summary>
    /// Called when entering the state. Initialize state-specific settings here.
    /// </summary>
    public void OnEnter();


    /// <summary>
    /// Called after OnEnter for any start-of-state logic.
    /// </summary>
    public void Start();

    /// <summary>
    /// Checks if conditions are met to enter this state.
    /// Return true to trigger a transition to this state.
    /// </summary>
    public void CheckEnterTransition();

    /// <summary>
    /// Handles behavior upon entering the state.
    /// </summary>
    public void EnterBehaviour();

    /// <summary>
    /// Called before the Update loop.
    /// </summary>
    public void PreUpdateBehaviour();

    /// <summary>
    /// Main state behavior logic. Called during Update.
    /// </summary>
    public void UpdateBehaviour();

    /// <summary>
    /// Called after the Update loop.
    /// </summary>
    public void PostUpdateBehaviour();

    /// <summary>
    /// Handles behavior when exiting the state.
    /// </summary>
    public void ExitBehaviour();

    /// <summary>
    /// Checks if conditions are met to exit this state.
    /// </summary>
    public void CheckExitTransition();
    /// <summary>
    /// Called when exiting the state.
    /// </summary>
    public void OnExit();

    /// <summary>
    /// FixedUpdate logic. Override this for physics-related updates.
    /// </summary>
    public void FixedUpdate();
}