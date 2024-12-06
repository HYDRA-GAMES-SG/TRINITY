// <copyright project="ThirdPersonCamera" file="FreeForm.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController))]
    [DefaultExecutionOrder(90)]
    public class FreeForm : MonoBehaviour
    {
        [FormerlySerializedAs("cameraEnabled")]
        [Header("Basic settings")]
        [Tooltip("Enables/Disables the camera rotation updates. Useful when you want to lock the camera in place or to turn off the camera, for example, when hovering an interface element with the mouse")]
        public bool CameraEnabled = true;
        
        [FormerlySerializedAs("minDistance")] [Tooltip("Sets a min distance for zooming in")]
        public float MinDistance = 1;

        [FormerlySerializedAs("maxDistance")] [Tooltip("Sets a max distance for zooming out")]
        public float MaxDistance = 5;

        [Tooltip("The minimum angle you can look down")]
        public float MinimumAngle = -90.0f;

        [Tooltip("The maximum angle you can look up")]
        public float MaximumAngle = 87.0f;

        [FormerlySerializedAs("stationaryModeHorizontal")] [Header("Stationary settings")] [Tooltip("Sets a stationary mode for the horizontal axis")]
        public StationaryModeType StationaryModeHorizontal = StationaryModeType.Free;

        [FormerlySerializedAs("stationaryModeVertical")] [Tooltip("Sets a stationary mode for the vertical axis")]
        public StationaryModeType StationaryModeVertical = StationaryModeType.Free;

        [FormerlySerializedAs("stationaryMaxAngleHorizontal")] [Tooltip("Maximum angle for the horizontal axis")]
        public float StationaryMaxAngleHorizontal = 30.0f;

        [FormerlySerializedAs("stationaryMaxAngleVertical")] [Tooltip("Maximum angle for the vertical axis")]
        public float StationaryMaxAngleVertical = 30.0f;

        [FormerlySerializedAs("smoothing")] [Header("Extra settings")] [Tooltip("Enables smoothing for the camera rotation when looking around - Beware, introduces interpolation lag")]
        public bool Smoothing;

        [FormerlySerializedAs("smoothSpeed")] [Tooltip("The speed at which the camera will lerp to the final rotation")]
        public float SmoothSpeed = 3.0f;

        [FormerlySerializedAs("forceCharacterDirection")] [Tooltip("Sets the targets y-axis to have the same y rotation as the camera.")]
        public bool ForceTargetDirection;

        [FormerlySerializedAs("stabilizeRotation")] [Tooltip("In case the rotation starts to drift in the z-axis, enable this to stabilize the rotation.")]
        public bool StabilizeRotation;
        
        [FormerlySerializedAs("lookingBackwardsEnabled")] [Tooltip("Enables looking backward with pressing middle mouse button")]
        public bool LookingBackwardsEnabled;

        [FormerlySerializedAs("customInput")] [Tooltip("Inform the script of using a custom input script to set the CameraInputFreeForm model")]
        public bool CustomInput;

        

        private CameraController cameraController;

        private bool lookingBackwards;
        private CameraInputFreeForm inputFreeForm;

        public Vector3 exposed;

        public void Start()
        {
            cameraController = GetComponent<CameraController>();

            if (!CustomInput)
            {
#if ENABLE_INPUT_SYSTEM
                var lookup = GetComponent<CameraInputSampling>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling not found. Consider adding it to get input sampling or enable customInput to skip this message");
#else
                var lookup = GetComponent<CameraInputSampling_FreeForm>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling_FreeForm not found on " + transform.name + ". Consider adding it to get input sampling or enable customInput to skip this message");
#endif
            }
        }

        public CameraInputFreeForm GetInput()
        {
            return inputFreeForm;
        }

        public void UpdateInput(CameraInputFreeForm newInput)
        {
            inputFreeForm = newInput;
        }

        public void LateUpdate()
        {
            if (!CameraEnabled)
                return;

            if (cameraController.Target == null)
                return;

            ref var state = ref cameraController.GetControllerState();
            Vector3 targetAngles = cameraController.Target.rotation.eulerAngles;
            
            if (inputFreeForm.InputFreeLook)
            {
                if (LookingBackwardsEnabled)
                {
                    if (inputFreeForm.MiddleMouseButtonPressed && !lookingBackwards) // flip y-axis when pressing middle mouse button
                    {
                        lookingBackwards = true;
                        state.CameraRotation = Quaternion.Euler(state.CameraAngles.x, state.CameraAngles.y + 180.0f, state.CameraAngles.z);
                    }
                    else if (!inputFreeForm.MiddleMouseButtonPressed && lookingBackwards)
                    {
                        lookingBackwards = false;
                        state.CameraRotation = Quaternion.Euler(state.CameraAngles.x, state.CameraAngles.y - 180.0f, state.CameraAngles.z);
                    }
                }

                if (cameraController.DesiredDistance < 0)
                    cameraController.DesiredDistance = 0;

                var tmpCameraInput = inputFreeForm.CameraInput;

                // stationary feature

                #region Stationary Feature
                {
                    var tmpPivotRotation = state.PivotRotation;
                    var tmpPivotRotationEuler = tmpPivotRotation.eulerAngles;

                    if (tmpPivotRotationEuler.x > 180)
                        tmpPivotRotationEuler.x -= 360;
                    if (tmpPivotRotationEuler.y > 180)
                        tmpPivotRotationEuler.y -= 360;

                    if (StationaryModeVertical != StationaryModeType.Free)
                    {
                        if (StationaryModeVertical != StationaryModeType.Fixed)
                            tmpPivotRotationEuler.x -= tmpCameraInput.y;

                        tmpCameraInput.y = 0;

                        if (StationaryModeVertical == StationaryModeType.Limited)
                        {
                            if (tmpPivotRotationEuler.x < -StationaryMaxAngleVertical)
                                tmpPivotRotationEuler.x = -StationaryMaxAngleVertical;
                            if (tmpPivotRotationEuler.x > StationaryMaxAngleVertical)
                                tmpPivotRotationEuler.x = StationaryMaxAngleVertical;
                        }
                        else if (StationaryModeVertical == StationaryModeType.RotateWhenLimited)
                        {
                            if (tmpPivotRotationEuler.x > StationaryMaxAngleVertical)
                            {
                                tmpCameraInput.y = tmpPivotRotationEuler.x - StationaryMaxAngleVertical;
                                tmpPivotRotationEuler.x = StationaryMaxAngleVertical;
                            }

                            if (tmpPivotRotationEuler.x < -StationaryMaxAngleVertical)
                            {
                                tmpCameraInput.y = tmpPivotRotationEuler.x + StationaryMaxAngleVertical;
                                tmpPivotRotationEuler.x = -StationaryMaxAngleVertical;
                            }
                        }
                    }

                    if (StationaryModeHorizontal != StationaryModeType.Free)
                    {
                        if (StationaryModeHorizontal != StationaryModeType.Fixed)
                            tmpPivotRotationEuler.y += tmpCameraInput.x;

                        tmpCameraInput.x = 0;

                        if (StationaryModeHorizontal == StationaryModeType.Limited)
                        {
                            if (tmpPivotRotationEuler.y < -StationaryMaxAngleHorizontal)
                                tmpPivotRotationEuler.y = -StationaryMaxAngleHorizontal;
                            if (tmpPivotRotationEuler.y > StationaryMaxAngleHorizontal)
                                tmpPivotRotationEuler.y = StationaryMaxAngleHorizontal;
                        }
                        else if (StationaryModeHorizontal == StationaryModeType.RotateWhenLimited)
                        {
                            if (tmpPivotRotationEuler.y > StationaryMaxAngleHorizontal)
                            {
                                tmpCameraInput.x = tmpPivotRotationEuler.y - StationaryMaxAngleHorizontal;
                                tmpPivotRotationEuler.y = StationaryMaxAngleHorizontal;
                            }

                            if (tmpPivotRotationEuler.y < -StationaryMaxAngleHorizontal)
                            {
                                tmpCameraInput.x = tmpPivotRotationEuler.y + StationaryMaxAngleHorizontal;
                                tmpPivotRotationEuler.y = -StationaryMaxAngleHorizontal;
                            }
                        }
                    }

                    state.PivotRotation = Quaternion.Euler(tmpPivotRotationEuler);
                }
                #endregion
                
                if (Smoothing)
                {
                    state.CameraAngles.x = Mathf.Lerp(state.CameraAngles.x, state.CameraAngles.x - tmpCameraInput.y, Time.deltaTime * SmoothSpeed);
                    state.CameraAngles.y = Mathf.Lerp(state.CameraAngles.y, state.CameraAngles.y + tmpCameraInput.x, Time.deltaTime * SmoothSpeed);
                }
                else
                {
                    state.CameraAngles.x -= tmpCameraInput.y;
                    state.CameraAngles.y += tmpCameraInput.x;
                }
            }
            
            if (!cameraController.DisableZooming)
            {
                // sample mouse scrollwheel for zooming in/out
                if (inputFreeForm.ZoomInput < 0) // back
                {
                    cameraController.DesiredDistance = Math.Min(cameraController.DesiredDistance + cameraController.ZoomOutStepValue, MaxDistance);
                }

                if (inputFreeForm.ZoomInput > 0) // forward
                {
                    cameraController.DesiredDistance = Math.Max(cameraController.DesiredDistance - cameraController.ZoomOutStepValue, MinDistance);
                }
            }

            // normalize angle
            if (state.CameraAngles.x > 180)
                state.CameraAngles.x -= 360.0f;
            
            // Prevent camera flipping
            state.CameraAngles.x = MathHelper.Clamp(state.CameraAngles.x, MinimumAngle, MaximumAngle);

            var eulerAngles = new Vector3(state.CameraAngles.x, state.CameraAngles.y, 0);

            exposed = state.CameraAngles;

            bool heightCheck = transform.position.y > cameraController.Target.position.y + cameraController.OffsetVector.y;

            if (!cameraController.SmartPivot || state.CameraNormalMode && (!state.GroundHit || heightCheck))
            {
                // normal mode    

                state.CameraRotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                // smart pivot mode
                if (!state.SmartPivotInitialized)
                {
                    InitSmartPivot(ref state);
                }

                eulerAngles.x = state.SmartPivotStartingX;
                state.CameraRotation = Quaternion.Euler(eulerAngles);
                state.SmartPivotRotation = Quaternion.AngleAxis(state.CameraAngles.x - state.SmartPivotStartingX, Vector3.right);

                if (state.CameraAngles.x > state.SmartPivotStartingX || (state.CameraAngles.x >= 0 && state.CameraAngles.x < 90))
                {
                    DisableSmartPivot(ref state);
                }
            }

            if (ForceTargetDirection && inputFreeForm.ForceTargetDirection)
            {
                cameraController.Target.rotation = Quaternion.Euler(targetAngles.x, state.CameraAngles.y, targetAngles.z);
            }

            if (StabilizeRotation)
            {
                transform.rotation = CameraController.StabilizeRotation(transform.rotation);
            }
        }

        public struct PositionAndRotation
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        public PositionAndRotation RotateAround(PositionAndRotation posAndRot, Vector3 point, Vector3 axis, float angle)
        {
            Vector3 position = posAndRot.position;
            var rot = Quaternion.AngleAxis(angle, axis);
            Vector3 vector3 = rot * (position - point);
            posAndRot.position = point + vector3;

            posAndRot.rotation *= (Quaternion.Inverse(posAndRot.rotation) * rot);

            return posAndRot;
        }

        private static void InitSmartPivot(ref CameraControllerState state)
        {
            state.SmartPivotInitialized = true;
            state.CameraNormalMode = false;
            state.SmartPivotRotation = Quaternion.identity;
            state.SmartPivotStartingX = state.CameraAngles.x;
        }

        private static void DisableSmartPivot(ref CameraControllerState state)
        {
            state.SmartPivotInitialized = false;
            state.CameraNormalMode = true;
            state.SmartPivotRotation = Quaternion.identity;
            state.SmartPivotStartingX = 0;
        }
    }
}