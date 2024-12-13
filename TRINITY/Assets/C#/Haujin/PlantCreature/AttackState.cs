using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : PlantCreatureState
{
    Vector3 PlayerPos;
    Vector3 PlantPos;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        PlayerPos = PlantCreatureFSM.PlayerController.transform.position;
        PlantPos = PlantCreatureFSM.PlantCreatureController.transform.position;

        PlantCreatureFSM.Animator.SetBool("Attack", true);
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
