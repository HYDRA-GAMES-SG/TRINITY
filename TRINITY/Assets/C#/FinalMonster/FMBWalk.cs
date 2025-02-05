using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class FMBWalk : FinalMonsterBossState
{
    [SerializeField] string AnimKeyRotation = "Rotation";
    [SerializeField] string AnimKeyMovement = "Movement";

    [SerializeField] float WaitTime = 0.3f;
    private float waitTime;

    [SerializeField] float TurnSmoothTime = 0.2f;

    AFinalMonsterController FMBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    float transitionCooldown = 1f;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FMBController = FinalMonsterBossFSM.FinalMonsterBossController;
        PlayerController = FinalMonsterBossFSM.PlayerController;
        AI = FinalMonsterBossFSM.FinalMonsterBossController.AI;

        if (AI != null)
        {
            AI.isStopped = false;
            AI.SetDestination(PlayerController.transform.position);
        }
        waitTime = WaitTime;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (AI == null || PlayerController == null)
        {
            return;
        }

        float distance = FMBController.CalculateGroundDistance();
        if (FMBController.CalculateGroundDistance() <= FMBController.CloseAttackRange)
        {
            AI.isStopped = true;
            FinalMonsterBossFSM.EnqueueTransition<FMBCloseAttack>();
            return;
        }
        if (FMBController.CalculateGroundDistance() > FMBController.LongAttackRange)
        {
            AI.isStopped = true;
            FinalMonsterBossFSM.EnqueueTransition<FMBIdle>();
            return;
        }
        AI.isStopped = false;
        AI.SetDestination(PlayerController.transform.position);
        AnimationRotateMove();
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        if (AI != null)
        {
            AI.isStopped = true;
        }
    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is FMBCloseAttack or FMBIdle or FMBDie;
    }

    private void AnimationRotateMove()
    {
        if (AI == null || FMBController.Animator == null)
        {
            return;
        }

        Vector3 direction = AI.velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            float turn = Vector3.SignedAngle(transform.forward, direction, Vector3.up) / 180.0f;
            float smoothTurn = Mathf.Lerp(FMBController.Animator.GetFloat(AnimKeyRotation), turn, TurnSmoothTime);
            FMBController.Animator.SetFloat(AnimKeyRotation, smoothTurn);
        }

        Vector3 movementSpeed = FMBController.transform.InverseTransformDirection(AI.velocity);
        float movement = movementSpeed.magnitude;
        FMBController.Animator.SetFloat(AnimKeyMovement, movement);
    }
}