using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NormalAttack : CrabState
{
    [SerializeField] string[] AnimKeyAttack;

    private string anim;

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is Pursue)
        {
            return true;
        }
        return false;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabFSM.CrabController.AI.enabled = false;
        CrabFSM.Animator.applyRootMotion = true;

        anim = RandomAttackAnim(AnimKeyAttack);
        CrabFSM.Animator.SetTrigger(anim);
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        RotateTowardTarget(faceDirection);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(anim) && stateInfo.normalizedTime > 0.95f)
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
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }

    private string RandomAttackAnim(string[] anim)
    {
        int index = Random.Range(0, anim.Length);
        return anim[index];
    }

    private void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, Time.deltaTime);
    }
}
