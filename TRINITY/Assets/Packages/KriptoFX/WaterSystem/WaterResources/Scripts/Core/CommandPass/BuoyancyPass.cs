using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    internal class BuoyancyPass : WaterPass
    {
        internal override string PassName => "Water.BuoyancyPass";


        Material _fftToHeightMaterial;
        ComputeShader                          _computeShader;
        static        Dictionary<int, FftData> _fftDatas              = new Dictionary<int, FftData>();
        private const int                      MaxBuoyancyFramesDelay = 10;

        internal static RenderTexture DebugHeightRT;
        private         CommandBuffer _cmd;

        public class FftData
        {
            public RTHandle      HeightRT;
            public ComputeBuffer Buffer;
            public Mesh          Mesh;
            public AsyncTextureSynchronizer<FftGPUHeightData> AsyncTextureSynchronizer = new AsyncTextureSynchronizer<FftGPUHeightData>();
            //public bool IsHeightDataReady;
            //public NativeArray<FftGPUHeightData> GpuHeightData;


            public FftData(int size)
            {
                Initialize(size);
            }

            public void Initialize(int size)
            {
                HeightRT = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R16_SFloat);
                Buffer   = new ComputeBuffer(size * size, sizeof(float) * 4); //height float + normal float x3
                Mesh     = MeshUtils.CreatePlaneMesh(size, 1.1f);
            }

            public void Release()
            {
                HeightRT?.Release();
                HeightRT = null;

                Buffer?.Release();
                Buffer = null;

                AsyncTextureSynchronizer.ReleaseATSResources();

                KW_Extensions.SafeDestroy(Mesh);
                //IsHeightDataReady = false;

            }
        }



        public BuoyancyPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;

            _computeShader       = KWS_CoreUtils.LoadComputeShader("Common/CommandPass/KWS_BuoyancyPass_HeightReaback");
            _fftToHeightMaterial = KWS_CoreUtils.CreateMaterial("Hidden/KriptoFX/KWS/KWS_BuoyancyPass_FftToHeight");
        }

        void InitializeTextures()
        {

            // this.WaterLog(WaterSharedResources.CausticRTArray);
        }


        void ReleaseTextures()
        {
            foreach (var heightDataTarget in _fftDatas) heightDataTarget.Value?.Release();
            _fftDatas.Clear();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }


        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseTextures();

            KW_Extensions.SafeDestroy(_fftToHeightMaterial, _computeShader);

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            //if (changedTabs.HasTab(WaterSystem.WaterTab.Caustic))
            {

            }
        }

        private bool _requireExecuteCmd;
        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
#if UNITY_EDITOR
            if (KWS_CoreUtils.IsFrameDebuggerEnabled()) return; //async readback doesnt works with frame debugger
#endif


#if KWS_DEBUG
            if (WaterSystem.UseNetworkBuoyancy == false && fixedUpdates.FramesCount_30fps == 0 && WaterSystem.RebuildMaxHeight == false) return;
#else
            if (WaterSystem.UseNetworkBuoyancy == false && fixedUpdates.FramesCount_30fps == 0) return;
