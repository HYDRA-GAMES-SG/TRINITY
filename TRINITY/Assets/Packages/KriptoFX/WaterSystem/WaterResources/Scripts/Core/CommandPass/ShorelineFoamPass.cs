//#define BAKE_AO


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    internal class ShorelineFoamPass : WaterPass
    {
        internal override string PassName => "Water.ShorelineFoamPass";

        RTHandle               _targetRT;
        private  ComputeBuffer _computeBufferFoam;
        private  ComputeBuffer _computeBufferFoamLight;
        private  ComputeBuffer _computeBufferLodIndexes;
        private  ComputeBuffer _computeBufferWaves;
        internal ComputeShader foamComputeShader;

        internal ComputeBuffer FoamDataBuffer;
        internal ComputeBuffer FoamDataCountOffsetBuffer;

        RenderParams           _renderParams;
        private Material       _finalFoamPassMaterial;

        private int _foamRT_ID = Shader.PropertyToID("_FoamRT");
        public readonly int FoamBufferID = Shader.PropertyToID("_FoamBuffer");
        public readonly int FoamDepthBufferID = Shader.PropertyToID("_FoamDepthBuffer");
        public readonly int FoamLightBufferID = Shader.PropertyToID("_FoamLightBuffer");


        private int _renderToBufferDispatchSize;
        private Vector2Int _renderToTextureDispatchSize;
        Vector4 _targetRTSize;

        List<ShorelineWavesScriptableData.ShorelineWave> _visibleFoamWaves = new List<ShorelineWavesScriptableData.ShorelineWave>();
        Dictionary<int, ShorelineWavesScriptableData.ShorelineWave>[] _visibleFoamWavesWithLods = new Dictionary<int, ShorelineWavesScriptableData.ShorelineWave>[KWS_Settings.Shoreline.LodDistances.Length];
        private List<int> _visibleFoamWavesLodIndexes = new List<int>();

        private const int maxParticles = 385871;
        private const int kernelSize = 8;
        int _clearKernel;
        int _drawToBufferKernel;
        int _drawToTextureKernel;

        private readonly Vector2Int _foamRTSize = new Vector2Int(1920, 1080);
        private readonly Vector2Int _foamRTSizeVR = new Vector2Int(1440, 1440);

#if BAKE_AO
        private ComputeBuffer _foamAOBuffer;
        public readonly int _AOBufferID = Shader.PropertyToID("_AOBuffer");
        private float[] _aoData;

        private int _AOframeNumber = 0;
        private uint[] _currentData;
#endif

        private bool _lastFastPath;

     
        public ShorelineFoamPass()
        {
            for (var i = 0; i < KWS_Settings.Shoreline.LodDistances.Length; i++)
            {
                _visibleFoamWavesWithLods[i] = new Dictionary<int, ShorelineWavesScriptableData.ShorelineWave>();
            }

            _finalFoamPassMaterial    = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ShorelineFoamDrawToScreenName);
            _renderParams             = new RenderParams(_finalFoamPassMaterial);
            _renderParams.worldBounds = new Bounds(Vector3.zero, 10000000 * Vector3.one);
        }

        void InitializeComputeShader()
        {
            if (foamComputeShader != null) return;

            foamComputeShader = KWS_CoreUtils.LoadComputeShader($"PlatformSpecific/KWS_ShorelineFoamParticlesCompute");
            if (foamComputeShader != null)
            {
                _clearKernel = foamComputeShader.FindKernel("ClearFoamBuffer");
                _drawToBufferKernel = foamComputeShader.FindKernel("RenderFoamToBuffer");
                _drawToTextureKernel = foamComputeShader.FindKernel("RenderFoamBufferToTexture");
            }

            if (FoamDataBuffer == null)
            {
                var rawBuffer = Resources.Load<ComputeBufferObject>("ComputeBuffers/ComputeFoamData0");
                FoamDataBuffer = rawBuffer.GetComputeBuffer();
                Resources.UnloadUnusedAssets();
            }

            if (FoamDataCountOffsetBuffer == null)
            {
                var rawBufferOffset = Resources.Load<ComputeBufferObject>("ComputeBuffers/ComputeFoamDataOffset0");
                FoamDataCountOffsetBuffer = rawBufferOffset.GetComputeBuffer();
                Resources.UnloadUnusedAssets();
            }

        }

        public override void Release()
        {
            _targetRT?.Release();
            _targetRT = null;

            _computeBufferFoam?.Dispose();
            _computeBufferFoamLight?.Dispose();
            _computeBufferLodIndexes?.Dispose();
            _computeBufferWaves?.Dispose();


            FoamDataBuffer?.Dispose();
            FoamDataCountOffsetBuffer?.Dispose();
#if BAKE_AO
            _foamAOBuffer?.Dispose();
            _foamAOBuffer = null;
#endif
            _computeBufferFoam = _computeBufferFoamLight = _computeBufferLodIndexes = _computeBufferWaves = FoamDataBuffer = FoamDataCountOffsetBuffer = null;

            KW_Extensions.SafeDestroy(_finalFoamPassMaterial);

            _lastFastPath  = false;

            KW_Extensions.WaterLog(this, "", KW_Extensions.WaterLogMessageType.Release);
        }


        void InitializeRT()
        {
            var format = WaterSharedResources.GlobalSettings.UseShorelineFoamFastMode ? GraphicsFormat.R32_UInt : GraphicsFormat.R8G8B8A8_UNorm;
            var size = KWS_CoreUtils.SinglePassStereoEnabled ? _foamRTSizeVR : _foamRTSize;

            _targetRT = KWS_CoreUtils.RTHandleAllocVR(size.x, size.y,  colorFormat: format, name : "_foamRT", enableRandomWrite : true);
            _targetRTSize = new Vector4(size.x, size.y, 1.0f / size.x, 1.0f / size.y);

            _renderToBufferDispatchSize  = Mathf.CeilToInt(Mathf.Sqrt(1f * maxParticles      / (kernelSize * kernelSize)));
            _renderToTextureDispatchSize = new Vector2Int(Mathf.CeilToInt(_targetRT.rt.width / 256f), Mathf.CeilToInt(_targetRT.rt.width / 1f));

            KW_Extensions.WaterLog("ShorelineFoamPass", _targetRT);
        }

        void InitializeComputeData()
        {
            if (_targetRT == null || WaterSharedResources.GlobalSettings.UseShorelineFoamFastMode != _lastFastPath)
            {
                _targetRT?.Release();
                _lastFastPath = WaterSharedResources.GlobalSettings.UseShorelineFoamFastMode;
                InitializeRT();
            }

            var stereoPasses = KWS_CoreUtils.SinglePassStereoEnabled ? 2 : 1;
            _computeBufferFoam = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref _computeBufferFoam, _targetRT.rt.width * _targetRT.rt.height * stereoPasses);
            _computeBufferFoamLight = KWS_CoreUtils.GetOrUpdateBuffer<uint>(ref _computeBufferFoamLight, _targetRT.rt.width * _targetRT.rt.height * stereoPasses);



