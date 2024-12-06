// <copyright project="ThirdPersonCamera" file="MathHelper.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using UnityEngine;

namespace ThirdPersonCamera
{
    public static class MathHelper
    {
        public static Vector3 Lerp(Vector3 x, Vector3 y, Vector3 s)
        {
            var tmp = (y - x);
            tmp.x *= s.x;
            tmp.y *= s.y;
            tmp.z *= s.z;
            return x + tmp; 
        }

        public static Vector3 Slerp(Vector3 a, Vector3 b, float t, Vector3 up)
        {
            var q1 = Quaternion.LookRotation(a, up);
            var q2 = Quaternion.LookRotation(b, up);

            var q3 = Quaternion.Slerp(q1, q2, t);

            return q3 * Vector3.forward;
        }
        
        public static float Clamp(float x, float a, float b)
        {
            return Math.Max(a, Math.Min(b, x));
        }
    }
}