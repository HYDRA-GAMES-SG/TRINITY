using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : PlantCreatureState
{
    Vector3 PlayerPos;
    Vector3 PlantPos;
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        PlantCreatureFSM.Animator.SetBool("Idle", true);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        PlayerPos = PlantCreatureFSM.PlayerController.transform.position;
        PlantPos = PlantCreatureFSM.PlantCreatureController.transform.position;

        Vector3 playerPos = new Vector3(PlayerPos.x, 0, PlayerPos.z);
        Vector3 plantPos = new Vector3(PlantPos.x, 0, PlantPos.z);

        float distanceToTarget = Vector3.Distance(playerPos, plantPos);

        Vector3 faceDirection = (PlayerPos - PlantPos).normalized;
        PlantCreatureFSM.RotateTowardTarget(faceDirection);

        if (distanceToTarget <= PlantCreatureFSM.PlantCreatureController.AttackRange)
        {
            PlantCreatureFSM.EnqueueTransition<AttackState>();
        }
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
