using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitIdle : RabbitState
{
    [SerializeField] float FleeDistanceCheck = 10f;
    [SerializeField] float RandomIdleTMin;
    [SerializeField] float RandomIdelTMax;
    float IdleTime;
    float timer;

    public override void Awake()
    {
    }

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is RabbitDead)
        {
            return false;
        }
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        timer = 0;

        float randomTime = Random.Range(RandomIdleTMin, RandomIdelTMax);
        IdleTime = randomTime;
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        timer = 0;
        IdleTime = 0;
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        timer += Time.fixedDeltaTime;
        if (timer > IdleTime)
        {
            RabbitFSM.EnqueueTransition<RabbitWander>();
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
}
