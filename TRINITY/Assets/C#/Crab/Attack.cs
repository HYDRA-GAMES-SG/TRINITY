using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : CrabState
{
    bool LongDistanceAttack;

    float MaxLongRangeLength,
          MinLongRangeLength,
          CloseRangeLength;

    public override bool CheckEnterTransition(IState fromState)
    {
        //base.CheckEnterTransition();
        return false;
    }

    public override void OnEnter()
    {
        //base.OnEnter();
        
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        
    }

    public override void PreUpdateBehaviour(float dt)
    {
        //base.PreUpdateBehaviour(dt);
    }


    public override void UpdateBehaviour(float dt)
    {
        //base.UpdateBehaviour(dt);
    }


    public override void PostUpdateBehaviour(float dt)
    {
        //base.PostUpdateBehaviour(dt);
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        //base.ExitBehaviour(dt, toState);
    }

    public override bool CheckExitTransition(IState toState)
    {
        //base.CheckExitTransition();
        return false;
    }

    public override void OnExit()
    {
        //base.OnExit();
    }

    public override void FixedUpdate()
    {
        //base.FixedUpdate();
    }
}