#if BAKE_AO
            _currentData = data;
            _foamAOBuffer = new ComputeBuffer(packedData.Length, Marshal.SizeOf(typeof(float)));
            _aoData = new float[packedData.Length];
            _foamAOBuffer.SetData(_aoData);
#endif
        }


#if BAKE_AO
        uint BakeAOToData(uint data, float ao)
        {
            uint encodedAO = (uint)(Mathf.Clamp01(ao) * 255.0f); //8 bits = 2^8 - 1
            encodedAO = encodedAO & 0xFF;

            return ((data >> 12) & 0xFFFFF) << 12 | (encodedAO << 4) | data & 0xF;
        }
#endif

        int GetLodIndexByDistance(float distance, WaterSystemScriptableData.ShorelineFoamQualityEnum quality)
        {
            var lodDistances = KWS_Settings.Shoreline.LodDistances;
            var offset       = KWS_Settings.Shoreline.LodOffset[quality];
            for (int i = 0; i < lodDistances.Length; i++)
            {
                if (distance < lodDistances[i] + offset) return i;
            }
            return lodDistances.Length - 1;
        }

        internal void UpdateShorelineBuffer(Camera cam)
        {
            _visibleFoamWaves.Clear();
            _visibleFoamWavesLodIndexes.Clear();
            foreach (var shorelineWave in _visibleFoamWavesWithLods) shorelineWave.Clear();

            var cameraPos    = cam.GetCameraPositionFast();
            var addedWaves   = 0;
            if (!WaterSharedResources.FrustumCaches.TryGetValue(cam, out var frustumCache)) return;

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (!KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)
                 || !waterInstance.Settings.UseShorelineRendering
                 || waterInstance.Settings.ShorelineWavesScriptableData == null) continue;

                var currentWaves = waterInstance.Settings.ShorelineWavesScriptableData.Waves;
                for (var waveIdx = 0; waveIdx < currentWaves.Count; waveIdx++)
                {
                    var wave = currentWaves[waveIdx];
                    if (!wave.IsWaveVisible(ref frustumCache.FrustumPlanes, ref frustumCache.FrustumCorners)) continue;

                    var distanceToWave = wave.GetDistanceToCamera(cameraPos);
                    var lodIdx         = GetLodIndexByDistance(distanceToWave, waterInstance.Settings.ShorelineFoamLodQuality);
                    _visibleFoamWavesWithLods[lodIdx].Add(addedWaves, wave);
                    _visibleFoamWaves.Add(wave);
                    addedWaves++;
                }
            }

            foreach (var lod in _visibleFoamWavesWithLods)
            {
                foreach (var wave in lod)
                {
                    _visibleFoamWavesLodIndexes.Add(wave.Key);
                }
            }

            MeshUtils.InitializePropertiesBuffer(_visibleFoamWaves, ref _computeBufferWaves, false);
        }

        public override void ExecuteBeforeCameraRendering(Camera cam)
        {
            UpdateShorelineBuffer(cam);
            if (_computeBufferWaves == null || _visibleFoamWaves.Count == 0) return;

            var foamRT = WaterSharedResources.ShorelineFoamParticlesRT;
            if (foamRT == null || foamRT.rt == null) return;


            var rt = foamRT.rt;
            _finalFoamPassMaterial.SetVector(Shader.PropertyToID("_FoamRTSize"),        new Vector4(rt.width, rt.height, 1.0f / rt.width, 1.0f / rt.height));
            _finalFoamPassMaterial.SetVector(Shader.PropertyToID("KWS_ShorelineColor"), WaterSharedResources.WaterInstances[0].Settings.ShorelineColor);
            _finalFoamPassMaterial.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.KWS_FOAM_USE_FAST_PATH, WaterSharedResources.WaterInstances[0].Settings.UseShorelineFoamFastMode);
            _finalFoamPassMaterial.SetTexture(KWS_CoreUtils._sourceRT_id, foamRT);

            _finalFoamPassMaterial.renderQueue = KWS_Settings.Water.DefaultWaterQueue + WaterSharedResources.GlobalSettings.TransparentSortingPriority + KWS_Settings.Water.ShorelineQueueOffset;

            _renderParams.camera = cam;
            Graphics.RenderPrimitives(in _renderParams, MeshTopology.Triangles, 3, 1);
        }


        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            var cmd = waterContext.cmd;
            var cam = waterContext.cam;

          
            //var buffer = waterInstance.InstanceData.ShorelineWaveBuffers;
            if (_computeBufferWaves == null || _visibleFoamWaves.Count == 0) return;
            

            InitializeComputeShader();
            if (foamComputeShader == null) return;

            InitializeComputeData();
            foamComputeShader.SetBuffer( _drawToBufferKernel, KWS_ShaderConstants.StructuredBuffers.KWS_ShorelineDataBuffer, _computeBufferWaves);

            Shader.SetGlobalFloat(KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaWavesCount, _visibleFoamWaves.Count);
