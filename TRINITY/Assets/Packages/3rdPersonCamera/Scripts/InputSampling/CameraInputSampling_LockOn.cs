// <copyright project="ThirdPersonCamera" file="CameraInputSampling_LockOn.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

#if ENABLE_LEGACY_INPUT_MANAGER
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCamera
{
    //[RequireComponent(typeof(LockOnTarget))]
    public class CameraInputSampling_LockOn : MonoBehaviour
    {
        [Header("Basic settings")]
        [Tooltip("The time in seconds after which next/previous cycling gets reset to the beginning or 0 index of available Targetables")]
        public float cycleCooldown = 1.5f;

        [Header("Input Mouse settings")]
        [Tooltip("A list of integer mouseButton values to lock on a target, default is right mouse button")]
        public List<int> mouseLockOnInput = new List<int>() { 1 }; // default is right mouse button
        [Tooltip("A list of integer mouseButton values to cycle to the next locked on target")]
        public List<int> mouseCycleTargetNext = new List<int>();
        [Tooltip("A list of integer mouseButton values to cycle to the previous locked on target")]
        public List<int> mouseCycleTargetPrevious = new List<int>();

        [Header("Input Keyboard settings")]
        [Tooltip("A list of KeyCode values to lock on a target")]
        public List<KeyCode> keyboardLockOnInput = new List<KeyCode>();
        [Tooltip("A list of KeyCode values to cycle to the next locked on target")]
        public List<KeyCode> keyboardCycleTargetNext = new List<KeyCode>() { KeyCode.Q };
        [Tooltip("A list of KeyCode values to cycle to the previous locked on target")]
        public List<KeyCode> keyboardCycleTargetPrevious = new List<KeyCode>() { KeyCode.E };

        [Header("Input Controller settings")]
        [Tooltip("A list of button strings to lock on a target")]
        public List<string> buttonLockOnInput = new List<string>();
        [Tooltip("A list of button strings to cycle to the next locked on target")]
        public List<string> buttonCycleTargetNext = new List<string>();
        [Tooltip("A list of button strings to cycle to the previous locked on target")]
        public List<string> buttonCycleTargetPrevious = new List<string>();

        [HideInInspector]
        public bool inputSamplingEnabled = true;

        private float lastCycle;

        private LockOnTarget lockOn;
        private CameraInputLockOn cameraInputLockOn;


        public void Awake()
        {
            lockOn = GetComponent<LockOnTarget>();
            cameraInputLockOn = new CameraInputLockOn();
        }

        public void Update()
        {
            if (!inputSamplingEnabled)
            {
                return;
            }

            var timeNow = Time.time;
                       
            // LockOnTarget writes values back to input model so get the current version
            cameraInputLockOn = lockOn.GetInput();

            bool lockOnTarget = cameraInputLockOn.LockOnTarget;
            int cycleIndex = cameraInputLockOn.CycleIndex;

            // sample input for lock on
            if (mouseLockOnInput.Count > 0) // right mouse click toggles lock on mode
            {
                for (int i = 0; i < mouseLockOnInput.Count; i++)
                {
                    if (Input.GetMouseButtonDown(mouseLockOnInput[i]))
                        lockOnTarget = !lockOnTarget;
                }
            }

            if (keyboardLockOnInput.Count > 0)
            {
                for (int i = 0; i < keyboardLockOnInput.Count; i++)
                {
                    if (Input.GetKeyDown(keyboardLockOnInput[i]))
                        lockOnTarget = !lockOnTarget;
                }
            }

            if (buttonLockOnInput.Count > 0)
            {
                for (int i = 0; i < buttonLockOnInput.Count; i++)
                {
                    if (Input.GetButton(buttonLockOnInput[i]))
                        lockOnTarget = !lockOnTarget;
                }
            }

            // sample input for previous/next target cycling
            #region sample input for previous/next target cycling

            if (timeNow > lastCycle + cycleCooldown)
            {
                cycleIndex = 0;
            }

            if (mouseCycleTargetNext.Count > 0)
            {
                for (int i = 0; i < mouseCycleTargetNext.Count; i++)
                {
                    if (Input.GetMouseButtonDown(mouseCycleTargetNext[i]))
                    {
                        cycleIndex++;
                        lastCycle = timeNow;
                    }
                }
            }

            if (keyboardCycleTargetNext.Count > 0)
            {
                for (int i = 0; i < keyboardCycleTargetNext.Count; i++)
                {
                    if (Input.GetKeyDown(keyboardCycleTargetNext[i]))
                    {
                        cycleIndex++;
                        lastCycle = timeNow;
                    }
                }
            }

            if (buttonCycleTargetNext.Count > 0)
            {
                for (int i = 0; i < buttonCycleTargetNext.Count; i++)
                {
                    if (Input.GetButton(buttonCycleTargetNext[i]))
                    {
                        cycleIndex++;
                        lastCycle = timeNow;
                    }
                }
            }

            if (mouseCycleTargetPrevious.Count > 0)
            {
                for (int i = 0; i < mouseCycleTargetPrevious.Count; i++)
                {
                    if (Input.GetMouseButtonDown(mouseCycleTargetPrevious[i]))
                    {
                        cycleIndex--;
                        lastCycle = timeNow;
                    }
                }
            }

            if (keyboardCycleTargetPrevious.Count > 0)
            {
                for (int i = 0; i < keyboardCycleTargetPrevious.Count; i++)
                {
                    if (Input.GetKeyDown(keyboardCycleTargetPrevious[i]))
                    {
                        cycleIndex--;
                        lastCycle = timeNow;
                    }
                }
            }

            if (buttonCycleTargetPrevious.Count > 0)
            {
                for (int i = 0; i < buttonCycleTargetPrevious.Count; i++)
                {
                    if (Input.GetButton(buttonCycleTargetPrevious[i]))
                    {
                        cycleIndex--;
                        lastCycle = timeNow;
                    }
                }
            }
            #endregion

            // update input values
            cameraInputLockOn.CycleIndex = cycleIndex;
            cameraInputLockOn.ForceCycle = (lastCycle == timeNow);
            cameraInputLockOn.LockOnTarget = lockOnTarget;

            // update LockOnTarget with new values
            lockOn.UpdateInput(cameraInputLockOn);
        }
    }
}
#endif