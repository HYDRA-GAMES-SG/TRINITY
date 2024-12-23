using System.Collections;
using System.Collections.Generic;
using ThirdPersonCamera;
using UnityEngine;
using UnityEngine.AI;

public class IBLongAttack_ThrowRock : InvincibleBossState
{
    [SerializeField] string AnimKeyCharge = "UnearthRock";
    [SerializeField] string AnimKeyThrowRock = "ThrowRock";

    [SerializeField] Transform LeftHand, RightHand;
    [SerializeField] float HoldOrbOffsetY;
    Vector3 bothHandCnetrelPos;

    [SerializeField] ParticleSystem OrbParticle;
    ParticleSystem orbObj;

    bool hasSpawned = false;
    bool isCharging = true;
    bool isFollow = false;

    [SerializeField] float TimeCharge;
    float timer;

    [SerializeField] float OrbTimeToTarget = 0.5f;

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
        if (isCharging)
        {
            Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
            InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, RotateSpeed);


            if (!hasSpawned)
            {
                orbObj = Instantiate(OrbParticle, bothHandCnetrelPos, Quaternion.identity);
                Orb orb = orbObj.GetComponent<Orb>();
                orb.GetController(InvincibleBossFSM.InvincibleBossController);
                hasSpawned = true;
            }

            timer += Time.fixedDeltaTime;

            if (timer >= TimeCharge)
            {
                isCharging = false;
                InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKeyThrowRock);
            }
        }
        if (!isFollow)
        {
            Vector3 offset = new Vector3(0, HoldOrbOffsetY, 0);
            bothHandCnetrelPos = (LeftHand.position + RightHand.position) / 2 + offset;
            orbObj.transform.position = bothHandCnetrelPos;
        }

        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);

        if (stateInfo.IsName(AnimKeyThrowRock) && (stateInfo.normalizedTime >= 0.2f && stateInfo.normalizedTime <= 0.3f)) // Between 20% and 50% of animation
        {
            isFollow = true;
            Throw();
        }
        else if (stateInfo.IsName(AnimKeyThrowRock) && stateInfo.normalizedTime >= 0.95f) // Animation nearly complete
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
        orbObj = null;
        hasSpawned = false;
        timer = 0;
        isCharging = true;
        isFollow = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
    private Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target, float time)// when 0.6 work
    {
        // Gravity
        float g = Mathf.Abs(Physics.gravity.y);

        // Calculate horizontal and vertical distances
        Vector3 distance = target - start;
        Vector3 horizontalDistance = new Vector3(distance.x, 0, distance.z);

        // Horizontal velocity
        Vector3 velocityXZ = horizontalDistance / time;

        // Adjusted vertical velocity
        float velocityY = (distance.y + 0.5f * g * time * time) / time;

        // Combine horizontal and vertical velocities
        return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }
    public void Throw()
    {
        Vector3 start = bothHandCnetrelPos;
        Vector3 targetPos = InvincibleBossFSM.PlayerController.transform.position;
        Vector3 throwVelocity = CalculateThrowVelocity(start, targetPos, OrbTimeToTarget);

        Rigidbody rb = orbObj.GetComponent<Rigidbody>();
        rb.useGravity = true;
        Collider collider = orbObj.GetComponent<Collider>();
        collider.enabled = true;
        if (rb != null)
        {
            rb.velocity = throwVelocity;
        }
        DrawTrajectory(start, throwVelocity);
    }
    private void DrawTrajectory(Vector3 start, Vector3 velocity)
    {
        Vector3 previousPoint = start;
        float timestep = 0.1f; // Smaller values for smoother lines
        float totalTime = OrbTimeToTarget;

        for (float t = 0; t <= totalTime; t += timestep)
        {
            Vector3 currentPoint = start + velocity * t + 0.5f * Physics.gravity * t * t;
            Debug.DrawLine(previousPoint, currentPoint, Color.red, 2.0f); // Draw trajectory
            previousPoint = currentPoint;
        }
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(bothHandCnetrelPos, 2);
    //}
}
