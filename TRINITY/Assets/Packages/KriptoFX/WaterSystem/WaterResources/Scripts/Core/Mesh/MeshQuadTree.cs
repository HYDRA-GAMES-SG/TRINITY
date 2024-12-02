using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace KWS
{
    internal class MeshQuadTree
    {
        internal Dictionary<Camera, QuadTreeInstance> Instances = new Dictionary<Camera, QuadTreeInstance>();

        public class QuadTreeInstance
        {
            public bool CanRender => InstanceMeshesArgs.Count > 0 && InstanceMeshesArgs[0] != null;
            public int ActiveMeshDetailingInstanceIndex = 0;

            public List<GraphicsBuffer> InstanceMeshesArgs = new List<GraphicsBuffer>();

            public List<GPUChunkInstance> VisibleGPUChunks = new List<GPUChunkInstance>();
            public ComputeBuffer         VisibleChunksComputeBuffer;

            internal HashSet<Node> VisibleNodes = new HashSet<Node>();
            internal Vector3       _lastCameraPosition;
            internal Vector3       _lastCameraForward;
            internal int           _lastCameraFarPlane;
            internal int           _lastCameraFOV;

            internal Vector3 _camPos;
            internal Vector3 _camForward;
            internal int     _camFarPlane;
            internal int     _camFOV;

            public Plane[]   FrustumPlanes  = new Plane[6];
            public Vector3[] FrustumCorners = new Vector3[8];


            public Vector3 _lastWaterPos;


            public void Release()
            {
                for (var i = 0; i < InstanceMeshesArgs.Count; i++)
                {
                    InstanceMeshesArgs[i]?.Release();
                    InstanceMeshesArgs[i] = null;
                }

                InstanceMeshesArgs.Clear();

                if (VisibleChunksComputeBuffer != null) VisibleChunksComputeBuffer.Release();
                VisibleChunksComputeBuffer = null;

                ActiveMeshDetailingInstanceIndex = 0;

                _lastCameraPosition = Vector3.positiveInfinity;
                _lastCameraForward = Vector3.forward;
                _lastWaterPos = Vector3.positiveInfinity;
            }
        }


        public struct GPUChunkInstance
        {
            public Vector3 Position;
            public Vector3 Size;

            public uint DownSeam;
            public uint LeftSeam;
            public uint TopSeam;
            public uint RightSeam;

            public uint DownInf;
            public uint LeftInf;
            public uint TopInf;
            public uint RightInf;
        }

        public struct QuadTreeRenderingContext
        {
            public   ComputeBuffer  visibleChunksComputeBuffer;
            public   GraphicsBuffer visibleChunksArgs;
            public   Mesh           chunkInstance;
            public   Mesh           underwaterMesh;
            internal HashSet<Node>  visibleNodes;
            internal int            activeMeshDetailingInstanceIndex;
        }

        public bool TryGetRenderingContext(Camera cam, bool isSimpleInstance, out QuadTreeRenderingContext context)
        {
#if KWS_DEBUG
            if (WaterSystem.DebugQuadtree) cam = Camera.main;
#endif
            if (cam != null && Instances.TryGetValue(cam, out var instance) && instance.CanRender)
            {
                var level = isSimpleInstance ? Mathf.Max(0, instance.ActiveMeshDetailingInstanceIndex - 4) : instance.ActiveMeshDetailingInstanceIndex;

                context = new QuadTreeRenderingContext()
                {
                    visibleChunksComputeBuffer = instance.VisibleChunksComputeBuffer,
                    visibleChunksArgs = instance.InstanceMeshesArgs[level],
                    chunkInstance = InstanceMeshes[level],
                    underwaterMesh = BottomUnderwaterSkirt,
                    visibleNodes = instance.VisibleNodes,
                    activeMeshDetailingInstanceIndex = instance.ActiveMeshDetailingInstanceIndex
                };
                return true;
            }
            else
            {
                context = default;
                return false;
            }
        }

        public enum QuadTreeTypeEnum
        {
            Finite,
            Infinite
        }

        internal List<Mesh> InstanceMeshes = new List<Mesh>();
        Mesh                BottomUnderwaterSkirt;

        private QuadTreeTypeEnum _quadTreeType;
        List<float> _lodDistances;
        private Vector3 _waterPivotWorldPos;
        private Bounds _quadTreeBounds;
        private float _maxWavesHeight;

        private Vector3 _currentWaterPos;

        private int[] _lastQuadTreeChunkSizesRelativeToWind;
        private Node _root;
        private Quaternion _waterPivotWorldRotation;
        private float _currentDisplacementOffset;
        private float _finiteWaterRotationScale;
        private bool _wideAngleMode;

        Vector2Int[] _downsampledFiniteLevels = new Vector2Int[maxFiniteLevels];

        static readonly int     maxFiniteLevels                  = 8;
        static readonly float   finiteBoundsRotationMaxScale     = 1.35f;
        static readonly Vector3 InfiniteChunkMaxDistance         = new Vector3(1000000, 0, 1000000);
        private const   int     MaxQuadtreeInstancesCount = 10;

        Bounds GetQuadTreeOceanBounds(WaterSystem waterInstance)
        {
            var farDist = waterInstance.Settings.OceanDetailingFarDistance;
            //if (quadTreeCamera != null) farDist = (int)Mathf.Min(farDist, quadTreeCamera.farClipPlane);
            return new Bounds(new Vector3(0, waterInstance.WaterRelativeWorldPosition.y - KWS_Settings.Mesh.MaxInfiniteOceanDepth, 0), new Vector3(farDist, KWS_Settings.Mesh.MaxInfiniteOceanDepth * 2, farDist));
        }

        Bounds GetQuadTreeFiniteBounds(WaterSystem waterInstance)
        {
            var bounds = new Bounds(new Vector3(0, -waterInstance.WaterSize.y / 2f, 0), waterInstance.WaterSize);
            bounds.size = Vector3.Max(bounds.size, KWS_Settings.Mesh.MinFiniteSize);
            return bounds;
        }

        public void Initialize(QuadTreeTypeEnum quadTreeType, WaterSystem waterInstance)
        {
            Release();

            _quadTreeType = quadTreeType;
            _quadTreeBounds = (quadTreeType == QuadTreeTypeEnum.Infinite) ? GetQuadTreeOceanBounds(waterInstance) : GetQuadTreeFiniteBounds(waterInstance);

            _lodDistances = _quadTreeType == QuadTreeTypeEnum.Finite
                ? InitialiseFiniteLodDistances(_quadTreeBounds.size, KWS_Settings.Mesh.QuadtreeFiniteOceanMinDistance)
                : InitialiseInfiniteLodDistances(_quadTreeBounds.size, KWS_Settings.Mesh.QuadtreeInfiniteOceanMinDistance);

            var leveledNodes = new List<LeveledNode>();
            for (int i = 0; i < _lodDistances.Count; i++) leveledNodes.Add(new LeveledNode());

            _waterPivotWorldPos = waterInstance.WaterPivotWorldPosition;

            _maxWavesHeight = waterInstance.CurrentMaxWaveHeight;
            _waterPivotWorldRotation = waterInstance.WaterPivotWorldRotation;
            _wideAngleMode = waterInstance.WideAngleCameraRenderingMode;

            _finiteWaterRotationScale = Mathf.Abs(Mathf.Sin(_waterPivotWorldRotation.eulerAngles.y * Mathf.Deg2Rad * 2f));
            _finiteWaterRotationScale = Mathf.Lerp(1, finiteBoundsRotationMaxScale, _finiteWaterRotationScale);

            _root = new Node(this, leveledNodes, 0, _quadTreeBounds.center, _quadTreeBounds.size);
            InitializeNeighbors(leveledNodes);

            _lastQuadTreeChunkSizesRelativeToWind = quadTreeType == QuadTreeTypeEnum.Infinite
                ? KWS_Settings.Water.QuadTreeChunkQuailityLevelsInfinite[waterInstance.Settings.WaterMeshQualityInfinite]
                : KWS_Settings.Water.QuadTreeChunkQuailityLevelsFinite[waterInstance.Settings.WaterMeshQualityFinite];


            for (int lodIndex = 0; lodIndex < _lastQuadTreeChunkSizesRelativeToWind.Length; lodIndex++)
            {
                var currentResolution = GetChunkLodResolution(quadTreeType, _quadTreeBounds, lodIndex, waterInstance.Settings.UseTesselation);
                var instanceMesh = MeshUtils.GenerateInstanceMesh(currentResolution, _quadTreeType);

                InstanceMeshes.Add(instanceMesh);
            }
            BottomUnderwaterSkirt = MeshUtils.GenerateUnderwaterBottomSkirt(Vector2Int.one);

            this.WaterLog($"Initialized Quadtree {quadTreeType} waterInstance: {waterInstance}", KW_Extensions.WaterLogMessageType.Initialize);
        }



        public void UpdateQuadTree(Camera cam, WaterSystem waterInstance, bool forceUpdate = false)
        {
            if (_root == null) return;
            
            if(Instances.Count > MaxQuadtreeInstancesCount) Instances.Clear();
            if (!Instances.ContainsKey(cam))
            {
                Instances.Add(cam, new QuadTreeInstance());
            }

            var instance = Instances[cam];

            instance._camPos      = cam.GetCameraPositionFast();
            instance._camForward  = cam.GetCameraForwardFast();
            instance._camFarPlane = (int)cam.farClipPlane;
            instance._camFOV      = (int)cam.fieldOfView;
            if (!WaterSharedResources.FrustumCaches.TryGetValue(cam, out var cache))
            {
                Debug.LogError($"FrustumCaches doesn't have camera {cam}");
                return;
            }
            
            instance.FrustumPlanes = cache.FrustumPlanes;
            //instance.FrustumCorners = cache.FrustumCorners;

            //forceUpdate = true;
            if (!forceUpdate && !IsRequireUpdateQuadTree(instance, waterInstance)) return;

            instance.VisibleNodes.Clear();
            instance.VisibleGPUChunks.Clear();

            _currentWaterPos           = waterInstance.WaterPivotWorldPosition;
            _maxWavesHeight            = waterInstance.CurrentMaxWaveHeight;
            _wideAngleMode             = waterInstance.WideAngleCameraRenderingMode;
            _waterPivotWorldPos        = waterInstance.WaterPivotWorldPosition;
            _waterPivotWorldRotation   = waterInstance.WaterPivotWorldRotation;
            _currentDisplacementOffset = KWS_Settings.Mesh.UpdateQuadtreeEveryMetersForward + _maxWavesHeight * KWS_Settings.Mesh.QuadTreeAmplitudeDisplacementMultiplier;


            _root.UpdateVisibleNodes(instance, this);

            var camOffset = instance._camPos;
            camOffset.y = 0;
            var halfOffset = _quadTreeBounds.size.y * 0.5f;

            //var sortedNode    = new HashSet<Node>();
         
            //for (int i = 0; i < _lodDistances.Count; i++)
            //{
            //    foreach (var visibleNode in instance.VisibleNodes)
            //    {
            //        if (visibleNode.CurrentLevel == i) sortedNode.Add(visibleNode);
            //    }
            //}
            

            foreach (var visibleNode in instance.VisibleNodes)
            {
                var meshData = new GPUChunkInstance();
                var center = visibleNode.ChunkCenter;

                meshData.Position = center;
                meshData.Position.y += halfOffset;
                if (_quadTreeType == QuadTreeTypeEnum.Finite) meshData.Position += _currentWaterPos;
                else meshData.Position += camOffset;
                
                meshData.Size = visibleNode.ChunkSize;
                meshData.Size.y = waterInstance.WaterSize.y;

                meshData = InitializeSeamDataRelativeToNeighbors(instance, ref meshData, visibleNode);

                instance.VisibleGPUChunks.Add(meshData);
            }

            instance._lastCameraPosition = instance._camPos;
            instance._lastCameraForward = instance._camForward;
            instance._lastCameraFarPlane = instance._camFarPlane;
            instance._lastCameraFOV = instance._camFOV;

            instance._lastWaterPos = waterInstance.WaterPivotWorldPosition;

            if (instance.InstanceMeshesArgs.Count == 0) instance.InstanceMeshesArgs.AddDefaultValues(InstanceMeshes.Count, null);

            for (int i = 0; i < InstanceMeshes.Count; i++)
            {
                instance.InstanceMeshesArgs[i] = MeshUtils.InitializeInstanceArgsGraphicsBuffer(InstanceMeshes[i], instance.VisibleGPUChunks.Count, instance.InstanceMeshesArgs[i], KWS_CoreUtils.SinglePassStereoEnabled);
            }

            MeshUtils.InitializePropertiesBuffer(instance.VisibleGPUChunks, ref instance.VisibleChunksComputeBuffer, KWS_CoreUtils.SinglePassStereoEnabled);

            UpdateQuadTreeDetailingRelativeToWind(instance, waterInstance);

#if KWS_DEBUG
            this.WaterLog($"Update Quadtree    waterInstance: {waterInstance}   camera: {cam}", KW_Extensions.WaterLogMessageType.DynamicUpdate);
#endif
        }

        bool IsRequireUpdateQuadTree(QuadTreeInstance instance, WaterSystem waterInstance)
        {
            var distanceToCamera = Vector3.Distance(instance._camPos, instance._lastCameraPosition);

            if (KW_Extensions.SqrMagnitudeFast(instance._camForward - instance._lastCameraForward) > KWS_Settings.Mesh.QuadtreeRotationThresholdForUpdate            ||
                distanceToCamera                                                                   >= KWS_Settings.Mesh.UpdateQuadtreeEveryMetersForward             ||
                (IsCameraMoveBackwards(instance, instance._camPos, instance._camForward) && distanceToCamera >= KWS_Settings.Mesh.UpdateQuadtreeEveryMetersBackward) ||
                Mathf.Abs(instance._lastWaterPos.y - waterInstance.WaterPivotWorldPosition.y) > 0.001f                                                                ||
                Math.Abs(_maxWavesHeight          - waterInstance.CurrentMaxWaveHeight)      > 0.001f                                                                ||
                instance._camFarPlane                                                        != instance._lastCameraFarPlane                                         ||
                instance._camFOV                                                             != instance._lastCameraFOV                                              ||
                !instance.CanRender) return true;
            return false;
        }

        bool IsCameraMoveBackwards(QuadTreeInstance instance, Vector3 cameraPos, Vector3 forwardVector)
        {
            var direction = (cameraPos - instance._lastCameraPosition).normalized;
            var angle = Vector3.Dot(direction, forwardVector);
            return angle < -0.1;
        }


        public void UpdateQuadTreeDetailingRelativeToWind(QuadTreeInstance instance, WaterSystem waterInstance)
        {
            var lodOffset = waterInstance.Settings.UseDynamicWaves || waterInstance.Settings.UseShorelineRendering ? KWS_Settings.Water.QuadTreeChunkLodOffsetForDynamicWaves : 0;
            var windScales = KWS_Settings.Water.QuadTreeChunkLodRelativeToWind;
            var maxInstanceIdx = instance.InstanceMeshesArgs.Count - 1;
            for (int i = 0; i < windScales.Length; i++)
            {
                if (waterInstance.Settings.CurrentWindSpeed < windScales[i])
                {
                    instance.ActiveMeshDetailingInstanceIndex = Mathf.Clamp(lodOffset + i, 0, maxInstanceIdx);
                    return;
                }
            }
            instance.ActiveMeshDetailingInstanceIndex = maxInstanceIdx;
        }

        public void Release()
        {
            foreach (var quadTreeInstance in Instances)
            {
                quadTreeInstance.Value?.Release();
            }
            Instances.Clear();

            foreach (var instance in InstanceMeshes) KW_Extensions.SafeDestroy(instance);
            InstanceMeshes.Clear();

            KW_Extensions.SafeDestroy(BottomUnderwaterSkirt);

        }


        //internal Vector3 GetRootOffset()
        //{
        //    Vector3 offset;
        //    if (_quadTreeType == QuadTreeTypeEnum.Infinite)
        //    {
        //        offset   = _camPos;
        //        offset.y = 0;
        //    }
        //    else offset = _waterTransform.InverseTransformVector(_currentWaterPos);
        //    return offset;
        //}


        Vector2Int GetChunkLodResolution(QuadTreeTypeEnum quadTreeType, Bounds bounds, int lodIndex, bool useTesselation)
        {
            Vector2Int chunkRes = Vector2Int.one;
            int lodRes;
            if (useTesselation)
            {
                lodRes = quadTreeType == QuadTreeTypeEnum.Infinite
                    ? KWS_Settings.Mesh.TesselationInfiniteMeshChunksSize
                    : KWS_Settings.Mesh.TesselationFiniteMeshChunksSize;
            }
            else lodRes = _lastQuadTreeChunkSizesRelativeToWind[lodIndex];
            var quarterRes = lodRes * 4;

            if (quadTreeType == QuadTreeTypeEnum.Infinite)
            {
                chunkRes *= quarterRes;
            }
            else
            {
                float x, z;
                var maxQuads = lodRes * lodRes;
                if (bounds.size.x < bounds.size.z)
                {
                    var aspect = Mathf.Clamp(bounds.size.z / bounds.size.x, 1, 4);
                    x = Mathf.Sqrt(maxQuads / aspect);
                    z = aspect * x;
                }
                else
                {
                    var aspect = Mathf.Clamp(bounds.size.x / bounds.size.z, 1, 4);
                    z = Mathf.Sqrt(maxQuads / aspect);
                    x = aspect * z;
                }
                chunkRes.x = ((int)Mathf.Clamp(x, 1, 64)) * 4;
                chunkRes.y = ((int)Mathf.Clamp(z, 1, 64)) * 4;
            }

            return chunkRes;
        }

        void InitializeNeighbors(List<LeveledNode> leveledNodes)
        {
            foreach (var leveledNode in leveledNodes)
            {
                foreach (var pair in leveledNode.Chunks)
                {
                    var chunk = pair.Value;
                    chunk.NeighborLeft = leveledNode.GetLeftNeighbor(chunk.UV);
                    chunk.NeighborRight = leveledNode.GetRightNeighbor(chunk.UV);
                    chunk.NeighborTop = leveledNode.GetTopNeighbor(chunk.UV);
                    chunk.NeighborDown = leveledNode.GetDownNeighbor(chunk.UV);
                }
            }
        }

        List<float> InitialiseFiniteLodDistances(Vector3 size, float minLodDistance)
        {
            var maxSize = Mathf.Max(size.x, size.z);
            var lodDistances = new List<float>();
            var divider = 2f;
            var sizeRatio = size.x < size.z ? size.x / size.z : size.z / size.x;


            var maxLevelsRelativeToSize = Mathf.CeilToInt(Mathf.Log(sizeRatio * maxSize / 4f, 2));
            var maxLevels = Mathf.Min(maxFiniteLevels, maxLevelsRelativeToSize);

            lodDistances.Add(float.MaxValue);
            var lastDistance = maxSize;
            while (lodDistances[lodDistances.Count - 1] > minLodDistance && lodDistances.Count <= maxLevels)
            {
                var currentDistance = maxSize / divider;
                lodDistances.Add(Mathf.Lerp(currentDistance, lastDistance, 0.6f));
                lastDistance = currentDistance;
                divider *= 2;

            }

            return lodDistances;
        }

        List<float> InitialiseInfiniteLodDistances(Vector3 size, float minLodDistance)
        {
            var maxSize = Mathf.Max(size.x, size.z);
            var lodDistances = new List<float>();
            var divider = 2f;

            lodDistances.Add(float.MaxValue);
            while (lodDistances[lodDistances.Count - 1] > minLodDistance)
            {
                lodDistances.Add(maxSize / divider);
                divider *= 2;
            }

            return lodDistances;
        }

        internal class LeveledNode
        {
            public Dictionary<uint, Node> Chunks = new Dictionary<uint, Node>();

            public void AddNodeToArray(Vector2Int uv, Node node)
            {
                node.UV = uv;
                //long hashIdx = uv.x + uv.y * MaxLevelsRange;
                var hashIdx = GetHashFromUV(uv);
                if (!Chunks.ContainsKey(hashIdx)) Chunks.Add(hashIdx, node);
            }

            public Node GetLeftNeighbor(Vector2Int uv)
            {
                //long hashIdx = (uv.x - 1) + uv.y * MaxLevelsRange;
                uv.x -= 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetRightNeighbor(Vector2Int uv)
            {
                //long hashIdx = (uv.x + 1) + uv.y * MaxLevelsRange;
                uv.x += 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetTopNeighbor(Vector2Int uv)
            {
                // long hashIdx = uv.x + (uv.y + 1) * MaxLevelsRange;
                uv.y += 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetDownNeighbor(Vector2Int uv)
            {
                //long hashIdx = uv.x + (uv.y - 1) * MaxLevelsRange;
                uv.y -= 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            uint GetHashFromUV(Vector2Int uv)
            {
                return (((uint)uv.x & 0xFFFF) << 16) | ((uint)uv.y & 0xFFFF);
            }

            //Vector2Int GetUVFromHash(uint p)
            //{
            //    return new Vector2Int((int)((p >> 16) & 0xFFFF), (int)(p & 0xFFFF));
            //}

        }


        GPUChunkInstance InitializeSeamDataRelativeToNeighbors(QuadTreeInstance instance, ref GPUChunkInstance meshData, Node node)
        {
            var topNeighbor = node.NeighborTop;
            if (topNeighbor == null || !instance.VisibleNodes.Contains(topNeighbor) && instance.VisibleNodes.Contains(topNeighbor.Parent))
            {
                meshData.TopSeam = 1;
            }

            var leftNeighbor = node.NeighborLeft;
            if (leftNeighbor == null || !instance.VisibleNodes.Contains(leftNeighbor) && instance.VisibleNodes.Contains(leftNeighbor.Parent))
            {
                meshData.LeftSeam = 1;
            }

            var downNeighbor = node.NeighborDown;
            if (downNeighbor == null || !instance.VisibleNodes.Contains(downNeighbor) && instance.VisibleNodes.Contains(downNeighbor.Parent))
            {
                meshData.DownSeam = 1;
            }

            var rightNeighbor = node.NeighborRight;
            if (rightNeighbor == null || !instance.VisibleNodes.Contains(rightNeighbor) && instance.VisibleNodes.Contains(rightNeighbor.Parent))
            {
                meshData.RightSeam = 1;
            }

            if (node.CurrentLevel <= 2 || _quadTreeType == QuadTreeTypeEnum.Finite)
            {
                if (topNeighbor == null)
                {
                    meshData.TopInf = 1;
                    meshData.TopSeam = 0;
                }

                if (leftNeighbor == null)
                {
                    meshData.LeftInf = 1;
                    meshData.LeftSeam = 0;
                }

                if (downNeighbor == null)
                {
                    meshData.DownInf = 1;
                    meshData.DownSeam = 0;
                }

                if (rightNeighbor == null)
                {
                    meshData.RightInf = 1;
                    meshData.RightSeam = 0;
                }


            }

            return meshData;
        }

        static Vector2Int PositionToUV(Vector3 pos, Vector3 quadSize, int chunksCounts)
        {
            //var uv = new Vector2(pos.x / quadSize.x, pos.z / quadSize.z); //range [-1.0 - 1.0]
            //var x = Mathf.CeilToInt(uv.x * chunksCounts);
            //var y = Mathf.CeilToInt(uv.y * chunksCounts);
            //return new Vector2Int(x, y);

            var uv = new Vector2(pos.x / quadSize.x, pos.z / quadSize.z); //range [-1.0 - 1.0]
            var x = (int)((uv.x * 0.5f + 0.5f) * chunksCounts * 0.999);
            var y = (int)((uv.y * 0.5f + 0.5f) * chunksCounts * 0.999);
            x = Mathf.Clamp(x, 0, chunksCounts - 1);
            y = Mathf.Clamp(y, 0, chunksCounts - 1);
            return new Vector2Int(x, y);
        }

        internal class Node
        {
            public int CurrentLevel;
            public Vector3 ChunkCenter;
            public Vector3 ChunkSize;

            public Vector3 RotatedCenter;

            public Node Parent;
            public Node[] Children;

            public Node NeighborLeft;
            public Node NeighborRight;
            public Node NeighborTop;
            public Node NeighborDown;

            public Vector2Int UV;
            public bool IsCenterNode;

            internal Node(MeshQuadTree root, List<LeveledNode> leveledNodes, int currentLevel, Vector3 quadTreeCenter, Vector3 quadTreeStartSize, Node parent = null)
            {
                Parent = parent ?? this;

                ChunkCenter = (quadTreeCenter);
                ChunkSize = quadTreeStartSize;

                RotatedCenter = root._waterPivotWorldRotation * ChunkCenter;
                if (root._quadTreeType == QuadTreeTypeEnum.Finite) RotatedCenter += root._waterPivotWorldPos;

                CurrentLevel = currentLevel;

                var maxDistanceForLevel = root._lodDistances[CurrentLevel];
                if (root._quadTreeType == QuadTreeTypeEnum.Infinite && (ChunkCenter - root._quadTreeBounds.center).magnitude > maxDistanceForLevel) return;

                if (currentLevel < root._lodDistances.Count - 1)
                {
                    Subdivide(root, leveledNodes);
                }
            }

            void Subdivide(MeshQuadTree root, List<LeveledNode> leveledNodes)
            {
                var nextLevel = CurrentLevel + 1;
                var quarterSize = ChunkSize / 4f;

                var quadTreeHalfSize = new Vector3(ChunkSize.x / 2f, ChunkSize.y, ChunkSize.z / 2f);
                var quadTreeRootHalfSize = new Vector3(root._quadTreeBounds.extents.x, root._quadTreeBounds.size.y, root._quadTreeBounds.extents.z);

                int chunksCounts = (int)Mathf.Pow(2, nextLevel);
                var level = leveledNodes[nextLevel];


                Vector3 center;

                if (root._quadTreeType == QuadTreeTypeEnum.Infinite)
                {
                    Children = new Node[4];
                    AddQuadNodes(root, leveledNodes, quarterSize, nextLevel, quadTreeHalfSize, level, quadTreeRootHalfSize, chunksCounts);
                }
                else
                {
                    var absAspectRatio = Mathf.Max(ChunkSize.x, ChunkSize.z) / Mathf.Min(ChunkSize.x, ChunkSize.z);

                    if (absAspectRatio > 2f)
                    {
                        Children = new Node[2];
                        if (ChunkSize.x < ChunkSize.z)
                        {
                            SetFiniteDownsampleLevels(root, 2, 1);

                            center = new Vector3(ChunkCenter.x, ChunkCenter.y, ChunkCenter.z + quarterSize.z);
                            Children[0] = new Node(root, leveledNodes, nextLevel, center, new Vector3(quadTreeHalfSize.x * 2, quadTreeHalfSize.y, quadTreeHalfSize.z), this);
                            level.AddNodeToArray(PositionToUV(center, quadTreeRootHalfSize, chunksCounts), Children[0]);

                            center = new Vector3(ChunkCenter.x, ChunkCenter.y, ChunkCenter.z - quarterSize.z);
                            Children[1] = new Node(root, leveledNodes, nextLevel, center, new Vector3(quadTreeHalfSize.x * 2, quadTreeHalfSize.y, quadTreeHalfSize.z), this);
                            level.AddNodeToArray(PositionToUV(center, quadTreeRootHalfSize, chunksCounts), Children[1]);
                        }
                        else
                        {
                            SetFiniteDownsampleLevels(root, 1, 2);

                            center = new Vector3(ChunkCenter.x + quarterSize.x, ChunkCenter.y, ChunkCenter.z);
                            Children[0] = new Node(root, leveledNodes, nextLevel, center, new Vector3(quadTreeHalfSize.x, quadTreeHalfSize.y, quadTreeHalfSize.z * 2), this);
                            level.AddNodeToArray(PositionToUV(center, quadTreeRootHalfSize, chunksCounts), Children[0]);

                            center = new Vector3(ChunkCenter.x - quarterSize.x, ChunkCenter.y, ChunkCenter.z);
                            Children[1] = new Node(root, leveledNodes, nextLevel, center, new Vector3(quadTreeHalfSize.x, quadTreeHalfSize.y, quadTreeHalfSize.z * 2), this);
                            level.AddNodeToArray(PositionToUV(center, quadTreeRootHalfSize, chunksCounts), Children[1]);
                        }
                    }
                    else
                    {
                        SetFiniteDownsampleLevels(root, 1, 1);
                        Children = new Node[4];
                        AddQuadNodes(root, leveledNodes, quarterSize, nextLevel, quadTreeHalfSize, level, quadTreeRootHalfSize, chunksCounts);
                    }

                }
            }

            private void AddQuadNodes(MeshQuadTree root, List<LeveledNode> leveledNodes, Vector3 quarterSize, int nextLevel, Vector3 quadTreeHalfSize, LeveledNode level, Vector3 quadTreeRootHalfSize, int chunksCounts)
            {
                if (root._quadTreeType == QuadTreeTypeEnum.Finite)
                {
                    var finiteLevelMul = root._downsampledFiniteLevels[CurrentLevel];
                    quadTreeRootHalfSize = new Vector3(quadTreeRootHalfSize.x * finiteLevelMul.x, quadTreeRootHalfSize.y, quadTreeRootHalfSize.z * finiteLevelMul.y);
                }

                var center = new Vector3(ChunkCenter.x - quarterSize.x, ChunkCenter.y, ChunkCenter.z + quarterSize.z);
                Children[0] = new Node(root, leveledNodes, nextLevel, center, quadTreeHalfSize, this);
                var uv1 = PositionToUV(center, quadTreeRootHalfSize, chunksCounts);
                level.AddNodeToArray(uv1, Children[0]);

                center = new Vector3(ChunkCenter.x + quarterSize.x, ChunkCenter.y, ChunkCenter.z + quarterSize.z); //right up
                Children[1] = new Node(root, leveledNodes, nextLevel, center, quadTreeHalfSize, this);
                var uv2 = PositionToUV(center, quadTreeRootHalfSize, chunksCounts);
                level.AddNodeToArray(uv2, Children[1]);

                center = new Vector3(ChunkCenter.x - quarterSize.x, ChunkCenter.y, ChunkCenter.z - quarterSize.z); //left down
                Children[2] = new Node(root, leveledNodes, nextLevel, center, quadTreeHalfSize, this);
                var uv3 = PositionToUV(center, quadTreeRootHalfSize, chunksCounts);
                level.AddNodeToArray(uv3, Children[2]);

                center = new Vector3(ChunkCenter.x + quarterSize.x, ChunkCenter.y, ChunkCenter.z - quarterSize.z); //right down
                Children[3] = new Node(root, leveledNodes, nextLevel, center, quadTreeHalfSize, this);
                var uv4 = PositionToUV(center, quadTreeRootHalfSize, chunksCounts);
                level.AddNodeToArray(uv4, Children[3]);
            }

            void SetFiniteDownsampleLevels(MeshQuadTree root, int axisX, int axisZ)
            {
                if (CurrentLevel == 0) root._downsampledFiniteLevels[CurrentLevel] = new Vector2Int(axisX, axisZ);
                else
                {
                    var lastDownsample = root._downsampledFiniteLevels[CurrentLevel - 1];
                    root._downsampledFiniteLevels[CurrentLevel] = new Vector2Int(Mathf.Max(lastDownsample.x, lastDownsample.x * axisX), Mathf.Max(lastDownsample.y, lastDownsample.y * axisZ));
                }
            }

            internal enum ChunkVisibilityEnum
            {
                Visible,
                NotVisibile,
                NotVisibleLod,
                PartialVisible
            }


            internal ChunkVisibilityEnum UpdateVisibleNodes(QuadTreeInstance instance, MeshQuadTree root)
            {
                var currentSize = ChunkSize;
                currentSize.y += root._maxWavesHeight;
                var currentCenter = root._quadTreeType == QuadTreeTypeEnum.Infinite ? ChunkCenter : RotatedCenter;
                currentCenter.y = root._waterPivotWorldPos.y - ChunkSize.y * 0.5f;

                RecalculateChunkDataRelativeToFrustum(root, instance._camPos, ref currentCenter, ref currentSize, out var min, out var max);
                if (!root._wideAngleMode && !KW_Extensions.IsBoxVisibleApproximated(ref instance.FrustumPlanes, min, max)) return ChunkVisibilityEnum.NotVisibile;

                var surfacePos = currentCenter;
                surfacePos.y += currentSize.y * 0.5f - root._maxWavesHeight;
                var distanceToCamera = (surfacePos - instance._camPos).magnitude;
                if (WaterSharedResources.ExtrudeVolumes.Count > 0)
                {
                    if(instance._camPos.y < surfacePos.y) distanceToCamera = 1;
                }

                if (distanceToCamera > root._lodDistances[CurrentLevel]) return ChunkVisibilityEnum.NotVisibleLod;

                if (Children == null)
                {
                    if (distanceToCamera < instance._camFarPlane * 2) instance.VisibleNodes.Add(this);
                    return ChunkVisibilityEnum.Visible;
                }

                foreach (var child in Children)
                {
                    if (child != null && child.UpdateVisibleNodes(instance, root) == ChunkVisibilityEnum.NotVisibleLod)
                    {
                        if (distanceToCamera < instance._camFarPlane * 2) instance.VisibleNodes.Add(child);
                    }
                }

                return ChunkVisibilityEnum.PartialVisible;
            }

            private void RecalculateChunkDataRelativeToFrustum(MeshQuadTree root, Vector3 camPos, ref Vector3 currentCenter, ref Vector3 currentSize, out Vector3 min, out Vector3 max)
            {
                if (root._quadTreeType == QuadTreeTypeEnum.Infinite)
                {
                    currentCenter.x += camPos.x;
                    currentCenter.z += camPos.z;

                    if (CurrentLevel <= 2)
                    {
                        var virtualCenter = currentCenter;
                        var offset = InfiniteChunkMaxDistance;
                        var halfOffset = offset * 0.5f;

                        currentSize += offset;
                        if (NeighborLeft == null && NeighborTop == null) virtualCenter += new Vector3(-halfOffset.x, 0, halfOffset.z);
                        if (NeighborRight == null && NeighborTop == null) virtualCenter += new Vector3(halfOffset.x, 0, halfOffset.z);
                        if (NeighborLeft == null && NeighborDown == null) virtualCenter += new Vector3(-halfOffset.x, 0, -halfOffset.z);
                        if (NeighborRight == null && NeighborDown == null) virtualCenter += new Vector3(halfOffset.x, 0, -halfOffset.z);

                        GetMinMax(root, currentSize, virtualCenter, out min, out max);
                    }
                    else
                    {
                        GetMinMax(root, currentSize, currentCenter, out min, out max);
                    }
                }
                else
                {
                    currentSize.x *= root._finiteWaterRotationScale;
                    currentSize.z *= root._finiteWaterRotationScale;
                    if (currentSize.x > currentSize.z) currentSize.z = currentSize.x;
                    else currentSize.x = currentSize.z;

                    GetMinMax(root, currentSize, currentCenter, out min, out max);
                }
            }

            private void GetMinMax(MeshQuadTree root, Vector3 currentSize, Vector3 center, out Vector3 min, out Vector3 max)
            {
                var halfSize = currentSize / 2f;
                halfSize.x += root._currentDisplacementOffset;
                halfSize.z += root._currentDisplacementOffset;

                min = center - halfSize;
                max = center + halfSize;
            }
        }

    }
}