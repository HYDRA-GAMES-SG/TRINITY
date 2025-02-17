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

    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is IBPursue || fromState is IBTaunt;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;

        InvincibleBossFSM.InvincibleBossController.Animator.applyRootMotion = true;

        if (fromState is IBTaunt)
        {
            AnimKey = "2HandsSmashAttack_RM";
        }
        else
        {
            int index = Random.Range(0, AnimKeyTriggerATK.Length);
            AnimKey = AnimKeyTriggerATK[index];
        }
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        string layerName = GetType().Name;
        int layerIndex = InvincibleBossFSM.InvincibleBossController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = InvincibleBossFSM.InvincibleBossController.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            AnimFinish = true;
            InvincibleBossFSM.EnqueueTransition<IBPursue>();
        }
        bool shouldRotate = !(stateInfo.IsName("2HandsSmashAttack_RM") && stateInfo.normalizedTime > 0.35f) || stateInfo.IsName("2HitComboAttackForward_RM");

        if (shouldRotate)
        {
            Vector3 faceDirection = (InvincibleBossFSM.PlayerController.transform.position - InvincibleBossFSM.InvincibleBossController.transform.position).normalized;
            InvincibleBossFSM.InvincibleBossController.RotateTowardTarget(faceDirection, RotateSpeed);
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
        return toState is IBPursue || toState is IBDead || (toState is IBIdle && AnimFinish);
    }
}
