using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FMBCloseAttack : FinalMonsterBossState
{
    [SerializeField] string CAtk1 = "HeavyAttack";
    [SerializeField] string CAtk2 = "Dash";
    [SerializeField] string CAtk3 = "HitTheGround";
    bool isAnimationPlaying = false;
    string currentAnimation = "";

    AFinalMonsterController FMBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    private bool isDashing = false;
    private float dashSpeed = 30f;
    private float dashDuration = 0.5f;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is FMBWalk or FMBLongRangeAttack;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        FMBController = FinalMonsterBossFSM.FinalMonsterBossController;
        PlayerController = FinalMonsterBossFSM.PlayerController;
        AI = FinalMonsterBossFSM.GetComponent<NavMeshAgent>();

        isAnimationPlaying = false;
        currentAnimation = "";
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        CheckTransitionToWalk();
        if (isDashing)
        {
            HandleDash(dt);
            return;
        }

        if (isAnimationPlaying)
        {
            AnimatorStateInfo stateInfo = FinalMonsterBossFSM.Animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1f)
            {
                isAnimationPlaying = false;
                currentAnimation = "";
                FinalMonsterBossFSM.Animator.ResetTrigger(currentAnimation);
            }
        }
        else
        {
            ChooseAttack();
        }
        float randomValue = Random.Range(0f, 1f);

        FMBController.RotateTowardTarget(FMBController.transform.position, 15f);

        if (randomValue < (1f / 3f) && FMBController.bHeavyAttack)
        {
            FMBController.PlayParticleSystem(
                "HeavyAttack",
                FMBController.FrozenPulsePrefab,
                FMBController.transform.position + FMBController.transform.forward * 2f,
                FMBController.transform.rotation
            );
            FinalMonsterBossFSM.Animator.SetTrigger(CAtk1);
            FMBController.bHeavyAttack = false;
        }
        else if (randomValue >= (1f / 3f) && randomValue < (2f / 3f) && FMBController.bDash)
        {
            StartDash();
            FMBController.bDash = false;
        }
        else if (randomValue >= (2f/3f) && FMBController.bHitTheGround)
        {
            FMBController.PlayParticleSystem(
                "HitTheGround",
                FMBController.OrbFrostPrefab,
                -FMBController.transform.position + FMBController.transform.up * 2f,
                FMBController.transform.rotation
            );
            FinalMonsterBossFSM.Animator.SetTrigger(CAtk3);
            FMBController.bHitTheGround = false;
        }
    }
    void ChooseAttack()
    {
        float randomValue = Random.value;

        FMBController.RotateTowardTarget(FMBController.transform.position, 15f);

        if (randomValue < (1f / 3f) && FMBController.bHeavyAttack)
        {
            PlayAttackAnimation(CAtk1, "HeavyAttack", FMBController.FrozenPulsePrefab);
            FMBController.bHeavyAttack = false;
        }
        else if (randomValue >= (1f / 3f) && randomValue < (2f / 3f) && FMBController.bDash)
        {
            PlayAttackAnimation(CAtk2, "Dash", FMBController.WaveFrostCastPrefab);
            FMBController.bDash = false;
        }
        else if (randomValue >= (2f / 3f) && FMBController.bHitTheGround)
        {
            PlayAttackAnimation(CAtk3, "HitTheGround", FMBController.OrbFrostPrefab);
            FMBController.bHitTheGround = false;
        }
    }
    void PlayAttackAnimation(string triggerName, string animationName, GameObject particlePrefab)
    {
        FinalMonsterBossFSM.Animator.SetTrigger(triggerName);
        FMBController.PlayParticleSystem(
            animationName,
            particlePrefab,
            FMBController.transform.position,
            FMBController.transform.rotation
        );

        isAnimationPlaying = true;
        currentAnimation = animationName;
    }
    void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        dashDirection = (PlayerController.transform.position - FMBController.transform.position).normalized;

        FinalMonsterBossFSM.Animator.SetTrigger(CAtk2);
        FMBController.PlayParticleSystem(
            "Dash", 
            FMBController.WaveFrostCastPrefab, 
            -FMBController.transform.position + FMBController.transform.forward * 2,
            FMBController.transform.rotation);

        if (AI != null) AI.isStopped = true;

        if (FMBController.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.velocity = dashDirection * dashSpeed;
        }
    }

    void HandleDash(float dt)
    {
        dashTimer += dt;

        if (dashTimer < dashDuration)
        {
            if (FMBController.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = dashDirection * dashSpeed;
            }
            else
            {
                FMBController.transform.position += dashDirection * dashSpeed * dt;
            }
        }
        else
        {
            isDashing = false;
            if (AI != null) AI.isStopped = false;

            if (FMBController.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
            }
            CheckTransitionToWalk();
        }
    }

    private void CheckTransitionToWalk()
    {
        float randomValue = Random.Range(0f, 1f);
        float distance = FMBController.CalculateGroundDistance();

        if (distance > FMBController.CloseAttackRange && distance <= FMBController.LongAttackRange)
        {
            if (randomValue < 0.5f)
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
            }
            else if (randomValue > 0.5f)
            {
                FinalMonsterBossFSM.EnqueueTransition<FMBLongRangeAttack>();
            }
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        StopAllParticleSystems(FMBController.gameObject);
    }
    void StopAllParticleSystems(GameObject target)
    {
        ParticleSystem[] particleSystems = target.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
            ps.Clear();
        }
    }
    public override bool CheckExitTransition(IState toState)
    {
        return toState is FMBWalk or FMBLongRangeAttack or FMBDie;
    }
}
