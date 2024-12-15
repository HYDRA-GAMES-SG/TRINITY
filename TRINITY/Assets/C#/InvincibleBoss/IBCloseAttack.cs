using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBCloseAttack : InvincibleBossState
{
    [SerializeField]
    string[] AnimKeyTriggerATK = new string[]
    {
        "LeftFootStompAttack","RightFootStompAttack","2HitComboAttackForward_RM", "2HandsSmashAttack_RM"
    };

    string AnimKey;

    AInvincibleBossController IBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        IBController = InvincibleBossFSM.InvincibleBossController;
        PlayerController = InvincibleBossFSM.PlayerController;

        AI = IBController.AI;

        IBController.Animator.applyRootMotion = true;

        int index = Random.Range(0,AnimKeyTriggerATK.Length);
        AnimKey = AnimKeyTriggerATK[index];
        IBController.Animator.SetTrigger(AnimKey);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (PlayerController.transform.position - IBController.transform.position).normalized;
        IBController.RotateTowardTarget(faceDirection);

        string layerName = GetType().Name;
        int layerIndex = IBController.Animator.GetLayerIndex(layerName);
        AnimatorStateInfo stateInfo = IBController.Animator.GetCurrentAnimatorStateInfo(layerIndex);
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
        IBController.Animator.applyRootMotion = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
