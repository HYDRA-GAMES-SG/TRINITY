
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace KWS
{
    [AddComponentMenu("")]
    [ExecuteAlways]
    internal partial class KWS_UpdateManager : MonoBehaviour
    {
        private float _fixedUpdateLeftTime_60fps = 0;
        private float _fixedUpdateLeftTime_45fps = 0;
        private float _fixedUpdateLeftTime_30fps = 0;
     
        const   int   maxAllowedFrames  = 2;
        const   float fixedUpdate_60fps = 1f / 60f;
        const   float fixedUpdate_45fps = 1f / 45f;
        const   float fixedUpdate_30fps = 1f / 30f;

        private CustomFixedUpdates _customFixedUpdates = new CustomFixedUpdates();

        internal static HashSet<Camera> LastFrameRenderedCameras = new HashSet<Camera>();

        
        void OnEnable()
        {
            OnEnablePlatformSpecific();

#if UNITY_EDITOR
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update           += EditorUpdate;
#endif
        }

        void OnDisable()
        {
            OnDisablePlatformSpecific();
            KWS_CoreUtils.ReleaseRTHandles();
            LastFrameRenderedCameras.Clear();


#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.update           -= EditorUpdate;
#endif
        }


#if UNITY_EDITOR
        private void OnHierarchyChanged()
        {
            WaterSharedResources.UpdateReflectionProbeCache();
        }

        private void EditorUpdate()
        {
            if (Application.isPlaying) return;
            ExecutePerFrame();
        }
#endif

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            ExecutePerFrame();
        }


        private void ExecutePerFrame()
        {
            KW_Extensions.UpdateEditorDeltaTime();

            if (!KWS_CoreUtils.CanRenderAnyWater()) return;
            UpdateCustomFixedUpdates();

            //as we can use keywords for non-water effect, like underwater particles, then I need to instal it here, even water is not visible
            Shader.SetKeyword(KWS_ShaderConstants.WaterKeywords.GlobalKeyword_KWS_USE_VOLUMETRIC_LIGHT,   WaterSharedResources.IsAnyWaterUseVolumetricLighting);
            Shader.SetKeyword(KWS_ShaderConstants.WaterKeywords.GlobalKeyword_KWS_USE_HALF_LINE_TENSION,  WaterSharedResources.GlobalSettings.UseUnderwaterHalfLineTensionEffect);

            LastFrameRenderedCameras.RemoveWhere(item => item == null);
            _passHandler.OnBeforeFrameRendering(LastFrameRenderedCameras, _customFixedUpdates);
            LastFrameRenderedCameras.Clear();
        }


        void ExecutePerCamera(Camera cam, ScriptableRenderContext context)
        {
#if ENABLE_VR_MODULE && ENABLE_VR && ENABLE_XR_MODULE
            KWS_CoreUtils.SinglePassStereoEnabled = KWS_CoreUtils.IsSinglePassStereoActive();
#endif
            if (!KWS_CoreUtils.CanRenderAnyWater()) return;
            if (!KWS_CoreUtils.CanRenderCurrentCamera(cam)) return;


            LastFrameRenderedCameras.Add(cam);

            WaterSystem.OnPreCameraRender(cam);
            Shader.SetKeyword(KWS_ShaderConstants.WaterKeywords.GlobalKeyword_KWS_USE_AQUARIUM_RENDERING, WaterSharedResources.IsAnyAquariumWaterVisible);

            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                waterInstance.CurrentContext = context;
                waterInstance.OnPreWaterRender(cam);
            }

            _passHandler.OnBeforeCameraRendering(cam, context);
        }


        public void UpdateCustomFixedUpdates()
        {
            var deltaTime = KW_Extensions.DeltaTime();
           
            _fixedUpdateLeftTime_60fps += deltaTime;
            _fixedUpdateLeftTime_45fps += deltaTime;
            _fixedUpdateLeftTime_30fps += deltaTime;

            UpdateFixedFrames(ref _fixedUpdateLeftTime_60fps, out _customFixedUpdates.FramesCount_60fps, fixedUpdate_60fps);
            UpdateFixedFrames(ref _fixedUpdateLeftTime_45fps, out _customFixedUpdates.FramesCount_45fps, fixedUpdate_45fps);
            UpdateFixedFrames(ref _fixedUpdateLeftTime_30fps, out _customFixedUpdates.FramesCount_30fps, fixedUpdate_30fps);


#if UNITY_EDITOR
            if (KWS_CoreUtils.IsFrameDebuggerEnabled())
            {
                _customFixedUpdates.FramesCount_60fps = 1;
                _customFixedUpdates.FramesCount_45fps = 1;
                _customFixedUpdates.FramesCount_30fps = 1;
            }
#endif
        }

        private void UpdateFixedFrames(ref float leftTimeVal, out int framesVal, float deltaTime)
        {
#if UNITY_EDITOR
            if (WaterSystem.DebugUpdateManager) deltaTime = 1;
#endif

                int frames = 0;
            while (leftTimeVal > 0f)
            {
                leftTimeVal -= deltaTime;
                frames++;
                if (frames > maxAllowedFrames)
                {
                    leftTimeVal = 0;
                    framesVal   = frames;
                    return;
                }
            }

            framesVal = frames;
        }
    }

    internal class CustomFixedUpdates
    {
        public int FramesCount_60fps;
        public int FramesCount_45fps;
        public int FramesCount_30fps;
    }
}