using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBLongAttack_ThrowRock : InvincibleBossState
{
    [SerializeField] string AnimKeyCharge = "UnearthRock";
    [SerializeField] string AnimKeyThrowRock = "ThrowRock";

    [SerializeField] Transform LeftHand, RightHand;
    Vector3 bothHandCnetrelPos;

    [SerializeField] ParticleSystem Orb;
    ParticleSystem orb;

    bool hasSpwan = false;

    [SerializeField] float TimeCharge;
    float timer;

    [SerializeField] float timeToTarget = 1.0f;

    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanThrow && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKeyCharge);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
        InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, RotateSpeed);

        bothHandCnetrelPos = (LeftHand.position + RightHand.position) / 2;
        if (!hasSpwan)
        {
            orb = Instantiate(Orb, bothHandCnetrelPos, Quaternion.identity);
            hasSpwan = true;
        }
        orb.transform.position = bothHandCnetrelPos;

        timer += Time.fixedDeltaTime;

        if (timer >= TimeCharge)
        {
            InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKeyThrowRock);
        }

        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKeyThrowRock) && stateInfo.normalizedTime >= 0.95f)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        InvincibleBossFSM.InvincibleBossController.bCanThrow = false;
        orb = null;
        hasSpwan = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    private Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target, float time)
    {
        // Gravity
        float g = Physics.gravity.y;

        // Calculate horizontal and vertical distances
        Vector3 distance = target - start;
        Vector3 horizontalDistance = new Vector3(distance.x, 0, distance.z);

        // Horizontal velocity
        Vector3 velocityXZ = horizontalDistance / time;

        // Vertical velocity
        float velocityY = (distance.y - 0.5f * g * time * time) / time;

        // Combine horizontal and vertical velocities
        return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }
    public void Throw()
    {
        // Get start and target positions
        Vector3 start = bothHandCnetrelPos;
        Vector3 targetPos = InvincibleBossFSM.PlayerController.transform.position;

        // Calculate the velocity
        Vector3 throwVelocity = CalculateThrowVelocity(start, targetPos, timeToTarget);

        // Apply the velocity to the object
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity =true;
            rb.velocity = throwVelocity;
        }
    }
}
