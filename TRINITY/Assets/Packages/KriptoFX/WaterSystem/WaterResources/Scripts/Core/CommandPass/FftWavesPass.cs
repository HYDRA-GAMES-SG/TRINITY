using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace KWS
{
    internal class FftWavesPass : WaterPass
    {
        internal override string PassName => "Water.FftWavesPass";

        static int kernelSpectrumInit;
        static int kernelSpectrumUpdate;
        static int kernelNormal;

        static Dictionary<int, Texture2D> _butterflyTextures = new Dictionary<int, Texture2D>();

        FftWavesData _globalFftWavesTextures = new FftWavesData();

        private WindZone      _lastWindZone;
        private float         _lastWindZoneSpeed;
        private float         _lastWindZoneTurbulence;
        private Vector3       _lastWindZoneRotation;
        private CommandBuffer _cmd;

        public class FftWavesData
        {
            public RTHandle[] DisplaceTexture = new RTHandle[2];
            public RTHandle[] NormalTextures  = new RTHandle[2];

            internal RTHandle spectrumInit;
            internal RTHandle spectrumDisplaceX; 
            internal RTHandle spectrumDisplaceY;
            internal RTHandle spectrumDisplaceZ;

            internal RTHandle fftTemp1;
            internal RTHandle fftTemp2;
            internal RTHandle fftTemp3;

            internal ComputeShader spectrumShader;
            internal ComputeShader shaderFFT;

            internal bool RequireReinitializeSpectrum;

            internal int Frame;

            public bool RequireReinitialize(WaterSystemScriptableData settings)
            {
                if (DisplaceTexture[0] == null || DisplaceTexture[0].rt == null) return true;

                var rt = DisplaceTexture[0].rt;
                if (rt.width != (int) settings.CurrentFftWavesQuality || rt.volumeDepth != settings.CurrentFftWavesCascades) return true;
              
                return false;
            }

            public void Initialize(WaterSystemScriptableData settings)
            {
                var size = (int)settings.CurrentFftWavesQuality;
                var slices = settings.CurrentFftWavesCascades;

                spectrumInit = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R32G32B32A32_SFloat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                spectrumDisplaceY = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R32G32_SFloat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                spectrumDisplaceX = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: spectrumDisplaceY.rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                spectrumDisplaceZ = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: spectrumDisplaceY.rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);

                fftTemp1 = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: spectrumDisplaceY.rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                fftTemp2 = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: spectrumDisplaceY.rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                fftTemp3 = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: spectrumDisplaceY.rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);

                DisplaceTexture[0] = KWS_CoreUtils.RTHandles.Alloc(size, size, name: "KWS_FftWavesDisplacement0", colorFormat: GraphicsFormat.R32G32B32A32_SFloat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);
                DisplaceTexture[1] = KWS_CoreUtils.RTHandles.Alloc(size, size, name: "KWS_FftWavesDisplacement1", colorFormat: DisplaceTexture[0].rt.graphicsFormat, enableRandomWrite: true, dimension: TextureDimension.Tex2DArray, slices: slices);

                NormalTextures[0] = KWS_CoreUtils.RTHandles.Alloc(size, size, name: "KWS_FftWavesNormal1", colorFormat: GraphicsFormat.R16G16B16A16_SFloat, enableRandomWrite: true,
                                                               autoGenerateMips: false, useMipMap: true, dimension: TextureDimension.Tex2DArray, slices: slices, filterMode: FilterMode.Trilinear);
                NormalTextures[1] = KWS_CoreUtils.RTHandles.Alloc(size, size, name: "KWS_FftWavesNormal2", colorFormat: NormalTextures[0].rt.graphicsFormat, enableRandomWrite: true,
                                                               autoGenerateMips: false, useMipMap: true, dimension: TextureDimension.Tex2DArray, slices: slices, filterMode: FilterMode.Trilinear);

                if (spectrumShader == null) spectrumShader = KWS_CoreUtils.LoadComputeShader("Common/CommandPass/KWS_WavesSpectrum");
                if (shaderFFT == null) shaderFFT    = KWS_CoreUtils.LoadComputeShader("Common/CommandPass/KWS_WavesFFT");

                
                if (spectrumShader != null)
                {
                    spectrumShader.name  = "WavesSpectrum";
                    kernelSpectrumInit   = spectrumShader.FindKernel("SpectrumInitalize");
                    kernelSpectrumUpdate = spectrumShader.FindKernel("SpectrumUpdate");

                    spectrumShader.SetTexture(kernelSpectrumUpdate, "SpectrumInit", spectrumInit);
                    spectrumShader.SetTexture(kernelSpectrumUpdate, "SpectrumDisplaceX", spectrumDisplaceX);
                    spectrumShader.SetTexture(kernelSpectrumUpdate, "SpectrumDisplaceY", spectrumDisplaceY);
                    spectrumShader.SetTexture(kernelSpectrumUpdate, "SpectrumDisplaceZ", spectrumDisplaceZ);
                }

                if (shaderFFT != null)
                {
                    shaderFFT.name = "WavesFFT";
                    kernelNormal   = shaderFFT.FindKernel("ComputeNormal");

                    var fftKernel = GetKernelBySize(size);

                    shaderFFT.SetTexture(fftKernel, "SpectrumDisplaceX", spectrumDisplaceX);
                    shaderFFT.SetTexture(fftKernel, "SpectrumDisplaceY", spectrumDisplaceY);
                    shaderFFT.SetTexture(fftKernel, "SpectrumDisplaceZ", spectrumDisplaceZ);
                    shaderFFT.SetTexture(fftKernel, "inputButterfly", GetOrCreateButterflyTexture(size));
                    shaderFFT.SetTexture(fftKernel, "_displaceX", fftTemp1);
                    shaderFFT.SetTexture(fftKernel, "_displaceY", fftTemp2);
                    shaderFFT.SetTexture(fftKernel, "_displaceZ", fftTemp3);

                    shaderFFT.SetTexture(fftKernel + 1, "SpectrumDisplaceX", fftTemp1);
                    shaderFFT.SetTexture(fftKernel + 1, "SpectrumDisplaceY", fftTemp2);
                    shaderFFT.SetTexture(fftKernel + 1, "SpectrumDisplaceZ", fftTemp3);
                    shaderFFT.SetTexture(fftKernel + 1, "inputButterfly",    GetOrCreateButterflyTexture(size));
                    //shaderFFT.SetTexture(fftKernel + 1, "_displaceXYZ",      DisplaceTexture);

                    shaderFFT.SetFloat("texelSize", 1f / NormalTextures[0].rt.width);
                    //shaderFFT.SetTexture(kernelNormal, "_displaceXYZ", DisplaceTexture);
                }

                this.WaterLog(DisplaceTexture[0], NormalTextures[0]);
            }

            public RTHandle GetCurrentTargetNormal()
            {
                return NormalTextures[Frame];
            }

            public RTHandle GetPreviousTargetNormal()
            {
                return NormalTextures[(Frame + 1) % 2];
            }

            public RTHandle GetCurrentDisplacement()
            {
                return DisplaceTexture[Frame];
            }

            public RTHandle GetPreviousDisplacement()
            {
                return DisplaceTexture[(Frame + 1) % 2];
            }

            public void SwapTargetNormal()
            {
                Frame = (Frame + 1) % 2;
            }

            public void Release()
            {
                ReleaseTextures();
                KW_Extensions.SafeDestroy(spectrumShader, shaderFFT);
                
                this.WaterLog(String.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
            }

            public void ReleaseTextures()
            {
                spectrumInit?.Release();
                spectrumDisplaceY?.Release();
                spectrumDisplaceX?.Release();
                spectrumDisplaceZ?.Release();

                fftTemp1?.Release();
                fftTemp2?.Release();
                fftTemp3?.Release();

                DisplaceTexture[0]?.Release();
                DisplaceTexture[1]?.Release();

                NormalTextures[0]?.Release();
                NormalTextures[1]?.Release();

                DisplaceTexture[0] = DisplaceTexture[1] = NormalTextures[0] = NormalTextures[1] = null;

                this.WaterLog(String.Empty, KW_Extensions.WaterLogMessageType.ReleaseRT);
            }
        }

        public FftWavesPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }


        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            
            foreach (var butterflyTexture in _butterflyTextures) KW_Extensions.SafeDestroy(butterflyTexture.Value);
            _butterflyTextures.Clear();

            _globalFftWavesTextures.Release();

            this.WaterLog(String.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        private void OnAnyWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTabs)
        {
            if (!changedTabs.HasTab(WaterSystem.WaterTab.Waves)) return;

            var data = waterInstance.Settings.UseGlobalWind ? _globalFftWavesTextures : waterInstance.InstanceData.FftWavesTextures;
            InitializeFftWavesData(waterInstance.Settings, data);
        }

        void InitializeFftWavesData(WaterSystemScriptableData settings, FftWavesData data)
        {
            if (data.RequireReinitialize(settings))
            {
                data.ReleaseTextures();
                data.Initialize(settings);
            }

            data.RequireReinitializeSpectrum = true;
        }

        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            
