// <copyright project="ThirdPersonCamera" file="OverTheShoulder.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    [RequireComponent(typeof(CameraController))]
    [DefaultExecutionOrder(70)]
    public class OverTheShoulder : MonoBehaviour
    {
        static public System.Action Aiming;
        
        [FormerlySerializedAs("aimOffsetLength")]
        [Header("Basic settings")]
        [Tooltip("The distance the camera moves away from its zero position. 0.5 means it'll set the max camera offset vector to (axis * maxValue), so -0.5f to 0.5f")]
        public float AimOffsetLength = 0.5f;
        
        [FormerlySerializedAs("aimDistance")] [Tooltip("The distance from the target when aiming")]
        public float AimDistance = 1;
        [FormerlySerializedAs("releaseDistance")] [Tooltip("The distance from the target when released")]
        public float ReleaseDistance = 3;
        
        [FormerlySerializedAs("baseOffset")] [Tooltip("The base offset serves as starting and endpoint when releasing.")]
        public Vector3 BaseOffset = new Vector3(0, 0, 0);
        [FormerlySerializedAs("slideAxis")] [Tooltip("You can tweak the axis on which the camera slides on, usually it will be just operating on the x axis to slide left and right from the targeted character but it can be changed to any direction in case gravity changes for example. The intended design is to use normalized values between - 1 and 1 The difference to the \"Additional Axis Movement\" vector is that the slide axis goes back and forth when aiming / releasing")]
        public Vector3 SlideAxis = new Vector3(1, 0, 0);
        [FormerlySerializedAs("additionalAxisMovement")] [Tooltip("This axis can be used to have additional offsets when aiming. Unlike the slide axis this axis is intended for non - normalized values much like the base offset. It can be used to make the camera zoom high above the character for example when aiming.")]
        public Vector3 AdditionalAxisMovement = new Vector3(0, 0, 0);

        [FormerlySerializedAs("customInput")]
        [Header("Extra settings")]
        [Tooltip("Inform the script of using a custom input method to set the CameraInputShoulder model")]
        public bool CustomInput;

        private CameraController cc;
        private CameraInputShoulder inputShoulder;

        private void Start()
        {
            cc = GetComponent<CameraController>();
            cc.DisableZooming = true;

            if (!CustomInput)
            {
#if ENABLE_INPUT_SYSTEM
                var lookup = GetComponent<CameraInputSampling>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling not found. Consider adding it to get input sampling or enable customInput to skip this message");
#else
                var lookup = GetComponent<CameraInputSampling_Shoulder>();
                if (lookup == null)
                    Debug.LogError("CameraInputSampling_Shoulder not found on " + transform.name + ". Consider adding it to get input sampling or enable customInput to skip this message");
#endif
            }
        }

        public CameraInputShoulder GetInput()
        {
            return inputShoulder;
        }

        public void UpdateInput(CameraInputShoulder newInput)
        {
            inputShoulder = newInput;
        }

        private void Update()
        {
            var currentBaseOffset = (inputShoulder.Left ? -BaseOffset : BaseOffset);

            if (inputShoulder.Aiming) // aim mode
            {
                if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PLAY)
                {
                    ATrinityGameManager.GetGUI().Crosshair.SetActive(false);
                }
                
                float value = (inputShoulder.Left ? -AimOffsetLength : AimOffsetLength);
                Vector3 newOffset = value * SlideAxis + currentBaseOffset + AdditionalAxisMovement;

                cc.UpdateCameraOffsetVector(newOffset);
                
                if (AimDistance >= 0)
                {
                    cc.DesiredDistance = AimDistance;
                }
            }
            else
            {
                if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PLAY)
                {
                    ATrinityGameManager.GetGUI().Crosshair.SetActive(true);//PlayerPrefs.GetInt("bCrossHairEnabled", 1) > 0 ? true : false);
                }
                

                cc.UpdateCameraOffsetVector(currentBaseOffset);
                
                if (ReleaseDistance >= 0)
                {
                    cc.DesiredDistance = ReleaseDistance;
                }
            }
        }
    }
}