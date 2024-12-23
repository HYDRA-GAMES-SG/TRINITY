using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBHandAttack : InvincibleBossState
{
    public string[] AnimKeyTriggerATK = new string[]
    {
        "2HitComboAttackForward_RM", "2HandsSmashAttack_RM"
    };
    [HideInInspector]
    public string AnimKey;


    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is IBPursue || fromState is IBTaunt;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.applyRootMotion = true;

        int index = Random.Range(0, AnimKeyTriggerATK.Length);
        AnimKey = AnimKeyTriggerATK[index];
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
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        InvincibleBossFSM.InvincibleBossController.Animator.applyRootMotion = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
