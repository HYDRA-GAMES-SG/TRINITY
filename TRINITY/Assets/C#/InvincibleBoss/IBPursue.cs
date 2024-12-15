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

    AInvincibleBossController IBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        IBController = InvincibleBossFSM.InvincibleBossController;
        PlayerController = InvincibleBossFSM.PlayerController;

        AI = IBController.AI;
    }

    public override void PreUpdateBehaviour(float dt)
    {
        waitTime -= Time.fixedDeltaTime;
        if (waitTime <= 0)
        {
            waitTime = WaitTime;

            AI.SetDestination(PlayerController.transform.position);
        }

        AnimationRotateMove();

        if (IBController.CalculateDistance() <= IBController.CloseAttack)
        {
            InvincibleBossFSM.EnqueueTransition<IBCloseAttack>();
        }
        else if (IBController.CalculateDistance() <= IBController.LongAttack && IBController.CalculateDistance() > IBController.CloseAttack)
        {
            InvincibleBossFSM.EnqueueTransition<IBLongAttack>();
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
        AI.ResetPath();
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    private void AnimationRotateMove()
    {
        Vector3 direction = AI.velocity.normalized;
        float turn = Vector3.SignedAngle(transform.forward, direction, Vector3.up) / 180.0f;
        float smoothTurn = Mathf.Lerp(IBController.Animator.GetFloat(AnimKeyRotation), turn, TurnSmoothTime);
        IBController.Animator.SetFloat(AnimKeyRotation, smoothTurn);

        Vector3 movementSpeed = IBController.transform.InverseTransformDirection(AI.velocity);
        float movement = movementSpeed.magnitude;
        //float maxSpeed = AI.velocity.magnitude;
        //movement = movement / maxSpeed;
        //movement = Mathf.Clamp01(movement);
        IBController.Animator.SetFloat(AnimKeyMovement, movement);
    }
}
