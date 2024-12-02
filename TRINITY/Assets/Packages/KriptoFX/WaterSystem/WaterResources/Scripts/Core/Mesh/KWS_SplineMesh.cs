using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace KWS
{
    [Serializable]
    internal class KWS_SplineMesh
    {
        public Mesh CurrentMesh { get; private set; }
       
        internal List<Vector3> EditorBadVertices = new List<Vector3>();

        private MeshCollider _currentMeshCollider;
        private GameObject _currentMeshColliderGameobject;
        private Bounds _currentBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        private float _meshColliderHeightOffset = 10000;

        public MeshCollider GetOrCreateMeshCollider(WaterSystem waterInstance)
        {
            if (isMeshColliderRequiredUpdate)
            {
                if (_currentMeshColliderGameobject == null)
                {
                    _currentMeshColliderGameobject                         = new GameObject("TempRiverCollider");
                    _currentMeshColliderGameobject.hideFlags               = HideFlags.DontSave;
                    _currentMeshColliderGameobject.transform.parent        = waterInstance.transform;
                    _currentMeshColliderGameobject.transform.localPosition = Vector3.down * _meshColliderHeightOffset;
                    _currentMeshCollider                                   = _currentMeshColliderGameobject.AddComponent<MeshCollider>();
                }

                if (CurrentMesh != null) _currentMeshCollider.sharedMesh = CurrentMesh;
                isMeshColliderRequiredUpdate = false;
            }

            return _currentMeshCollider;


        }

        private bool isMeshColliderRequiredUpdate = false;
        readonly Color _aboveSurfaceWater = Color.white;
        readonly Color _underSurfaceWater = Color.black;
        
        internal List<SplineScriptableData.Spline> CurrentSplines = new List<SplineScriptableData.Spline>();
        MeshData _currentMeshData = new MeshData();
        Dictionary<SplineScriptableData.Spline, MeshData> _currentSplineMeshes = new Dictionary<SplineScriptableData.Spline, MeshData>();

        class MeshData
        {
            public List<Vector3> Vertices = new List<Vector3>();
            public List<int> Triangles = new List<int>();
            public List<Color> Colors = new List<Color>();
            public List<Vector3> Normals = new List<Vector3>();

            public void Combine(Dictionary<SplineScriptableData.Spline, MeshData> datas)
            {
                Release();
                var triangleIdx = 0;
                foreach (var data in datas)
                {
                    var meshData = data.Value;
                    Vertices.AddRange(meshData.Vertices);

                    Colors.AddRange(meshData.Colors);
                    Normals.AddRange(meshData.Normals);

                    for (int i = 0; i < meshData.Triangles.Count; i++)
                    {
                        Triangles.Add(meshData.Triangles[i] + triangleIdx);
                    }

                    triangleIdx += meshData.Vertices.Count;
                }
            }

            public void Release()
            {
                Vertices.Clear();
                Triangles.Clear();
                Colors.Clear();
                Normals.Clear();
            }
        }

        class BezierPointCache
        {
            public Vector3 PointCurrent;
            public Vector3 DirectionCurrent;

            public Vector3 DirectionNext;
            public Vector3 PointNext;

            public float Scale;
            public int RightDistance;
            public int LeftDistance;

            public int Density;
        }

        public void Release()
        {
            KW_Extensions.SafeDestroy(CurrentMesh, _currentMeshColliderGameobject);
            _currentMeshData.Release();
            _currentSplineMeshes.Clear();
            EditorBadVertices.Clear();
            isMeshColliderRequiredUpdate = false;
        }

        public void LoadOrCreateSpline(WaterSystem waterInstance)
        {
            CurrentSplines = waterInstance.Settings.SplineScriptableData == null
                ? new List<SplineScriptableData.Spline>()
                : waterInstance.Settings.SplineScriptableData.Splines;
        }

        public void UpdateSplinePivotOffset(WaterSystem waterInstance)
        {
            foreach (var spline in CurrentSplines)
            {
                spline.InitialSplinePosition = waterInstance.WaterPivotWorldPosition;
            }
        }

        public SplineScriptableData.Spline GetSplineByID(int id)
        {
            if (id < 0 && id > CurrentSplines.Count - 1)
            {
                Debug.LogError("Incorrect spline ID");
                return null;
            }

            return CurrentSplines[id];
        }

        public void SaveSplineData(WaterSystem waterInstance)
        {
#if UNITY_EDITOR
            var data = ScriptableObject.CreateInstance<SplineScriptableData>();
            data.Splines                                = CurrentSplines;
            waterInstance.Settings.SplineScriptableData = data.SaveScriptableData(waterInstance.WaterInstanceID, null, "SplineProfile");
#else
            Debug.LogError("You can't save waves data in runtime");
#endif
        }


        public Mesh CreateMeshFromSpline(WaterSystem waterInstance)
        {
            LoadOrCreateSpline(waterInstance);
            UpdateAllMeshes(waterInstance);
            return CurrentMesh;
        }

        public void UpdateAllMeshes(WaterSystem waterInstance)
        {
            if (CurrentMesh == null)
            {
                CurrentMesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave };
                CurrentMesh.indexFormat = IndexFormat.UInt32;
                if (waterInstance.SplineMeshInEditMode) CurrentMesh.MarkDynamic();
            }

            //if (CurrentSplines == null || CurrentSplines.Count == 0) return;

            _currentSplineMeshes.Clear();

            foreach (var spline in CurrentSplines)
            {
                var meshData = CreateSplineMesh(waterInstance, spline);
                _currentSplineMeshes.Add(spline, meshData);
            }

            UpdateMeshVBO(waterInstance);
            // Debug.Log("UpdateAllMeshes");
        }

        public void UpdateSelectedMesh(WaterSystem waterInstance, SplineScriptableData.Spline spline)
        {
            if (CurrentMesh == null) UpdateAllMeshes(waterInstance);
            else
            {
                var meshData = CreateSplineMesh(waterInstance, spline);
                _currentSplineMeshes[spline] = meshData;

                UpdateMeshVBO(waterInstance);
                // Debug.Log("UpdateSelectedMesh ");
            }
        }

        public Bounds GetBounds()
        {
            return _currentBounds;
        }

        public float GetSplineSurfaceHeight(WaterSystem waterInstance, Vector3 worldPos)
        {
            if (RaycastRiverCollider(waterInstance, new Ray(worldPos + new Vector3(0, 1000, 0), Vector3.down), out var hit, _meshColliderHeightOffset * 2))
            {
                return hit.point.y + _meshColliderHeightOffset;
            }

            return worldPos.y;
        }

        public bool GetSplineSurfaceHeight(WaterSystem waterInstance, Vector3 worldPos, out Vector3 surfaceWorldPos, out Vector3 surfaceNormal)
        {
            if (RaycastRiverCollider(waterInstance, new Ray(worldPos + new Vector3(0, 1000, 0), Vector3.down), out var hit, _meshColliderHeightOffset * 2))
            {
                surfaceWorldPos   =  hit.point;
                surfaceWorldPos.y += _meshColliderHeightOffset;
                surfaceNormal     =  hit.normal;
                return true;
            }

            surfaceWorldPos = worldPos;
            surfaceNormal = Vector3.up;
            return false;
        }

        private List<float>   _splineSurfaceHeights = new List<float>();
        private List<Vector3> _splineSurfaceNormals = new List<Vector3>();

        public void GetSplineSurfaceHeights(WaterSystem waterInstance, Vector3[] worldPositions, out List<float> surfaceHeights, out List<Vector3> surfaceNormals)
        {
            _splineSurfaceHeights.Clear();
            _splineSurfaceNormals.Clear();

            for (int i = 0; i < worldPositions.Length; i++)
            {
                if (RaycastRiverCollider(waterInstance, new Ray(worldPositions[i] + new Vector3(0, 1000, 0), Vector3.down), out var hit, _meshColliderHeightOffset * 2))
                {
                    _splineSurfaceHeights.Add(hit.point.y + _meshColliderHeightOffset);
                    _splineSurfaceNormals.Add(hit.normal);
                }
            }

            surfaceHeights = _splineSurfaceHeights;
            surfaceNormals = _splineSurfaceNormals;
        }

        public bool RaycastRiverCollider(WaterSystem waterInstance, Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            var meshCollider = GetOrCreateMeshCollider(waterInstance);
            if (meshCollider == null)
            {
                hitInfo = default;
                return false;
            }

            var origin = ray.origin;
            origin.y -= _meshColliderHeightOffset;
            ray.origin = origin;
            return meshCollider.Raycast(ray, out hitInfo, 1000000);
        }


        void UpdateMeshVBO(WaterSystem waterInstance)
        {
            _currentMeshData.Combine(_currentSplineMeshes);
            CurrentMesh.Clear();
            CurrentMesh.SetVertices(_currentMeshData.Vertices);
            CurrentMesh.SetTriangles(_currentMeshData.Triangles, 0);
            CurrentMesh.SetColors(_currentMeshData.Colors);
            CurrentMesh.SetNormals(_currentMeshData.Normals);

            CurrentMesh.RecalculateBounds();
            CurrentMesh.Optimize();

            if (_currentMeshData.Vertices.Count >= 3)
            {
                isMeshColliderRequiredUpdate = true;
                _currentBounds               = CurrentMesh.bounds;
            }
        }

        MeshData CreateSplineMesh(WaterSystem waterInstance, SplineScriptableData.Spline spline)
        {
            var waterPos       = waterInstance.SplineMeshInEditMode ? waterInstance.WaterPivotWorldPosition : spline.InitialSplinePosition;
            var bezierCache    = PrecacheBezierPonts(waterInstance, waterPos, spline.SplinePoints, spline.VertexCountBetweenPoints);
            var splineMeshData = GetSplineMesh(bezierCache);
            KW_Extensions.WeldVertices(ref splineMeshData.Vertices, ref splineMeshData.Triangles, ref splineMeshData.Colors, ref splineMeshData.Normals);
            RecalculateWireframe(splineMeshData);
            InitializeUnderwaterSurfaceMesh(splineMeshData, bezierCache, spline.Depth);

            return splineMeshData;
        }

        private void RecalculateWireframe(MeshData meshData)
        {
            for (var i = 0; i < meshData.Colors.Count; i++)
            {
                var color = meshData.Colors[i];
                color.b = -1;
                meshData.Colors[i] = color;
            }

            for (var i = 0; i < meshData.Triangles.Count - 6; i += 6) FillBarycentricQuad(meshData, i);
            for (var i = meshData.Triangles.Count - 6; i > 6; i -= 6) FillBarycentricQuad(meshData, i);
        }

        private void FillBarycentricQuad(MeshData meshData, int i)
        {
            var idx0 = meshData.Triangles[i];
            var idx1 = meshData.Triangles[i + 1];
            var idx2 = meshData.Triangles[i + 2];

            var idx3 = meshData.Triangles[i + 3];
            var idx4 = meshData.Triangles[i + 4];
            var idx5 = meshData.Triangles[i + 5];

            var p0 = meshData.Colors[idx0].b;
            var p1 = meshData.Colors[idx1].b;
            var p2 = meshData.Colors[idx2].b;

            var p3 = meshData.Colors[idx3].b;
            var p4 = meshData.Colors[idx4].b;
            var p5 = meshData.Colors[idx5].b;

            var inactivePointsTris1 = 0;
            var inactivePointsTris2 = 0;
            if (p0 < -0.1f) inactivePointsTris1++;
            if (p1 < -0.1f) inactivePointsTris1++;
            if (p2 < -0.1f) inactivePointsTris1++;

            if (p3 < -0.1f) inactivePointsTris2++;
            if (p4 < -0.1f) inactivePointsTris2++;
            if (p5 < -0.1f) inactivePointsTris2++;


            if (i == 0)
            {
                FillBarycentricTriangle(meshData, idx0, idx1, idx2);
                FillBarycentricTriangle(meshData, idx3, idx4, idx5);
            }

            if (inactivePointsTris1 == 3 && inactivePointsTris2 == 3) return;

            if (inactivePointsTris1 < inactivePointsTris2)
            {
                FillBarycentricTriangle(meshData, idx0, idx1, idx2);
                FillBarycentricTriangle(meshData, idx3, idx4, idx5);
            }
            else
            {
                FillBarycentricTriangle(meshData, idx3, idx4, idx5);
                FillBarycentricTriangle(meshData, idx0, idx1, idx2);
            }
        }

        void FillBarycentricTriangle(MeshData meshData, int idx1, int idx2, int idx3)
        {
            var color1 = meshData.Colors[idx1];
            var color2 = meshData.Colors[idx2];
            var color3 = meshData.Colors[idx3];



            if (color1.b < 0 && color2.b < 0 && color3.b < 0)
            {
                color1.b = 0.0f;
                color2.b = 0.5f;
                color3.b = 1f;
            }
            else
            {
                var usedX = IsBarycentricX(color1.b, color2.b, color3.b);
                var usedY = IsBarycentricY(color1.b, color2.b, color3.b);
                var usedZ = IsBarycentricZ(color1.b, color2.b, color3.b);

                if (color1.b < -0.1f)
                {
                    if (usedX && usedY && !usedZ) color1.b = 1;
                    else if (usedX && !usedY && usedZ) color1.b = 0.5f;
                    else if (!usedX && usedY && usedZ) color1.b = 0;
                }
                else if (color2.b < -0.1f)
                {
                    if (usedX && usedY && !usedZ) color2.b = 1;
                    else if (usedX && !usedY && usedZ) color2.b = 0.5f;
                    else if (!usedX && usedY && usedZ) color2.b = 0;
                }
                else if (color3.b < -0.1f)
                {
                    if (usedX && usedY && !usedZ) color3.b = 1;
                    else if (usedX && !usedY && usedZ) color3.b = 0.5f;
                    else if (!usedX && usedY && usedZ) color3.b = 0;
                }
            }

            meshData.Colors[idx1] = color1;
            meshData.Colors[idx2] = color2;
            meshData.Colors[idx3] = color3;
        }

        bool IsBarycentricX(float color1, float color2, float color3)
        {
            return (color1 >= 0f && color1 < 0.01f)
                || (color2 >= 0f && color2 < 0.01f)
                || (color3 >= 0f && color3 < 0.01f);
        }

        bool IsBarycentricY(float color1, float color2, float color3)
        {
            return (color1 > 0.49 && color1 < 0.51)
                || (color2 > 0.49 && color2 < 0.51)
                || (color3 > 0.49 && color3 < 0.51);
        }

        bool IsBarycentricZ(float color1, float color2, float color3)
        {
            return color1 > 0.99 || color2 > 0.99 || color3 > 0.99;
        }


        List<BezierPointCache> PrecacheBezierPonts(WaterSystem waterInstance, Vector3 initialSplinePosition, List<SplineScriptableData.SplinePoint> points, int gridDensity)
        {
            var bezierCache = new List<BezierPointCache>();
            var minDistance = 1;
            var t = waterInstance.WaterRootTransform;
            var lastPointIndex = points.Count - 3;

            var density = (int)Mathf.Lerp(10, 1, 1f * gridDensity / KWS_Settings.Mesh.SplineRiverMaxVertexCount);

            for (int pointIndex = 0; pointIndex <= lastPointIndex; pointIndex++)
            {
                InitializeBezierPoints(initialSplinePosition, points, pointIndex, out var p0, out var p1, out var p2);
                InitializeBezierPointsWidth(points, pointIndex, out var s0, out var s1, out var s2);

                var pointStep = 1.0f / gridDensity;
                if (pointIndex == points.Count - 3) pointStep = 1.0f / (gridDensity - 1.0f);

                for (int bezierPointIndex = 0; bezierPointIndex < gridDensity; bezierPointIndex++)
                {
                    var pointCache = new BezierPointCache();

                    var t1 = bezierPointIndex * pointStep;
                    var t2 = (bezierPointIndex + 1) * pointStep;

                    pointCache.PointCurrent = GetBezierPoint(p0, p1, p2, t1);
                    pointCache.PointNext = GetBezierPoint(p0, p1, p2, t2);
                    pointCache.DirectionCurrent = GetOrientationHorizontal(p0, p1, p2, t1);
                    pointCache.DirectionNext = GetOrientationHorizontal(p0, p1, p2, t2);
                    pointCache.Scale = GetBezierScale(s0, s1, s2, t1);

                    pointCache.Density = density;
                    pointCache.RightDistance = Mathf.Max(minDistance, Mathf.CeilToInt(RaycastFromVertex(t, pointCache.PointCurrent, pointCache.DirectionCurrent, pointCache.Scale) / pointCache.Density) + 1);
                    pointCache.LeftDistance = Mathf.Max(minDistance, Mathf.CeilToInt(RaycastFromVertex(t, pointCache.PointCurrent, -pointCache.DirectionCurrent, pointCache.Scale) / pointCache.Density) + 1);

                    bezierCache.Add(pointCache);
                }
            }

            return bezierCache;
        }

        MeshData GetSplineMesh(List<BezierPointCache> bezierCache)
        {
            var meshData = new MeshData();
            var cacheCount = bezierCache.Count;
            for (int bezierPointIndex = 0; bezierPointIndex < cacheCount; bezierPointIndex++)
            {
                var cache = bezierCache[bezierPointIndex];
                var gridDensity = cache.Density;
                for (int offsetIdx = -cache.LeftDistance * gridDensity; offsetIdx < cache.RightDistance * gridDensity; offsetIdx += gridDensity)
                {
                    var vert1 = cache.PointCurrent + cache.DirectionCurrent * offsetIdx;
                    var vert2 = cache.PointNext + cache.DirectionNext * offsetIdx;
                    var vert3 = cache.PointCurrent + cache.DirectionCurrent * (offsetIdx + gridDensity);
                    var vert4 = cache.PointNext + cache.DirectionNext * (offsetIdx + gridDensity);

                    AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _aboveSurfaceWater, _aboveSurfaceWater, _aboveSurfaceWater, false);
                }
            }

            return meshData;
        }

        void InitializeUnderwaterSurfaceMesh(MeshData meshData, List<BezierPointCache> bezierCache, float depth)
        {
            var cacheCount = bezierCache.Count;
            int lastRightDistance = 0, lastLeftDistance = 0;
            for (int bezierPointIndex = 0; bezierPointIndex < cacheCount; bezierPointIndex++)
            {
                var cache = bezierCache[bezierPointIndex];
                var heightOffset = Vector3.up * depth;
                var leftDistance = cache.LeftDistance * cache.Density;
                var rightDistance = cache.RightDistance * cache.Density;
                //start fringe
                if (bezierPointIndex == 0)
                {
                    for (int offsetIdx = -leftDistance; offsetIdx < rightDistance; offsetIdx += cache.Density)
                    {
                        var vert1 = cache.PointCurrent + cache.DirectionCurrent * offsetIdx;
                        var vert2 = vert1 - heightOffset;
                        var vert3 = cache.PointCurrent + cache.DirectionCurrent * (offsetIdx + cache.Density);
                        var vert4 = vert3 - heightOffset;
                        AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _underSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, true);
                    }
                }

                //end fringe
                if (bezierPointIndex == cacheCount - 1)
                {
                    for (int offsetIdx = -leftDistance; offsetIdx < rightDistance; offsetIdx += cache.Density)
                    {
                        var vert1 = cache.PointNext + cache.DirectionNext * offsetIdx;
                        var vert2 = vert1 - heightOffset;
                        var vert3 = cache.PointNext + cache.DirectionNext * (offsetIdx + cache.Density);
                        var vert4 = vert3 - heightOffset;
                        AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _underSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, false);
                    }
                }

                //right side
                {
                    var offset = rightDistance;
                    var vert1 = cache.PointCurrent + cache.DirectionCurrent * offset;
                    var vert2 = vert1 - heightOffset;
                    var vert3 = cache.PointNext + cache.DirectionNext * offset;
                    var vert4 = vert3 - heightOffset;
                    AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _underSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, true);

                    if (lastRightDistance < rightDistance)
                    {

                        var nextDistanceOffset = rightDistance - lastRightDistance;
                        for (int currentOffset = 0; currentOffset < nextDistanceOffset; currentOffset += cache.Density)
                        {
                            vert1 = cache.PointCurrent + cache.DirectionCurrent * (offset - currentOffset - cache.Density);
                            vert2 = cache.PointCurrent + cache.DirectionCurrent * (offset - currentOffset);
                            vert3 = vert1 - heightOffset;
                            vert4 = vert2 - heightOffset;

                            AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, _underSurfaceWater, false);
                        }
                    }

                    if (lastRightDistance > rightDistance)
                    {
                        var nextDistanceOffset = lastRightDistance - rightDistance;
                        for (int currentOffset = cache.Density; currentOffset <= nextDistanceOffset; currentOffset += cache.Density)
                        {
                            vert1 = cache.PointCurrent + cache.DirectionCurrent * (offset + currentOffset - cache.Density);
                            vert2 = cache.PointCurrent + cache.DirectionCurrent * (offset + currentOffset);
                            vert3 = vert1 - heightOffset;
                            vert4 = vert2 - heightOffset;
                            AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, _underSurfaceWater, true);
                        }

                    }
                }

                //left side
                {
                    var offset = -leftDistance;
                    var vert1 = cache.PointCurrent + cache.DirectionCurrent * (offset);
                    var vert2 = vert1 - heightOffset;
                    var vert3 = cache.PointNext + cache.DirectionNext * (offset);
                    var vert4 = vert3 - heightOffset;
                    AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _underSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, false);

                    if (lastLeftDistance < leftDistance)
                    {
                        var nextDistanceOffset = leftDistance - lastLeftDistance;
                        for (int currentOffset = 0; currentOffset < nextDistanceOffset; currentOffset += cache.Density)
                        {
                            vert1 = cache.PointCurrent + cache.DirectionCurrent * (offset + currentOffset);
                            vert2 = cache.PointCurrent + cache.DirectionCurrent * (offset + currentOffset + cache.Density);
                            vert3 = vert1 - heightOffset;
                            vert4 = vert2 - heightOffset;
                            AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, _underSurfaceWater, false);
                        }
                    }

                    if (lastLeftDistance > leftDistance)
                    {
                        var nextDistanceOffset = lastLeftDistance - leftDistance;
                        for (int currentOffset = cache.Density; currentOffset <= nextDistanceOffset; currentOffset += cache.Density)
                        {
                            vert1 = cache.PointCurrent + cache.DirectionCurrent * (offset - currentOffset);
                            vert2 = cache.PointCurrent + cache.DirectionCurrent * (offset - currentOffset + cache.Density);
                            vert3 = vert1 - heightOffset;
                            vert4 = vert2 - heightOffset;
                            AddQuad(meshData, vert1, vert2, vert3, vert4, _aboveSurfaceWater, _aboveSurfaceWater, _underSurfaceWater, _underSurfaceWater, true);
                        }

                    }
                }

                //bottom side
                {
                    var vert1 = cache.PointCurrent + cache.DirectionCurrent * -leftDistance - heightOffset;
                    var vert2 = cache.PointNext + cache.DirectionNext * -leftDistance - heightOffset;
                    var vert3 = cache.PointCurrent + cache.DirectionCurrent * rightDistance - heightOffset;
                    var vert4 = cache.PointNext + cache.DirectionNext * rightDistance - heightOffset;

                    AddQuad(meshData, vert1, vert2, vert3, vert4, _underSurfaceWater, _underSurfaceWater, _underSurfaceWater, _underSurfaceWater, true);
                }

                lastRightDistance = rightDistance;
                lastLeftDistance = leftDistance;
            }
        }

        void AddQuad(MeshData meshData, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 vert4, Color color1, Color color2, Color color3, Color color4, bool isReversed)
        {
            var vertexIndex = meshData.Vertices.Count;
            meshData.Vertices.Add(vert1);
            meshData.Vertices.Add(vert2);
            meshData.Vertices.Add(vert3);
            meshData.Vertices.Add(vert4);

            if (isReversed)
            {
                meshData.Triangles.Add(vertexIndex + 2);
                meshData.Triangles.Add(vertexIndex + 1);
                meshData.Triangles.Add(vertexIndex + 0);

                meshData.Triangles.Add(vertexIndex + 3);
                meshData.Triangles.Add(vertexIndex + 1);
                meshData.Triangles.Add(vertexIndex + 2);

                var normal1 = Vector3.Cross(vert1 - vert3, vert1 - vert2).normalized;
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);

            }
            else
            {
                meshData.Triangles.Add(vertexIndex + 0);
                meshData.Triangles.Add(vertexIndex + 1);
                meshData.Triangles.Add(vertexIndex + 2);

                meshData.Triangles.Add(vertexIndex + 2);
                meshData.Triangles.Add(vertexIndex + 1);
                meshData.Triangles.Add(vertexIndex + 3);

                var normal1 = Vector3.Cross(vert1 - vert2, vert1 - vert3).normalized;
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);
                meshData.Normals.Add(normal1);

            }

            //decoded barycentric coordinates for wireframe mode
            color1.b = 0;
            color2.b = 0.5f;
            color3.b = 1.0f;
            color4.b = 0;

            meshData.Colors.Add(color1);
            meshData.Colors.Add(color2);
            meshData.Colors.Add(color3);
            meshData.Colors.Add(color4);

            vertexIndex += 4;
        }

        void InitializeBezierPoints(Vector3 waterPivotWorldPosition, List<SplineScriptableData.SplinePoint> points, int index, out Vector3 p0, out Vector3 p1, out Vector3 p2)
        {

            var p0_local = (points[index].WorldPosition) - waterPivotWorldPosition;
            var p1_local = (points[index + 1].WorldPosition) - waterPivotWorldPosition;
            var p2_local = (points[index + 2].WorldPosition) - waterPivotWorldPosition;

            if (index == 0) p0 = (points[0].WorldPosition - waterPivotWorldPosition);
            else p0 = 0.5f * (p0_local + p1_local);

            p1 = p1_local;

            if (index == points.Count - 3) p2 = (points[points.Count - 1].WorldPosition - waterPivotWorldPosition);
            else p2 = 0.5f * (p1_local + p2_local);
        }

        void InitializeBezierPointsWidth(List<SplineScriptableData.SplinePoint> points, int index, out float s0, out float s1, out float s2)
        {
            var s0_local = points[index].Width;
            var s1_local = points[index + 1].Width;
            var s2_local = points[index + 2].Width;

            if (index == 0) s0 = points[0].Width;
            else s0 = 0.5f * (s0_local + s1_local);

            s1 = s1_local;

            if (index == points.Count - 3) s2 = points[points.Count - 1].Width;
            else s2 = 0.5f * (s1_local + s2_local);
        }

        float RaycastFromVertex(Transform t, Vector3 vertexPosition, Vector3 direction, float maxDistance)
        {
            var worldVertexPos = t.TransformPoint(vertexPosition);

            var raycastHits = Physics.RaycastAll(worldVertexPos, direction, maxDistance);
            if (raycastHits.Length == 0) return maxDistance;
            float currentMaxDistance = raycastHits.Max(n => n.distance);
            return currentMaxDistance;

            //var reversedRaycastHits = Physics.RaycastAll(worldVertexPos + direction * maxDistance, -direction, maxDistance);
            //if (reversedRaycastHits.Length == 0) return maxDistance;
            //float currentReversedMaxDistance = reversedRaycastHits.Max(n => (n.point- worldVertexPos).magnitude);


            //return Mathf.Max(currentMaxDistance, currentReversedMaxDistance);
        }


        public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        }

        public static float GetBezierScale(float p0, float p1, float p2, float t)
        {
            return (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        }

        public static Vector3 GetBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float alpha = 1f - t;
            float alpha_2 = alpha * alpha;
            float t_2 = t * t;

            var tangent = p0 * (-alpha_2) +
                          p1 * (3f * alpha_2 - 2 * alpha) +
                          p1 * (-3f * t_2 + 2 * t) +
                          p2 * (t_2);

            return tangent.normalized;
        }

        public static Vector3 GetBezierNormal(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 tangent = GetBezierTangent(p0, p1, p2, t);
            Vector3 binormal = Vector3.Cross(Vector3.up, tangent).normalized;

            return Vector3.Cross(tangent, binormal);
        }

        public static Vector3 GetOrientationHorizontal(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var tangent = GetBezierTangent(p0, p1, p2, t);
            var normal = GetBezierNormal(p0, p1, p2, t);
            //return Quaternion.LookRotation(tangent, normal);
            var orientation = Vector3.Cross(-tangent, normal);
            orientation.y = 0;
            return orientation.normalized;
        }
    }
}