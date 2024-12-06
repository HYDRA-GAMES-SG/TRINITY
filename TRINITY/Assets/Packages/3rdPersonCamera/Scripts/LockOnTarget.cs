// <copyright project="ThirdPersonCamera" file="LockOnTarget.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController)), RequireComponent(typeof(FreeForm))]
    [DefaultExecutionOrder(80)]
    public class LockOnTarget : MonoBehaviour
    {
        [FormerlySerializedAs("followTarget")] [Header("Basic settings")] [Tooltip("When not null, the camera will align itself to focus the Follow Target")]
        public Targetable FollowTarget;

        [FormerlySerializedAs("castRadius")] [Tooltip("The radius/units around the target in which targets will be acquired.")] 
        public float CastRadius = 30.0f;

        [FormerlySerializedAs("targetLayer")] [Tooltip("The layer which should be used for acquiring targets. Usually Enemies, NPCs, ...")]
        public LayerMask TargetLayer;

        [FormerlySerializedAs("rotationSpeed")] [Tooltip("How fast the camera should align to the Follow Target")]
        public float RotationSpeed = 3.0f;

        [Tooltip("Applies an additional vector to the target position to tilt the camera")]
        public Vector3 tiltVector;

        [FormerlySerializedAs("normalizeHeight")] [Tooltip("Ignores the resulting height difference from the direction to the target")]
        public bool NormalizeHeight;

        [FormerlySerializedAs("disableTime")] [Tooltip("The default time in seconds the script will disable 'Smooth Pivot' when the player has input in FreeForm")]
        public float DisableTime = 1.5f;

        [Header("Targetables sorting settings")]
        [FormerlySerializedAs("defaultDistance")]  [Tooltip("A default distance value to handle targetables sorting. Increase when game world and distances are very large")]
        public float DefaultDistance = 10.0f;

        [FormerlySerializedAs("angleWeight")] [Tooltip("A weight value to handle targetables sorting. Increase to prefer angles over distance")]
        public float AngleWeight = 1.0f;

        [FormerlySerializedAs("distanceWeight")] [Tooltip("A weight value to handle targetables sorting. Increase to prefer distance over angles")]
        public float DistanceWeight = 1.0f;

        [Header("Extra settings")] 
        [FormerlySerializedAs("forwardFromTarget")] [Tooltip("Use the forward vector of the target to find the nearest Targetable. Default is the camera forward vector")]
        public bool ForwardFromTarget;

        [FormerlySerializedAs("customInput")] [Tooltip("Inform the script of using a custom input method to set the CameraInputLockOn model")]
        public bool CustomInput;

        [FormerlySerializedAs("maxAllocatedTargets")] [Tooltip("The amount of maximum targets that will be acquired for the sphere overlap cast.")]
        public int MaxAllocatedTargets = 20;

#if TPC_DEBUG
        public bool debugLockedOnTargets;
#endif

        private CameraController cameraController;
        private FreeForm freeFormComponent;
        private Follow followComponent;
        private SortTargetables sortTargetsMethod;

        private CameraInputLockOn inputLockOn;

        private float currentDisableTime;

        private bool hasFollow;
        private Collider[] availableTargets;
        private List<TargetableWithDistance> targetsCollector;

        private void Start()
        {
            currentDisableTime = 0.0f;

            availableTargets = new Collider[MaxAllocatedTargets];
            targetsCollector = new List<TargetableWithDistance>(MaxAllocatedTargets);

            cameraController = GetComponent<CameraController>();
            freeFormComponent = GetComponent<FreeForm>();
            followComponent = GetComponent<Follow>();
            sortTargetsMethod = new SortTargetables(); // init the sort method

            hasFollow = followComponent != null;

            if (!CustomInput)
            {
#if ENABLE_INPUT_SYSTEM
                var lookup = GetComponent<CameraInputSampling>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling not found. Consider adding it to get input sampling or enable customInput to skip this message");
#else
                var lookup = GetComponent<CameraInputSampling_LockOn>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling_LockOn not found on " + transform.name + ". Consider adding it to get input sampling or enable customInput to skip this message");
#endif
            }
        }

        public CameraInputLockOn GetInput()
        {
            return inputLockOn;
        }

        public void UpdateInput(CameraInputLockOn newInput)
        {
            inputLockOn = newInput;
        }

        private void Update()
        {
            if (cameraController.Target == null)
                return;

            ref var state = ref cameraController.GetControllerState();
            var input = freeFormComponent.GetInput();

            bool requestLockOnTarget = inputLockOn.LockOnTarget || inputLockOn.ForceCycle;

            if (input.HasInput() && !requestLockOnTarget)
            {
                currentDisableTime = DisableTime;
                return;
            }

            var avatarPosition = cameraController.Target.transform.position;

            if (requestLockOnTarget)
            {
                if (FollowTarget == null || inputLockOn.ForceCycle)
                {
                    // find a viable target   

                    Vector3 forward = ForwardFromTarget ? cameraController.Target.transform.forward : cameraController.transform.forward;

                    var hitCount = Physics.OverlapSphereNonAlloc(avatarPosition, CastRadius, availableTargets, TargetLayer);

                    if (hitCount > 0)
                    {
                        UpdateTargetablesList(hitCount, avatarPosition, forward);

                        int targetIndex = Cycle(inputLockOn.CycleIndex, targetsCollector.Count);

                        if (targetsCollector[targetIndex].target == FollowTarget)
                        {
                            targetIndex = Cycle(inputLockOn.CycleIndex > 0 ? inputLockOn.CycleIndex + 1 : inputLockOn.CycleIndex - 1, targetsCollector.Count);
                        }

                        FollowTarget = targetsCollector[targetIndex].target;
                        currentDisableTime = 0;
                    }
                }
                else
                {
                    // clear locked on target
                    FollowTarget = null;

                    if (hasFollow)
                    {
                        followComponent.EnableFollow = true;
                    }
                }
            }
            
            if (currentDisableTime > 0)
            {
                cameraController.SmoothPivot = false;
                currentDisableTime -= Time.deltaTime;
                return;
            }

            if (!cameraController.SmoothPivot)
            {
                cameraController.SmoothPivot = true;
            }
            
            bool isStationaryFree = freeFormComponent.StationaryModeHorizontal == StationaryModeType.Free && freeFormComponent.StationaryModeVertical == StationaryModeType.Free;
            
            if (FollowTarget != null)
            {
                // target acquired
                
                Vector3 lockOnTargetPosition = FollowTarget.transform.position + FollowTarget.Offset;

                if (isStationaryFree)
                {
                    if (hasFollow)
                    {
                        followComponent.EnableFollow = false;
                    }

                    Quaternion tmpRotation;
                    
                    if (state.CameraNormalMode)
                    {
                        Vector3 dirToTarget = lockOnTargetPosition - (avatarPosition + cameraController.OffsetVector);
                            
                        if (NormalizeHeight)
                            dirToTarget.y = 0;
                            
                        tmpRotation = Quaternion.Slerp(state.CameraRotation, Quaternion.LookRotation(dirToTarget, Vector3.up), Time.deltaTime * RotationSpeed);
                    }
                    else
                    {
                        Vector3 dirToTarget = lockOnTargetPosition - transform.position;
                            
                        if (NormalizeHeight)
                            dirToTarget.y = 0;
                            
                        tmpRotation = Quaternion.Slerp(state.CameraRotation * state.SmartPivotRotation, Quaternion.LookRotation(dirToTarget, Vector3.up),
                            Time.deltaTime * RotationSpeed);
                    }
                    
                    var tmpEulerAngles = tmpRotation.eulerAngles;
                         
                    //Debug.Log($"{tmpEulerAngles}");
                    
                    // normalize angle
                    if (tmpEulerAngles.x > 180)
                        tmpEulerAngles.x -= 360.0f;
                        
                    tmpEulerAngles.x = MathHelper.Clamp(tmpEulerAngles.x, freeFormComponent.MinimumAngle, freeFormComponent.MaximumAngle);
                    
                    state.CameraAngles = new Vector3(tmpEulerAngles.x, tmpEulerAngles.y, 0);
                }
                else
                {
                    Vector3 dirToTarget = lockOnTargetPosition - (avatarPosition + cameraController.OffsetVector);

                    if (NormalizeHeight)
                        dirToTarget.y = 0;

                    var toRotation = Quaternion.Inverse(state.CameraRotation) * Quaternion.LookRotation(dirToTarget, Vector3.up);
                        
                    //Debug.DrawLine(transform.position, transform.position + to * 10);

                    state.PivotRotation = toRotation;
                }
            }
            else
            {
                if (!isStationaryFree)
                {
                    state.PivotRotation = Quaternion.identity;
                }
            }
        }

        private void UpdateTargetablesList(int hitCount, Vector3 avatarPosition, Vector3 forward)
        {
            targetsCollector.Clear();

            for (var i = 0; i < hitCount; i++)
            {
                var target = availableTargets[i];
                if (!target.TryGetComponent<Targetable>(out var targetable))
                    continue;

                var dirToTarget = (target.transform.position - avatarPosition);
                var distToTarget = dirToTarget.magnitude;
                var angleToTarget = Vector3.Angle(forward, dirToTarget);

                // calculate normalized score for sorting
                float score = ((Mathf.Abs(angleToTarget) / 180.0f) * AngleWeight) + ((distToTarget / DefaultDistance) * DistanceWeight);

                var twd = new TargetableWithDistance()
                {
                    target = targetable,
                    distance = distToTarget,
                    angle = angleToTarget,
                    score = score
                };

                targetsCollector.Add(twd);
            }

            targetsCollector.Sort(sortTargetsMethod);
        }

        private static int Cycle(int direction, int max)
        {
            int index = 0;
            if (direction > 0)
            {
                for (int i = 0; i < direction; i++)
                {
                    index++;

                    if (index >= max)
                        index = 0;
                }
            }
            else
            {
                direction = -direction;
                for (int i = 0; i < direction; i++)
                {
                    index--;

                    if (index < 0)
                        index = max - 1;
                }
            }

            return index;
        }
    }
}