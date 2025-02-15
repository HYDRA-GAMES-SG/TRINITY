using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Pursue : CrabState
{
    [Header("AI Setting")]
    [SerializeField] float ThresholdAngle = 5;

    [Header("Check Range")]
    [SerializeField] float RangeAttack = 28;
    [SerializeField] float CloseAttackRange = 6f;
    [SerializeField] float JumpAwayRange = 6f;
    [SerializeField] float ChargeFastMoveRange = 15;


    NavMeshAgent CrabAI;

    private string AnimKeyTurnDirection = "RotateDirection";
    private string AnimKeyMoveValue = "MoveValue";

    public override bool CheckEnterTransition(IState fromState)
    {
        return !(fromState is Death);
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabAI = CrabFSM.CrabController.AI;

        CrabAI.updateRotation = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }


    public override void UpdateBehaviour(float dt)
    {
        if (CrabFSM.PlayerController == null)
        {
            return;
        }

        if (CrabFSM.CrabController.CalculateGroundDistance() >= RangeAttack && CrabFSM.CrabController.CalculateGroundDistance() <= RangeAttack + 2) //between 28 - 30
        {
            float random = Random.value;
            if (random <= 0.5f)
            {
                CrabFSM.EnqueueTransition<JumpSmash>();
            }
            else
            {
                CrabFSM.EnqueueTransition<RoarIceSpray>();
            }
        }
        else if (CrabFSM.CrabController.CalculateGroundDistance() >= ChargeFastMoveRange && !(CrabFSM.CurrentState is ChargeFastAttack)/*&& CrabFSM.CrabController.CalculateGroundDistance() <= ChargeFastMoveRange + 2*/) //between 15 - 17
        {
            CrabFSM.EnqueueTransition<ChargeFastAttack>();

        }
        else if (CrabFSM.CrabController.CalculateGroundDistance() >= CloseAttackRange && CrabFSM.CrabController.CalculateGroundDistance() <= CrabAI.stoppingDistance) //between 6 - 8
        {
            CrabFSM.EnqueueTransition<ComboAttack>();
            CrabFSM.EnqueueTransition<NormalAttack>();
        }
        else if (CrabFSM.CrabController.CalculateGroundDistance() <= JumpAwayRange) //less then 6
        {
            CrabFSM.EnqueueTransition<JumpAway>();
        }

        RotateAndMoveTowardTarget();
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
        return true;
    }


    void RotateAndMoveTowardTarget()
    {
        Transform playerTransform = CrabFSM.PlayerController.transform;
        Transform crabTransform = CrabFSM.CrabController.transform;

        Vector3 directionToTarget = (playerTransform.position - crabTransform.position).normalized;
        float distanceToTarget = Vector3.Distance(playerTransform.position, crabTransform.position);
        float angleToTarget = RotateTowardTarget(directionToTarget, RotateSpeed);

        if (angleToTarget > ThresholdAngle)
        {
            if (distanceToTarget > CrabAI.stoppingDistance && angleToTarget <= ThresholdAngle + 30)
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
    private float RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
    }
}