#if KWS_DEBUG
            if (WaterSystem.UseNetworkBuoyancy == false && fixedUpdates.FramesCount_60fps == 0 && WaterSystem.RebuildMaxHeight == false) return;
#else
            if (WaterSystem.UseNetworkBuoyancy == false && fixedUpdates.FramesCount_60fps == 0) return;
#endif
            if (_cmd == null) _cmd = new CommandBuffer() { name = PassName };
            _cmd.Clear();
            bool requireExecuteCmd = false;

            if (WaterSharedResources.IsAnyWaterUseGlobalWind 
             && (WaterSharedResources.WaterInstances.Any(water => KWS_CoreUtils.IsWaterVisibleAndActive(water)) || WaterSystem.UseNetworkBuoyancy))
            {
                if (WaterSharedResources.GlobalSettings.GlobalWindZone != null && IsWindZoneChanged(WaterSharedResources.GlobalSettings)) 
                    _globalFftWavesTextures.RequireReinitializeSpectrum = true;
              
                ExecuteInstance(_cmd, WaterSharedResources.GlobalSettings, _globalFftWavesTextures);
                WaterSharedResources.FftWavesDisplacement = _globalFftWavesTextures.GetCurrentDisplacement();
                WaterSharedResources.FftWavesNormal       = _globalFftWavesTextures.GetCurrentTargetNormal();

                Shader.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesDisplace, WaterSharedResources.FftWavesDisplacement);
                Shader.SetGlobalTexture(KWS_ShaderConstants.FFT.KWS_FftWavesNormal,   WaterSharedResources.FftWavesNormal);
                requireExecuteCmd = true;
            }

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (IsRequireRenderFft(waterInstance))
                {
                    ExecuteInstance(_cmd, waterInstance.Settings, waterInstance.InstanceData.FftWavesTextures);
                    requireExecuteCmd = true;
                }
            }
          
            if(requireExecuteCmd) Graphics.ExecuteCommandBuffer(_cmd);
        }

        void ExecuteInstance(CommandBuffer cmd, WaterSystemScriptableData settings, FftWavesData data)
        {
            if (data.DisplaceTexture[0] == null) InitializeFftWavesData(settings, data);

            cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, settings.CurrentWavesAreaScale);

            if (data.RequireReinitializeSpectrum) InitializeSpectrum(cmd, settings, data);
            UpdateSpectrum(cmd, settings, data);
            DispatchFFT(cmd, settings, data);
        }


        void InitializeSpectrum(CommandBuffer cmd, WaterSystemScriptableData settings, FftWavesData lod)
        {
            var size = (int) settings.CurrentFftWavesQuality;
            var spectrumShader = lod.spectrumShader;

            cmd.SetComputeFloatParam(spectrumShader, "KWS_WindSpeed", settings.CurrentWindSpeed);
            cmd.SetComputeFloatParam(spectrumShader, "KWS_Turbulence",  settings.CurrentWindTurbulence);
            cmd.SetComputeFloatParam(spectrumShader, "KWS_WindRotation", settings.CurrentWindRotation);

            cmd.SetComputeIntParam(spectrumShader, "KWS_Size", size);

            //cmd.SetComputeFloatParams(spectrumShader, KWS_ShaderConstants.ConstantWaterParams.KWS_WavesDomainSizes, KWS_Settings.FFT.FftDomainSize);
            cmd.SetComputeFloatParam(spectrumShader, KWS_ShaderConstants.ConstantWaterParams.KWS_WavesAreaScale, settings.CurrentWavesAreaScale);

            cmd.SetComputeTextureParam(spectrumShader, kernelSpectrumInit, "RW_SpectrumInit", lod.spectrumInit);

            cmd.DispatchCompute(spectrumShader, kernelSpectrumInit, size / 8, size / 8, settings.CurrentFftWavesCascades);
            lod.RequireReinitializeSpectrum = false;

            this.WaterLog($"InitializeSpectrum");
        }

        void UpdateSpectrum(CommandBuffer cmd, WaterSystemScriptableData settings, FftWavesData lod)
        {
            var spectrumShader = lod.spectrumShader;
            var time = KW_Extensions.TotalTime() * settings.CurrentTimeScale;
            var size = (int)settings.CurrentFftWavesQuality;

#if KWS_DEBUG
            if (WaterSystem.RebuildMaxHeight) time *= 5;
#endif

            cmd.SetComputeFloatParam(spectrumShader, "time", time);

            if (spectrumShader == null)
            {
                Debug.LogError($"Water UpdateSpectrum error: {spectrumShader}");
                return;
            }
            cmd.DispatchCompute(spectrumShader, kernelSpectrumUpdate, size / 8, size / 8, settings.CurrentFftWavesCascades);
        }

        void DispatchFFT(CommandBuffer cmd, WaterSystemScriptableData settings, FftWavesData lod)
        {
            var size = (int)settings.CurrentFftWavesQuality;
            var fftKernel = GetKernelBySize(size);

            if (lod.shaderFFT == null)
            {
                Debug.LogError($"Water DispatchFFT error: {lod.shaderFFT}");
                return;
            }

            cmd.SetComputeTextureParam(lod.shaderFFT, fftKernel + 1, "_displaceXYZ", lod.GetCurrentDisplacement());
            cmd.DispatchCompute(lod.shaderFFT, fftKernel, 1, size, settings.CurrentFftWavesCascades);
            cmd.DispatchCompute(lod.shaderFFT, fftKernel + 1, size, 1, settings.CurrentFftWavesCascades);

            cmd.SetComputeTextureParam(lod.shaderFFT, kernelNormal, "_displaceXYZ",             lod.GetCurrentDisplacement());
            cmd.SetComputeTextureParam(lod.shaderFFT, kernelNormal, "KWS_NormalFoamTargetRW",   lod.GetCurrentTargetNormal());
            cmd.SetComputeTextureParam(lod.shaderFFT, kernelNormal, "KWS_PrevNormalFoamTarget", lod.GetPreviousTargetNormal());
            cmd.SetComputeFloatParam(lod.shaderFFT, "KWS_WindSpeed", settings.CurrentWindSpeed);
            cmd.SetComputeFloatParam(lod.shaderFFT, "KWS_WindTurbulence", settings.CurrentWindTurbulence);
            cmd.SetComputeFloatParam(lod.shaderFFT, "KWS_OceanFoamStrength", settings.OceanFoamStrength);

            cmd.DispatchCompute(lod.shaderFFT, kernelNormal, size / 8, size / 8, settings.CurrentFftWavesCascades);
            cmd.GenerateMips(lod.GetCurrentTargetNormal());

            lod.SwapTargetNormal();
        }

        bool IsRequireRenderFft(WaterSystem waterInstance)
        {
            //if (!KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)) return false; //todo cause problem with multiple cameras and if the instance is not visible for one of them
            if (KWS_UpdateManager.LastFrameRenderedCameras.Count == 1)
            {
                if (WaterSystem.UseNetworkBuoyancy == false && !KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)) return false;
            }
            if (waterInstance.Settings.UseGlobalWind == false) return true;
            
            return false;
        }

        static Texture2D GetOrCreateButterflyTexture(int size)
        {
            if (!_butterflyTextures.ContainsKey(size)) _butterflyTextures.Add(size, InitializeButterfly(size));
            
            return _butterflyTextures[size];
        }

        static Texture2D InitializeButterfly(int size)
        {
            var log2Size = Mathf.RoundToInt(Mathf.Log(size, 2));
            var butterflyColors = new Color[size * log2Size];

            int offset = 1, numIterations = size >> 1;
            for (int rowIndex = 0; rowIndex < log2Size; rowIndex++)
            {
                int rowOffset = rowIndex * size;
                {
                    int start = 0, end = 2 * offset;
                    for (int iteration = 0; iteration < numIterations; iteration++)
                    {
                        var bigK = 0.0f;
                        for (int K = start; K < end; K += 2)
                        {
                            var phase = 2.0f * Mathf.PI * bigK * numIterations / size;
                            var cos   = Mathf.Cos(phase);
                            var sin   = Mathf.Sin(phase);
                            butterflyColors[rowOffset         + K / 2]  = new Color(cos,  -sin, 0, 1);
                            butterflyColors[rowOffset + K / 2 + offset] = new Color(-cos, sin,  0, 1);

                            bigK += 1.0f;
                        }
                        start += 4 * offset;
                        end   =  start + 2 * offset;
                    }
                }
                numIterations >>= 1;
                offset        <<= 1;
            }
            var texButterfly = new Texture2D(size, log2Size, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            texButterfly.SetPixels(butterflyColors);
            texButterfly.Apply();
            return texButterfly;
        }

        uint GetHashFromUV(Vector2Int uv)
        {
            return (((uint)uv.x & 0xFFFF) << 16) | ((uint)uv.y & 0xFFFF);
        }

        Vector2Int GetUVFromHash(uint p)
        {
            return new Vector2Int((int)((p >> 16) & 0xFFFF), (int)(p & 0xFFFF));
        }


        bool IsWindZoneChanged(WaterSystemScriptableData settings)
        {
            var windZone = settings.GlobalWindZone;
            if (settings.GlobalWindZone != _lastWindZone)
            {
                _lastWindZone = settings.GlobalWindZone;
                return true;
            }

            if (Math.Abs(_lastWindZoneSpeed - windZone.windMain * settings.GlobalWindZoneSpeedMultiplier) > 0.001f)
            {
                _lastWindZoneSpeed = windZone.windMain * settings.GlobalWindZoneSpeedMultiplier;
                return true;
            }

            if (Math.Abs(_lastWindZoneTurbulence - windZone.windTurbulence * settings.GlobalWindZoneTurbulenceMultiplier) > 0.001f)
            {
                _lastWindZoneTurbulence = windZone.windTurbulence * settings.GlobalWindZoneTurbulenceMultiplier;
                return true;
            }

            var forward = windZone.transform.forward;
            if (Math.Abs(_lastWindZoneRotation.x - forward.x) > 0.001f || Math.Abs(_lastWindZoneRotation.z - forward.z) > 0.001f)
            {
                _lastWindZoneRotation = forward;
                return true;
            }

            return false;
        }


       
        static int GetKernelBySize(int size)
        {
            var kernelOffset = 0;
            kernelOffset = size switch
            {
                (int)WaterSystemScriptableData.FftWavesQualityEnum.Low    => 0,
                (int)WaterSystemScriptableData.FftWavesQualityEnum.Medium => 2,
                (int)WaterSystemScriptableData.FftWavesQualityEnum.High   => 4,
                (int)WaterSystemScriptableData.FftWavesQualityEnum.Ultra  => 6,
                //(int) WaterSystemScriptableData.FftWavesQualityEnum.Size_512 => 8,
                _ => kernelOffset
            };
            return kernelOffset;
        }

    }
}