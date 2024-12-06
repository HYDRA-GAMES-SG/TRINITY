// <copyright project="ThirdPersonCamera" file="CameraControllerState.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using UnityEngine;

namespace ThirdPersonCamera
{
    [Serializable]
    public struct CameraControllerState
    {
        public Quaternion CameraRotation;   // do not directly write to cameraRotation, use cameraAngles instead
        public Quaternion SmartPivotRotation;
        public Quaternion CurrentPivotRotation;
        public Quaternion PivotRotation;

        public Vector3 CameraAngles;
        public Vector3 TargetPosition;
        public Vector3 SmoothedTargetPosition;
        public Vector3 CurrentCameraOffsetVector;
        public Vector3 DesiredPosition;
        public Vector3 DirToDesiredPosition;
        public Vector3 PrevPosition;

        public float Distance;
        public float PresentationDistance;
        public float SmartPivotStartingX;
        public float CameraOffsetMagnitude;
        public float OffsetSafeguardStartingY;
        public float OffsetSafeguardStartingDistance;

        public bool OffsetSafeguardInitialized;
        public bool CameraNormalMode;
        public bool GroundHit;
        public bool SmartPivotInitialized;
        public bool OcclusionHitHappened;
        public bool InvertCameraOffsetVector;
    }
}