#if BAKE_AO
            cmd.SetComputeBufferParam(_foamComputeShader, clearKernel, _AOBufferID, _foamAOBuffer);
            cmd.SetComputeBufferParam(_foamComputeShader, drawToBufferKernel, _AOBufferID, _foamAOBuffer);

            if (WaterSystem.Test4.x >= 1)
            {
                if (_AOframeNumber == 0)
                {
                    _aoData = new float[_currentData.Length / 2];
                    _foamAOBuffer.SetData(_aoData);
                }

                cmd.SetComputeFloatParam(_foamComputeShader, "_customTime", _AOframeNumber);
                _AOframeNumber++;
                WaterSystem.Test4.x = _AOframeNumber;
                if (_AOframeNumber > 70)
                {
                    WaterSystem.Test4.x = 0;

                    _foamAOBuffer.GetData(_aoData);
                    for (int i = 0; i < _currentData.Length - 1; i += 2)
                    {
                        _currentData[i + 1] = BakeAOToData(_currentData[i + 1], _aoData[i / 2]);
                    }

                    var pathToBakedDataFolder = KW_Extensions.GetPathToStreamingAssetsFolder();
                    KW_Extensions.SaveDataToFile(_currentData, Path.Combine(pathToBakedDataFolder, _pathToShoreLineFolder, _nameFoamData));
                    Debug.Log("saved");
                    _AOframeNumber = 0;
                }
            }
