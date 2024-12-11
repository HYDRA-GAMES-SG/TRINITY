using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CrabDemo : CrabState
{
    [Header("AI Setting")]
    [SerializeField] float MoveSpeed = 7;
    [SerializeField] float StopDistance = 9;
    [SerializeField] float RotateSpeed = 2;
    [SerializeField] float ThresholdAngle = 5;

    [Header("Tiredness Settings")]
    [SerializeField] float TiredWaitTime = 2f;
    [SerializeField] float PursueTime = 4f;

    NavMeshAgent CrabAI;
    private bool bTired = false;
    private Coroutine tirednessCoroutine;

    private string AnimKeyTurnDirection = "RotateDirection";
    private string AnimKeyMoveValue = "MoveValue";

    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        CrabAI = CrabFSM.CrabController.AI;

        CrabAI.speed = MoveSpeed;
        CrabAI.stoppingDistance = StopDistance;
        CrabAI.updateRotation = false;

        // Start the tiredness cycle when entering this state
        bTired = false;
        StartTirednessCycle();
    }

    private void StartTirednessCycle()
    {
        // Stop any existing coroutine
        if (tirednessCoroutine != null)
        {
            StopCoroutine(tirednessCoroutine);
        }

        // Start a new coroutine to manage tiredness cycle
        tirednessCoroutine = StartCoroutine(TirednessCycle());
    }

    private IEnumerator TirednessCycle()
    {
        while (true)
        {
            // Wait state
            bTired = true;
            yield return new WaitForSeconds(TiredWaitTime);

            // Pursue state
            bTired = false;
            yield return new WaitForSeconds(PursueTime);
        }
    }

    public override void UpdateBehaviour(float dt)
    {
        if (!bTired)
        {
            RotateAndMoveTowardTarget();
        }
        else
        {
            // Stop moving when tired
            CrabAI.ResetPath();
            CrabFSM.Animator.SetFloat(AnimKeyMoveValue, 0);
            CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, 0);
        }
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabAI.updateRotation = true;

        // Stop the tiredness coroutine when exiting the state
        if (tirednessCoroutine != null)
        {
            StopCoroutine(tirednessCoroutine);
        }
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is NormalAttack || toState is ComboAttack || toState is JumpSmash || toState is RoarIceSpray || toState is ChargeFastAttack || toState is JumpAway || toState is IcePhaseRoar || toState is GetHit || toState is Death)
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
        float angleToTarget = RotateTowardTarget(directionToTarget, RotateSpeed);

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

    private float RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
    }
}