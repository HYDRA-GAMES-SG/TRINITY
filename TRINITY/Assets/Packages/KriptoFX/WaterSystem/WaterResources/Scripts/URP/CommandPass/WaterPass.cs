using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace KWS
{
    internal abstract class WaterPass : ScriptableRenderPass
    {
        public bool ColorPassWriteAccess;
        public bool DepthPassWriteAccess;

        readonly RenderTargetIdentifier _cameraDepthTextureRT = new RenderTargetIdentifier(Shader.PropertyToID("_CameraDepthTexture"));
        readonly static FieldInfo depthTextureFieldInfo = typeof(UniversalRenderer).GetField("m_DepthTexture", BindingFlags.NonPublic | BindingFlags.Instance);

        static RTHandle _dummyRT;
        public static RTHandle dummyRT => _dummyRT ?? (_dummyRT = RTHandles.Alloc(1, 1, colorFormat: GraphicsFormat.R8G8B8A8_UNorm));
        internal struct WaterPassContext
        {
            public Camera cam;
            public CommandBuffer cmd;

#if UNITY_2022_1_OR_NEWER
            public RTHandle cameraDepth;
            public RTHandle cameraColor;
#else
            public RenderTargetIdentifier cameraDepth;
            public RenderTargetIdentifier cameraColor;
            public RenderTargetIdentifier cameraDepthTexture;
#endif
            //public int RequiredFixedUpdateCount;
            public CustomFixedUpdates FixedUpdates;

            public ScriptableRenderContext RenderContext;
            public UniversalAdditionalCameraData AdditionalCameraData;
        }


        WaterPassContext _waterContext;


        internal void SetWaterContext(WaterPassContext waterContext)
        {
            _waterContext = waterContext;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //if (useStereoTarget && KWS_CoreUtils.SinglePassStereoEnabled) CoreUtils.SetRenderTarget(cmd, BuiltinRenderTextureType.CurrentActive);

            //ConfigureTarget doesnt work properly with configureClear and MRT, so I use CoreUtils.SetRenderTarget anyway.
            //But urp rendering requires using ConfigureTarget to indicate "use camera target" or "use custom target"
            ConfigureTarget(dummyRT, dummyRT);

            Shader.SetGlobalInteger("KWS_AdditionalLightsCount", renderingData.lightData.additionalLightsCount);

            _waterContext.cmd = CommandBufferPool.Get(PassName);
            _waterContext.cmd.Clear();

#if UNITY_2022_1_OR_NEWER
            _waterContext.cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
            _waterContext.cameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            //_waterContext.cameraDepth = depthTextureFieldInfo.GetValue(renderingData.cameraData.renderer) as RTHandle;
#else
            _waterContext.cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
            //_waterContext.cameraDepth = renderingData.cameraData.renderer.cameraDepthTarget; //doesnt work in editor and also editor camera ignores "water depth write" issue
            _waterContext.cameraDepth = _cameraDepthTextureRT;
#endif

            ExecuteCommandBuffer(_waterContext);
            _waterContext.RenderContext.ExecuteCommandBuffer(_waterContext.cmd);

            CommandBufferPool.Release(_waterContext.cmd);
        }

#if UNITY_6000_0_OR_NEWER

        private class PassData
        {
            internal TextureHandle cameraColorTarget;
            internal TextureHandle cameraDepthTarget;
        }


        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>(PassName, out var passData))
            {
                UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();

                passData.cameraColorTarget = resourceData.activeColorTexture;
                builder.UseTexture(passData.cameraColorTarget, ColorPassWriteAccess ? AccessFlags.Write : AccessFlags.Read);
                
                if (DepthPassWriteAccess)
                {
                    passData.cameraDepthTarget = resourceData.cameraDepthTexture;
                    builder.UseTexture(passData.cameraDepthTarget, AccessFlags.Write);
                }
                else
                {
                    passData.cameraDepthTarget = resourceData.activeDepthTexture;
                    builder.UseTexture(passData.cameraDepthTarget);
                }

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        void ExecutePass(PassData passData, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
            _waterContext.cameraColor        = passData.cameraColorTarget;
            _waterContext.cameraDepth        = passData.cameraDepthTarget;

            _waterContext.cmd = cmd;
            ExecuteCommandBuffer(_waterContext);

        }
#endif
        internal virtual string PassName { get; }
        public virtual void ExecuteCommandBuffer(WaterPassContext waterContext) { }
        public virtual void ExecuteBeforeCameraRendering(Camera cam) { }
        public virtual void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates) { }
        public abstract void Release();
    }
}