using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


namespace KWS
{
    internal class DrawToPosteffectsDepthPass : WaterPass
    {

        RTHandleSystem _RTHandleSystem;
        RTHandle _copyDepthRT;
      
        private Material _drawToDepthMaterial;
        private Material _copyDepthMaterial;
        
        internal override string PassName => "Water.DrawToPosteffectsDepthPass";

        public DrawToPosteffectsDepthPass()
        {
            _drawToDepthMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.DrawToDepthShaderName);
            _copyDepthMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CopyDepthShaderName);
        }

       
        public override void Release()
        {
            _copyDepthRT?.Release();
            _copyDepthRT = null;

            _RTHandleSystem?.Dispose();

            KW_Extensions.SafeDestroy(_drawToDepthMaterial, _copyDepthMaterial);

            this.WaterLog(string.Empty, KW_Extensions.WaterLogMessageType.Release);
        }

        void InitializeTextures()
        {
            if (_RTHandleSystem == null)
            {
                _RTHandleSystem = new RTHandleSystem();
                var screenSize = KWS_CoreUtils.GetScreenSize(KWS_CoreUtils.SinglePassStereoEnabled);
                _RTHandleSystem.Initialize(screenSize.x, screenSize.y);
            }


            var dimension = KWS_CoreUtils.SinglePassStereoEnabled ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices = KWS_CoreUtils.SinglePassStereoEnabled ? 2 : 1;
            _copyDepthRT = _RTHandleSystem.Alloc(Vector2.one, name: "_copyDepthRT", colorFormat: GraphicsFormat.R32_SFloat, slices: slices, dimension: dimension);
         
            KW_Extensions.WaterLog("DrawToDepthPass", _copyDepthRT);
        }


        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            if (WaterSharedResources.IsAnyWaterUseDrawToDepth)
            {
                if (!KWS_Settings.Water.IsPostfxRequireDepthWriting) return;
                //if (waterInstance.Settings.UnderwaterQueue == WaterSystem.UnderwaterQueueEnum.AfterTransparent && WaterSystem.IsCameraUnderwater) return;

                if (_copyDepthRT == null) InitializeTextures();

                 var screenSize = KWS_CoreUtils.GetScreenSize(KWS_CoreUtils.SinglePassStereoEnabled);
                _RTHandleSystem.SetReferenceSize(screenSize.x, screenSize.y);

                waterContext.cmd.BlitTriangleRTHandle(_copyDepthRT, _copyDepthMaterial, ClearFlag.None, Color.clear);
                //WaterSharedResources.DepthCopyBeforeWaterWriting = _copyDepthRT;
                CoreUtils.SetRenderTarget(waterContext.cmd, waterContext.cameraColor, waterContext.cameraDepth);
                waterContext.cmd.BlitTriangle(_copyDepthRT, _copyDepthRT.rtHandleProperties.rtHandleScale, waterContext.cameraDepth, _drawToDepthMaterial);

            }

        }
    }
}