// <copyright project="ThirdPersonCamera" file="LayerMaskHelper.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;

namespace ThirdPersonCamera
{
    public static class LayerMaskHelper
    {

        public static bool IsInLayerMask(this LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) > 0);
        }

        public static bool IsInLayerMask(this LayerMask mask, GameObject obj)
        {
            return ((mask.value & (1 << obj.layer)) > 0);
        }
    }
}

