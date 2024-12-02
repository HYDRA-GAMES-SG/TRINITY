using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    [RequireComponent(typeof(Rigidbody))]
   // [RequireComponent(typeof(Collider))]
    public class KWS_Buoyancy : MonoBehaviour
    {
        public ModelSourceEnum VolumeSource = ModelSourceEnum.Mesh;
        public MeshCollider    OverrideVolumeCollider;
        public Transform       OverrideCenterOfMass;

        [Range(100, 1000)]
        public float Density = 450;
        [Range(1, 6)]
        public int SlicesPerAxisX = 2;
        [Range(1, 6)]
        public int SlicesPerAxisY = 2;
        [Range(1, 6)]
        public int SlicesPerAxisZ = 2;
        public bool isConcave = false;

        [Range(2, 32)]
        public int VoxelsLimit = 16;
        public float AngularDrag = 0.25f;
        public float Drag = 0.25f;
        [Range(0, 1)]
        public float NormalForce = 0.35f;
        public bool DebugForces = false;

        private const float DAMPFER = 0.1f;
        private const float WATER_DENSITY = 950;

        private Vector3         localArchimedesForce;
        private List<Vector3>   voxels;
        private Vector3[]         _voxelsWorldPos;

        private Vector3[] _waterWorldPos;
        private Vector3[] _waterWorldNormals;

        private bool            isMeshCollider;
        private List<Vector3[]> debugForces; // For drawing force gizmos

        private Rigidbody _rigidBody;
        private Collider _collider;

        float bounceMaxSize;

        public enum ModelSourceEnum
        {
            Collider,
            Mesh
        }
        /// <summary>
        /// Provides initialization.
        /// </summary>
        private void OnEnable()
        {
            debugForces = new List<Vector3[]>(); // For drawing force gizmos

            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();


            // Store original rotation and position
            var originalRotation = transform.rotation;
            var originalPosition = transform.position;
            transform.rotation = Quaternion.identity;
            transform.position = Vector3.zero;
            var bounds = _collider.bounds;
            bounceMaxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            isMeshCollider = GetComponent<MeshCollider>() != null;


            if (OverrideCenterOfMass) _rigidBody.centerOfMass = transform.InverseTransformPoint(OverrideCenterOfMass.transform.position);
            //else rigidBody.centerOfMass = new Vector3(0, -bounds.extents.y * 0f, 0) + transform.InverseTransformPoint(bounds.center);

            _rigidBody.angularDrag = AngularDrag;
            _rigidBody.drag = Drag;
            voxels = SliceIntoVoxels(isMeshCollider && isConcave);
        
            // Restore original rotation and position
            transform.rotation = originalRotation;
            transform.position = originalPosition;

            float volume = _rigidBody.mass / Density;

            WeldPoints(voxels, VoxelsLimit);

            float archimedesForceMagnitude = WATER_DENSITY * Mathf.Abs(Physics.gravity.y) * volume;
            localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Count;

            _voxelsWorldPos     = new Vector3[voxels.Count];

            _waterWorldPos     = new Vector3[voxels.Count];
            _waterWorldNormals = new Vector3[voxels.Count];
        }

        private void OnDisable()
        {
            
        }

        Bounds GetCurrentBounds()
        {
            Bounds bounds = new Bounds();
            if (VolumeSource == ModelSourceEnum.Mesh) bounds = GetComponent<Renderer>().bounds;
            else if (VolumeSource == ModelSourceEnum.Collider)
            {
                var meshCollider = OverrideVolumeCollider ? OverrideVolumeCollider : GetComponent<MeshCollider>();

                if (meshCollider != null)
                {
                    bounds = meshCollider.sharedMesh.bounds;
                }
                else bounds = GetComponent<Collider>().bounds;
            }
            return bounds;
        }

        private List<Vector3> SliceIntoVoxels(bool concave)
        {
            var points = new List<Vector3>(SlicesPerAxisX * SlicesPerAxisY * SlicesPerAxisZ);

            var bounds = GetCurrentBounds();

            if (concave)
            {
                var meshCol = OverrideVolumeCollider ? OverrideVolumeCollider : GetComponent<MeshCollider>();

                var convexValue  = meshCol.convex;
                meshCol.convex = false;

                // Concave slicing

                for (int ix = 0; ix < SlicesPerAxisX; ix++)
                {
                    for (int iy = 0; iy < SlicesPerAxisY; iy++)
                    {
                        for (int iz = 0; iz < SlicesPerAxisZ; iz++)
                        {
                            float x = bounds.min.x + bounds.size.x / SlicesPerAxisX * (0.5f + ix);
                            float y = bounds.min.y + bounds.size.y / SlicesPerAxisY * (0.5f + iy);
                            float z = bounds.min.z + bounds.size.z / SlicesPerAxisZ * (0.5f + iz);

                            var p = transform.InverseTransformPoint(new Vector3(x, y, z));

                            if (PointIsInsideMeshCollider(meshCol, p))
                            {
                                points.Add(p);
                            }
                        }
                    }
                }
                if (points.Count == 0)
                {
                    points.Add(bounds.center);
                }

                meshCol.convex = convexValue;
            }
            else
            {
                // Convex slicing

                for (int ix = 0; ix < SlicesPerAxisX; ix++)
                {
                    for (int iy = 0; iy < SlicesPerAxisY; iy++)
                    {
                        for (int iz = 0; iz < SlicesPerAxisZ; iz++)
                        {
                            float x = bounds.min.x + bounds.size.x / SlicesPerAxisX * (0.5f + ix);
                            float y = bounds.min.y + bounds.size.y / SlicesPerAxisY * (0.5f + iy);
                            float z = bounds.min.z + bounds.size.z / SlicesPerAxisZ * (0.5f + iz);

                            var p = transform.InverseTransformPoint(new Vector3(x, y, z));

                            points.Add(p);
                        }
                    }
                }
            }

            return points;
        }

        private static bool PointIsInsideMeshCollider(Collider c, Vector3 p)
        {
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            foreach (var ray in directions)
            {
                RaycastHit hit;
                if (c.Raycast(new Ray(p - ray * 1000, ray), out hit, 1000f) == false)
                {
                    return false;
                }
            }

            return true;
        }


        private static void FindClosestPoints(IList<Vector3> list, out int firstIndex, out int secondIndex)
        {
            float minDistance = float.MaxValue, maxDistance = float.MinValue;
            firstIndex = 0;
            secondIndex = 1;

            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    float distance = Vector3.Distance(list[i], list[j]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        firstIndex = i;
                        secondIndex = j;
                    }
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }
            }
        }


        private static void WeldPoints(IList<Vector3> list, int targetCount)
        {
            if (list.Count <= 2 || targetCount < 2)
            {
                return;
            }

            while (list.Count > targetCount)
            {
                int first, second;
                FindClosestPoints(list, out first, out second);

                var mixed = (list[first] + list[second]) * 0.5f;
                list.RemoveAt(second); // the second index is always greater that the first => removing the second item first
                list.RemoveAt(first);
                list.Add(mixed);
            }
        }

        private void FixedUpdate()
        {
            if (DebugForces) debugForces.Clear(); // For drawing force gizmos

            for (int i = 0; i < _voxelsWorldPos.Length; i++)
            {
                _voxelsWorldPos[i] = transform.TransformPoint(voxels[i]);
                _waterWorldPos[i] = _voxelsWorldPos[i];
            }

            WaterSystem.GetWaterSurfaceDataArray(_waterWorldPos, _waterWorldNormals);

            for (int i = 0; i < _voxelsWorldPos.Length; i++)
            {
                var wp                = _voxelsWorldPos[i];
                var waterPos          = _waterWorldPos[i];
                var waterNormal       = _waterWorldNormals[i];

                var velocity          = _rigidBody.GetPointVelocity(wp);
                var localDampingForce = -velocity * DAMPFER * _rigidBody.mass;

                float k = waterPos.y - wp.y;
                if (k > 1)
                {
                    k = 1f;
                }
                else if (k < 0)
                {
                    k = 0;
                    localDampingForce *= 0.2f;
                }

                float volume                   = _rigidBody.mass / Density;
                float archimedesForceMagnitude = WATER_DENSITY   * Mathf.Abs(Physics.gravity.y) * volume;
                localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Count;

                var force = localDampingForce + Mathf.Sqrt(k) * localArchimedesForce;
                
                force.x += waterNormal.x * NormalForce * _rigidBody.mass;
                force.z += waterNormal.z * NormalForce * _rigidBody.mass;
               
                _rigidBody.AddForceAtPosition(force, wp);
                if (DebugForces) debugForces.Add(new[] { wp, force }); // For drawing force gizmos

            }
        }

        private void OnDrawGizmos()
        {
            if (!DebugForces) return;

            if (voxels == null || debugForces == null)
            {
                return;
            }

            float gizmoSize = 0.02f * bounceMaxSize;
            Gizmos.color = Color.yellow;

            foreach (var p in voxels)
            {
                Gizmos.DrawCube(transform.TransformPoint(p), new Vector3(gizmoSize, gizmoSize, gizmoSize));
            }

            Gizmos.color = Color.cyan;

            foreach (var force in debugForces)
            {
                Gizmos.DrawCube(force[0], new Vector3(gizmoSize, gizmoSize, gizmoSize));
                Gizmos.DrawRay(force[0], (force[1] / _rigidBody.mass) * bounceMaxSize * 0.25f);
            }

        }
    }
}
