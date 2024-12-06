// <copyright project="ThirdPersonCamera" file="Follow.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController))]
    [DefaultExecutionOrder(60)]
    public class Follow : MonoBehaviour
    {
        [FormerlySerializedAs("follow")]
        [Header("Basic settings")]
        [Tooltip("Enables/Disables the follow mode")]
        public bool EnableFollow = true;

        [FormerlySerializedAs("followMode")] [Tooltip("Align to the target Forward vector or align to LookAt.")]
        public FollowMode FollowMode = FollowMode.TargetForward;
        [FormerlySerializedAs("rotationSpeed")] [Tooltip("How fast the camera should align to the transform.forward of the target.")]
        public float RotationSpeed = 7.0f;
        [FormerlySerializedAs("tiltVector")] [Tooltip("Only works for FollowMode.TargetForward!\nApplies an additional vector to tilt the camera in place. Useful when the camera should always have a certain angle while following.")]
        public Vector3 TiltVector;
        [FormerlySerializedAs("disableTime")] [Tooltip("The default time in seconds the script will be disabled when the player has input in FreeForm.")]
        public float DisableTime = 1.5f;
        
        [FormerlySerializedAs("enablePreemptiveSweeping")]
        [Header("Preemptive Sweeping")]
        [Tooltip("Enable preemptive sweeping.")]
        public bool EnablePreemptiveSweeping;
        [FormerlySerializedAs("sweepSpeed")] [Tooltip("How fast the camera should align to the target.forward when sweeping.")]
        public float SweepSpeed = 1.0f;

        [FormerlySerializedAs("alignOnSlopes")]
        [Header("Slope aligning")]
        [Tooltip("Enables/Disables automatic alignment of the camera when the target is moving on upward or downward slopes.")]
        public bool AlignOnSlopes = true;
        [FormerlySerializedAs("rotationSpeedSlopes")] [Tooltip("The speed at which the camera lerps to the adjusted slope rotation.")]
        public float RotationSpeedSlopes = 0.5f;
        [FormerlySerializedAs("alignDirectionDownwards")] [Tooltip("Set the camera to rotate downwards instead of upwards when the target hits a slope.")]
        public bool AlignDirectionDownwards;
        [Tooltip("The layer which should be used for the ground/slope checks. Usually just the Default layer.")]
        public LayerMask GroundLayerMask;

        [FormerlySerializedAs("lookBackwards")]
        [Header("Backward motion aligning")]
        [Tooltip("Enables/Disables looking backwards")]
        public bool LookBackwards;
        [FormerlySerializedAs("checkMotionForBackwards")] [Tooltip("Enables/Disables automatic checking when the camera should look back.")]
        public bool CheckMotionForBackwards = true;
        [FormerlySerializedAs("backwardsMotionThreshold")] [Tooltip("The minimum magnitude of the motion vector when the camera should consider looking back.")]
        public float BackwardsMotionThreshold = 0.05f;
        [FormerlySerializedAs("angleThreshold")] [Tooltip("The minimum angle when the camera should consider looking back.")]
        public float AngleThreshold = 170.0f;

        private float currentDisableTime;
        private CameraController cameraController;
        private FreeForm freeForm;
        private bool hasFreeFormComponent;

        // state variables
        private Vector3 forwardState;
        private float lookAtSweeping;
        private float lookAtSweepingCooldown;


        private void Start()
        {
            cameraController = GetComponent<CameraController>();
            freeForm = GetComponent<FreeForm>();

            hasFreeFormComponent = freeForm != null;

            if (cameraController == null)
            {
                Debug.LogError("No CameraController was found for Follow!");
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (cameraController.Target == null)
                return;
            
            ref var state = ref cameraController.GetControllerState();
            
            if (hasFreeFormComponent)
            {
                var input = freeForm.GetInput();
                if (input.InputFreeLook)
                {
                    currentDisableTime = DisableTime;
                }
            }
            
            if (cameraController.DesiredDistance == 0 || state.Distance == 0 || state.DesiredPosition.sqrMagnitude == 0)
                return;

            if (currentDisableTime > 0)
            {
                currentDisableTime -= Time.deltaTime;

                if (currentDisableTime < 0)
                    forwardState = state.CameraRotation * Vector3.forward;
                
                return;
            }

            if (!EnableFollow)
            {
                cameraController.SmoothPivot = false;
                return;
            }

            cameraController.SmoothPivot = true;

            var targetTransform = cameraController.Target.transform;
            Vector3 offsetVectorTransformed;
            Vector3 targetPosWithOffset;

            if (cameraController.SmoothTargetMode)
            {
                offsetVectorTransformed = targetTransform.rotation * cameraController.OffsetVector;
                var smoothPosition = MathHelper.Lerp(cameraController.GetSmoothedTargetPosition(), targetTransform.position, cameraController.SmoothTargetValue * Time.deltaTime);
                targetPosWithOffset = (smoothPosition + offsetVectorTransformed);
            }
            else
            {
                offsetVectorTransformed = targetTransform.rotation * cameraController.OffsetVector;
                targetPosWithOffset = (targetTransform.position + offsetVectorTransformed);
            }
            
            if (Physics.SphereCast(new Ray(targetPosWithOffset, Vector3.up), cameraController.CollisionDistance * 2, cameraController.CollisionDistance * 2, cameraController.CollisionLayer))
            {
                // LookAt has problems when the ceiling is < targetOffset + collisionDistance
                // it zooms in, very close and with relative movement the camera panning is too fast
                // so disable following in that case
                return;
            }

            if (CheckMotionForBackwards)
            {
                Vector3 motionVector = targetTransform.position - state.PrevPosition;

                if (motionVector.magnitude > BackwardsMotionThreshold)
                {
                    float angle = Vector3.Angle(motionVector, forwardState);

                    LookBackwards = angle > AngleThreshold;
                }
            }

            Quaternion toRotation;

            if (FollowMode == FollowMode.LookAt)
            {
                if (state.OcclusionHitHappened)
                {
                    lookAtSweeping = 0;
                    lookAtSweepingCooldown = 0;
                }
                
                // very peculiar and important calculation of the forward, the targetPos is the new one, the camera pos is an old one
                // that way we can calculate an angle that the camera stays in place
                // we also use DesiredPosition because that's more stable than just PrevPosition
                var tmpForward = (targetPosWithOffset - state.DesiredPosition).normalized;
                //Debug.Log($"cameraForward {cameraForward} tmpForward {tmpForward} diff {tmpForward - cameraForward}");
                
                if (EnablePreemptiveSweeping)
                {
                    if (lookAtSweepingCooldown == 0)
                    {
                        var sweepDir = (state.PrevPosition - targetPosWithOffset).normalized;
                        var targetForward = targetTransform.forward;
                        var isSweepForward = Vector3.Dot(targetForward, sweepDir) < 0;

                        //Debug.Log($"sweepforward {dot}");
                        if (isSweepForward)
                        {
                            if (!state.OcclusionHitHappened &&
                                Physics.SphereCast(new Ray(targetPosWithOffset, sweepDir), cameraController.CollisionDistance * 2, cameraController.DesiredDistance, cameraController.CollisionLayer))
                            {
                                //Debug.Log("Follow repos");
                                forwardState = MathHelper.Slerp(forwardState, targetForward, Time.deltaTime * SweepSpeed, targetTransform.up);
                                lookAtSweeping = 120;
                            }
                        }

                        if (lookAtSweeping > 0)
                        {
                            //Debug.Log("sweep to target forward " + followState.lookAtSweeping);
                            forwardState = MathHelper.Slerp(forwardState, targetForward, Time.deltaTime * SweepSpeed, targetTransform.up);
                            lookAtSweeping--;
                        }
                        else
                        {
                            //Debug.Log("normal look at follow");
                            forwardState = tmpForward;
                        }
                    }
                    else
                    {
                        lookAtSweepingCooldown--;
                        forwardState = tmpForward;
                    }
                }
                else
                {
                    forwardState = tmpForward;
                }

                toRotation = Quaternion.LookRotation((!LookBackwards ? forwardState : -forwardState), Vector3.up);
            }
            else
            {
                // FollowMode.TargetForward path
                forwardState = targetTransform.forward;
                toRotation = Quaternion.LookRotation((!LookBackwards ? forwardState : -forwardState) + TiltVector, Vector3.up);
            }
            
            state.PivotRotation = Quaternion.Slerp(state.PivotRotation, Quaternion.identity, Time.deltaTime * RotationSpeed);

            if (FollowMode == FollowMode.LookAt)
            {
                state.CameraAngles.y = toRotation.eulerAngles.y;
                
                if (AlignOnSlopes)
                {
                    var newAngle = GetSlopedRotation(targetPosWithOffset);
                    state.CameraAngles.x = Mathf.Lerp(state.CameraAngles.x, newAngle, Time.deltaTime * RotationSpeedSlopes);
                }
            }
            else
            {
                if (AlignOnSlopes)
                {
                    var angle = GetSlopedRotation(targetPosWithOffset);
                    var tmpRotation = Quaternion.AngleAxis(angle, transform.right);
                    toRotation *= tmpRotation;
                }
                
                state.CameraAngles = CameraController.StabilizeSlerpRotation(state.CameraRotation * state.SmartPivotRotation, toRotation, Time.deltaTime * RotationSpeed).eulerAngles;
            }
        }

        private Vector3 GetGroundNormal(Vector3 position)
        {
            Vector3 upVector = Vector3.up;

            if (Physics.Raycast(position, Vector3.down, out var raycastHit, 100.0f, GroundLayerMask)) // if the range of 100.0 is not enough, increase the value
            {
                upVector = raycastHit.normal;
            }

            return upVector;
        }

        private float GetSlopedRotation(Vector3 position)
        {
            var upVector = GetGroundNormal(position);
            float angle = Vector3.Angle(Vector3.up, upVector);
            
            if (AlignDirectionDownwards)
                angle = -angle;

            return angle;
        }
    }
}