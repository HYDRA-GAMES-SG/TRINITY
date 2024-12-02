using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{
    internal class ReflectionFinalPass : WaterPass
    {
        internal override string PassName => "Water.ReflectionFinalPass";

        RTHandle _planarFinalRT;
        RTHandle                                            _ssrFinalRT;
        private Material                                    _anisoMaterial;


        public ReflectionFinalPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnWaterSettingsChanged;
            _anisoMaterial                                 =  KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ReflectionFiltering);
        }
        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnWaterSettingsChanged;

            ReleaseTextures();

            KW_Extensions.SafeDestroy(_anisoMaterial);
        }

        void ReleaseTextures()
        {
            _planarFinalRT?.Release();
            _ssrFinalRT?.Release();

            _planarFinalRT = _ssrFinalRT = null;
        }

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (changedTab.HasTab(WaterSystem.WaterTab.Reflection))
            {
                if (!waterInstance.Settings.UseAnisotropicReflections) return;

                if (WaterSharedResources.IsAnyWaterUseSsr) ReinitializeSsrTexture();
                if (WaterSharedResources.IsAnyWaterUsePlanar) ReinitializePlanarTexture();
            }
        }


        void ReinitializeSsrTexture()
        {
            _ssrFinalRT?.Release();

            var source = WaterSharedResources.SsrReflection;
            if (source == null || source.rt == null) return;
            _ssrFinalRT = KWS_CoreUtils.RTHandleAllocVR(ScreenSpaceReflectionPass.ScaleFunc, name: "_ssrFinalRT", colorFormat: source.rt.graphicsFormat, useMipMap: true, autoGenerateMips: false);
          
            KW_Extensions.WaterLog(this, _ssrFinalRT);
        }

        void ReinitializePlanarTexture()
        {
            _planarFinalRT?.Release();

            var source = WaterSharedResources.PlanarReflection;
            if (source == null || source.width <= 1) return;
            _planarFinalRT = KWS_CoreUtils.RTHandleAllocVR(source.width, source.height, colorFormat: source.graphicsFormat, name: "_planarFilteredRT", useMipMap: true, autoGenerateMips: false);

           KW_Extensions.WaterLog(this, _planarFinalRT);
        }


        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            if (WaterSharedResources.IsAnyWaterUseSsr)
            {
                ExecuteSsrPass(waterContext);
            }

            if (WaterSharedResources.IsAnyWaterUsePlanar)
            {
                if (WaterSharedResources.WaterInstances[WaterSharedResources.PlanarInstanceID - 1]) ExecutePlanarPass(waterContext);
            }
        }

      
        void ExecuteSsrPass(WaterPass.WaterPassContext waterContext)
        {
            var settings = WaterSharedResources.GlobalSettings;
            var cmd           = waterContext.cmd;
            var sourceRT      = WaterSharedResources.SsrReflection;
            if (sourceRT == null || sourceRT.rt == null) return;


            if (settings.UseAnisotropicReflections)
            {
                if (_ssrFinalRT == null || _ssrFinalRT.rt == null) ReinitializeSsrTexture();
                CoreUtils.SetRenderTarget(waterContext.cmd, _ssrFinalRT, ClearFlag.Color, Color.black);

                cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_AnisoReflectionsScale, settings.AnisotropicReflectionsScale);
                cmd.BlitTriangleRTHandle(sourceRT, _ssrFinalRT, _anisoMaterial, ClearFlag.None, Color.clear, settings.AnisotropicReflectionsHighQuality ? 1 : 0);
              
                cmd.GenerateMips(_ssrFinalRT);
                WaterSharedResources.SsrReflection = _ssrFinalRT;

            }
            else
            {
                cmd.GenerateMips(sourceRT);
                WaterSharedResources.SsrReflection = sourceRT;
            }

            cmd.SetGlobalTexture(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflectionRT, WaterSharedResources.SsrReflection);
            cmd.SetGlobalVector(KWS_ShaderConstants.SSR_ID.KWS_ScreenSpaceReflection_RTHandleScale, WaterSharedResources.SsrReflection.rtHandleProperties.rtHandleScale);
        }

        void ExecutePlanarPass(WaterPass.WaterPassContext waterContext)
        {
            var settings = WaterSharedResources.GlobalSettings;
            var cmd      = waterContext.cmd;
            var sourceRT = WaterSharedResources.PlanarReflection;
            if (sourceRT == null || sourceRT.width <= 1) return;


            if (settings.UseAnisotropicReflections)
            {
                if (_planarFinalRT == null || _planarFinalRT.rt == null) ReinitializePlanarTexture();
                CoreUtils.SetRenderTarget(waterContext.cmd, _planarFinalRT, ClearFlag.Color, Color.black);

                cmd.SetGlobalFloat(KWS_ShaderConstants.ConstantWaterParams.KWS_AnisoReflectionsScale, settings.AnisotropicReflectionsScale);
                cmd.BlitTriangleRTHandle(sourceRT, Vector4.one, _planarFinalRT, _anisoMaterial, ClearFlag.None, Color.clear, settings.AnisotropicReflectionsHighQuality ? 1 : 0);
                cmd.GenerateMips(_planarFinalRT);
                WaterSharedResources.PlanarReflection = _planarFinalRT.rt;
                Shader.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, WaterSharedResources.PlanarReflection);
            }
           
        }
    }
}