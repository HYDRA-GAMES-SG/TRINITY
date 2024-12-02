using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    [ExecuteInEditMode]
    public class KWS_InteractWithWater : MonoBehaviour
    {
        public InteractMeshTypeEnum MeshType = InteractMeshTypeEnum.Sphere;
        [Range(0.025f, 10)]
        public float Size = 0.5f;
        [Range(-1, 1)]
        public float StrengthMultiplier = 0.5f;
        [Range(-1, 1)]
        public float AdditionalConstantStrength = 0f;
        public Vector3 Offset = Vector3.zero;

        [Range(0.1f, 1)]
        public float MeshIntersectionThreshold = 0.25f;

        Transform _t;
        private Mesh _mesh;
        private float _meshBoundsRadius;
        private WaterSystem _lastIntersectedWaterInstance;

        private float _lastForceUpdateTime;

        const float updateForceEachSeconds = 0.2f;
        const float minForce = 0.1f;
        const float forceMul = 0.25f;

        private float _force;
        internal DynamicWaveDataStruct DynamicWaveData;

        internal static List<KWS_InteractWithWater> Instances = new List<KWS_InteractWithWater>();
        internal        bool                        IsWaterIntersected;
        private         Vector3                     _relativePos;
        private         Vector3                     _lastPos;
        private         float                       _lastWaterSurfaceHeight;

        internal Mesh Mesh;

        //don't forget about 32 bits pad!
        public struct DynamicWaveDataStruct
        {
            public uint ProceduralType;
            public float Force;
            public float WaterHeight;
            public float Size;

            public Vector3 Position;
            public float _pad;
        }

        public enum InteractMeshTypeEnum
        {
            Sphere = 1,
            Mesh = 0
        }

        public Transform t
        {
            get
            {
                if (_t == null) _t = transform;
                return _t;
            }
        }

       

        void OnEnable()
        {
            Instances.Add(this);

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh = meshFilter.sharedMesh;
                _meshBoundsRadius = KW_Extensions.BoundsToSphereRadius(Mesh.bounds, t.localScale) * 2.0f;
            }
            else if (MeshType == InteractMeshTypeEnum.Mesh) Debug.LogError(name + " (KWS_DynamicWaves) Can't find the mesh");

            UpdateData();
            
        }

        void OnDisable()
        {
            Instances.Remove(this);
        }

        void OnDrawGizmos()
        {
            if (MeshType == InteractMeshTypeEnum.Sphere)
            {
                // Draw a yellow sphere at the transform's position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(t.TransformPoint(Offset), Size * 0.5f);
            }
            if (MeshType == InteractMeshTypeEnum.Mesh)
            {
                // Draw a yellow sphere at the transform's position
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(t.TransformPoint(Offset), _meshBoundsRadius * 0.5f);
            }
        }

        float SqrDistanceFast(Vector3 vec1, Vector3 vec2)
        {
            var posX = vec1.x - vec2.x;
            var posY = vec1.y - vec2.y;
            var posZ = vec1.z - vec2.z;
            return posX * posX + posY * posY + posZ * posZ;
        }

        void UpdateData()
        {
            _lastForceUpdateTime += KW_Extensions.DeltaTime();

            var sqrDistance = SqrDistanceFast(_lastPos, _relativePos);
            if (sqrDistance > 0.001f)
            {
                var newForce = forceMul * Mathf.Sqrt(sqrDistance) + minForce;
                newForce = 1 - Mathf.Exp(-newForce);
                _force = Mathf.Max(_force, newForce);

                _lastPos             = _relativePos;
                _lastForceUpdateTime = 0;
            }

            _force                         = Mathf.Max(_force - 0.05f, 0);

            DynamicWaveData.ProceduralType = (uint)MeshType;
            DynamicWaveData.Position       = _lastPos;
            DynamicWaveData.Size           = Size;
            DynamicWaveData.Force          = _force * StrengthMultiplier + AdditionalConstantStrength;

            //if (_lastIntersectedWaterInstance != null) DynamicWaveData.WaterHeight = _lastIntersectedWaterInstance.WaterPivotWorldPosition.y;
            DynamicWaveData.WaterHeight = _lastWaterSurfaceHeight;
        }

       
        bool IsIntersectWater(WaterSystem instance, float size)
        {
            _lastWaterSurfaceHeight = instance.GetCurrentWaterSurfaceHeightFast(_relativePos);
            if (_relativePos.y + size * 0.5f < _lastWaterSurfaceHeight)
            {
                return false;
            }

            return  KW_Extensions.IsAABBIntersectSphereOptimized(instance.WorldSpaceBounds, _relativePos, size * 0.5f);
        }

        internal void CustomUpdate()
        {
            var size = MeshType == InteractMeshTypeEnum.Sphere ? Size : _meshBoundsRadius;
            _relativePos = t.TransformPoint(Offset);

            if (_lastIntersectedWaterInstance == null || !IsIntersectWater(_lastIntersectedWaterInstance, size))
            {
                _lastIntersectedWaterInstance = null;
                IsWaterIntersected            = false;
                foreach (var waterInstance in WaterSharedResources.WaterInstances)
                {
                    if (IsIntersectWater(waterInstance, size))
                    {
                        _lastIntersectedWaterInstance = waterInstance;
                        IsWaterIntersected            = true;
                        break;
                    }
                }
            }

            if(_lastIntersectedWaterInstance != null) UpdateData();
        }
    }
}