// <copyright project="ThirdPersonCamera" file="SortData.cs" version="1.1.2">
// Copyright © 2015-2024 Thomas Enzenebner. All rights reserved.
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCamera
{
    public class SortDistance : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b)
        {
            if (a.distance > b.distance)
                return 1;
            if (a.distance < b.distance) 
                return -1;
            
            return 0;
        }
    }

    // Data for LockOnTarget
    public struct TargetableWithDistance
    {
        public Targetable target { get; set; }
        public float distance { get; set; }
        public float angle { get; set; }

        public float score;
    }

    public class SortTargetables : IComparer<TargetableWithDistance>
    {
        public int Compare(TargetableWithDistance a, TargetableWithDistance b)
        {
            if (a.score > b.score)
                return 1;
            else if (a.score < b.score)
                return -1;
            else
                return 0;
        }
    }
}
