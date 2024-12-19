using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FBHover : FlyingBossState
{
    AFlyingBossController FBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    Vector3 TargetHoverPosition;
    Animator animator;

    private const string blendTreeParameter = "FlyBlend";
    private const string verticalParameter = "Vertical";
    private const string horizontalParameter = "Horizontal";
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FBController = FlyingBossFSM.FlyingBossController;
        PlayerController = FlyingBossFSM.PlayerController;

        AI = FBController.AI;
        animator = FBController.GetComponent<Animator>();

        TargetHoverPosition = GetRandomHoverPosition();
        AI.SetDestination(TargetHoverPosition);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (AI.remainingDistance <= AI.stoppingDistance)
        {
            if (FBController.bCanRandomFly)
            {
                FBController.bCanRandomFly = false;
                TargetHoverPosition = GetRandomHoverPosition();
                AI.SetDestination(TargetHoverPosition);
            }
            else
            {
                TargetHoverPosition = GetPositionBehindInvincibleBoss();
                AI.SetDestination(TargetHoverPosition);
            }
        }

        if (Vector3.Distance(FBController.transform.position, FBController.InvincibleBoss.position) > FBController.InvincibleBossRange)
        {
            TargetHoverPosition = GetPositionBehindInvincibleBoss();
            AI.SetDestination(TargetHoverPosition);
        }

        UpdateBlendTreeParameter();

        if (FBController.bCanElectricChargeAttack && FBController.CalculateDistance() <= FBController.LongAttackRange)
        {
            FlyingBossFSM.EnqueueTransition<FBAttack>();
            FBController.bCanElectricChargeAttack = false;
        }
        else if ((FBController.bCanSpikeAttack && FBController.CalculateDistance() <= FBController.CloseAttackRange))
        {
            FlyingBossFSM.EnqueueTransition<FBAttack>();
            FBController.bCanSpikeAttack = false;
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        animator.SetFloat(verticalParameter, 0f);
        animator.SetFloat(horizontalParameter, 0f);
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    Vector3 GetRandomHoverPosition()
    {
        Vector3 invincibleBossPosition = FBController.InvincibleBoss.position;
        /*float randomX = Random.Range(-FBController.HoverXAxis, FBController.HoverXAxis);
        float randomY = Random.Range(-FBController.HoverYAxis, FBController.HoverYAxis);
        float randomZ = Random.Range(-FBController.HoverZAxis, FBController.HoverZAxis);

        Vector3 randomPosition = invincibleBossPosition + new Vector3(randomX, randomY, randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return GetRandomHoverPosition();
        }*/
        for (int attempt = 0; attempt < 5; attempt++)
        {
            float randomX = Random.Range(-FBController.HoverXAxis, FBController.HoverXAxis);
            float randomY = Random.Range(-FBController.HoverYAxis, FBController.HoverYAxis);
            float randomZ = Random.Range(-FBController.HoverZAxis, FBController.HoverZAxis);

            Vector3 randomPosition = invincibleBossPosition + new Vector3(randomX, randomY, randomZ);

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return invincibleBossPosition;
    }
    Vector3 GetPositionBehindInvincibleBoss()
    {
        Vector3 invincibleBossPosition = FBController.InvincibleBoss.position;
        Vector3 directionToInvincibleBoss = (FBController.transform.position - invincibleBossPosition).normalized;
        Vector3 behindPosition = invincibleBossPosition - directionToInvincibleBoss * FBController.InvincibleBossRange;

        int maxAttempts = 5;
        float sampleRadius = 10f;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (NavMesh.SamplePosition(behindPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            behindPosition += Random.insideUnitSphere * 2f;
        }

        Debug.LogWarning("Failed to find a valid position behind the invincible boss. Returning fallback position.");
        return invincibleBossPosition;
    }
    void UpdateBlendTreeParameter()
    {
        Vector3 velocity = AI.velocity;
        Vector3 direction = velocity.normalized;

        float vertical = Vector3.Dot(direction, FBController.transform.forward);
        float horizontal = Vector3.Dot(direction, FBController.transform.right);

        vertical = Mathf.Clamp(vertical, -1f, 1f);
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);

        animator.SetFloat(verticalParameter, vertical);
        animator.SetFloat(horizontalParameter, horizontal);

        float blendValue = Mathf.Sqrt(vertical * vertical + horizontal * horizontal);
        animator.SetFloat(blendTreeParameter, blendValue);
    }
}