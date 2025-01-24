using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ChargeFastAttack : CrabState
{
    [Header("Charge and Dash Settings")]
    [SerializeField] float ChargeTime;
    [SerializeField] float DashSpeed;
    [SerializeField] float PedictionMultiplier;
    [SerializeField] float DashDuration;

    [SerializeField] Transform Indicator;
    [SerializeField] Vector3 targetScale;
    [SerializeField] Vector3 initialScale;
    [SerializeField] float scaleSpeed = 30f;
    [SerializeField] bool isScaling = false;

    [Header("Components")]
    [SerializeField] Collider CapCollider;

    private Vector3 PredictedPosition;
    private bool bIsCharging = false;
    private bool bIsDashing = false;
    private float StateTimer = 0f;
    bool AnimFinish = false;
    public override bool CheckEnterTransition(IState fromState)
    {
        return fromState is Pursue && CrabFSM.CrabController.CanCharageMoveFast && CrabFSM.CrabController.FacingTarget();
    }

    public override void EnterBehaviour(float dt, IState fromState)
    {
        AnimFinish = false;

        CrabFSM.CrabController.bCanChill = false;

        CrabFSM.CrabController.AI.enabled = false;
        bIsCharging = true;
        StateTimer = 0f;

        initialScale = Indicator.localScale;
        isScaling = false;
    }

    public override void PreUpdateBehaviour(float dt)
    {
    }

    public override void UpdateBehaviour(float dt)
    {
        if (bIsCharging)
        {
            isScaling = true;
            Indicator.gameObject.SetActive(true);
            PredictTargetPosition();
            Vector3 faceDirection = (CrabFSM.PlayerController.transform.position - CrabFSM.CrabController.transform.position).normalized;
            CrabFSM.CrabController.RotateTowardTarget(faceDirection, RotateSpeed);

            StateTimer += Time.deltaTime;
            if (StateTimer >= ChargeTime)
            {
                bIsCharging = false;
                bIsDashing = true;
                StateTimer = 0f;
                CapCollider.enabled = true;
                CrabFSM.Animator.applyRootMotion = true;
                CrabFSM.Animator.SetBool("Release", true);
                CrabFSM.Animator.SetFloat("Multiplier", DashSpeed);
            }
        }
        else if (bIsDashing)
        {
            Indicator.gameObject.SetActive(false);
            Vector3 start = CrabFSM.CrabController.transform.position + Vector3.up * CrabFSM.CrabController.AI.height;
            Debug.DrawRay(start, CrabFSM.CrabController.transform.forward * 10, Color.red);
            if (Physics.Raycast(start, CrabFSM.CrabController.transform.forward, 10, LayerMask.GetMask("Obstacle")))
            {
                bIsDashing = false;
                CapCollider.enabled = false;
                CrabFSM.Animator.SetBool("Release", false);
                CrabFSM.EnqueueTransition<Pursue>();
            }
            StateTimer += Time.deltaTime;
            if (StateTimer >= DashDuration)
            {
                bIsDashing = false;
                CapCollider.enabled = false;
                CrabFSM.Animator.SetBool("Release", false);
                AnimFinish = true;
                CrabFSM.EnqueueTransition<Pursue>();
            }
        }

        if (isScaling)
        {
            Indicator.localScale = Vector3.Lerp(Indicator.localScale, targetScale, Time.deltaTime / (1f / scaleSpeed));

            if (Mathf.Abs(Indicator.localScale.z - targetScale.z) < 0.01f)
            {
                Indicator.localScale = targetScale;
            }
        }
    }

    public override void PostUpdateBehaviour(float dt)
    {
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        CrabFSM.Animator.applyRootMotion = false;
        CrabFSM.CrabController.AI.enabled = true;
        CapCollider.enabled = false;
        CrabFSM.CrabController.CanCharageMoveFast = false;
        CrabFSM.CrabController.bCanChill = true;
        Indicator.localScale = initialScale;
        Indicator.gameObject.SetActive(false);
        isScaling = false;

    }

    public override bool CheckExitTransition(IState toState)
    {
        return toState is Pursue || toState is Death || (toState is Idle && AnimFinish);
    }
    private void PredictTargetPosition()
    {
        Rigidbody targetRb = CrabFSM.PlayerController.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 targetVelocity = targetRb.velocity;
            PredictedPosition = CrabFSM.PlayerController.transform.position + targetVelocity * PedictionMultiplier;
        }
        else
        {
            PredictedPosition = CrabFSM.PlayerController.transform.position;
        }
    }

    private void OnDrawGizmos()
    {
        if (bIsCharging || bIsDashing)
        {
            Gizmos.color = Color.red;
            Vector3 groundPosition = new Vector3(PredictedPosition.x, 0f, PredictedPosition.z);
            Gizmos.DrawSphere(groundPosition, 1f);
        }
    }

}
