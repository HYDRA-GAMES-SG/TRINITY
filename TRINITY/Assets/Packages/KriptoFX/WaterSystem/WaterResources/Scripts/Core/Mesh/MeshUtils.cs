using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static KWS.KWS_ShaderConstants;

namespace KWS
{
    internal static class MeshUtils
    {
        static uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
        static GraphicsBuffer.IndirectDrawIndexedArgs[] _indirectDrawIndexedArgs = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        public static ComputeBuffer InitializeInstanceArgsBuffer(Mesh instancedMesh, int instanceCount, ComputeBuffer argsBuffer, bool isStereo)
        {
            if (instancedMesh == null) return argsBuffer;

            if (isStereo) instanceCount *= 2;

            // Arguments for drawing mesh.
            // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
            _args[0] = instancedMesh.GetIndexCount(0);
            _args[1] = (uint)instanceCount;
            _args[2] = instancedMesh.GetIndexStart(0);
            _args[3] = instancedMesh.GetBaseVertex(0);
            if (argsBuffer == null) argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(_args);
            return argsBuffer;
        }
        public static GraphicsBuffer InitializeInstanceArgsGraphicsBuffer(Mesh instancedMesh, int instanceCount, GraphicsBuffer argsBuffer, bool isStereo)
        {
            if (instancedMesh == null) return argsBuffer;
            if (isStereo) instanceCount *= 2;

            _indirectDrawIndexedArgs[0].baseVertexIndex = instancedMesh.GetBaseVertex(0);
            _indirectDrawIndexedArgs[0].indexCountPerInstance = instancedMesh.GetIndexCount(0);
            _indirectDrawIndexedArgs[0].instanceCount = (uint)instanceCount;
            _indirectDrawIndexedArgs[0].startIndex = instancedMesh.GetIndexStart(0);

            if (argsBuffer == null) argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            argsBuffer.SetData(_indirectDrawIndexedArgs);
            return argsBuffer;
        }
        public static void InitializePropertiesBuffer<T>(List<T> meshProperties, ref ComputeBuffer propertiesBuffer, bool isStereo) where T : struct
        {
            var instanceCount = meshProperties.Count;
            if (instanceCount == 0) return;

            if (isStereo) instanceCount *= 2;
           
            if (propertiesBuffer == null)
            {
                propertiesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(T)));
            }
            else if (instanceCount > propertiesBuffer.count)
            {
                propertiesBuffer.Dispose();
                propertiesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(T)));
                //Debug.Log("ReInitialize PropertiesBuffer");
            }
           
            if (isStereo)
            {
                var tempArr = new T[meshProperties.Count * 2];
                var idx = 0;
                for (var i = 0; i < tempArr.Length; i+= 2)
                {
                    var meshProperty = meshProperties[idx];
                    tempArr[i] = meshProperty;
                    tempArr[i + 1] = meshProperty;
                    idx++;
                }

                propertiesBuffer.SetData(tempArr);
            }
            else propertiesBuffer.SetData(meshProperties);
        }

        
        public static Mesh GenerateInstanceMesh(Vector2Int meshResolution, MeshQuadTree.QuadTreeTypeEnum quadTreeType)
        {
            var vertices = KW_Extensions.InitializeListWithDefaultValues((meshResolution.x + 1) * (meshResolution.y + 1), Vector3.zero);
            var uv = KW_Extensions.InitializeListWithDefaultValues(vertices.Count, Vector2.zero);
            var colors = KW_Extensions.InitializeListWithDefaultValues(vertices.Count, Color.white);
            var normals = KW_Extensions.InitializeListWithDefaultValues(vertices.Count, Vector3.up);
            var triangles = KW_Extensions.InitializeListWithDefaultValues(meshResolution.x * meshResolution.y * 6, 0);

            var quadOffset = new Vector2(1f / meshResolution.x, 1f / meshResolution.y);

            for (int i = 0, y = 0; y <= meshResolution.y; y++)
                for (var x = 0; x <= meshResolution.x; x++, i++)
                {
                    vertices[i] = new Vector3((float)x / meshResolution.x - 0.5f, 0, (float)y / meshResolution.y - 0.5f);

                    //uv used as mask for seam vertexes, 0.1 = down, 0.2 = left, 0.3 = top, 0.4 = right,
                    //pattern for down looks like that
                    //  □---------□---------□           □---------□---------□
                    //  |      ∕  |      ∕  |           |      ∕  |      ∕  |
                    //  |    ∕    |    ∕    |           |    ∕    |    ∕    |
                    //  |  ∕      |  ∕      |           |  ∕      |  ∕      |
                    //  □---------□---------□     ->    □---------□---------□  
                    //  |      ∕  |      ∕  |           |      ∕     \      |
                    //  |    ∕    |    ∕    |           |    ∕         \    |
                    //  |  ∕      |  ∕      |           |  ∕             \  |
                    //  □---------■---------□           □---------□---->----■
                    //  1.0      0.1       1.0         1.0       1.0       0.1
                    uint flag = 0;
                    float offset = 0;

                    if (y == 0 && x % 2 == 1)
                    {
                        flag = SetFlag(flag, 1);
                        offset = quadOffset.x;
                    }
                    if (x == 0 && y % 2 == 1)
                    {
                        flag = SetFlag(flag, 2);
                        offset = quadOffset.y;
                    }
                    if (y == meshResolution.y && x % 2 == 1)
                    {
                        flag = SetFlag(flag, 3);
                        offset = quadOffset.x;
                    }
                    if (x == meshResolution.x && y % 2 == 1)
                    {
                        flag = SetFlag(flag, 4);
                        offset = quadOffset.y;
                    }

                    if (quadTreeType == MeshQuadTree.QuadTreeTypeEnum.Infinite)
                    {
                        if (y == 0) flag = SetFlag(flag, 5);
                        if (x == 0) flag = SetFlag(flag, 6);
                        if (y == meshResolution.y) flag = SetFlag(flag, 7);
                        if (x == meshResolution.x) flag = SetFlag(flag, 8);
                    }

                    uv[i] = new Vector2(flag, offset);
                }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution.y; y++, vi++)
                for (var x = 0; x < meshResolution.x; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution.x + 1;
                    triangles[ti + 5] = vi + meshResolution.x + 2;
                }

            if (quadTreeType == MeshQuadTree.QuadTreeTypeEnum.Finite) GenerateSkirt(meshResolution, vertices, triangles, uv, colors, normals);

            var mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uv);

            return mesh;
        }

        static uint SetFlag(uint data, int bit)
        {
            return data | 1u << bit;
        }

        public static void GenerateSkirt(Vector2Int meshResolution, List<Vector3> quadTreeVertices, List<int> quadTreeTriangles, List<Vector2> quadTreeUV, List<Color> quadTreeColors, List<Vector3> quadTreeNormals)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var colors = new List<Color>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();


            var quadOffset = new Vector2(1f / meshResolution.x, 1f / meshResolution.y);
            var weldLockQuadOffset = quadOffset * 0.9999f;
            var trianglesIdx = 0;
            for (int y = -1; y <= meshResolution.y; y++)
            {
                for (int x = -1; x <= meshResolution.x; x++)
                {
                    if (x != -1 && x != meshResolution.x && y != -1 && y != meshResolution.y) continue;

                    var quadPivot = new Vector3((float)x / meshResolution.x - 0.5f, 0, (float)y / meshResolution.y - 0.5f);
                    var vert1 = quadPivot;
                    var vert2 = quadPivot + new Vector3(quadOffset.x, 0, 0);
                    var vert3 = quadPivot + new Vector3(0, 0, quadOffset.y);
                    var vert4 = quadPivot + new Vector3(quadOffset.x, 0, quadOffset.y);

                    var uv1 = Vector2.zero;
                    var uv2 = Vector2.zero;
                    var uv3 = Vector2.zero;
                    var uv4 = Vector2.zero;

                    var color1 = new Color(0.0f, 1.0f, 1f);
                    var color2 = new Color(0.0f, 1.0f, 1f);
                    var color3 = new Color(0.0f, 1.0f, 1f);
                    var color4 = new Color(0.0f, 1.0f, 1f);

                    uint flag = 0;

                    if (y == -1)
                    {
                        vert1.z += weldLockQuadOffset.y;
                        vert2.z += weldLockQuadOffset.y;
                        uv1.x = SetFlag(flag, 5);
                        uv2.x = SetFlag(flag, 5);
                        color1 = color2 = new Color(0.0f, 0.0f, 1f);
                    }

                    if (x == -1)
                    {
                        vert1.x += weldLockQuadOffset.x;
                        vert3.x += weldLockQuadOffset.x;
                        uv1.x = SetFlag(flag, 6);
                        uv3.x = SetFlag(flag, 6);
                        color1 = color3 = new Color(0.0f, 0.0f, 1f);
                    }

                    if (y == meshResolution.y)
                    {
                        vert3.z -= weldLockQuadOffset.y;
                        vert4.z -= weldLockQuadOffset.y;
                        uv3.x = SetFlag(flag, 7);
                        uv4.x = SetFlag(flag, 7);
                        color3 = color4 = new Color(0.0f, 0.0f, 1f);
                    }

                    if (x == meshResolution.x)
                    {
                        vert2.x -= weldLockQuadOffset.x;
                        vert4.x -= weldLockQuadOffset.x;
                        uv2.x = SetFlag(flag, 8);
                        uv4.x = SetFlag(flag, 8);
                        color2 = color4 = new Color(0.0f, 0.0f, 1f);
                    }


                    vertices.Add(vert1);
                    vertices.Add(vert2);
                    vertices.Add(vert3);
                    vertices.Add(vert4);

                    triangles.Add(trianglesIdx);
                    triangles.Add(trianglesIdx + 2);
                    triangles.Add(trianglesIdx + 1);
                    triangles.Add(trianglesIdx + 1);
                    triangles.Add(trianglesIdx + 2);
                    triangles.Add(trianglesIdx + 3);

                    uv.Add(uv1);
                    uv.Add(uv2);
                    uv.Add(uv3);
                    uv.Add(uv4);

                    colors.Add(color1);
                    colors.Add(color2);
                    colors.Add(color3);
                    colors.Add(color4);

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    trianglesIdx += 4;
                }
            }

            KW_Extensions.WeldVertices(ref vertices, ref triangles, ref colors, ref normals, ref uv);

            quadTreeVertices.AddRange(vertices);
            quadTreeColors.AddRange(colors);
            quadTreeNormals.AddRange(normals);
            quadTreeUV.AddRange(uv);

            var quadTreeVertCount = quadTreeTriangles[quadTreeTriangles.Count - 1] + 1;
            foreach (var triangleIdx in triangles)
            {
                quadTreeTriangles.Add(triangleIdx + quadTreeVertCount);
            }
        }
        static void CreatePlane(Vector2Int res, List<Vector3> vertices, List<Vector3> normals, List<Color> colors, List<int> triangles)
        {
            var lastTrisIdx = triangles.Count;
            var lastVertIdx = vertices.Count;

            for (int y = 0; y <= res.y; y++)
                for (var x = 0; x <= res.x; x++)
                {
                    vertices.Add(new Vector3((float)x / res.x - 0.5f, -1.0f, (float)y / res.y - 0.5f));
                    normals.Add(Vector3.down);
                    colors.Add(Color.black);
                }


            var trisCounts = res.x * res.y * 6;
            for (int i = 0; i < trisCounts; i++) triangles.Add(0);

            for (int y = 0; y < res.y; y++)
            {
                for (int x = 0; x < res.x; x++)
                {
                    int ti = lastTrisIdx + (y * (res.y) + x) * 6;
                    triangles[ti + 0] = lastVertIdx + (y * (res.x + 1)) + x;
                    triangles[ti + 2] = lastVertIdx + ((y + 1) * (res.x + 1)) + x;
                    triangles[ti + 1] = lastVertIdx + ((y + 1) * (res.x + 1)) + x + 1;

                    triangles[ti + 3] = lastVertIdx + (y * (res.x + 1)) + x;
                    triangles[ti + 5] = lastVertIdx + ((y + 1) * (res.x + 1)) + x + 1;
                    triangles[ti + 4] = lastVertIdx + (y * (res.x + 1)) + x + 1;
                }
            }
        }

        public static Mesh CreatePlaneXZMesh(int meshResolution, float scale)
        {
            var vertices  = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
            var uv        = new Vector2[vertices.Length];
            var triangles = new int[meshResolution * meshResolution * 6];
            var normals = new Vector3[vertices.Length];

            for (int i = 0, y = 0; y <= meshResolution; y++)
            for (var x = 0; x <= meshResolution; x++, i++)
            {
                vertices[i] = new Vector3(x * scale / meshResolution - 0.5f     * scale, 0, y * scale / meshResolution - 0.5f * scale);
                uv[i]       = new Vector2(x         * scale / meshResolution, y * scale / meshResolution);
                normals[i]  = new Vector3(0, 1, 0);
            }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
            for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
            {
                triangles[ti]     = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
                triangles[ti + 5] = vi + meshResolution + 2;
            }

            var indexFormat = vertices.Length >= 65536 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            var mesh        = new Mesh { hideFlags = HideFlags.DontSave, indexFormat = indexFormat };

            mesh.Clear();
            mesh.vertices  = vertices;
            mesh.uv        = uv;
            mesh.triangles = triangles;
            mesh.normals   = normals;
            return mesh;
        }

        public static Mesh CreatePlaneMesh(int meshResolution, float scale)
        {
            var vertices = new Vector3[(meshResolution + 1) * (meshResolution + 1)];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[meshResolution * meshResolution * 6];

            for (int i = 0, y = 0; y <= meshResolution; y++)
                for (var x = 0; x <= meshResolution; x++, i++)
                {
                    vertices[i] = new Vector3(x * scale / meshResolution - 0.5f * scale, y * scale / meshResolution - 0.5f * scale, 0);
                    uv[i] = new Vector2(x * scale / meshResolution, y * scale / meshResolution);
                }

            for (int ti = 0, vi = 0, y = 0; y < meshResolution; y++, vi++)
                for (var x = 0; x < meshResolution; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution + 1;
                    triangles[ti + 5] = vi + meshResolution + 2;
                }

            var mesh = new Mesh { hideFlags = HideFlags.DontSave, indexFormat = IndexFormat.UInt32 };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            return mesh;
        }

        public static Mesh CreateCubeMesh(float heightOffset = 0)
        {
            Vector3[] vertices =
            {
                new Vector3(-0.5f, -0.5f + heightOffset, -0.5f),
                new Vector3(0.5f,  -0.5f + heightOffset, -0.5f),
                new Vector3(0.5f,  0.5f  + heightOffset, -0.5f),
                new Vector3(-0.5f, 0.5f  + heightOffset, -0.5f),
                new Vector3(-0.5f, 0.5f  + heightOffset, 0.5f),
                new Vector3(0.5f,  0.5f  + heightOffset, 0.5f),
                new Vector3(0.5f,  -0.5f + heightOffset, 0.5f),
                new Vector3(-0.5f, -0.5f + heightOffset,                0.5f),
            };

            int[] triangles =
            {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            var cubeMesh = new Mesh {hideFlags = HideFlags.HideAndDontSave};

            cubeMesh.Clear();
            cubeMesh.vertices  = vertices;
            cubeMesh.triangles = triangles;
            cubeMesh.RecalculateNormals();

            return cubeMesh;
        }

        public static Mesh CreateMeshFromBounds(Bounds bounds, Vector3 waterSize, Vector3 waterPos)
        {
            var size    = bounds.size;
            size   =  KW_Extensions.ClampVector3(size, Vector3.one, Vector3.one * 20000);
            size.x /= waterSize.x;
            size.y /= waterSize.y;
            size.z /= waterSize.z;
           
            var extents = size * 0.5f;
            var center  = bounds.center;
            center -= waterPos;

            var min = center - extents;
            var max = center + extents;
      
            Vector3[] vertices =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, max.y, max.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(min.x, min.y, max.z),
            };
            
            int[] triangles =
            {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            var cubeMesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };

            cubeMesh.Clear();
            cubeMesh.vertices  = vertices;
            cubeMesh.triangles = triangles;
            cubeMesh.RecalculateNormals();

            return cubeMesh;
        }

        public static Mesh GenerateUnderwaterBottomSkirt(Vector2Int meshResolution)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();

            CreatePlane(new Vector2Int(1, 1), vertices, normals, colors, triangles);

            var mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            return mesh;
        }

       
        static Vector3?[,] GetSurfacePositions(Mesh sourceMesh, Vector3 meshScale, WaterSystem waterSystem, Vector3 position, int textureSize, float areaSize)
        {
            var meshColliderGO = KW_Extensions.CreateHiddenGameObject("WaterMeshCollider");
            meshColliderGO.transform.parent = waterSystem.transform;
            meshColliderGO.transform.position = waterSystem.WaterRootTransform.position;
            meshColliderGO.transform.rotation = waterSystem.WaterRootTransform.rotation;
            meshColliderGO.transform.localScale = meshScale;

            var meshCollider = meshColliderGO.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = sourceMesh;

            areaSize /= 2;

            var halfTexSize = textureSize / 2f;
            var pixelsPetMeter = halfTexSize / areaSize;
            var meshColliderMaxHeight = waterSystem.WorldSpaceBounds.max.y + 100;

            var surfacePositions = new Vector3?[textureSize, textureSize];
            var worldRay = new Ray(Vector3.zero, Vector3.down);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    worldRay.origin = new Vector3(position.x + (x - halfTexSize) / pixelsPetMeter, meshColliderMaxHeight, position.z + (y - halfTexSize) / pixelsPetMeter);
                    if (meshCollider.Raycast(worldRay, out var surfaceHit, 10000))
                    {
                        surfacePositions[x, y] = surfaceHit.point;
                    }
                }
            }
            KW_Extensions.SafeDestroy(meshCollider);
            return surfacePositions;
        }

        public static Texture2D GetRiverOrthoDepth(WaterSystem waterSystem, Vector3 position, int areaSize, int textureSize, float nearPlane, float farPlane)
        {
#if UNITY_EDITOR
            var currentHitBackfaces = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;

            var maxDepth = KWS_Settings.SurfaceDepth.MaxSurfaceDepthMeters;
            var colors = new Color[textureSize * textureSize];

            Vector3?[,] surfacePositions = null;
            if (waterSystem.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.River)
            {
                surfacePositions = GetSurfacePositions(waterSystem.SplineRiverMesh, Vector3.one,  waterSystem, position, textureSize, areaSize);
            }
            else if (waterSystem.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh)
            {
                surfacePositions = GetSurfacePositions(waterSystem.Settings.CustomMesh, waterSystem.WaterSize, waterSystem, position, textureSize, areaSize);
            }

            var maxRayDistanceAbove = 1.0f;
            var halfTexSize = textureSize / 2f;
            var metersInPixel = (float)areaSize / textureSize;
            var pixelHalfOffset = metersInPixel / 2f;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    var depth = maxDepth;
                    Vector3? rayPos = null;
                    if (waterSystem.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.River || waterSystem.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh)
                    {
                        rayPos = surfacePositions[x, y];
                    }
                    else rayPos = position + new Vector3((x - halfTexSize) * metersInPixel + pixelHalfOffset, 0, (y - halfTexSize) * metersInPixel + pixelHalfOffset);

                    if (rayPos != null)
                    {
                        var surfacePoint = (Vector3)rayPos;

                        if (Physics.Raycast(surfacePoint, Vector3.down, out var hitDepth, maxDepth)) depth = hitDepth.distance;
                        if (Physics.Raycast(new Vector3(surfacePoint.x, surfacePoint.y + maxRayDistanceAbove, surfacePoint.z), Vector3.down, maxRayDistanceAbove)) depth = 0;
                        if (Physics.CheckSphere(surfacePoint, metersInPixel)) depth = 0;

                    }

                    colors[y * textureSize + x] = new Color(depth, 0, 0, 1);
                }
            }

            var tempTex = new Texture2D(textureSize, textureSize, GraphicsFormat.R16_UNorm, TextureCreationFlags.None);
            tempTex.SetPixels(colors);
            tempTex.Apply();

            Physics.queriesHitBackfaces = currentHitBackfaces;

            return tempTex;
#else 
        return null;
#endif
        }
    }
}