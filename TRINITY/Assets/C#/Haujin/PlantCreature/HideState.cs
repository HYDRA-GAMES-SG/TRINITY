using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideState : PlantCreatureState
{
    Vector3 PlayerPos;
    Vector3 PlantPos;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
        PlayerPos = PlantCreatureFSM.PlayerController.transform.position;
        PlantPos = PlantCreatureFSM.PlantCreatureController.transform.position;

        Vector3 playerPos = new Vector3(PlayerPos.x, 0, PlayerPos.z);
        Vector3 plantPos = new Vector3(PlantPos.x, 0, PlantPos.z);

        float distanceToTarget = Vector3.Distance(playerPos, plantPos);
        if (distanceToTarget <= PlantCreatureFSM.PlantCreatureController.AttackRange)
        {
            PlantCreatureFSM.EnqueueTransition<IdleState>();
        }
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
        return true;
    }
}
