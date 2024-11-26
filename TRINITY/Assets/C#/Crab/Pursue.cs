using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pursue: CrabState
{
    [SerializeField] ATrinityController PlayerTarget;
    [SerializeField] ACrabController CrabBoss;
    [SerializeField] float MoveSpeed;
    [SerializeField] float StopDistance;
    [SerializeField] float RotateSpeed;
    [SerializeField] float ThresholdAngle;
    NavMeshAgent BossAgent;
    public override bool CheckEnterTransition(IState fromState)
    {
        return false;
    }

    public override void OnEnter()
    {
       
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        BossAgent = CrabBoss.gameObject.GetComponent<NavMeshAgent>();
        BossAgent.speed = MoveSpeed;
        BossAgent.stoppingDistance = StopDistance;
        BossAgent.updateRotation = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        if (PlayerTarget == null) return;

        RotateAndMoveTowardTarget();
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
    }

    public override bool CheckExitTransition(IState toState)
    {
        return false;
    }

    public override void OnExit()
    {
    }

    public override void FixedUpdate()
    {
    }

    void RotateAndMoveTowardTarget()
    {
        Vector3 directionToTarget = (PlayerTarget.transform.position - CrabBoss.transform.position).normalized;
        float distanceToTarget = Vector3.Distance(PlayerTarget.transform.position, CrabBoss.transform.position);
        float angleToTarget = RotateTowardTarget(directionToTarget);

        if (angleToTarget > ThresholdAngle)
        {
            if (distanceToTarget > StopDistance && angleToTarget <= ThresholdAngle + 30)
            {
                HandleMovement();
            }
            else
            {
                HandleRotationWithAnimator(directionToTarget);
            }
        }
        else
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        StateMachine.Animator.SetFloat("RotateDirection", 0);
        BossAgent.SetDestination(PlayerTarget.transform.position);

        Vector3 localDesiredVelocity = CrabBoss.transform.InverseTransformDirection(BossAgent.velocity);
        StateMachine.Animator.SetFloat("MoveValue", localDesiredVelocity.z);
    }

    float RotateTowardTarget(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        CrabBoss.transform.rotation = Quaternion.Slerp(CrabBoss.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabBoss.transform.forward, direction);
    }

    void HandleRotationWithAnimator(Vector3 directionToRotate)
    {
        float turnDirection = Mathf.Sign(Vector3.Cross(CrabBoss.transform.forward, directionToRotate).y);
        StateMachine.Animator.SetFloat("RotateDirection", turnDirection);

        BossAgent.ResetPath();
    }
}