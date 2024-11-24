public interface IState
{
    /// <summary>
    /// Called when the state is first initialized. Use this for setup.
    /// </summary>
    public void Awake();

    /// <summary>
    /// Called when entering the state. Initialize state-specific settings here.
    /// </summary>
    public void OnEnter();

    /// <summary>
    /// Called when exiting the state.
    /// </summary>
    public void OnExit();

    /// <summary>
    /// Handles behavior upon entering the state.
    /// </summary>
    public void EnterBehaviour(float dt, IState fromState);

    /// <summary>
    /// Called before the Update loop.
    /// </summary>
    public void PreUpdateBehaviour(float dt);

    /// <summary>
    /// Main state behavior logic. Called during Update.
    /// </summary>
    public void UpdateBehaviour(float dt);

    /// <summary>
    /// Called after the Update loop.
    /// </summary>
    public void PostUpdateBehaviour(float dt);

    /// <summary>
    /// Handles behavior when exiting the state.
    /// </summary>
    public void ExitBehaviour(float dt, IState toState);

    /// <summary>
    /// Checks if conditions are met to enter this state.
    /// Return true to trigger a transition to this state.
    /// </summary>
    public bool CheckEnterTransition(IState fromState);

    /// <summary>
    /// Checks if conditions are met to exit this state.
    /// </summary>
    public void CheckExitTransition();
}