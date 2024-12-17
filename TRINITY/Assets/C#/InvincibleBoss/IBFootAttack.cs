using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBFootAttack : InvincibleBossState
{
    [SerializeField]
    string[] AnimKeyTriggerATK = new string[]
    {
        "LeftFootStompAttack","RightFootStompAttack"
    };

    string AnimKey;


    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is IBPursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        int index = Random.Range(0, AnimKeyTriggerATK.Length);
        AnimKey = AnimKeyTriggerATK[index];
        InvincibleBossFSM.InvincibleBossController.Animator.SetTrigger(AnimKey);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        //Vector3 faceDirection = (PlayerController.transform.position - IBController.transform.position).normalized;
        //IBController.RotateTowardTarget(faceDirection);

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
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }
}
