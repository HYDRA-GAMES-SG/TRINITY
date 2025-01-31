// <copyright project="ThirdPersonCamera" file="CameraController.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

//#define TPC_DEBUG // uncomment for more options and visualizing debug raycasts

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [DefaultExecutionOrder(100)]
    public class CameraController : MonoBehaviour
    {
        [FormerlySerializedAs("target")] [Tooltip("Set this to the transform the camera should follow, can be changed at runtime")]
        public Transform Target;

        [FormerlySerializedAs("desiredDistance")] [Header("Basic settings")] [Tooltip("The distance how far away the camera should be away from the target")]
        public float DesiredDistance = 5.0f;

        [FormerlySerializedAs("collisionDistance")] [Tooltip("Offset for the camera to not clip in objects")]
        public float CollisionDistance = 0.25f;

        [FormerlySerializedAs("offsetVector")] [Tooltip("Change this vector to offset the pivot the camera is rotating around")]
        public Vector3 OffsetVector = new Vector3(0, 1.0f, 0);

        [FormerlySerializedAs("cameraOffsetVector")] [Tooltip("Offset the camera in any axis without changing the pivot point")]
        public Vector3 CameraOffsetVector = new Vector3(0, 0, 0);

        [Tooltip("Completely disable zooming in/out manually")]
        public bool DisableZooming;

        [FormerlySerializedAs("zoomOutStepValue")] [Tooltip("The distance how fast the player can zoom in/out with each mousewheel event.")]
        public float ZoomOutStepValue = 1.0f;

        [FormerlySerializedAs("occludedZoomInSpeed")] [Tooltip("The speed, how fast the camera can zoom in automatically to the desired distance.")]
        public float OccludedZoomInSpeed = 60.0f;

        [FormerlySerializedAs("unoccludedZoomOutSpeed")] [Tooltip("The speed, how fast the camera can zoom out automatically to the desired distance.")]
        public float UnoccludedZoomOutSpeed = 7.0f;

        [FormerlySerializedAs("cameraOffsetChangeSpeed")] [Tooltip("How fast the offset can change.")]
        public float CameraOffsetChangeSpeed = 3.0f;

        [FormerlySerializedAs("cameraOffsetMinDistance")] [Tooltip("The minimum distance the camera offset can decrease to.")]
        public float CameraOffsetMinDistance = 0.2f;

        [FormerlySerializedAs("hideSkinnedMeshRenderers")] [Tooltip("Automatically turns off a targets skinned mesh renderer when the camera collider hits the player collider")]
        public bool HideSkinnedMeshRenderers;

        [FormerlySerializedAs("collisionLayer")] [Header("Collision layer settings")] [Tooltip("Set this layermask to specify with which layers the camera should collide")]
        public LayerMask CollisionLayer;

        [FormerlySerializedAs("playerLayer")] [Tooltip("Set this to your player layer so ground checks will ignore the player collider")]
        public LayerMask PlayerLayer;

        [FormerlySerializedAs("occlusionCheck")] [Header("Features")] [Tooltip("Automatically reposition the camera when the character is blocked by an obstacle.")]
        public bool OcclusionCheck = true;

        [FormerlySerializedAs("smartPivot")] [Tooltip("Uses a pivot when the camera hits the ground and prevents moving the camera closer to the target when looking up.")]
        public bool SmartPivot = true;

        [FormerlySerializedAs("thicknessCheck")] [Tooltip("Thickness checking can be configured to ignore  smaller obstacles like sticks, trees, etc... to not reposition or zoom in the camera.")]
        public bool ThicknessCheck = true;

        [FormerlySerializedAs("smoothTargetMode")]
        [Header("Smooth target mode")]
        [Tooltip(
            "Enable to smooth out target following, to hide noisy position changes that should not be picked up by the camera immediately or to enable smooth transitions to other targets that are changed during runtime")]
        public bool SmoothTargetMode;

        [FormerlySerializedAs("smoothTargetValue")] [Tooltip("The speed at which the camera lerps to the actual target position for each axis. 0 means no smoothing will be applied!")]
        public Vector3 SmoothTargetValue = new Vector3(15.0f, 4.0f, 15.0f);

        [FormerlySerializedAs("maxThickness")]
        [Header("Thickness check settings")]
        [Tooltip(
            "Adjusts the thickness check. The higher, the thicker the objects can be and there will be no occlusion check.Warning: Very high values could make Occlusion checking obsolete and as a result the followed target can be occluded")]
        public float MaxThickness = 0.3f;

        [FormerlySerializedAs("smartPivotOnlyGround")] [Header("Smart Pivot settings")] [Tooltip("Smart pivot is only activated on stable grounds")]
        public bool SmartPivotOnlyGround = true;

        [FormerlySerializedAs("floorNormalUp")] [Tooltip("Default ground up for smart pivot")]
        public Vector3 FloorNormalUp = new Vector3(0, 1, 0);

        [FormerlySerializedAs("maxFloorDot")] [Tooltip("Only hit results from maxFloorDot to 1 will be valid")]
        public float MaxFloorDot = 0.85f;

        [FormerlySerializedAs("pivotRotationEnabled")]
        [Header("Static pivot rotation settings")]
        [Tooltip("Enable pivot rotation feature to rotate the camera in any direction without the camera moving or rotation around. Used in LockOn and Follow.")]
        public bool PivotRotationEnabled;

        [FormerlySerializedAs("smoothPivot")] [Tooltip("When enabled the pivot will smoothly slerp to the new pivot angles")]
        public bool SmoothPivot;

        [FormerlySerializedAs("customPivot")] [Tooltip("Enable overriding the pivot with the public variable 'pivotAngles'")]
        public bool CustomPivot;

        [FormerlySerializedAs("pivotAngles")] [Tooltip("Use to override pivot")]
        public Vector3 PivotAngles;

        [FormerlySerializedAs("maxRaycastHitCount")]
        [Header("Extra settings")]
        [Tooltip(
            "The maximum amount of raycast hits that are available for occlusion hits. Can be tweaked for performance. Don't set amount less than 2 when your geometry has many polygons as this can make repositioning unreliable.")]
        public int MaxRaycastHitCount = 6;

        [FormerlySerializedAs("maxThicknessRaycastHitCount")]
        [Tooltip(
            "The maximum amount of raycast hits that are available for thickness hits. Can be tweaked for performance. Use an even amount to account for back-front face and don't use less than 2.")]
        public int MaxThicknessRaycastHitCount = 6;

#if TPC_DEBUG
        [Header("Debugging")] public bool showDesiredPosition;
        public bool drawSphereCast;
        public bool cameraOffsetProbingRay;
        public bool drawOcclusionHit;
        public bool drawDirToTarget;

        // offset safeguard debug
        public bool drawSafeguardHit;

        // thickness check debug
        public bool drawThicknessCubes;
        public bool drawThicknessEndHits;
        public bool drawThicknessStartHits;

        private readonly List<DrawCubeRequest> drawCubeRequests = new List<DrawCubeRequest>();
        private class DrawCubeRequest
        {
            public Vector3 Origin;
            public Vector3 Size;
            public Quaternion Rotation;
            public Vector3 p2;
        }
#endif

#if !TPC_DEBUG
        [FormerlySerializedAs("playerCollision")] [HideInInspector]
#endif
        public bool PlayerCollision;
        
#if !TPC_DEBUG
        [FormerlySerializedAs("bGroundHit")] [HideInInspector]
#endif
        public bool GroundHit;

#if TPC_DEBUG
        [SerializeField]
#endif
        private CameraControllerState state;

        private bool initDone;

        private SkinnedMeshRenderer[] smrs;

        private SortDistance sortMethodHitDistance;

        private RaycastHit[] startHits;
        private RaycastHit[] endHits;
        private RaycastHit[] occlusionHits;
        private List<RaycastHit> occlusionHitsCollector;
        private List<RaycastHit> startHitsCollector;
        private List<RaycastHit> endHitsCollector;

        private void Awake()
        {
            initDone = false;

            state = new CameraControllerState();

            startHits = new RaycastHit[MaxThicknessRaycastHitCount];
            endHits = new RaycastHit[MaxThicknessRaycastHitCount];
            occlusionHits = new RaycastHit[MaxRaycastHitCount];

            occlusionHitsCollector = new List<RaycastHit>(MaxRaycastHitCount);
            startHitsCollector = new List<RaycastHit>(MaxRaycastHitCount);
            endHitsCollector = new List<RaycastHit>(MaxRaycastHitCount);

            state.CameraRotation = transform.rotation;
            state.CameraAngles = transform.rotation.eulerAngles;
            state.SmartPivotRotation = Quaternion.identity;
            state.PivotRotation = Quaternion.identity;
            state.CurrentPivotRotation = Quaternion.identity;

            sortMethodHitDistance = new SortDistance();

            state.Distance = DesiredDistance;

            state.CameraNormalMode = true;
            PlayerCollision = false;

            UpdateOffsetVector(OffsetVector);
            UpdateCameraOffsetVector(CameraOffsetVector);

            InitFromTarget();
        }

        private void InitFromTarget()
        {
            InitFromTarget(Target);
        }

        /// <summary>
        /// Use this to re-init a target, for example, when switching through multiple characters
        /// </summary>
        /// <param name="newTarget"></param>
        public void InitFromTarget(Transform newTarget)
        {
            Target = newTarget;

            if (Target == null)
                return;

            var tmpTransform = transform;
            var tmpTargetPosition = Target.transform.position;

            state.CameraRotation = tmpTransform.rotation;
            state.PrevPosition = tmpTransform.position;
            state.TargetPosition = tmpTargetPosition;
            state.SmoothedTargetPosition = tmpTargetPosition;

            smrs = HideSkinnedMeshRenderers ? Target.GetComponentsInChildren<SkinnedMeshRenderer>() : null;

            // get colliders from target
            var colliders = Target.GetComponentsInChildren<Collider>();

            foreach (var col in colliders)
            {
                if (!PlayerLayer.IsInLayerMask(col.gameObject))
                {
                    var colliderName = col.gameObject.name;
                    Debug.LogWarning(
                        $"The target \"{colliderName}\" has a collider which is not in the player layer. To fix: Change the layer of {colliderName} to the layer referenced in CameraController->Player layer");
                }
            }

            var selfCollider = GetComponent<Collider>();

            if (selfCollider == null && HideSkinnedMeshRenderers)
            {
                Debug.LogWarning("HideSkinnedMeshRenderers is activated but there's no collider on the camera. Example to fix: Atach a BoxCollider to the camera and set it to trigger.");
            }

            if (selfCollider && CollisionLayer.IsInLayerMask(gameObject))
            {
                Debug.LogWarning(
                    "Camera is colliding with the collision layer. Consider changing the camera to a layer that isn't in the collision layer. Example to fix: Change the camera to the player layer.");
            }

            initDone = true;
        }

        private void LateUpdate()
        {

            if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PAUSED)
            {
                return;
            }
            
#if TPC_DEBUG
            ResetCubes();
#endif

            if (Target == null)
                return;

            if (!initDone)
                return;

            if (state.Distance < 0)
                state.Distance = 0;

            // disable player character when too close
            if (HideSkinnedMeshRenderers && smrs != null)
            {
                if (PlayerCollision || state.Distance <= CollisionDistance)
                {
                    foreach (var t in smrs)
                    {
                        t.enabled = false;
                    }
                }
                else
                {
                    foreach (var t in smrs)
                    {
                        t.enabled = true;
                    }
                }
            }

            Quaternion transformRotation = state.CameraRotation;

            Vector3 offsetVectorTransformed = Target.rotation * OffsetVector;
            Vector3 cameraOffsetVectorTransformed = transformRotation * state.CurrentCameraOffsetVector;
            Vector3 cameraOffsetVectorTransformedStable = transformRotation * (state.InvertCameraOffsetVector ? -CameraOffsetVector : CameraOffsetVector);

            if (SmoothTargetMode)
            {
                if (SmoothTargetValue.x != 0)
                    state.SmoothedTargetPosition.x = Mathf.Lerp(state.SmoothedTargetPosition.x, Target.position.x, SmoothTargetValue.x * Time.deltaTime);
                if (SmoothTargetValue.y != 0)
                    state.SmoothedTargetPosition.y = Mathf.Lerp(state.SmoothedTargetPosition.y, Target.position.y, SmoothTargetValue.y * Time.deltaTime);
                if (SmoothTargetValue.z != 0)
                    state.SmoothedTargetPosition.z = Mathf.Lerp(state.SmoothedTargetPosition.z, Target.position.z, SmoothTargetValue.z * Time.deltaTime);

                state.TargetPosition = state.SmoothedTargetPosition;
            }
            else
            {
                state.TargetPosition = Target.position;
            }

            Vector3 targetPosWithOffset = (state.TargetPosition + offsetVectorTransformed);
            Vector3 targetPosWithCameraOffset = targetPosWithOffset + cameraOffsetVectorTransformed;
            state.DesiredPosition = transformRotation * (new Vector3(0, 0, -DesiredDistance) + state.CurrentCameraOffsetVector) + offsetVectorTransformed + state.TargetPosition;

            state.CameraOffsetMagnitude = CameraOffsetVector.magnitude;

            if (state.CameraOffsetMagnitude < 0.001f)
            {
                CameraOffsetVector = Vector3.zero;
                state.CameraOffsetMagnitude = 0;
            }

            var currentOffsetLength = state.CurrentCameraOffsetVector.magnitude;
            
            
            if (state.CameraOffsetMagnitude > 0 || currentOffsetLength > 0)
            {
                // camera offset probing
#if TPC_DEBUG
                if (cameraOffsetProbingRay)
                {
                    DebugVisualizationHelper.DrawDirectionArrow(targetPosWithOffset, cameraOffsetVectorTransformedStable, Color.magenta);
                }
#endif
                Vector3 tmpCurrentCameraOffsetVector;
                var safeGuardRotated = Quaternion.AngleAxis(-45.0f, Vector3.up) * cameraOffsetVectorTransformedStable;

                bool hasHits1 = Physics.SphereCast(targetPosWithOffset, CollisionDistance, cameraOffsetVectorTransformedStable.normalized, out var closestHit2, cameraOffsetVectorTransformedStable.magnitude, CollisionLayer);
                bool hasHits2 = Physics.SphereCast(targetPosWithOffset, CollisionDistance, safeGuardRotated.normalized, out var closestHit3, cameraOffsetVectorTransformedStable.magnitude, CollisionLayer);

                //bool hasHits1 = Physics.CapsuleCast(targetPosWithOffset, targetPosWithOffset - offsetVectorTransformed, CollisionDistance, tmpDir.normalized, out var closestHit2, tmpDir.magnitude + CollisionDistance, CollisionLayer);
                //bool hasHits2 = Physics.CapsuleCast(safeguardPos, safeguardPos - offsetVectorTransformed, CollisionDistance, -tmpDir.normalized, out var closestHit3, tmpDir.magnitude, CollisionLayer);

                
#if TPC_DEBUG
                if (drawSafeguardHit)
                {
                    DebugVisualizationHelper.DrawDirectionArrow(targetPosWithOffset, safeGuardRotated, Color.grey);
                }
#endif

                if (hasHits1 || hasHits2)
                {
                    float mag1 = state.CameraOffsetMagnitude + CollisionDistance;
                    float mag2 = state.CameraOffsetMagnitude + CollisionDistance;

                    if (hasHits1)
                    {
                        var tmpMag = closestHit2.distance + CollisionDistance * 0.95f; // slight decrease so occlusion check doesn't cast inside itself
                        mag1 = Math.Max(CameraOffsetMinDistance, Math.Min(tmpMag, state.CameraOffsetMagnitude + CollisionDistance));

#if TPC_DEBUG
                        if (drawSafeguardHit)
                        {
                            DebugVisualizationHelper.DrawArrow(targetPosWithOffset, closestHit2.point, Color.blue);
                        }
#endif
                    }

                    if (hasHits2)
                    {
                        if (!state.OffsetSafeguardInitialized)
                        {
                            state.OffsetSafeguardInitialized = true;
                            state.OffsetSafeguardStartingY = state.CameraAngles.y;
                            state.OffsetSafeguardStartingDistance = closestHit3.distance;
                        }

                        var tmpMag = Mathf.Lerp(state.CameraOffsetMagnitude, state.OffsetSafeguardStartingDistance, (state.OffsetSafeguardStartingY - state.CameraAngles.y) / 45.0f) +
                                     CollisionDistance;

                        mag2 = Math.Max(CameraOffsetMinDistance, Math.Min(tmpMag, state.CameraOffsetMagnitude + CollisionDistance));

#if TPC_DEBUG
                        if (drawSafeguardHit)
                        {
                            DebugVisualizationHelper.DrawArrow(targetPosWithOffset, closestHit3.point, Color.blue);
                        }
#endif
                    }
                    else
                    {
                        state.OffsetSafeguardInitialized = false;
                    }

                    float mag;
                    if (hasHits1 && hasHits2)
                        mag = mag1;
                    else if (hasHits1)
                        mag = mag1;
                    else
                        mag = mag2;

                    tmpCurrentCameraOffsetVector = (state.InvertCameraOffsetVector ? -CameraOffsetVector : CameraOffsetVector).normalized * (mag - CollisionDistance);
                    cameraOffsetVectorTransformed = transformRotation * tmpCurrentCameraOffsetVector;
                    targetPosWithCameraOffset = targetPosWithOffset + cameraOffsetVectorTransformed;
                    var newDesiredPosition = transformRotation * (new Vector3(0, 0, -DesiredDistance) + state.CurrentCameraOffsetVector) + offsetVectorTransformed + state.TargetPosition;

                    var tmpDirToDesiredPosition = (newDesiredPosition - targetPosWithOffset).normalized;
                    if (Physics.SphereCastNonAlloc(targetPosWithOffset, CollisionDistance, tmpDirToDesiredPosition, occlusionHits, DesiredDistance, CollisionLayer) > 0)
                    {
                        state.CurrentCameraOffsetVector = tmpCurrentCameraOffsetVector;
                    }
                }
                else
                {
                    state.OffsetSafeguardInitialized = false;
                    tmpCurrentCameraOffsetVector = (state.InvertCameraOffsetVector ? -CameraOffsetVector : CameraOffsetVector).normalized * state.CameraOffsetMagnitude;
                }

                state.CurrentCameraOffsetVector = Vector3.Lerp(state.CurrentCameraOffsetVector, state.CameraOffsetMagnitude > 0 ? 
                        tmpCurrentCameraOffsetVector : Vector3.zero, Time.deltaTime * CameraOffsetChangeSpeed);
            }


            var dirToDesiredPosition = (state.DesiredPosition - targetPosWithCameraOffset).normalized;
            state.DirToDesiredPosition = dirToDesiredPosition;

#if TPC_DEBUG
            if (showDesiredPosition)
                ShowThicknessCube(state.DesiredPosition, "desiredPosition");
#endif

            // sweep from target to desiredPosition and check for collisions
            {
                int hitCount = Physics.SphereCastNonAlloc(targetPosWithCameraOffset, CollisionDistance, dirToDesiredPosition, occlusionHits, DesiredDistance, CollisionLayer);

#if TPC_DEBUG
                if (drawSphereCast)
                {
                    Debug.DrawLine(targetPosWithCameraOffset, targetPosWithCameraOffset + dirToDesiredPosition * DesiredDistance, Color.yellow);
                }

                if (drawOcclusionHit)
                {
                    for (int i = 0; i < hitCount; i++)
                        Debug.DrawLine(targetPosWithCameraOffset, occlusionHits[i].point, Color.red);
                }
#endif
                SortAndFilterHits(occlusionHitsCollector, occlusionHits, hitCount, dirToDesiredPosition);
            }

            float thickness = float.MaxValue;

            state.OcclusionHitHappened = OcclusionCheck && occlusionHitsCollector.Count > 0;

            if (state.OcclusionHitHappened)
            {
                if (Physics.Raycast(targetPosWithCameraOffset, state.DirToDesiredPosition, out var closesHit, state.Distance + CollisionDistance, CollisionLayer))
                {
                    occlusionHitsCollector.Insert(0, closesHit);
                }

                if (ThicknessCheck)
                {
                    thickness = GetThicknessFromCollider(targetPosWithCameraOffset, state.DesiredPosition);
                }
            }

            // evaluate ground hit
            if ((SmartPivot && state.OcclusionHitHappened) || state.GroundHit)
            {
                if (Physics.CheckSphere(state.PrevPosition, CollisionDistance * 1.5f, CollisionLayer))
                {
                    if (SmartPivotOnlyGround)
                    {
#if TPC_DEBUG
                        Debug.DrawLine(state.PrevPosition, state.PrevPosition - (Target.up * (CollisionDistance * 2.0f)), Color.red);
#endif

                        if (Physics.Raycast(state.PrevPosition, -Target.up, out var groundHit, CollisionDistance * 2.0f, CollisionLayer))
                        {
                            state.GroundHit = Vector3.Dot(FloorNormalUp, groundHit.normal) > MaxFloorDot;
                        }
                        else
                            state.GroundHit = false;
                    }
                    else
                        state.GroundHit = true;
                }
                else
                    state.GroundHit = false;
            }
            else if (!state.OcclusionHitHappened && state.CameraNormalMode)
                state.GroundHit = false;

            // Avoid that the character is not visible
            if (OcclusionCheck && state.OcclusionHitHappened)
            {
                Vector3 vecA = occlusionHitsCollector[0].point - targetPosWithCameraOffset;

                float vecAMagnitude = vecA.magnitude;
                float newDist = vecAMagnitude - CollisionDistance;

                if (thickness > MaxThickness)
                {
                    if (newDist > DesiredDistance)
                        newDist = DesiredDistance;

                    state.Distance = newDist;
                }
                else if (state.CameraNormalMode)
                {
                    LerpDistance();
                }
            }
            else if (!state.OcclusionHitHappened)
            {
                LerpDistance();
            }

            // camera offset path
            // this was previously running as first but it's important to have an up-to-date distance
            // so this was moved here


            LerpPresentationDistance();
            
            

            if (!state.CameraNormalMode)
            {
                transformRotation *= state.SmartPivotRotation;
            }

            if (PivotRotationEnabled)
            {
                var newPivot = !CustomPivot ? state.PivotRotation : Quaternion.Euler(PivotAngles);
                state.CurrentPivotRotation = SmoothPivot ? Quaternion.Slerp(state.CurrentPivotRotation, newPivot, Time.deltaTime) : newPivot;

                transformRotation *= state.CurrentPivotRotation;
            }

            transformRotation = StabilizeRotation(transformRotation);
            transform.rotation = transformRotation;

            var newPosition = GetCameraPosition(state, state.TargetPosition, offsetVectorTransformed);
            transform.position = newPosition;
            state.PrevPosition = newPosition;
        }

        private void LerpDistance()
        {
            float lerpFactor = Time.deltaTime * ((state.Distance < DesiredDistance) ? OccludedZoomInSpeed : UnoccludedZoomOutSpeed);
            state.Distance = Mathf.Lerp(state.Distance, DesiredDistance, lerpFactor);
        }

        private void LerpPresentationDistance()
        {
            float lerpFactor = Time.deltaTime * ((state.Distance < state.PresentationDistance) ? OccludedZoomInSpeed : UnoccludedZoomOutSpeed);
            state.PresentationDistance = Mathf.Lerp(state.PresentationDistance, state.Distance, lerpFactor);
        }

        private void SortAndFilterHits(List<RaycastHit> collector, RaycastHit[] raycastHits, int hitCount, Vector3 targetDir)
        {
            Array.Sort(raycastHits, 0, hitCount, sortMethodHitDistance);

            collector.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                var hit = raycastHits[i];

                if (IsValidRayCastHit(hit, targetDir))
                {
                    //Debug.Log("Added hit point: " + hit.point + " normal: " + hit.normal);
                    collector.Add(hit);
                }
            }
        }

        private static bool IsValidRayCastHit(RaycastHit hit, Vector3 targetDir)
        {
            return (hit.point != Vector3.zero || hit.normal != -targetDir) &&
                   (hit.point != Vector3.zero && hit.normal != Vector3.zero) &&
                   Vector3.Dot(hit.normal, targetDir) < 0.0f;
        }
        
        public static Vector3 GetCameraPosition(in Quaternion cameraRotation, in Vector3 targetPosition, in Vector3 currentCameraOffsetVector, in Vector3 preComputedOffsetVectorTransformed, float desiredDistance)
        {
            return cameraRotation * (new Vector3(0, 0, -desiredDistance) + currentCameraOffsetVector) + preComputedOffsetVectorTransformed + targetPosition;
        }

        public static Vector3 GetCameraPosition(in CameraControllerState state, in Vector3 targetPosition, in Vector3 preComputedOffsetVectorTransformed)
        {
            return state.CameraRotation * (new Vector3(0, 0, -state.PresentationDistance) + state.CurrentCameraOffsetVector) + preComputedOffsetVectorTransformed + targetPosition;
        }

        public static Vector3 GetCameraPosition(in CameraControllerState state, in Vector3 targetPosition, in Quaternion targetRotation, in Vector3 offsetVector)
        {
            Vector3 offsetVectorTransformed = targetRotation * offsetVector;
            return state.CameraRotation * (new Vector3(0, 0, -state.PresentationDistance) + state.CurrentCameraOffsetVector) + offsetVectorTransformed + targetPosition;
        }

        public void OnTriggerEnter(Collider c)
        {
            if (c.transform == Target || PlayerLayer.IsInLayerMask(c.gameObject))
            {
                PlayerCollision = true;
            }
        }

        public void OnTriggerExit(Collider c)
        {
            if (c.transform == Target || PlayerLayer.IsInLayerMask(c.gameObject))
            {
                PlayerCollision = false;
            }
        }