#endif

            if (!Application.isPlaying) return;
            if (_computeShader == null) return;

            if (_cmd == null) _cmd = new CommandBuffer() { name = PassName };
            _cmd.Clear();
            _requireExecuteCmd = false;

            if (IsRequireUpdateGlobalBuoyancy() || WaterSystem.UseNetworkBuoyancy)
            {
                ExecuteInstance(_cmd, WaterSharedResources.GlobalWaterSystem);
            }

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (IsRequireUpdateInstanceBuoyancy(waterInstance) || WaterSystem.UseNetworkBuoyancy)
                {
                    ExecuteInstance(_cmd, waterInstance);
                }
            }
           
            if(_requireExecuteCmd) Graphics.ExecuteCommandBuffer(_cmd);
        }

      
        void ExecuteInstance(CommandBuffer cmd, WaterSystem waterInstance)
        {
            var size = GetInstanceSize(waterInstance);

#if KWS_DEBUG
            if (WaterSystem.RebuildMaxHeight) size = Mathf.Clamp(size * 4, 128, 512);
#endif

            var data = GetOrCreateFftData(size);
            if (data.AsyncTextureSynchronizer.IsBusy()) return;
            if (data.Mesh == null || data.HeightRT == null || _fftToHeightMaterial == null || _computeShader == null)
            {
                Debug.LogError($"Water Buoyancy rendering error: {data.Mesh}, {data.HeightRT}, {_fftToHeightMaterial}, {_computeShader}");
                return;
            }
            
            CoreUtils.SetRenderTarget(cmd, data.HeightRT, ClearFlag.Color, Color.black);
            waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.FFT);
            cmd.DrawMesh(data.Mesh, Matrix4x4.identity, _fftToHeightMaterial);
            DebugHeightRT = data.HeightRT.rt;

            cmd.SetComputeFloatParam(_computeShader, "KWS_TexSize", size);
            cmd.SetComputeTextureParam(_computeShader, 0, "RawHeightDataTex", data.HeightRT);
            cmd.SetComputeBufferParam(_computeShader, 0, "RawHeightData", data.Buffer);
            cmd.DispatchCompute(_computeShader, 0, size / 8, size / 8, 1);

            data.AsyncTextureSynchronizer.EnqueueRequest(cmd, data.Buffer);

            //Debug.Log($"buoyancy ExecuteInstance {waterInstance}");
            if (waterInstance.Settings.UseGlobalWind)
            {
                WaterSharedResources.FftGpuHeightAsyncData = data.AsyncTextureSynchronizer;
                WaterSharedResources.FftHeightDataTexSize = size;
                WaterSharedResources.GlobalWindBuoyancyLeftFramesAfterRequest++;
                WaterSharedResources.FftBuoyancyHeight = data.HeightRT;
            }
            else
            {
                waterInstance.InstanceData.FftGpuHeightAsyncData = data.AsyncTextureSynchronizer;
                waterInstance.InstanceData.FftHeightDataTexSize  = size;
                waterInstance.InstanceData.BuoyancyLeftFramesAfterRequest++;
                waterInstance.InstanceData.FftBuoyancyHeight = data.HeightRT;
            }

            _requireExecuteCmd = true;
        }

        static int GetInstanceSize(WaterSystem waterInstance)
        {
            var domainSize           = KWS_Settings.FFT.FftDomainSizes[waterInstance.Settings.CurrentFftWavesCascades - 1];
            var domainScale          = waterInstance.Settings.CurrentWavesAreaScale;
            var sizeRelativeToDomain = Mathf.Clamp(domainSize * domainScale * 0.3f, 32, 256);
            var sizeBase             = Mathf.RoundToInt(Mathf.Log(sizeRelativeToDomain, 2));
            return (int)Mathf.Pow(2, sizeBase);
        }

        FftData GetOrCreateFftData(int size)
        {
            if (!_fftDatas.ContainsKey(size))
            {
                _fftDatas.Add(size, new FftData(size));
            }

            return _fftDatas[size];
        }

        bool IsRequireUpdateInstanceBuoyancy(WaterSystem waterInstance)
        {
            if (WaterSystem.UseNetworkBuoyancy == false && !KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)) return false;
            if (waterInstance.Settings.UseGlobalWind) return false;
            if (waterInstance.InstanceData.BuoyancyLeftFramesAfterRequest > MaxBuoyancyFramesDelay) return false;

            return true;
        }

        bool IsRequireUpdateGlobalBuoyancy()
        {
            if (!WaterSharedResources.IsAnyWaterUseGlobalWind) return false;
            if (WaterSharedResources.GlobalWindBuoyancyLeftFramesAfterRequest > MaxBuoyancyFramesDelay) return false;
            if (WaterSystem.UseNetworkBuoyancy == false && !WaterSharedResources.WaterInstances.Any(KWS_CoreUtils.IsWaterVisibleAndActive)) return false;

            return true;
        }

        private static WaterSurfaceData _surfaceData;

        public static WaterSurfaceData GetWaterSurfaceData(WaterSystem waterInstance, Vector3 worldPosition)
        {
            var asyncData    = waterInstance.Settings.UseGlobalWind ? WaterSharedResources.FftGpuHeightAsyncData : waterInstance.InstanceData.FftGpuHeightAsyncData;
            var texSize = waterInstance.Settings.UseGlobalWind ? WaterSharedResources.FftHeightDataTexSize : waterInstance.InstanceData.FftHeightDataTexSize;

            if (asyncData == null || !asyncData.IsCreated()) return GetDefault(worldPosition, waterInstance.WaterBoundsSurfaceHeight);

            var buffer = asyncData.CurrentBuffer();

            var cascades = waterInstance.Settings.CurrentFftWavesCascades;
            var domainSize = KWS_Settings.FFT.FftDomainSizes[cascades - 1] * waterInstance.Settings.CurrentWavesAreaScale;

            //todo add better support for custom meshes with different pivot
            var surfaceHeight = waterInstance.Settings.WaterMeshType == WaterSystemScriptableData.WaterMeshTypeEnum.CustomMesh 
                ? waterInstance.WaterBoundsSurfaceHeight
                : waterInstance.WaterPivotWorldPosition.y;
         
            var x = (worldPosition.x + domainSize * 0.5f) % domainSize;
            var z = (worldPosition.z + domainSize * 0.5f) % domainSize;

            if (x < 0) x = domainSize + x;
            if (z < 0) z = domainSize + z;

            x = texSize * (x / domainSize);
            z = texSize * (z / domainSize);

            var pixelIdx = (int) x + texSize * (int) z;
            if (pixelIdx > buffer.Length - 1)
            {
                _surfaceData.IsActualDataReady = false;
                _surfaceData.Position          = worldPosition;
                _surfaceData.Normal            = Vector3.up;
                return _surfaceData;
            }

            var rawData = buffer[pixelIdx];
            
            _surfaceData.IsActualDataReady = true;
            _surfaceData.Position          = new Vector3(worldPosition.x, rawData.height + surfaceHeight, worldPosition.z);
            _surfaceData.Normal            = rawData.normal;
            
            return _surfaceData;
        }

        public static void GetWaterSurfaceData(WaterSystem waterInstance, Vector3[] worldPositions, Vector3[] worldNormals, float surfaceHeight)
        {
            var asyncData = waterInstance.Settings.UseGlobalWind ? WaterSharedResources.FftGpuHeightAsyncData : waterInstance.InstanceData.FftGpuHeightAsyncData;
            var texSize   = waterInstance.Settings.UseGlobalWind ? WaterSharedResources.FftHeightDataTexSize : waterInstance.InstanceData.FftHeightDataTexSize;

            if (asyncData == null || !asyncData.IsCreated()) return;

            var buffer     = asyncData.CurrentBuffer();

            var cascades   = waterInstance.Settings.CurrentFftWavesCascades;
            var domainSize = KWS_Settings.FFT.FftDomainSizes[cascades - 1] * waterInstance.Settings.CurrentWavesAreaScale;
          
            for (int i = 0; i < worldPositions.Length; i++)
            {
                var x = (worldPositions[i].x + domainSize * 0.5f) % domainSize;
                var z = (worldPositions[i].z + domainSize * 0.5f) % domainSize;

                if (x < 0) x = domainSize + x;
                if (z < 0) z = domainSize + z;

                x = texSize * (x / domainSize);
                z = texSize * (z / domainSize);

                var pixelIdx = (int)x + texSize * (int)z;
                if (pixelIdx <= buffer.Length - 1)
                {
                    var rawData = buffer[pixelIdx];

                    worldPositions[i] = new Vector3(worldPositions[i].x, rawData.height + surfaceHeight, worldPositions[i].z);
                    worldNormals[i]   = rawData.normal;
                }
            }
        }

        public static WaterSurfaceData GetDefault(Vector3 worldPosition, float waterLevel)
        {
            _surfaceData.IsActualDataReady = false;
            _surfaceData.Position          = worldPosition;
            _surfaceData.Position.y        = waterLevel;
            _surfaceData.Normal            = Vector3.up;
            return _surfaceData;
        }

    }

    public struct WaterSurfaceData
    {
        public bool IsActualDataReady;
        public Vector3 Position;
        public Vector3 Normal;
    }

    public struct WaterSurfaceDataArray
    {
        public bool          IsActualDataReady;
        public Vector3[] Positions;
        public Vector3[] Normals;
    }

    public struct FftGPUHeightData
    {
        public float   height;
        public Vector3 normal;
    }

}