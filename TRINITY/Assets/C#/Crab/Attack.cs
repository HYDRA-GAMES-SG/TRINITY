using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack : CrabState
{
    private string AnimKeyNormalAttackLeft = "NormalAttackLeft";
    private string AnimKeyNormalAttackRight = "NormalAttackRight";

    NavMeshAgent CrabAI;
    public override bool CheckEnterTransition(IState fromState)
    {
        //float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);

        //if (distanceToTarget < CrabFSM.CrabController.AI.stoppingDistance + 2)
        //{
        //    Debug.Log(distanceToTarget);
        //    return true;
        //}

        return true;
    }

    public override void OnEnter()
    {

    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabAI = CrabFSM.CrabController.AI;

        string animKey;
        float random = Random.value;

        if (random > 0.5f)
        {
            animKey = AnimKeyNormalAttackLeft;
        }
        else
        {
            animKey = AnimKeyNormalAttackRight;
        }

        CrabFSM.Animator.SetTrigger(animKey);
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

    public override void OnExit()
    {
    }

    public override void FixedUpdate()
    {
    }
}