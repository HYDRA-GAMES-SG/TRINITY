using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IBLongAttack : InvincibleBossState
{
    [SerializeField]
    string[] AnimKeyTriggerLongATK = new string[]
    {
        "SpitterShot2","Taunt","ThrowRock"
    };

    string AnimKey;

    [SerializeField] Transform ShockBluePos;
    [SerializeField] ParticleSystem ShockBlue;

    AInvincibleBossController IBController;
    ATrinityController PlayerController;
    NavMeshAgent AI;

    Dictionary<string, System.Action> exitActions;

    public override bool CheckEnterTransition(IState fromState)
    {
        if (InvincibleBossFSM.InvincibleBossController.bCanShotShock || InvincibleBossFSM.InvincibleBossController.bCanTaunt || InvincibleBossFSM.InvincibleBossController.bCanThrow)
        {
            return true;
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        IBController = InvincibleBossFSM.InvincibleBossController;
        PlayerController = InvincibleBossFSM.PlayerController;

        AI = IBController.AI;

        InitializeExitActions();

        bool[] conditions = { IBController.bCanShotShock, IBController.bCanTaunt, IBController.bCanThrow };
        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i])
            {
                AnimKey = AnimKeyTriggerLongATK[i];
                IBController.Animator.SetTrigger(AnimKey);
                return;
            }
        }

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

        if(AnimKey == "SpitterShot2")
        {
            Vector3 iceSprayDirection = (PlayerController.transform.position - ShockBluePos.position).normalized;
            ShockBluePos.transform.rotation = Quaternion.LookRotation(iceSprayDirection, Vector3.up);
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        if (exitActions.TryGetValue(AnimKey, out var action))
        {
            action.Invoke();
        }
    }

    public override bool CheckExitTransition(IState toState)
    {
        return true;
    }

    void InitializeExitActions()
    {
        exitActions = new Dictionary<string, System.Action>
        {
            { "SpitterShot2", () => IBController.bCanShotShock = false },
            { "Taunt", () => IBController.bCanTaunt = false },
            { "ThrowRock", () => IBController.bCanThrow = false }
        };
    }
}
