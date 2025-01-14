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

    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is FMBWalk;
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
        if (isAnimationPlaying)
        {
            AnimatorStateInfo stateInfo = FinalMonsterBossFSM.Animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 0f)
            {
                isAnimationPlaying = false;
                currentAnimation = "";
                FinalMonsterBossFSM.Animator.ResetTrigger(currentAnimation);
                CheckTransitionToWalk();
            }
        }
        else
        {
            ChooseAttack();
        }
        /*float randomValue = Random.Range(0f, 1f);

        FMBController.RotateTowardTarget(FMBController.transform.position, 15f);
        //done the animation then transit
        if (FMBController.CalculateGroundDistance() <= FMBController.LongAttackRange && FMBController.CalculateGroundDistance() > FMBController.CloseAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
        }

        if (randomValue < (1f / 3f) && FMBController.bHeavyAttack)
        {
            FMBController.PlayParticleSystem(
                "HeavyAttack",
                FMBController.FrozenPulsePrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FinalMonsterBossFSM.Animator.SetTrigger(CAtk1);
            FMBController.bHeavyAttack = false;
        }
        else if (randomValue < (2f / 3f) && FMBController.bDash)
        {
            FMBController.PlayParticleSystem(
                "Dash",
                FMBController.WaveFrostCastPrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FinalMonsterBossFSM.Animator.SetTrigger(CAtk2);
            FMBController.bDash = false;
        }
        else if (randomValue < 1f && FMBController.bHitTheGround)
        {
            FMBController.PlayParticleSystem(
                "HitTheGround",
                FMBController.OrbFrostPrefab,
                FMBController.transform.position,
                FMBController.transform.rotation
            );
            FinalMonsterBossFSM.Animator.SetTrigger(CAtk3);
            FMBController.bHitTheGround = false;
        }*/
    }
    void ChooseAttack()
    {
        float randomValue = Random.Range(0f, 1f);

        FMBController.RotateTowardTarget(FMBController.transform.position, 15f);

        if (randomValue < (1f / 3f) && FMBController.bHeavyAttack)
        {
            PlayAttackAnimation(CAtk1, "HeavyAttack", FMBController.FrozenPulsePrefab);
            FMBController.bHeavyAttack = false;
        }
        else if (randomValue < (2f / 3f) && FMBController.bDash)
        {
            PlayAttackAnimation(CAtk2, "Dash", FMBController.WaveFrostCastPrefab);
            FMBController.bDash = false;
        }
        else if (randomValue < 1f && FMBController.bHitTheGround)
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
    private void CheckTransitionToWalk()
    {
        float randomValue = Random.Range(0f, 1f);
        float distance = FMBController.CalculateGroundDistance();

        if (randomValue < 0.5f && distance > FMBController.CloseAttackRange && distance <= FMBController.LongAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBWalk>();
        }
        else if (randomValue > 0.5f && distance > FMBController.CloseAttackRange && distance <= FMBController.LongAttackRange)
        {
            FinalMonsterBossFSM.EnqueueTransition<FMBLongRangeAttack>();
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        StopAllParticleSystems(FMBController.gameObject);
    }
    private void StopAllParticleSystems(GameObject target)
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
        return toState is FMBWalk;
    }
}
