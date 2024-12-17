using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBPursue : InvincibleBossState
{
    [SerializeField] string AnimKeyRotation = "Rotation";
    [SerializeField] string AnimKeyMovement = "Movement";

    [SerializeField] float WaitTime = 0.3f;
    float waitTime;

    [SerializeField] float TurnSmoothTime = 0.2f;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        waitTime -= Time.fixedDeltaTime;
        if (waitTime <= 0)
        {
            waitTime = WaitTime;

            InvincibleBossFSM.InvincibleBossController.AI.SetDestination(InvincibleBossFSM.PlayerController.transform.position);
        }

        AnimationRotateMove();

        float distance = InvincibleBossFSM.InvincibleBossController.CalculateGroundDistance();

        if (distance < InvincibleBossFSM.InvincibleBossController.CloseAttack)
        {
            InvincibleBossFSM.EnqueueTransition<IBFootAttack>();
        }
        else if (distance >= InvincibleBossFSM.InvincibleBossController.CloseAttack && distance <= InvincibleBossFSM.InvincibleBossController.CloseAttack + 2)
        {
            InvincibleBossFSM.EnqueueTransition<IBHandAttack>();
        }
        else if (distance >= InvincibleBossFSM.InvincibleBossController.LongAttack && distance <= InvincibleBossFSM.InvincibleBossController.LongAttack + 2)
        {
            InvincibleBossFSM.EnqueueTransition<IBLongAttack_ShotShock>();
            InvincibleBossFSM.EnqueueTransition<IBLongAttack_ThrowRock>();
            InvincibleBossFSM.EnqueueTransition<IBTaunt>();
        }
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        InvincibleBossFSM.InvincibleBossController.AI.ResetPath();
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    private void AnimationRotateMove()
    {
        Vector3 direction = InvincibleBossFSM.InvincibleBossController.AI.velocity.normalized;
        float turn = Vector3.SignedAngle(transform.forward, direction, Vector3.up) / 180.0f;
        float smoothTurn = Mathf.Lerp(InvincibleBossFSM.InvincibleBossController.Animator.GetFloat(AnimKeyRotation), turn, TurnSmoothTime);
        InvincibleBossFSM.InvincibleBossController.Animator.SetFloat(AnimKeyRotation, smoothTurn);

        Vector3 movementSpeed = InvincibleBossFSM.InvincibleBossController.transform.InverseTransformDirection(InvincibleBossFSM.InvincibleBossController.AI.velocity);
        float movement = movementSpeed.magnitude;
        //float maxSpeed = AI.velocity.magnitude;
        //movement = movement / maxSpeed;
        //movement = Mathf.Clamp01(movement);
        InvincibleBossFSM.InvincibleBossController.Animator.SetFloat(AnimKeyMovement, movement);
    }
}
