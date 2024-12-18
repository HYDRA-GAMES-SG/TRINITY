
using UnityEngine;

public abstract class CrabState : MonoBehaviour, IState
{
    [Header("Animator Controller")]
    public RuntimeAnimatorController StateAnimController;

    protected ACrabFSM CrabFSM;

    public float RotateSpeed = 3f;

    public void Awake()
    {
    }

    public void Start()
    {

    }

    public void Update()
    {
    }


    public virtual bool CheckEnterTransition(IState fromState)
    {
        return false;
    }
    

    public virtual void EnterBehaviour(float dt, IState fromState)
    {
        if (CrabFSM)
        {
            CrabFSM.Animator.runtimeAnimatorController = StateAnimController;
        }
    }

    public virtual void PreUpdateBehaviour(float dt)
    {
    }

    public virtual void UpdateBehaviour(float dt)
    {
    }

    public virtual void PostUpdateBehaviour(float dt)
    {

    }

    public virtual bool CheckExitTransition(IState toState)
    {
        return false;
    }

    public virtual void ExitBehaviour(float dt, IState toState)
    {
    }

    public virtual void SetStateMachine(ACrabFSM aCrabStateMachine)
    {
        CrabFSM = aCrabStateMachine;
    }
}