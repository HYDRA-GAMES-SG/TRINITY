using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{

    internal partial class KWS_UpdateManager
    {
        private KWS_WaterPassHandler _passHandler;

        void OnEnablePlatformSpecific()
        {
            //Debug.Log("Initialized update manager");
            RenderPipelineManager.beginCameraRendering += OnBeforeCameraRendering;
            RenderPipelineManager.endCameraRendering += OnAfterCameraRendering;
            if (_passHandler == null) _passHandler = new KWS_WaterPassHandler();
        }

       
        void OnDisablePlatformSpecific()
        {
            //Debug.Log("Removed update manager");
            RenderPipelineManager.beginCameraRendering -= OnBeforeCameraRendering;
            RenderPipelineManager.endCameraRendering   -= OnAfterCameraRendering;
            _passHandler?.Release();

            KWS_CoreUtils.ReleaseRTHandles();
        }

        private void OnBeforeCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            ExecutePerCamera(cam, context);
        }

        private void OnAfterCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            _passHandler.OnAfterCameraRendering(cam, context);
        }
    }
}