#endif
            var settings = WaterSharedResources.GlobalSettings;
            var stereoPasses = KWS_CoreUtils.SinglePassStereoEnabled ? 2 : 1;
            if (_targetRT.rt.volumeDepth != stereoPasses) return;


            CoreUtils.SetRenderTarget(waterContext.cmd, _targetRT, ClearFlag.Color, Color.clear);


            ///////////////////////////////////// clear pass ////////////////////////////////////////////////////////////////////////////////////////////////////
            cmd.SetComputeFloatParam(foamComputeShader, "KW_GlobalTimeScale", settings.GlobalTimeScale);

            cmd.SetComputeBufferParam(foamComputeShader, _clearKernel, FoamBufferID, _computeBufferFoam);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToBufferKernel, FoamBufferID, _computeBufferFoam);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToTextureKernel, FoamBufferID, _computeBufferFoam);

            cmd.SetComputeBufferParam(foamComputeShader, _clearKernel, FoamLightBufferID, _computeBufferFoamLight);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToBufferKernel, FoamLightBufferID, _computeBufferFoamLight);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToTextureKernel, FoamLightBufferID, _computeBufferFoamLight);

            cmd.SetComputeTextureParam(foamComputeShader, _clearKernel, _foamRT_ID, _targetRT);
            cmd.DispatchCompute(foamComputeShader, _clearKernel, _renderToTextureDispatchSize.x, _renderToTextureDispatchSize.y, stereoPasses);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            /////////////////////////////////////////////////  RenderToBuffer //////////////////////////////////////////////////////////////////////////////////////////////////
            //waterInstance.InstanceData.UpdateDynamicShaderParams(cmd, WaterInstanceResources.DynamicPassEnum.VolumetricLighting, foamComputeShader, drawToBufferKernel);
            //WaterSharedResources.WaterInstances[0].InstanceData.UpdateConstantShaderParams(foamComputeShader, 1); //todo update fftdata only
            //WaterSharedResources.WaterInstances[0].InstanceData.UpdateConstantShaderParams(foamComputeShader, 2);
            
            cmd.SetComputeFloatParam(foamComputeShader, KWS_ShaderConstants.ConstantWaterParams.KWS_WavesCascades,  WaterSharedResources.GlobalSettings.GlobalFftWavesCascades);
            cmd.SetComputeFloatParam(foamComputeShader, KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, WaterSharedResources.GlobalSettings.GlobalWavesAreaScale);
            cmd.SetComputeTextureParam(foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.FftWavesDisplacement.GetSafeArrayTexture());
            

            cmd.SetComputeVectorParam(foamComputeShader, Shader.PropertyToID("_FoamRTSize"), _targetRTSize);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToBufferKernel, "_foamDataBuffer", FoamDataBuffer);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToBufferKernel, "_foamDataCountOffsetBuffer", FoamDataCountOffsetBuffer);

            cmd.SetComputeFloatParam(foamComputeShader, Shader.PropertyToID("_DispatchSize"), _renderToBufferDispatchSize);
            cmd.SetComputeFloatParam(foamComputeShader, Shader.PropertyToID("_MaxParticles"), maxParticles);

            //cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, "_CameraDepthTexture", depthRT); //todo urp rendering doesn't work properly with editor camera?
            cmd.SetComputeTextureParam(foamComputeShader, _drawToBufferKernel, _foamRT_ID, _targetRT);

            foamComputeShader.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.KWS_FOAM_USE_FAST_PATH, settings.UseShorelineFoamFastMode);
            foamComputeShader.SetKeyword(KWS_ShaderConstants.ShorelineKeywords.FOAM_RECEIVE_SHADOWS, settings.ShorelineFoamReceiveDirShadows);

            //if (WaterSharedResources.IsAnyWaterUseVolumetricLighting)
            //{
            //    foamComputeShader.SetKeyword(KWS_ShaderConstants.WaterKeywords.USE_VOLUMETRIC_LIGHT, WaterSharedResources.IsAnyWaterUseVolumetricLighting);
            //    cmd.SetComputeTextureParam(foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT, WaterSharedResources.VolumetricLightingRT);
            //}

            var lods = _visibleFoamWavesWithLods;
            var allIndexes = _visibleFoamWavesLodIndexes;
            _computeBufferLodIndexes = KWS_CoreUtils.GetOrUpdateBuffer<int>(ref _computeBufferLodIndexes, allIndexes.Count);
            _computeBufferLodIndexes.SetData(allIndexes);
            cmd.SetComputeBufferParam(foamComputeShader, _drawToBufferKernel, "_lodIndexes", _computeBufferLodIndexes);
           
            cmd.SetComputeTextureParam(foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.OrthoDepth.KWS_WaterOrthoDepthRT, WaterSharedResources.OrthoDepth);
            cmd.SetComputeVectorParam(foamComputeShader, KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthPos, WaterSharedResources.OrthoDepthPosition);
            cmd.SetComputeVectorParam(foamComputeShader, KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthNearFarSize, WaterSharedResources.OrthoDepthNearFarSize);
            
            //WaterInstance.SetDynamicShaderParams(_foamComputeShader);
            //WaterInstance.SetConstantShaderParams(_foamComputeShader, WaterSystem.WaterTab.Shoreline);
            //cmd.SetComputeConstantBufferParam(_foamComputeShader, nameof(KWS_ShaderVariables), WaterInstance.SharedData.WaterConstantBuffer, 0, WaterInstance.SharedData.WaterConstantBuffer.stride);

            //cmd.SetComputeTextureParam(_foamComputeShader, _drawToBufferKernel, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLightRT, WaterInstance.SharedData.VolumetricLightingRT);
            //cmd.SetComputeVectorParam(_foamComputeShader, KWS_ShaderConstants.VolumetricLightConstantsID.KWS_VolumetricLight_RTHandleScale, WaterInstance.SharedData.VolumetricLightingRTSize);

            cmd.SetComputeShadersDefaultPlatformSpecificValues(foamComputeShader, 1);
            
            int lodOffset = 0;
            float lodSizeMultiplier = 1;
            var particlesMultiplier = KWS_Settings.Shoreline.LodParticlesMultiplier[settings.ShorelineFoamLodQuality];
            
            foreach (var lod in lods)
            {
                var lodDispatchSize = (int)(_renderToBufferDispatchSize / lodSizeMultiplier);
                if (lod.Count != 0 && lodDispatchSize > 1)
                {
                    cmd.SetComputeFloatParam(foamComputeShader, "_lodSizeMultiplier", lodSizeMultiplier);
                    cmd.SetComputeIntParam(foamComputeShader, "_lodOffset", lodOffset);
                    cmd.DispatchCompute(foamComputeShader, _drawToBufferKernel, lodDispatchSize, lodDispatchSize, lod.Count);
                }
                lodOffset += lod.Count;
                lodSizeMultiplier *= particlesMultiplier;
            }
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            ///////////////////////////////////////////////// DrawFromBufferToTexture ///////////////////////////////////////////////////
            if (!settings.UseShorelineFoamFastMode)
            {
                cmd.SetComputeTextureParam(foamComputeShader, _drawToTextureKernel, _foamRT_ID, _targetRT);
                cmd.DispatchCompute(foamComputeShader, _drawToTextureKernel, _renderToTextureDispatchSize.x, _renderToTextureDispatchSize.y, stereoPasses);
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            WaterSharedResources.ShorelineFoamParticlesRT = _targetRT;
        }

     
    }
}