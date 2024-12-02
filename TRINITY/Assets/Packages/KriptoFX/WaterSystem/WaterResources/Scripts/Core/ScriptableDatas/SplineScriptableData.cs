using System;
using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    public class SplineScriptableData : ScriptableObject
    {
        [SerializeField] public List<Spline> Splines = new List<Spline>();

        [Serializable]
        public class Spline
        {
            [SerializeField] public Vector3 InitialSplinePosition;
            [SerializeField] public int               ID;
            [SerializeField] public List<SplinePoint> SplinePoints             = new List<SplinePoint>();
            [SerializeField] public int               VertexCountBetweenPoints = 20;
            [SerializeField] public float             Depth                    = 10;
        }

        [Serializable]
        public class SplinePoint
        {
            [SerializeField] public int     ID;
            [SerializeField] public Vector3 WorldPosition;
            [SerializeField] public float   Width = 10;

            public SplinePoint(int id, Vector3 worldPos)
            {
                ID            = id;
                WorldPosition = worldPos;
            }
        }
    }
}