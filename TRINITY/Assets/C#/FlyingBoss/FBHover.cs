using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FBHover : FlyingBossState
{
    private ATrinityController playerController;
    private Rigidbody rb;
    private Animator animator;

    private Vector3 targetHoverPosition;
    private const string VerticalParameter = "Vertical";
    private const string HorizontalParameter = "Horizontal";

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float hoverSmoothness = 2f;
    [SerializeField] private float hoverRadius = 10f;
    [SerializeField] private float positionTolerance = 3f;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        playerController = FlyingBossFSM.PlayerController;
        rb = FlyingBossFSM.FlyingBossController.GetComponent<Rigidbody>();
        animator = FlyingBossFSM.FlyingBossController.GetComponent<Animator>();

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

        CheckForAttackTransitions();
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        animator.SetFloat(VerticalParameter, 0f);
        animator.SetFloat(HorizontalParameter, 0f);
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }

    private void MoveTowardsTarget(float dt)
    {
        Vector3 direction = (targetHoverPosition - FlyingBossFSM.FlyingBossController.transform.position).normalized;
        Vector3 smoothVelocity = Vector3.Lerp(rb.velocity, direction * moveSpeed, hoverSmoothness * dt);
        rb.velocity = smoothVelocity;

        FlyingBossFSM.FlyingBossController.transform.forward = Vector3.Lerp(FlyingBossFSM.FlyingBossController.transform.forward, direction, hoverSmoothness * dt);
    }

    Vector3 GetRandomHoverPosition()
    {
        Debug.Log("B");
        Vector3 invincibleBossPosition = FlyingBossFSM.FlyingBossController.InvincibleBoss.position;
        Vector3 bossForward = FlyingBossFSM.FlyingBossController.InvincibleBoss.forward;

        Vector3 basePositionBehindBoss = invincibleBossPosition - bossForward * FlyingBossFSM.FlyingBossController.InvincibleBossRange;

        for (int attempt = 0; attempt < 5; attempt++)
        {
            // Generate random offsets around the base position
            float randomX = Random.Range(0, FlyingBossFSM.FlyingBossController.HoverXAxis);
            float randomY = Random.Range(5, FlyingBossFSM.FlyingBossController.HoverYAxis);
            float randomZ = Random.Range(0, FlyingBossFSM.FlyingBossController.HoverZAxis);

            Vector3 randomPosition = basePositionBehindBoss + new Vector3(randomX, randomY, randomZ);

            if (IsPositionValid(randomPosition))
            {
                return randomPosition;
            }
        }
        return basePositionBehindBoss;
    }

    private Vector3 GetPositionBehindInvincibleBoss()
    {
        Debug.Log("A");
        Vector3 bossPosition = FlyingBossFSM.FlyingBossController.InvincibleBoss.position;
        Vector3 directionToBoss = (FlyingBossFSM.FlyingBossController.transform.position - bossPosition).normalized;
        Vector3 offsetPosition = bossPosition - directionToBoss * FlyingBossFSM.FlyingBossController.InvincibleBossRange;

        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            Vector3 candidatePosition = offsetPosition + randomOffset;
            if (IsPositionValid(candidatePosition))
            {
                return candidatePosition;
            }
        }

        Debug.LogWarning("Failed to find a valid position behind the boss. Returning fallback position.");
        return bossPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        return position.y > 0; ;
    }

    private void UpdateBlendTreeParameters()
    {
        Vector3 velocity = rb.velocity;
        Vector3 localVelocity = FlyingBossFSM.FlyingBossController.transform.InverseTransformDirection(velocity);

        animator.SetFloat(VerticalParameter, Mathf.Clamp(localVelocity.z, -1f, 1f));
        animator.SetFloat(HorizontalParameter, Mathf.Clamp(localVelocity.x, -1f, 1f));
    }

    private void CheckForAttackTransitions()
    {
        var controller = FlyingBossFSM.FlyingBossController;

        if (controller.bCanElectricChargeAttack && controller.CalculateDistance() <= controller.LongAttackRange)
        {
            FlyingBossFSM.EnqueueTransition<FBAttack>();
            controller.bCanElectricChargeAttack = false;
        }
        else if (controller.bCanSpikeAttack && controller.CalculateDistance() <= controller.CloseAttackRange)
        {
            FlyingBossFSM.EnqueueTransition<FBAttack>();
            controller.bCanSpikeAttack = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetHoverPosition, 2f);
    }
}