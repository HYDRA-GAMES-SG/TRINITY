using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class JumpAway : CrabState
{
    [SerializeField] float JumpForce = 15f;
    [SerializeField] float JumpUpForce = 10f;

    [SerializeField] Rigidbody RigBody;
    NavMeshAgent CrabAI;

    [SerializeField] LayerMask GroundLayer;
    [SerializeField] float GroundCheckDistance = 0.2f;

    [SerializeField] string AnimKeyJumpForward = "JumpForward";
    [SerializeField] string AnimKeyJumpBackward = "JumpBackward";

    private Vector3 LandingPosFront;
    private Vector3 LandingPosBack;

    private Vector3[] trajectoryPoints;

    private Coroutine jumpCoroutine;

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
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }
        jumpCoroutine = StartCoroutine(HandleJump());
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {


    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        RigBody.isKinematic = true;
    }
    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Pursue || toState is Death)
        {
            return true;
        }
        return false;
    }
    private IEnumerator HandleJump()
    {
        CrabAI.enabled = false;
        CrabFSM.Animator.applyRootMotion = false;
        RigBody.isKinematic = false;

        Vector3 forwardDirection = CrabFSM.CrabController.transform.forward;
        CalculateLandingPosition(forwardDirection);

        bool canJumpBack = IsValidJumpPosition(LandingPosBack);
        bool canJumpFront = IsValidJumpPosition(LandingPosFront);
        Debug.Log(canJumpBack);
        Debug.Log(canJumpFront);
        if (!canJumpBack && !canJumpFront)
        {
            CrabFSM.EnqueueTransition<Pursue>();
        }
        else if (canJumpBack)
        {
            ApplyJumpForce(-forwardDirection);
            CrabFSM.Animator.SetBool(AnimKeyJumpBackward, true);
        }
        else if (canJumpFront)
        {
            ApplyJumpForce(forwardDirection);
            CrabFSM.Animator.SetBool(AnimKeyJumpForward, true);
        }
        yield return new WaitForSeconds(0.2f);

        while (!IsGrounded())
        {
            yield return null;
        }

        CrabFSM.Animator.SetBool(AnimKeyJumpBackward, false);
        CrabFSM.Animator.SetBool(AnimKeyJumpForward, false);
        CrabFSM.EnqueueTransition<Pursue>();
    }
    private void ApplyJumpForce(Vector3 direction)
    {
        RigBody.AddForce(CrabFSM.CrabController.transform.up * JumpUpForce, ForceMode.Impulse);
        RigBody.AddForce(direction * JumpForce, ForceMode.Impulse);
    }
    private bool IsGrounded()
    {
        return Physics.Raycast(RigBody.transform.position, Vector3.down, GroundCheckDistance, GroundLayer);
    }

    private void CalculateLandingPosition(Vector3 direction)
    {
        float mass = RigBody.mass;
        float g = Physics.gravity.magnitude;

        float verticalVelocity = JumpUpForce / mass;
        float horizontalVelocity = JumpForce / mass;

        float timeToApex = verticalVelocity / g;
        float totalTime = 2 * timeToApex;

        int resolution = 50; // Number of points in the trajectory
        trajectoryPoints = new Vector3[resolution];
        Vector3 startPosition = CrabFSM.CrabController.transform.position;
        Vector3 horizontalDirection = direction.normalized;
        float horizontalDistance = horizontalVelocity * totalTime;

        for (int i = 0; i < resolution; i++)
        {
            float t = (i / (float)(resolution - 1)) * totalTime; // Time step
            float x = horizontalVelocity * t;
            float y = verticalVelocity * t - 0.5f * g * t * t;

            trajectoryPoints[i] = startPosition + horizontalDirection * x + Vector3.up * y;
        }
        LandingPosFront = CrabFSM.CrabController.transform.position + horizontalDirection * horizontalDistance;
        LandingPosBack = CrabFSM.CrabController.transform.position + -horizontalDirection * horizontalDistance;
    }

    private bool IsValidJumpPosition(Vector3 targetPosition)
    {
        NavMeshHit navMeshHit;
        bool isOnNavMesh = NavMesh.SamplePosition(targetPosition, out navMeshHit, CrabAI.radius, NavMesh.AllAreas);

        Debug.Log($"Checking NavMesh for position {targetPosition}: {isOnNavMesh}");

        if (!isOnNavMesh)
            return false;

        // Check for obstacles in the path
        Vector3 start = CrabFSM.CrabController.transform.position + Vector3.up * 3.5f;
        Vector3 direction = navMeshHit.position - start;
        float distance = direction.magnitude;

        Debug.DrawRay(start, direction.normalized * distance, Color.red, 2.0f);

        if (Physics.Raycast(start, direction.normalized, distance, LayerMask.GetMask("Obstacle")))
        {
            Debug.Log("Obstacle detected in path!");
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //for (int i = 0; i < trajectoryPoints.Length - 1; i++)
        //{
        //    Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
        //}
        if (CrabAI != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(LandingPosFront, CrabAI.radius);
            Gizmos.DrawSphere(LandingPosBack, CrabAI.radius);

            Gizmos.color = Color.red;
            Vector3 start = CrabFSM.CrabController.transform.position;
            Vector3 direction = Vector3.down;
            Gizmos.DrawRay(start, direction * GroundCheckDistance);
        }
    }
}
