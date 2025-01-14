using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RabbitWander : RabbitState
{
    [SerializeField] float FleeDistanceCheck = 15f;
    [SerializeField] float MoveSpeed;
    [SerializeField] float WanderMinDistance;
    [SerializeField] float WanderMaxDistance;

    public override void Awake()
    {
    }

    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is RabbitIdle;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        RabbitFSM.RabbitController.AI.speed = MoveSpeed;
        RabbitFSM.RabbitController.AI.SetDestination(GenerateRandomPosition());
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (HasFinishMoving())
        {
            RabbitFSM.EnqueueTransition<RabbitIdle>();
        }

        if (RabbitFSM.RabbitController.CalculateDistance() <= FleeDistanceCheck)
        {
            RabbitFSM.EnqueueTransition<RabbitFlee>();
        }
    }
    public override bool CheckExitTransition(IState fromState)
    {
        return true;
    }
    Vector3 GenerateRandomPosition()
    {
        float distance = Random.Range(WanderMinDistance, WanderMaxDistance);
        Vector3 randPos = Random.insideUnitCircle.normalized * distance;
        randPos.z = randPos.y;
        randPos.y = 0;
        return RabbitFSM.RabbitController.transform.position + randPos;
    }
    public bool HasFinishMoving()
    {
        return !RabbitFSM.RabbitController.AI.pathPending && !RabbitFSM.RabbitController.AI.hasPath;
    }
}
