using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NormalAttack : CrabState
{
    Transform LeftClaw;
    Transform RightClaw;

    public static System.Action<Transform> OnNormalAttack;

    [SerializeField] string[] AnimKeyAttack;
    private string anim;

    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;
        CrabFSM.CrabController.AI.enabled = false;
        CrabFSM.Animator.applyRootMotion = true;

        anim = RandomAttackAnim(AnimKeyAttack);
        CrabFSM.Animator.SetTrigger(anim);

        if (anim.Contains("Left"))
        {
            OnNormalAttack?.Invoke(CrabFSM.CrabController.LeftClaw);
        }
        if (anim.Contains("Right"))
        {
            OnNormalAttack?.Invoke(CrabFSM.CrabController.RightClaw);
        }
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);

        AnimatorStateInfo stateInfo = CrabFSM.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(anim) && stateInfo.normalizedTime > 0.95f)
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
    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Pursue || toState is IcePhaseRoar || toState is GetHit || toState is Death || (toState is Idle && AnimFinish);
    }

    private string RandomAttackAnim(string[] anim)
    {
        int index = Random.Range(0, anim.Length);
        return anim[index];
    }
}
