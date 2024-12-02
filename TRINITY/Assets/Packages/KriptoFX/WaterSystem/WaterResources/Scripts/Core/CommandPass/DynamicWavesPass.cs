using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace KWS
{
    internal class DynamicWavesPass : WaterPass
    {
        internal override string PassName => "Water.DynamicWavesPass";

        List<KWS_InteractWithWater>                       _interactScriptsInArea = new List<KWS_InteractWithWater>();
        List<KWS_InteractWithWater.DynamicWaveDataStruct> _visibleDynamicWaves   = new List<KWS_InteractWithWater.DynamicWaveDataStruct>();
        private Material                                  _dynamicWavesMaterial;
        private ComputeBuffer                             _computeBufferDynamicWavesMask;
        private long                                      _frameNumber;
        private Vector3                                   _interactPos;
        private Vector3                                   _lastInteractPos;
        WavesData[]                                       _wavesDatas = new WavesData[3]{ new WavesData(), new WavesData(), new WavesData()};

        private           CommandBuffer _cmd;

        public DynamicWavesPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
            _dynamicWavesMaterial                          =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.DynamicWavesShaderName);
        }

        void InitializeTextures()
        {
            var settings = WaterSharedResources.GlobalSettings;
            var texSize  = settings.DynamicWavesResolutionPerMeter * settings.DynamicWavesAreaSize;

            WaterSharedResources.DynamicWavesMaskRT = KWS_CoreUtils.RTHandles.Alloc(texSize, texSize, name: "_dynamicWavesMaskRT", colorFormat: GraphicsFormat.R8_SNorm);
            WaterSharedResources.DynamicWavesRT     = KWS_CoreUtils.RTHandles.Alloc(texSize, texSize, name: "_dynamicWavesRT",     colorFormat: GraphicsFormat.R16_SFloat);

            Shader.SetGlobalTexture("DynamicWavesMaskRT", WaterSharedResources.DynamicWavesMaskRT);
            Shader.SetGlobalTexture("DynamicWavesRT", WaterSharedResources.DynamicWavesRT);

            foreach (var data in _wavesDatas) data.Initialize();

            this.WaterLog(WaterSharedResources.DynamicWavesMaskRT, WaterSharedResources.DynamicWavesRT);
        }

        void ReleaseTextures()
        {
            WaterSharedResources.DynamicWavesMaskRT?.Release();
            WaterSharedResources.DynamicWavesRT?.Release();
            WaterSharedResources.DynamicWavesMaskRT = WaterSharedResources.DynamicWavesRT = null;

            foreach (var data in _wavesDatas) data.Release();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
        }


        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            ReleaseTextures();
            _computeBufferDynamicWavesMask?.Release();
            _computeBufferDynamicWavesMask = null;
            KW_Extensions.SafeDestroy(_dynamicWavesMaterial);
            foreach (var wavesData in _wavesDatas) wavesData.Release();

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem instance, WaterSystem.WaterTab changedTabs)
        {
            if (changedTabs.HasTab(WaterSystem.WaterTab.DynamicWaves))
            {
                if (WaterSharedResources.IsAnyWaterUseDynamicWaves)
                {
                    var newTexSize = WaterSharedResources.GlobalSettings.DynamicWavesResolutionPerMeter * WaterSharedResources.GlobalSettings.DynamicWavesAreaSize;
                    if (WaterSharedResources.DynamicWavesRT == null || newTexSize != WaterSharedResources.DynamicWavesRT.rt.width)
                    {
                        ReleaseTextures();
                        InitializeTextures();
                    }

                }
            }
        }

        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            if (WaterSharedResources.IsAnyWaterUseDynamicWaves == false) return;
            
            if (fixedUpdates.FramesCount_60fps == 0) return;
       
            var cam = KWS_CoreUtils.GetFixedUpdateCamera(cameras);
            if (cam == null) return;
         
            if (_cmd         == null) _cmd = new CommandBuffer() { name = PassName };
            _cmd.Clear();

            if (WaterSharedResources.DynamicWavesRT == null) InitializeTextures();
            UpdateInteractPos(cam, _cmd);
            DrawIntersectionMask(cam, _cmd);
            
            ExecuteDynamicWaves(_cmd, fixedUpdates.FramesCount_60fps);

            Graphics.ExecuteCommandBuffer(_cmd);
        }

        private void UpdateInteractPos(Camera cam, CommandBuffer cmd)
        {
            var settings = WaterSharedResources.GlobalSettings;
            var areaSize = settings.DynamicWavesAreaSize;
            var texSize  = settings.DynamicWavesResolutionPerMeter * areaSize;
            _interactPos =  KW_Extensions.GetRelativeToCameraAreaPos(cam, areaSize, 0.5f);
            //_interactPos =  cam.GetCameraPositionFast();
            _interactPos += ComputeAreaSimulationJitter(3f / texSize);
            cmd.SetGlobalVector("DynamicWavesMaskPos", new Vector4(_interactPos.x, _interactPos.y, _interactPos.z, areaSize));
        }


        void DrawIntersectionMask(Camera cam, CommandBuffer cmd)
        {
            var settings     = WaterSharedResources.GlobalSettings;
            var areaSize = settings.DynamicWavesAreaSize;
            var interactScripts = GetInteractScriptsInArea(_interactPos, areaSize);

            KWS_CoreUtils.SetOrthoMatrix_VP(cmd, areaSize, _interactPos);
            _visibleDynamicWaves.Clear();
            CoreUtils.SetRenderTarget(cmd, WaterSharedResources.DynamicWavesMaskRT, ClearFlag.Color, Color.black);

            foreach (var instance in interactScripts)
            {
                instance.CustomUpdate();
            }

            foreach (var instance in interactScripts)
            {
                if (instance.MeshType == KWS_InteractWithWater.InteractMeshTypeEnum.Mesh)
                {
                    if (!instance.IsWaterIntersected || instance.Mesh == null) continue;

                    var matrix = Matrix4x4.TRS(instance.t.position, instance.t.rotation, instance.t.localScale);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KWS_DynamicWavesWaterSurfaceHeight, instance.DynamicWaveData.WaterHeight);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KWS_DynamicWavesForce, instance.DynamicWaveData.Force);
                    cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KWS_MeshIntersectionThreshold, instance.MeshIntersectionThreshold);
                    cmd.DrawMesh(instance.Mesh, matrix, _dynamicWavesMaterial, 0, shaderPass: 0);
                } 
                else 
                {
                    if (instance.IsWaterIntersected) _visibleDynamicWaves.Add(instance.DynamicWaveData);
                }
            }

            //RestoreMatrixes(cmd);

            if (_visibleDynamicWaves.Count > 0)
            {
                MeshUtils.InitializePropertiesBuffer(_visibleDynamicWaves, ref _computeBufferDynamicWavesMask, false);
                Shader.SetGlobalBuffer(KWS_ShaderConstants.StructuredBuffers.KWS_DynamicWavesMaskBuffer, _computeBufferDynamicWavesMask);
                cmd.DrawProcedural(Matrix4x4.identity, _dynamicWavesMaterial, shaderPass: 1, MeshTopology.Triangles, 6, _visibleDynamicWaves.Count);
            }
        }

        void ExecuteDynamicWaves(CommandBuffer cmd, int count)
        {
            var target = _wavesDatas[0];
            var settings = WaterSharedResources.GlobalSettings;
            var areaSize = settings.DynamicWavesAreaSize;
         
            cmd.SetKeyword("KW_USE_RAIN_EFFECT", settings.UseDynamicWavesRainEffect);
            if (settings.UseDynamicWavesRainEffect) cmd.SetGlobalFloat("KWS_DynamicWavesRainStrength", settings.DynamicWavesRainStrength);
            cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KW_InteractiveWavesPixelSpeed, settings.DynamicWavesPropagationSpeed);
            cmd.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KWS_DynamicWavesGlobalForceScale, settings.DynamicWavesGlobalForceScale);

            for (int i = 0; i < count; i++)
            {
                GetPingPongTargets(out var lastSource, out var source, out target);
              
                var offset = (_interactPos - _lastInteractPos) / (areaSize);
                _lastInteractPos   = _interactPos;
                target.WorldOffset = offset;

                cmd.SetGlobalVector(KWS_ShaderConstants.DynamicWaves.KW_AreaOffset, offset);
                cmd.SetGlobalVector(KWS_ShaderConstants.DynamicWaves.KW_LastAreaOffset, source.WorldOffset + offset);


                cmd.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KWS_PreviousTarget, lastSource.DataRT);
                cmd.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KWS_CurrentTarget, source.DataRT);

                CoreUtils.SetRenderTarget(cmd, target.MRT, target.DataRT.rt.depthBuffer);
                cmd.BlitTriangle(_dynamicWavesMaterial, 2);

              
            }

            Shader.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KWS_DynamicWaves,       target.DataRT);
            Shader.SetGlobalTexture(KWS_ShaderConstants.DynamicWaves.KWS_DynamicWavesNormal, target.NormalRT);
            Shader.SetGlobalFloat(KWS_ShaderConstants.DynamicWaves.KW_DynamicWavesAreaSize, WaterSharedResources.GlobalSettings.DynamicWavesAreaSize);
          
          
        }

        private void GetPingPongTargets(out WavesData lastSource, out WavesData source, out WavesData target)
        {
            switch (_frameNumber)
            {
                case 0:
                    lastSource = _wavesDatas[0];
                    source     = _wavesDatas[1];
                    target     = _wavesDatas[2];
                    break;
                case 1:
                    lastSource = _wavesDatas[1];
                    source     = _wavesDatas[2];
                    target     = _wavesDatas[0];
                    break;
                default:
                    lastSource = _wavesDatas[2];
                    source     = _wavesDatas[0];
                    target     = _wavesDatas[1];
                    break;
            }
            _frameNumber++;
            if (_frameNumber > 2) _frameNumber = 0;

        }

        class WavesData
        {
            public RTHandle       DataRT;
            public RTHandle       NormalRT;
            public RenderTargetIdentifier[] MRT = new RenderTargetIdentifier[2];
            public Vector3        WorldOffset;

            public void Initialize()
            {
                var settings = WaterSharedResources.GlobalSettings;
                var texSize  = settings.DynamicWavesResolutionPerMeter * settings.DynamicWavesAreaSize;
                DataRT = KWS_CoreUtils.RTHandles.Alloc(texSize, texSize, name: "_dynamicWavesDataRT", colorFormat: GraphicsFormat.R32_SFloat);
                NormalRT = KWS_CoreUtils.RTHandles.Alloc(texSize, texSize, name: "_dynamicWavesNormalRT", colorFormat: GraphicsFormat.R16G16_SFloat);
                MRT[0] = DataRT.rt;
                MRT[1] = NormalRT.rt;
            }

            public void Release()
            {
                for (int i = 0; i < 3; i++)
                {
                    DataRT?.Release();
                    NormalRT?.Release();
                    WorldOffset = Vector3.zero;
                }
            }
        }


        Vector3 ComputeAreaSimulationJitter(float offset)
        {
            var randTime  = Time.time * 30;
            var jitterSin = Mathf.Sin(randTime);
            var jitterCos = Mathf.Cos(randTime);
            var jitter    = new Vector3(offset * jitterSin, 0, offset * jitterCos);

            return jitter;
        }

        List<KWS_InteractWithWater> GetInteractScriptsInArea(Vector3 pos, float area)
        {
            _interactScriptsInArea.Clear();

            foreach (var instance in KWS_InteractWithWater.Instances)
            {
                var dist = KW_Extensions.DistanceXZ(instance.t.position, pos);
                if (dist < area * 1.1f)
                {
                    _interactScriptsInArea.Add(instance);
                }
            }

            return _interactScriptsInArea;
        }

    }
}