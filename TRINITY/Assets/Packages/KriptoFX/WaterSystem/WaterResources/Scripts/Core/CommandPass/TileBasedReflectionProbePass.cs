using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


namespace KWS
{
    internal class TileBasedReflectionProbePass : WaterPass
    {
        internal override string     PassName => "Water.TileBasedReflectionProbeRendering";

        private ComputeShader             _cs;
        private ComputeBuffer             _probesBuffer;
        private ComputeBuffer             _tileProbesBuffer;

        private List<ReflectionProbeData> _probeDatas    = new List<ReflectionProbeData>();
        private List<ReflectionProbe>     _visibleProbes = new List<ReflectionProbe>();

        private int      _clearReflectionProbeListKernel;
        private int      _buildReflectionProbeListKernel;
        private RTHandle rtHandleTex;

        private       RTHandle _envCubemapTextures;
        private const uint     MaxCubemapsPerScreen      = 6;
        private       string[] _cubemapTextureShaderNames = new[]
        {
            "KWS_EnvCubemapTexture0", 
            "KWS_EnvCubemapTexture1", 
            "KWS_EnvCubemapTexture2", 
            "KWS_EnvCubemapTexture3",
            "KWS_EnvCubemapTexture4",
            "KWS_EnvCubemapTexture5"
        };

        struct ReflectionProbeData
        {
            //dont forget about padding
            private Vector3 MinBounds;
            private float   Intensity;

            private Vector3 MaxBounds;
            private float   BlendDistance;

            private float   Importance;
            private Vector3 _pad;

            public ReflectionProbeData(ReflectionProbe probe)
            {
                MinBounds     = probe.bounds.min + probe.center;
                MaxBounds     = probe.bounds.max + probe.center;
                Intensity     = probe.intensity;
                BlendDistance = probe.blendDistance;
                Importance    = probe.importance;

                _pad = Vector3.zero;
            }
        }

        public override void ExecuteCommandBuffer(WaterPassContext waterContext)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Water.BuildReflectionProbeTileList");
            BuildReflectionProbeTileList(waterContext);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public override void Release()
        {
           KW_Extensions.SafeDestroy(_cs);
           _probesBuffer?.Dispose();
           _probesBuffer = null;

           _tileProbesBuffer?.Dispose();
           _tileProbesBuffer = null;

           rtHandleTex?.Release();

           _envCubemapTextures?.Release();
        }

        private void BakeReflectionProbe()
        {
            //_envCubemapTextures
        }



        void BuildReflectionProbeTileList(WaterPassContext waterContext)
        {
            if (!WaterSharedResources.FrustumCaches.TryGetValue(waterContext.cam, out var frustumCache)) return;
            if (WaterSharedResources.WaterInstances.Count == 0) return;

            var cmd = waterContext.cmd;
            //if (_cs == null) _cs = KWS_CoreUtils.LoadComputeShader();
            if (_cs == null)
            {
                _cs = Resources.Load<ComputeShader>(KWS_ShaderConstants.ShaderNames.TileBasedReflectionProbesComputeShaderName);
                _buildReflectionProbeListKernel = _cs.FindKernel("BuildReflectionProbeList");
            }

           
            var frustumCameraPlanes = frustumCache.FrustumPlanes;
            var probes              = WaterSharedResources.ReflectionProbesCache;

            
            
            _probeDatas.Clear();
            _visibleProbes.Clear();
            for (var idx = 0; idx < probes.Length; idx++)
            {
                var probe = probes[idx];
                if (!KW_Extensions.IsBoxVisibleApproximated(ref frustumCameraPlanes, probe.bounds.min, probe.bounds.max)) continue;

                _visibleProbes.Add(probe);
                // _probeDatas.Add(new ReflectionProbeData(probe));
            }

            _visibleProbes.Sort((x, y) => y.importance.CompareTo(x.importance));
            var probesCount = Mathf.Min(_visibleProbes.Count, MaxCubemapsPerScreen);
            for (var idx = 0; idx < probesCount; idx++)
            {
                var probe = _visibleProbes[idx];
                _probeDatas.Add(new ReflectionProbeData(probe));
                Shader.SetGlobalTexture(_cubemapTextureShaderNames[idx], probe.texture);
            }


            Shader.SetGlobalFloat("KWS_VisibleReflectionProbesCount", _probeDatas.Count);
            if (_probeDatas.Count == 0) return;

            Shader.SetGlobalBuffer("KWS_ReflectionProbeData", _probesBuffer);

            var targetResolution = new Vector2(256, 512);
            Shader.SetGlobalVector("KWS_ReflectionProbeTilesCount",       targetResolution);

            _probesBuffer = KWS_CoreUtils.GetOrUpdateBuffer<ReflectionProbeData>(ref _probesBuffer, _probeDatas.Count);
            _probesBuffer.SetData(_probeDatas);

            _tileProbesBuffer = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref _tileProbesBuffer, (int)(targetResolution.x * targetResolution.y));

            //if (rtHandleTex == null) rtHandleTex = KWS_CoreUtils.RTHandleAllocVR(tileCountX, tileCountY, enableRandomWrite: true);

            cmd.SetComputeTextureParam(_cs, _buildReflectionProbeListKernel, KWS_ShaderConstants.MaskPassID.KWS_WaterDepthRT, WaterSharedResources.WaterDepthRT);
            cmd.SetComputeBufferParam(_cs, _buildReflectionProbeListKernel, "KWS_ReflectionProbeData", _probesBuffer);
            cmd.SetComputeIntParam(_cs, "KWS_ReflectionProbeDataCount", _probeDatas.Count);

            cmd.SetComputeBufferParam(_cs, _clearReflectionProbeListKernel, "KWS_ReflectionProbeTileListRW", _tileProbesBuffer);
            cmd.SetComputeBufferParam(_cs, _buildReflectionProbeListKernel, "KWS_ReflectionProbeTileListRW", _tileProbesBuffer);

            cmd.SetComputeTextureParam(_cs, _buildReflectionProbeListKernel, "_result", rtHandleTex);
            cmd.SetComputeVectorParam(_cs, "KWS_TargetResolution", targetResolution);
            
            //cmd.DispatchCompute(_cs, _clearReflectionProbeListKernel, Mathf.CeilToInt(targetResolution.x / 8f ), Mathf.CeilToInt(targetResolution.y / 8f), 1);
            cmd.DispatchCompute(_cs, _buildReflectionProbeListKernel, Mathf.CeilToInt(targetResolution.x / 8f),  Mathf.CeilToInt(targetResolution.y / 8f), 1);
            
            
            cmd.SetGlobalBuffer("KWS_ReflectionProbeTileList", _tileProbesBuffer);
            cmd.SetGlobalTexture("rtHandleTex", rtHandleTex);
        }

        
    }
}