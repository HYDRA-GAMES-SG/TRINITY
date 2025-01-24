using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBTaunt : InvincibleBossState
{
    [SerializeField]
    string AnimKey = "Taunt";

    [SerializeField] GameObject Indecator;
    [SerializeField] ParticleSystem TauntParticle;
    [SerializeField] Collider TauntCollider;
    [SerializeField] float OnTauntParticleDelay;
    [SerializeField] float OnTauntParticleDuration;
    float DelayTimer;
    float DurationTimer;
    bool bOnTaunt = false;
    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanTaunt && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish=false;
        bOnTaunt = false;
        DelayTimer = 0;
        DurationTimer = 0;

        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);
        Indecator.SetActive(true);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
        InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, RotateSpeed);

        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);

        DelayTimer += Time.fixedDeltaTime;
        if (DelayTimer >= OnTauntParticleDelay && !bOnTaunt)
        {
            bOnTaunt = true;
            Indecator.SetActive(false);
            TauntParticle.Play();
            TauntCollider.enabled = true;
            InvincibleBossFSM.InvincibleBossController.MediumCameraShake(3f);
        }
        if (bOnTaunt)
        {
            DurationTimer += Time.fixedDeltaTime;
            if (DurationTimer >= OnTauntParticleDuration)
            {
                AnimFinish = true;
                InvincibleBossFSM.EnqueueTransition<IBPursue>();
            }
        }
        if (InvincibleBossFSM.InvincibleBossController.CalculateGroundDistance() <= InvincibleBossFSM.InvincibleBossController.CloseAttack)
        {
                AnimFinish = true;
            InvincibleBossFSM.EnqueueTransition<IBHandAttack>();
        }
        else if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
        }

    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        InvincibleBossFSM.InvincibleBossController.bCanTaunt = false;
        TauntCollider.enabled = false;
        TauntParticle.Stop();
    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is IBPursue || toState is IBDead || (toState is IBIdle && AnimFinish);
    }
}
