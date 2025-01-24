using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ComboAttack : CrabState
{
    [SerializeField] string AnimKeyClawAttackCombo = "2HitComboClawsAttack_RM_End";
    [SerializeField] string AnimKeySmashAttackCombo = "2HitComboSmashAttack_RM_End";
    [HideInInspector]
    public string AnimKey;
    bool AnimFinish = false;

    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue && CrabFSM.CrabController.CanComboAttack;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;

        CrabFSM.CrabController.AI.enabled = false;
        CrabFSM.Animator.applyRootMotion = true;

        float random = Random.value;
        if (random > 0.5f)
        {
            AnimKey = AnimKeyClawAttackCombo;
        }
        else
        {
            AnimKey = AnimKeySmashAttackCombo;
        }
        CrabFSM.Animator.SetTrigger(AnimKey);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInTransition = CrabFSM.Animator.IsInTransition(0);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.95f)
        {
            AnimFinish = true;
            CrabFSM.EnqueueTransition<Pursue>();
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        CrabFSM.CrabController.CanComboAttack = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Pursue || toState is Death || (toState is Idle && AnimFinish);
    }
}