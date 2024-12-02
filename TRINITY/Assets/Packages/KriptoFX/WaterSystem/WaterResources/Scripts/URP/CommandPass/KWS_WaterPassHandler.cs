using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    internal class KWS_WaterPassHandler
    {

        private List<WaterPass> _waterPasses;

        OrthoDepthPass   _orthoDepthPass   = new();
        FftWavesPass     _fftWavesPass     = new();
        BuoyancyPass     _buoyancyPass     = new();
        FlowPass         _flowPass         = new();
        DynamicWavesPass _dynamicWavesPass = new();

        ShorelineWavesPass     _shorelineWavesPass     = new();
        WaterPrePass           _waterPrePass           = new();
        MotionVectorsPass      _motionVectorsPass      = new();
        CausticPrePass         _causticPrePass         = new();
        VolumetricLightingPass _volumetricLightingPass = new();
        CausticDecalPass       _causticDecalPass       = new();

        ScreenSpaceReflectionPass  _ssrPass             = new();
        ReflectionFinalPass        _reflectionFinalPass = new();
        DrawMeshPass               _drawMeshPass        = new();
        ShorelineFoamPass          _shorelineFoamPass   = new();
        UnderwaterPass             _underwaterPass      = new();
        DrawToPosteffectsDepthPass _drawToDepthPass     = new();

        private Dictionary<WaterSystemScriptableData.RefractionResolutionEnum, Downsampling> _downsamplingQuality = new Dictionary<WaterSystemScriptableData.RefractionResolutionEnum, Downsampling>()
        {
            { WaterSystemScriptableData.RefractionResolutionEnum.Full, Downsampling.None },
            { WaterSystemScriptableData.RefractionResolutionEnum.Half, Downsampling._2xBilinear },
            { WaterSystemScriptableData.RefractionResolutionEnum.Quarter, Downsampling._4xBilinear },
        };
        private Downsampling _defaultDownsampling = Downsampling._2xBilinear;
        private FieldInfo    _downsampleProp;

        internal KWS_WaterPassHandler()
        {

            _shorelineWavesPass.renderPassEvent     = RenderPassEvent.BeforeRenderingSkybox;
            _waterPrePass.renderPassEvent            = RenderPassEvent.BeforeRenderingSkybox;
            _motionVectorsPass.renderPassEvent      = RenderPassEvent.BeforeRenderingSkybox;
            _causticPrePass.renderPassEvent         = RenderPassEvent.BeforeRenderingSkybox;
            _volumetricLightingPass.renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
            _causticDecalPass.renderPassEvent       = RenderPassEvent.BeforeRenderingSkybox;

            _ssrPass.renderPassEvent             = RenderPassEvent.BeforeRenderingTransparents;
            _reflectionFinalPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            _shorelineFoamPass.renderPassEvent   = RenderPassEvent.BeforeRenderingTransparents;

            _drawToDepthPass.renderPassEvent     = RenderPassEvent.AfterRenderingTransparents;

            _waterPasses = new List<WaterPass>
            {
                _orthoDepthPass, _fftWavesPass, _buoyancyPass, _flowPass, _dynamicWavesPass,
                _shorelineWavesPass, _waterPrePass, _motionVectorsPass, _causticPrePass, _volumetricLightingPass, _causticDecalPass, 
                _ssrPass, _reflectionFinalPass, _drawMeshPass, _shorelineFoamPass, _underwaterPass, _drawToDepthPass
            };




//#if UNITY_EDITOR
            var urpAsset = UniversalRenderPipeline.asset;
            if (urpAsset != null)
            {
                urpAsset.supportsCameraOpaqueTexture = true;
                urpAsset.supportsCameraDepthTexture  = true;
                _defaultDownsampling                 = urpAsset.opaqueDownsampling;
            }
//#endif
            
        }

        void SetAssetSettings(UniversalAdditionalCameraData data, UniversalRenderPipelineAsset urpAsset)
        {
            data.requiresColorOption = CameraOverrideOption.On;
            data.requiresDepthOption = CameraOverrideOption.On;
            if (urpAsset.opaqueDownsampling != _defaultDownsampling)
            {
                _defaultDownsampling = urpAsset.opaqueDownsampling;
                Debug.Log("downsample changed " + _defaultDownsampling);
            }

            _downsampleProp = urpAsset.GetType().GetField("m_OpaqueDownsampling", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var downsample = KWS_CoreUtils.CanRenderUnderwater() ? Downsampling.None : _downsamplingQuality[WaterSharedResources.GlobalSettings.RefractionResolution];
            if (_downsampleProp != null) _downsampleProp.SetValue(urpAsset, downsample);

        }

        void RestoreAssetSettings()
        {
            var urpAsset = UniversalRenderPipeline.asset;
            var prop     = urpAsset.GetType().GetField("m_OpaqueDownsampling", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(urpAsset, _defaultDownsampling);
        }


        internal void OnBeforeFrameRendering(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            foreach (var waterPass in _waterPasses) waterPass.ExecutePerFrame(cameras, fixedUpdates);

        }


        internal void OnBeforeCameraRendering(Camera cam, ScriptableRenderContext ctx)
        {
            try
            {
                var urpAsset = UniversalRenderPipeline.asset;
                if (urpAsset == null) return;

                var data = cam.GetUniversalAdditionalCameraData();
                if (data == null) return;
                SetAssetSettings(data, urpAsset);

                var cameraSize = KWS_CoreUtils.GetScreenSizeLimited(KWS_CoreUtils.SinglePassStereoEnabled);
                KWS_CoreUtils.RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y);

                WaterPass.WaterPassContext waterContext = default;
                waterContext.cam = cam;

                waterContext.RenderContext        = ctx;
                waterContext.AdditionalCameraData = data;

                foreach (var waterPass in _waterPasses)
                {
                    waterPass.SetWaterContext(waterContext);
                    waterPass.ExecuteBeforeCameraRendering(cam);
                }

                var srpRenderer = data.scriptableRenderer;
                _waterPrePass.ConfigureInput(ScriptableRenderPassInput.Depth); //we need depth texture before "copy color" pass and caustic rendering

                _shorelineWavesPass.ColorPassWriteAccess = true;
                _causticDecalPass.ColorPassWriteAccess   = true;
                _drawToDepthPass.DepthPassWriteAccess    = true;


                if (WaterSharedResources.IsAnyWaterUseShoreline) srpRenderer.EnqueuePass(_shorelineWavesPass);
                srpRenderer.EnqueuePass(_waterPrePass);
                //srpRenderer.EnqueuePass(_motionVectorsPass);
                if (WaterSharedResources.IsAnyWaterUseCaustic) srpRenderer.EnqueuePass(_causticPrePass);
                if (WaterSharedResources.IsAnyWaterUseVolumetricLighting) srpRenderer.EnqueuePass(_volumetricLightingPass);
                if (WaterSharedResources.IsAnyWaterUseCaustic) srpRenderer.EnqueuePass(_causticDecalPass);
                if (WaterSharedResources.IsAnyWaterUseSsr) srpRenderer.EnqueuePass(_ssrPass);
                if (WaterSharedResources.IsAnyWaterUseCaustic || WaterSharedResources.IsAnyWaterUsePlanar) srpRenderer.EnqueuePass(_reflectionFinalPass);
                if (WaterSharedResources.IsAnyWaterUseShoreline) srpRenderer.EnqueuePass(_shorelineFoamPass);
                if (WaterSharedResources.IsAnyWaterUseDrawToDepth) srpRenderer.EnqueuePass(_drawToDepthPass);

            }
            catch (Exception e)
            {
                Debug.LogError("Water rendering error: " + e.InnerException);
            }
        }

        internal void OnAfterCameraRendering(Camera cam, ScriptableRenderContext ctx)
        {
            if (UniversalRenderPipeline.asset == null) return;

            if (_downsampleProp               != null) _downsampleProp.SetValue(UniversalRenderPipeline.asset, _defaultDownsampling);
        }


        public void Release()
        {
            if (_waterPasses != null)
            {
                foreach (var waterPass in _waterPasses) waterPass?.Release();
            }

            RestoreAssetSettings();
        }
    }
}