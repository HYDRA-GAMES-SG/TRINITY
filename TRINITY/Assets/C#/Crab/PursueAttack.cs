using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PursueAttack : CrabState
{
    [Header("AI Setting")]
    [SerializeField] float MoveSpeed;
    [SerializeField] float StopDistance;
    [SerializeField] float RotateSpeed;
    [SerializeField] float ThresholdAngle;

    [Header("Attack Range")]
    [SerializeField] float RangeAttack;
    [SerializeField] float ComboRangeAttack;

    NavMeshAgent CrabAI;

    private string AnimKeyTurnDirection = "RotateDirection";
    private string AnimKeyMoveValue = "MoveValue";

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is ComboAttack || fromState is JumpSmash)
        {
            return true;
        }
        return false;
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
        if (CrabFSM.PlayerController == null) return;

        float distanceToTarget = Vector3.Distance(CrabFSM.PlayerController.transform.position, CrabFSM.CrabController.transform.position);
        Debug.Log(distanceToTarget);
        if (distanceToTarget >= RangeAttack && distanceToTarget <= RangeAttack + 2)
        {
            Debug.Log("Can range");
            CrabFSM.EnqueueTransition<JumpSmash>();
            CrabFSM.EnqueueTransition<RoarStun>();
        }
        else if (distanceToTarget <= ComboRangeAttack)
        {
            Debug.Log("Can combo");
            CrabFSM.EnqueueTransition<ComboAttack>();
        }
        else
        {
            RotateAndMoveTowardTarget();
        }
    }


    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabAI.updateRotation = true;
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is ComboAttack || toState is JumpSmash)
        {
            return true;
        }

        return false;
    }


    void RotateAndMoveTowardTarget()
    {
        Debug.Log("Normal");
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
        CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, 0);
        CrabAI.SetDestination(CrabFSM.PlayerController.transform.position);

        Vector3 localDesiredVelocity = CrabFSM.CrabController.transform.InverseTransformDirection(CrabAI.velocity);
        CrabFSM.Animator.SetFloat(AnimKeyMoveValue, localDesiredVelocity.z);
    }

    float RotateTowardTarget(Vector3 directionToTarget)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
    }
}