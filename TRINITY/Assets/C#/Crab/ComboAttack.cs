using UnityEngine;
using UnityEngine.AI;

public class ComboAttack : CrabState
{
    [SerializeField] float RotateSpeed;

    private bool NextStage = false;
    NavMeshAgent CrabAI;

    private string AnimKeyClawAttackCombo = "ClawsAttackCombo";
    private string AnimKeySmashAttackCombo = "SmashAttackCombo";

    private string AnimKey;

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is PursueAttack)
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
        CrabAI = CrabFSM.CrabController.AI;

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
        CrabAI.SetDestination(CrabFSM.PlayerController.transform.position);

        Vector3 directionToTarget = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;

        RotateTowardTarget(directionToTarget);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInTransition = CrabFSM.Animator.IsInTransition(0);
        if (stateInfo.IsName(AnimKey) && stateInfo.normalizedTime >= 0.9f)
        {
            CrabFSM.EnqueueTransition<PursueAttack>();
            NextStage = true;
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.CrabController.CanComboAttack = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        return NextStage;
    }

    void RotateTowardTarget(Vector3 directionToTarget)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

    }
}