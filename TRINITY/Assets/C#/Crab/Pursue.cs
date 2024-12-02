using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Pursue : CrabState
{
    [Header("AI Setting")]
    [SerializeField] float MoveSpeed = 7;
    [SerializeField] float StopDistance = 11;
    [SerializeField] float RotateSpeed = 2;
    [SerializeField] float ThresholdAngle = 5;

    [Header("Attack Range")]
    [SerializeField] float RangeAttack = 28;
    [SerializeField] float CloseAttackRange = 7;
    [SerializeField] float JumpAwayRange = 7;


    NavMeshAgent CrabAI;

    private string AnimKeyTurnDirection = "RotateDirection";
    private string AnimKeyMoveValue = "MoveValue";

    public override bool CheckEnterTransition(IState fromState)
    {
        if (fromState is ComboAttack || fromState is JumpSmash || fromState is RoarStun || fromState is NormalAttack || fromState is Jump)
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

        if (distanceToTarget >= RangeAttack && distanceToTarget <= RangeAttack + 2)
        {
            float random = Random.value;
            if (random <= 0.5f)
            {
                CrabFSM.EnqueueTransition<JumpSmash>();
            }
            else
            {
                CrabFSM.EnqueueTransition<RoarStun>();
            }
        }
        else if (distanceToTarget >= CloseAttackRange && distanceToTarget <= CloseAttackRange + 3)
        {
            CrabFSM.EnqueueTransition<ComboAttack>();
            CrabFSM.EnqueueTransition<NormalAttack>();
        }
        else if (distanceToTarget <= JumpAwayRange)
        {
            CrabFSM.EnqueueTransition<Jump>();
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
        if (toState is ComboAttack || toState is JumpSmash || toState is RoarStun || toState is NormalAttack || toState is Jump || toState is Death)
        {
            return true;
        }

        return false;
    }


    void RotateAndMoveTowardTarget()
    {
        Transform playerTransform = CrabFSM.PlayerController.transform;
        Transform crabTransform = CrabFSM.CrabController.transform;

        Vector3 directionToTarget = (playerTransform.position - crabTransform.position).normalized;
        float distanceToTarget = Vector3.Distance(playerTransform.position, crabTransform.position);
        float angleToTarget = RotateTowardTarget(directionToTarget);

        if (angleToTarget > ThresholdAngle)
        {
            if (distanceToTarget > StopDistance && angleToTarget <= ThresholdAngle + 30)
            {
                MoveTowardTarget(playerTransform.position);
            }
            else
            {
                TurnInPlace(directionToTarget);
            }
        }
        else
        {
            MoveTowardTarget(playerTransform.position);
        }
    }
    private void MoveTowardTarget(Vector3 targetPosition)
    {
        CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, 0);
        CrabAI.SetDestination(targetPosition);

        Vector3 localDesiredVelocity = CrabFSM.CrabController.transform.InverseTransformDirection(CrabAI.velocity);
        CrabFSM.Animator.SetFloat(AnimKeyMoveValue, localDesiredVelocity.z);
    }
    private void TurnInPlace(Vector3 directionToTarget)
    {
        float turnDirection = Mathf.Sign(Vector3.Cross(CrabFSM.CrabController.transform.forward, directionToTarget).y);
        CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, turnDirection);

        CrabAI.ResetPath();
    }
    private float RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
    }
}