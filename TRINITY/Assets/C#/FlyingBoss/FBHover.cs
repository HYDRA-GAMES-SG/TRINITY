using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBHover : FlyingBossState
{
    private Vector3 targetHoverPosition;

    private const string VerticalParameter = "Vertical";
    private const string HorizontalParameter = "Horizontal";

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float positionTolerance = 3f;
    [SerializeField] private Vector3 behindBossOffset;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        targetHoverPosition = GetRandomHoverPosition();
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 currentPosition = FlyingBossFSM.FlyingBossController.transform.position;

        if (Vector3.Distance(currentPosition, targetHoverPosition) <= positionTolerance)
        {
            if (FlyingBossFSM.FlyingBossController.bCanRandomFly)
            {
                FlyingBossFSM.FlyingBossController.bCanRandomFly = false;
                targetHoverPosition = GetRandomHoverPosition();
            }
            else
            {
                targetHoverPosition = GetPositionBehindInvincibleBoss();
            }
        }

        MoveTowardsTarget(dt);
        UpdateBlendTreeParameters();


        if (FlyingBossFSM.FlyingBossController.bCanElectricChargeAttack && ATrinityGameManager.GetGameFlowState() != EGameFlowState.DEAD)
        {
            FlyingBossFSM.EnqueueTransition<FBAttack>();
        }
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        FlyingBossFSM.FlyingBossController.RB.velocity = Vector3.zero;

        FlyingBossFSM.Animator.SetFloat(VerticalParameter, 0f);
        FlyingBossFSM.Animator.SetFloat(HorizontalParameter, 0f);
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }

    private void MoveTowardsTarget(float dt)
    {
        Vector3 direction = (targetHoverPosition - FlyingBossFSM.FlyingBossController.transform.position).normalized;
        Vector3 smoothVelocity = Vector3.Lerp(FlyingBossFSM.FlyingBossController.RB.velocity, direction * moveSpeed, RotateSpeed * dt);
        FlyingBossFSM.FlyingBossController.RB.velocity = smoothVelocity;

        Vector3 faceDirection = (FlyingBossFSM.PlayerController.transform.position - FlyingBossFSM.FlyingBossController.transform.position).normalized;

        FlyingBossFSM.FlyingBossController.RotateTowardTarget(faceDirection, RotateSpeed);       
    }

    Vector3 GetRandomHoverPosition()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            // Generate random offsets around the base position
            float randomX = Random.Range(0, FlyingBossFSM.FlyingBossController.HoverXAxis);
            float randomY = Random.Range(5, FlyingBossFSM.FlyingBossController.HoverYAxis);
            float randomZ = Random.Range(0, FlyingBossFSM.FlyingBossController.HoverZAxis);

            Vector3 randomPosition = GetPositionBehindInvincibleBoss() + new Vector3(randomX, randomY, randomZ);

            if (IsPositionValid(randomPosition))
            {
                return randomPosition;
            }
        }
        return GetPositionBehindInvincibleBoss();
    }

    private Vector3 GetPositionBehindInvincibleBoss()
    {
        Vector3 invincibleBossPosition = FlyingBossFSM.FlyingBossController.InvincibleBoss.position + behindBossOffset;
        Vector3 bossForward = FlyingBossFSM.FlyingBossController.InvincibleBoss.forward;

        Vector3 basePositionBehindBoss = invincibleBossPosition - bossForward;
        basePositionBehindBoss.y = 5;
        return basePositionBehindBoss;
    }

    private bool IsPositionValid(Vector3 position)
    {
        return position.y > 0;
    }

    private void UpdateBlendTreeParameters()
    {
        Vector3 velocity = FlyingBossFSM.FlyingBossController.RB.velocity;
        Vector3 localVelocity = FlyingBossFSM.FlyingBossController.transform.InverseTransformDirection(velocity);

        FlyingBossFSM.Animator.SetFloat(VerticalParameter, Mathf.Clamp(localVelocity.z, -1f, 1f));
        FlyingBossFSM.Animator.SetFloat(HorizontalParameter, Mathf.Clamp(localVelocity.x, -1f, 1f));
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetHoverPosition, 2f);
    }
}