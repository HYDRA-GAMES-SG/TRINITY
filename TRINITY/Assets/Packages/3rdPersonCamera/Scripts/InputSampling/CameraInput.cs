// <copyright project="ThirdPersonCamera" file="CameraInput.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using UnityEngine;

namespace ThirdPersonCamera
{
    [Serializable]
    public enum FreeLookMouseHideMode : byte
    {
        OnInput,
        Always
    }
    
    [Serializable]
    public struct CameraInput
    {
        public CameraInputFreeForm InputFreeForm;
        public CameraInputLockOn InputLockOn;
        public CameraInputShoulder InputShoulder;
    }
    
    [Serializable]
    public struct CameraInputFreeForm
    {
        public Vector2 CameraInput;
        public Vector2 MouseSensitivity;
        public Vector2 ControllerSensitivity;
        
        public float ZoomInput;
        
        public FreeLookMouseHideMode MouseHideMode;
        public bool InputFreeLook; 
        public bool MiddleMouseButtonPressed;
        public bool ForceTargetDirection;
        public bool ControllerInvertY;
        public bool MouseInvertY;

        public bool HasInput()
        {
            return InputFreeLook && (CameraInput.x != 0 || CameraInput.y != 0);
        }
    }
    
    [Serializable]
    public struct CameraInputLockOn
    {
        public int CycleIndex;
        public bool LockOnTarget;
        
        internal bool ForceCycle;
    }
    
    [Serializable]
    public struct CameraInputShoulder
    {
        public bool Aiming;
        public bool Left;
    }
}