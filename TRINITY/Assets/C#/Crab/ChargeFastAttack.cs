using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ChargeFastAttack : CrabState
{
    [SerializeField] float ChargeTime;
    [SerializeField] float DashSpeed;
    [SerializeField] float PedictionMultiplier;
    [SerializeField] float DashDuration;

    [SerializeField] Collider CapCollider;

    private Vector3 predictedPosition;
    private bool isCharging = false;
    private bool isDashing = false;
    private float originerSpeed;

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
        originerSpeed = CrabFSM.CrabController.AI.speed;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (isCharging || isDashing)
        {
            PredictTargetPosition();
        }
        if (isCharging)
        {
            Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
            RotateTowardTarget(faceDirection);
        }
        if (!isCharging && !isDashing)
        {
            StartCoroutine(ChargeAndDash());
        }

        if (isDashing)
        {
            CrabFSM.Animator.SetBool("Release", true);
            CrabFSM.CrabController.AI.speed = DashSpeed;
            CrabFSM.Animator.SetFloat("Multiplier", DashSpeed/2);
            CrabFSM.CrabController.AI.SetDestination(predictedPosition);
        }
    }
    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CapCollider.enabled = false;
    }

    public override bool CheckExitTransition(IState toState)
    {
        if (toState is Death || toState is Pursue)
        {
            return true;
        }
        return false;
    }
    private void RotateTowardTarget(Vector3 directionToTarget)
    {
        Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
        CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation, targetRotation, 2 * Time.deltaTime);
    }
    private void PredictTargetPosition()
    {
        Rigidbody targetRb = CrabFSM.PlayerController.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 targetVelocity = targetRb.velocity;
            predictedPosition = CrabFSM.PlayerController.transform.position + targetVelocity * PedictionMultiplier;
        }
        else
        {
            predictedPosition = CrabFSM.PlayerController.transform.position; // Default to current position if no velocity
        }
    }
    private IEnumerator ChargeAndDash()
    {
        isCharging = true;

        yield return new WaitForSeconds(ChargeTime);

        isCharging = false;
        isDashing = true;

        yield return new WaitForSeconds(DashDuration);
        CrabFSM.Animator.SetBool("Release", false);
        CrabFSM.Animator.SetFloat("Multiplier", originerSpeed);

        isDashing = false;
        CrabFSM.CrabController.AI.speed = originerSpeed;
    }

    private void OnDrawGizmos()
    {
        if (isCharging || isDashing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(predictedPosition, 1f);
        }
    }

}
