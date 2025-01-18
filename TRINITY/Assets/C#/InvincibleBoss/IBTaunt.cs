using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBTaunt : InvincibleBossState
{
    [SerializeField]
    string AnimKey = "Taunt";

    [SerializeField] ParticleSystem TauntParticle;
    [SerializeField] Collider TauntCollider;
    [SerializeField] float OnTauntParticleDelay;
    [SerializeField] float OnTauntParticleDuration;
    float DelayTimer;
    float DurationTimer;
    bool bOnTaunt = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanTaunt && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        bOnTaunt = false;
        DelayTimer = 0;
        DurationTimer = 0;

        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);

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
            TauntParticle.Play();
            TauntCollider.enabled = true;
            MediumCameraShake(OnTauntParticleDuration);
        }
        if (bOnTaunt)
        {
            DurationTimer += Time.fixedDeltaTime;
            if (DurationTimer >= OnTauntParticleDuration)
            {
                InvincibleBossFSM.EnqueueTransition<IBPursue>();
            }
        }
        if (InvincibleBossFSM.InvincibleBossController.CalculateGroundDistance() <= InvincibleBossFSM.InvincibleBossController.CloseAttack)
        {
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
        return true;
    }
    public void MediumCameraShake(float duration = 0.5f)
    {
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.6f, duration, transform);
    }
}
