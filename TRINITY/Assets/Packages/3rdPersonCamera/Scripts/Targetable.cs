// <copyright project="ThirdPersonCamera" file="Targetable.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonCamera
{
    public class Targetable : MonoBehaviour
    {
        [FormerlySerializedAs("offset")] 
        [Tooltip("Set an offset for the target when the transform.position is not fitting.")]
        public Vector3 Offset;
    }
}
