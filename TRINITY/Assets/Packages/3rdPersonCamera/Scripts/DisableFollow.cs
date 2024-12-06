// <copyright project="ThirdPersonCamera" file="DisableFollow.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController)), RequireComponent(typeof(Follow)), RequireComponent(typeof(FreeForm))]
    public class DisableFollow : MonoBehaviour
    {
        [FormerlySerializedAs("activateMotionCheck")]
        [Header("Basic settings")]
        [Tooltip("Enables motion check with its motionThreshold")]
        public bool ActivateMotionCheck = true;
        [FormerlySerializedAs("activateTimeCheck")] [Tooltip("Enables timed checks and resets to follow after timeToActivate seconds are elapsed")]
        public bool ActivateTimeCheck = true;
        [FormerlySerializedAs("activateMouseCheck")] [Tooltip("Enables reactivation of follow when mouse buttons are released")]
        public bool ActivateMouseCheck = true;

        [FormerlySerializedAs("timeToActivate")] [Tooltip("Time in seconds when follow is reactivated. It will also not reactivate when the user still has input.")]
        public float TimeToActivate = 1.0f;
        [FormerlySerializedAs("motionThreshold")] [Tooltip("Distance which has to be crossed to reactivate follow")]
        public float MotionThreshold = 0.05f;

        private CameraController cameraController;
        private FreeForm freeForm;
        private Follow follow;

        private bool followDisabled;
        private Vector3 prevPosition;

        private Coroutine timeCheckRoutine;

        private void Start()
        {
            cameraController = GetComponent<CameraController>();
            follow = GetComponent<Follow>();
            freeForm = GetComponent<FreeForm>();
            followDisabled = !follow.EnableFollow;
        }

        private void Update()
        {
            if (!followDisabled && freeForm.GetInput().HasInput())
            {
                follow.EnableFollow = false;
                followDisabled = true;
            }

            if (followDisabled)
            {
                if (ActivateMotionCheck)
                {
                    Vector3 motionVector = cameraController.Target.transform.position - prevPosition;

                    if (motionVector.magnitude > MotionThreshold)
                    {
                        follow.EnableFollow = true;
                        followDisabled = false;
                    }
                }

                if (ActivateTimeCheck && timeCheckRoutine == null)
                {
                    timeCheckRoutine = StartCoroutine(ActivateFollow(TimeToActivate));
                }

                if (ActivateMouseCheck && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
                {            
                    follow.EnableFollow = true;
                    followDisabled = false;
                }
            }

            prevPosition = cameraController.Target.transform.position;
        }

        private IEnumerator ActivateFollow(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            
            timeCheckRoutine = null;

            if (!freeForm.GetInput().HasInput())
            {
                follow.EnableFollow = true;
                followDisabled = false;
                
                timeCheckRoutine = StartCoroutine(ActivateFollow(TimeToActivate));
            }
        }
    }
}