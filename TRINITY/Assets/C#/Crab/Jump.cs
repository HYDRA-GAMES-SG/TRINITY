using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Jump : CrabState
{
    public Rigidbody rb;
    [SerializeField] bool bJumped;
    [SerializeField] float JumpRangeCheck = 20f;
    [SerializeField] float JumpForce = 100f;
    [SerializeField] float JumpUpForce = 500f;
    [SerializeField] float EnqueueTimer = 1f;
    [SerializeField] float ResetTimer = 5f;
    [SerializeField] string forward = "JumpForward";
    [SerializeField] string backward = "JumpBackward";

    NavMeshAgent CrabAI;
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
        CrabAI = CrabFSM.CrabController.AI;
        CrabFSM.CrabController.AI.enabled = false;
        CrabFSM.Animator.applyRootMotion = true;
        rb.isKinematic = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (bJumped)
        {
            EnqueueTimer -= Time.deltaTime;

            if (EnqueueTimer <= 0)
            {
                CrabFSM.EnqueueTransition<Pursue>();
                bJumped = false;
            }
            return;
        }

        Vector3 forwardDirection = rb.transform.forward;
        Vector3 targetPosition = rb.transform.position + forwardDirection * JumpRangeCheck;
        NavMeshHit hit;
        bool canJumpForward = NavMesh.SamplePosition(targetPosition, out hit, JumpRangeCheck, NavMesh.AllAreas);

        if (canJumpForward)
        {
            ApplyJumpForce(forwardDirection);
            CrabFSM.Animator.SetTrigger(forward);
            bJumped = true;
        }
        else
        {
            ApplyJumpForce(-forwardDirection);
            CrabFSM.Animator.SetTrigger(backward);
            bJumped = true;
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        CrabFSM.CrabController.CanJumpSmash = false;
        bJumped = false;
        rb.isKinematic = true;
        EnqueueTimer = ResetTimer;
    }
    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }
    private void ApplyJumpForce(Vector3 direction)
    {
        rb.AddForce(rb.transform.up * JumpUpForce, ForceMode.Force);
        rb.AddForce(direction * JumpForce, ForceMode.Force);
    }
}
