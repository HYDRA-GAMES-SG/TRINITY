using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBAttack : FlyingBossState
{
    [SerializeField] string AnimKey = "FlyElectroShot";

    [SerializeField] float moveSpeed;
    [SerializeField] float hoverSmoothness;

    Vector3 AttackPos;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        Vector3[] attackPositions = AttackPositionBehind();
        AttackPos = attackPositions[Random.Range(0, attackPositions.Length)];
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        MoveTowardsTarget(dt);
        if (IsAtAttackPosition())
        {
            FlyingBossFSM.Animator.SetTrigger(AnimKey);
        }

        string layerName = GetType().Name;
        int layerIndex = FlyingBossFSM.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = FlyingBossFSM.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            FlyingBossFSM.EnqueueTransition<FBHover>();
        }
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        FlyingBossFSM.FlyingBossController.ElectricCharge.Stop();
        FlyingBossFSM.FlyingBossController.ElectricShot.Stop();

        FlyingBossFSM.FlyingBossController.bCanElectricChargeAttack = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }

    Vector3[] AttackPositionBehind()
    {
        Vector3 IBPos = FlyingBossFSM.FlyingBossController.InvincibleBoss.transform.position;
        Transform bossTransform = FlyingBossFSM.FlyingBossController.InvincibleBoss.transform;

        float shoulderOffset = 5f;
        float shoulderHighOffset = 11f;
        float headOffset = 13f;

        Vector3 leftShoulder = IBPos - bossTransform.right * shoulderOffset + bossTransform.up * shoulderHighOffset;
        Vector3 rightShoulder = IBPos + bossTransform.right * shoulderOffset + bossTransform.up * shoulderHighOffset;
        Vector3 aboveHead = IBPos + bossTransform.up * headOffset;

        return new Vector3[] { leftShoulder, rightShoulder, aboveHead };
    }

    private void MoveTowardsTarget(float dt)
    {
        Vector3 direction = (AttackPos - FlyingBossFSM.FlyingBossController.transform.position).normalized;
        Vector3 smoothVelocity = Vector3.Lerp(FlyingBossFSM.FlyingBossController.RB.velocity, direction * moveSpeed, hoverSmoothness * dt);
        FlyingBossFSM.FlyingBossController.RB.velocity = smoothVelocity;

        Vector3 faceDirection = (FlyingBossFSM.PlayerController.transform.position - FlyingBossFSM.FlyingBossController.transform.position).normalized;

        FlyingBossFSM.FlyingBossController.RotateTowardTarget(faceDirection, hoverSmoothness);
    }

    private bool IsAtAttackPosition()
    {
        float positionTolerance = 0.5f;
        return Vector3.Distance(FlyingBossFSM.FlyingBossController.transform.position, AttackPos) <= positionTolerance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(AttackPos, 2f);
    }
}


