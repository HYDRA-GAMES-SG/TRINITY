using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pursue: CrabState
{
    [SerializeField] float MoveSpeed;
    [SerializeField] float StopDistance;
    [SerializeField] float RotateSpeed;
    [SerializeField] float ThresholdAngle;
    NavMeshAgent CrabAI;


    private string AnimKeyTurnDirection = "RotateDirection";
    
    public override bool CheckEnterTransition(IState fromState)
    {
        float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
        
        if (distanceToTarget > StopDistance)
        {
            return true;
        }
        
        return false;
    }

    public override void OnEnter()
    {
       
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabAI = CrabFSM.CrabController.AI;
        CrabAI.speed = MoveSpeed;
        CrabAI.stoppingDistance = StopDistance;
        CrabAI.updateRotation = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {

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
        float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
        
        if (distanceToTarget < StopDistance)
        {
            CrabFSM.EnqueueTransition<Attack>();
            return true;
        }
        
        return false;
    }


    void RotateAndMoveTowardTarget()
    {
        Vector3 directionToTarget = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
        
        float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
        float angleToTarget = RotateTowardTarget(directionToTarget);

        if (angleToTarget > ThresholdAngle)
        {
            if (distanceToTarget > StopDistance && angleToTarget <= ThresholdAngle + 30)
            {
                HandleMovement();
            }
            else
            {
                float turnDirection = Mathf.Sign(Vector3.Cross(CrabFSM.CrabController.transform.forward, directionToTarget).y);
                CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, turnDirection);

                CrabAI.ResetPath();
            }
        }
        else
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        CrabFSM.Animator.SetFloat("RotateDirection", 0);
        CrabAI.SetDestination(CrabFSM.PlayerController.transform.position);

        Vector3 localDesiredVelocity = CrabFSM.CrabController.transform.InverseTransformDirection(CrabAI.velocity);
        CrabFSM.Animator.SetFloat("MoveValue", localDesiredVelocity.z);
    }

    float RotateTowardTarget(Vector3 directionToTarget)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
    }
}