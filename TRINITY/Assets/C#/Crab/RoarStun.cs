using UnityEngine;

public class RoarStun : CrabState
{
    public override bool CheckEnterTransition(IState fromState)
    {
      
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override bool CheckExitTransition(IState toState)
    {
        return false;
    }
}
