using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBTaunt : InvincibleBossState
{
    [SerializeField]
    string AnimKey = "Taunt";

    [SerializeField] float RotateSpeed;

    [SerializeField] ParticleSystem TauntParticle;
    [SerializeField] Collider TauntCollider;

    public override bool CheckEnterTransition(IState fromState)
    {
        return InvincibleBossFSM.InvincibleBossController.bCanTaunt && fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);

        TauntParticle.Play();
        TauntCollider.enabled = true;
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

        if (InvincibleBossFSM.InvincibleBossController.CalculateGroundDistance() <= InvincibleBossFSM.InvincibleBossController.CloseAttack)
        {
            InvincibleBossFSM.EnqueueTransition<IBHandAttack>();
        }
        else if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
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
}
