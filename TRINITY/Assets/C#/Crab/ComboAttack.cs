using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ComboAttack : CrabState
{
    [SerializeField] float RotateSpeed;


    private string AnimKeyClawAttackCombo = "2HitComboClawsAttack_RM";
    private string AnimKeySmashAttackCombo = "2HitComboSmashAttack_RM";

    private string AnimKey;

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is Pursue)
        {
            if (CrabFSM.CrabController.CanComboAttack)
            {
                return true;
            }
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
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
        RotateTowardTarget(faceDirection);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInTransition = CrabFSM.Animator.IsInTransition(0);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.9f)
        {
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
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }

    private void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, Time.deltaTime);
    }
}