#if TPC_DEBUG
        private readonly Queue<GameObject> debugThicknessCubes = new Queue<GameObject>();
        private readonly Queue<GameObject> activeDebugThicknessCubes = new Queue<GameObject>();

        private void ShowThicknessCube(Vector3 position, string cubeName)
        {
            GameObject cube;

            if (debugThicknessCubes.Count > 0)
            {
                cube = debugThicknessCubes.Dequeue();
            }
            else
            {
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                Destroy(cube.GetComponent<BoxCollider>());
            }

            cube.SetActive(true);
            cube.transform.position = position;
            cube.name = cubeName;
            activeDebugThicknessCubes.Enqueue(cube);
        }

        private void ResetCubes()
        {
            while (activeDebugThicknessCubes.Count > 0)
            {
                GameObject cube = activeDebugThicknessCubes.Dequeue();

                cube.name = "unused";
                cube.SetActive(false);

                debugThicknessCubes.Enqueue(cube);
            }
        }
#endif

        private class ThicknessCheckList
        {
            public Vector3 Point;

            public int ColliderId;
            public int LinkedIndex;

            public bool Marked;
        }

        private float GetThicknessFromCollider(Vector3 start, Vector3 end)
        {
            Vector3 dir = (end - start);
            float length = dir.magnitude;
            Vector3 dirToHit = dir.normalized;

            int startHitCount = Physics.SphereCastNonAlloc(start, CollisionDistance, dirToHit, startHits, length, CollisionLayer);
            SortAndFilterHits(startHitsCollector, startHits, startHitCount, dirToHit);

            var endHitCount = Physics.SphereCastNonAlloc(end, CollisionDistance, -dirToHit, endHits, length, CollisionLayer);
            SortAndFilterHits(endHitsCollector, endHits, endHitCount, -dirToHit);

            if (startHitsCollector.Count == 0 || endHitsCollector.Count == 0)
                return float.MaxValue;

            ThicknessCheckList[] startHitList = new ThicknessCheckList[startHitsCollector.Count];
            ThicknessCheckList[] endHitList = new ThicknessCheckList[endHitsCollector.Count];
            Dictionary<int, float> uniqueColliders = new Dictionary<int, float>(0);

            for (int i = 0; i < startHitsCollector.Count; i++)
            {
                var tmp = startHitsCollector[i];
                var colliderId = tmp.collider.GetHashCode();

#if TPC_DEBUG
                if (drawThicknessStartHits)
                    Debug.DrawLine(tmp.point, tmp.point + tmp.normal, Color.red);
#endif

                startHitList[i] = new ThicknessCheckList()
                {
                    ColliderId = colliderId,
                    Point = tmp.point,
                    LinkedIndex = -1,
                    Marked = false
                };

                if (!uniqueColliders.ContainsKey(colliderId))
                    uniqueColliders.Add(colliderId, 0);
            }

            for (int i = 0; i < endHitsCollector.Count; i++)
            {
                var tmp = endHitsCollector[i];
                var colliderId = tmp.collider.GetHashCode();
#if TPC_DEBUG
                if (drawThicknessEndHits)
                    Debug.DrawLine(tmp.point, tmp.point + tmp.normal, Color.green);
#endif

                endHitList[i] = new ThicknessCheckList()
                {
                    ColliderId = colliderId,
                    Point = tmp.point,
                    LinkedIndex = -1,
                    Marked = false
                };

                if (!uniqueColliders.ContainsKey(colliderId))
                    uniqueColliders.Add(colliderId, 0);
            }

            var keyArray = uniqueColliders.Keys.ToArray();
            float finalThickness = float.MaxValue;
            bool finalThicknessFound = false;

            foreach (var colliderId in keyArray)
            {
                for (int i = 0; i < startHitList.Length; i++)
                {
                    var tmpStart = startHitList[i];

                    if (tmpStart.ColliderId == 0)
                        continue;

                    if (tmpStart.Marked || tmpStart.ColliderId != colliderId)
                        continue;

                    for (int ii = endHitList.Length - 1; ii >= 0; ii--)
                    {
                        var tmpEnd = endHitList[ii];

                        if (tmpEnd.ColliderId == 0)
                            continue;

                        if (tmpEnd.Marked || tmpEnd.ColliderId != colliderId)
                            continue;

                        tmpStart.Marked = true;
                        tmpEnd.Marked = true;
                        tmpStart.LinkedIndex = ii;
                        tmpEnd.LinkedIndex = i;
                    }
                }

                for (int i = 0; i < startHitList.Length; i++)
                {
                    var tmpStart = startHitList[i];

                    if (tmpStart.ColliderId == 0)
                        continue;

                    if (!tmpStart.Marked || tmpStart.ColliderId != colliderId || tmpStart.LinkedIndex == -1)
                        continue;

                    var tmpEnd = endHitList[tmpStart.LinkedIndex];

                    if (!finalThicknessFound)
                    {
                        finalThicknessFound = true;
                        finalThickness = 0;
                    }

#if TPC_DEBUG
                    if (drawThicknessCubes)
                    {
                        Debug.DrawLine(tmpStart.Point, tmpEnd.Point, Color.blue);
                    }
#endif

                    float thicknessFraction = (tmpStart.Point - tmpEnd.Point).magnitude;

                    var currentEntityThickness = uniqueColliders[colliderId];
                    currentEntityThickness += thicknessFraction;

                    if (currentEntityThickness > finalThickness)
                        finalThickness = currentEntityThickness;

                    uniqueColliders[colliderId] = currentEntityThickness;
                }
            }

            return Math.Min(finalThickness, float.MaxValue);
        }

        /// <summary>
        /// Use this method to update the offset vector during runtime.
        /// In case the offset vector is inside the collision sphere
        /// the offset vector gets moved to a position outside of it
        /// </summary>
        /// <param name="newOffset"></param>
        public void UpdateOffsetVector(Vector3 newOffset)
        {
            if (newOffset.sqrMagnitude > CollisionDistance)
                OffsetVector = newOffset;
            else
            {
                // when the offset vector ends up inside the collision sphere, add a small vector plus the collision distance to fix collision glitches
                OffsetVector += Vector3.up * (CollisionDistance + 0.01f);
            }
        }

        /// <summary>
        /// Use this method to update the camera offset vector during runtime.
        /// Updating it like that removes the need to calculate the sqrMagnitude of the vector
        /// every frame.
        /// </summary>
        /// <param name="newOffset"></param>
        public void UpdateCameraOffsetVector(Vector3 newOffset)
        {
            CameraOffsetVector = newOffset;
        }

        /// <summary>
        /// Slerping to new rotation sometimes leads to wrong rotation in Z-axis        
        /// so any Z rotation will be removed
        /// </summary>
        public static Quaternion StabilizeRotation(Quaternion rotation)
        {
            return Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        }

        public static Quaternion StabilizeSlerpRotation(Quaternion from, Quaternion to, float timeModifier)
        {
            var slerpedRotation = Quaternion.Slerp(from, to, timeModifier);
            return Quaternion.Euler(slerpedRotation.eulerAngles.x, slerpedRotation.eulerAngles.y, 0);
        }

        public ref CameraControllerState GetControllerState()
        {
            return ref state;
        }
        
        public void ApplyControllerState(CameraControllerState newState)
        {
            state = newState;
        }

        public Vector3 GetSmoothedTargetPosition()
        {
            return state.SmoothedTargetPosition;
        }

        private void OnDrawGizmos()
        {
#if TPC_DEBUG
            foreach (var drawCubeRequest in drawCubeRequests)
            {
                DebugVisualizationHelper.DrawBoxLines(transform, drawCubeRequest.Origin, drawCubeRequest.p2, drawCubeRequest.Size, true);
            }

            drawCubeRequests.Clear();

            Gizmos.color = Color.green;
            var targetPosWithCameraOffset = state.TargetPosition + Target.rotation * OffsetVector + transform.rotation * state.CurrentCameraOffsetVector;
            Gizmos.DrawWireSphere(targetPosWithCameraOffset, CollisionDistance);
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (Target == null)
                return;
            
            var targetPosition = GetTargetPosition();
            var cameraPosition = GetCameraPosition();
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.1f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cameraPosition, CollisionDistance);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cameraPosition, targetPosition);
        }
        
        private Vector3 GetCameraPosition()
        {
            Vector3 offsetVectorTransformed = Target.transform.rotation * OffsetVector;
            return GetCameraPosition(transform.rotation, Target.transform.position, offsetVectorTransformed, CameraOffsetVector, DesiredDistance);
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 offsetVectorTransformed = Target.transform.rotation * OffsetVector;
            Vector3 cameraOffsetVectorTransformed = transform.rotation * CameraOffsetVector;
            
            return Target.transform.position + offsetVectorTransformed + cameraOffsetVectorTransformed;
        }
    }
}