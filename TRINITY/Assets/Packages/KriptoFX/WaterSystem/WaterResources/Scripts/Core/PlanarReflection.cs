using UnityEngine;
using UnityEngine.Rendering;

#if ENABLE_VR_MODULE && ENABLE_VR && ENABLE_XR_MODULE
using UnityEngine.XR;
#endif

namespace KWS
{
    internal class PlanarReflection : ReflectionPass
    {
        private WaterSystem _waterInstance;
        private WaterSystemScriptableData _waterSettings;

        private GameObject _reflCameraGo;
        private Camera _reflectionCamera;
        private Transform _reflectionCameraTransform;

        RenderTexture         _planarRT;
        RenderTexture         _planarLeftRT;
        RenderTexture         _planarRightRT;
        private Material      _stereoBlitter;
        private CommandBuffer _stereoCmd;

        public override void RenderReflection(WaterSystem waterInstance, Camera currentCamera)
        {
            _waterInstance = waterInstance;
            _waterSettings = waterInstance.Settings;

            if (!_waterSettings.UsePlanarReflection) return;
            //if (!_waterSettings.EnabledMeshRendering) return;

            RenderPlanar(waterInstance, currentCamera);
        }
        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasTab(WaterSystem.WaterTab.Reflection)) return;

            _waterInstance = waterInstance;
            _waterSettings = waterInstance.Settings;

            if (!_waterSettings.UsePlanarReflection) return;
            //if (!_waterSettings.EnabledMeshRendering) return;

            ReleaseTextures();
            InitializeTextures(waterInstance);
        }

        public override void OnEnable()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnWaterSettingsChanged;
        }

  
        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnWaterSettingsChanged;

            if (_reflectionCamera != null) _reflectionCamera.targetTexture = null;
            KW_Extensions.SafeDestroy(_reflCameraGo, _stereoBlitter);
            ReleaseTextures();
           
           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        }


        void ReleaseTextures()
        {
            KW_Extensions.SafeDestroy(_planarRT, _planarLeftRT, _planarRightRT);
            _planarRT = _planarLeftRT = _planarRightRT = null;
        }

           // KW_Extensions.WaterLog(this, "Release", KW_Extensions.WaterLogMessageType.Release);
        void InitializeTextures(WaterSystem waterInstance)
        {
            var format   = KWS_CoreUtils.GetGraphicsFormatHDR();
            var height   = (int)waterInstance.Settings.PlanarReflectionResolutionQuality;
            var width    = height * 2; // typical resolution ratio is 16x9 (or 2x1), for better pixel filling we use [2 * width] x [height], instead of square [width] * [height].
            var isStereo = KWS_CoreUtils.SinglePassStereoEnabled;

            var dimension = isStereo ? TextureDimension.Tex2DArray : TextureDimension.Tex2D;
            var slices    = isStereo ? 2 : 1;

            _planarRT = new RenderTexture(width, height, 24, format) { name = "_planarRT", useMipMap = true, autoGenerateMips = false, volumeDepth = slices, dimension = dimension };
            _planarRT.Create();
            if (isStereo)
            {
                _planarLeftRT  = new RenderTexture(width, height, 24, format) { name = "_planarLeftEye", useMipMap = false };
                _planarRightRT = new RenderTexture(width, height, 24, format) { name = "_planarRightEye", useMipMap = false };

                _planarLeftRT.Create();
                _planarRightRT.Create();
            }
        
            KW_Extensions.WaterLog(this, _planarRT);
        }


        void CreateCamera()
        {
            _reflCameraGo = ReflectionUtils.CreateReflectionCamera("WaterPlanarReflectionCamera", _waterInstance, out _reflectionCamera, out _reflectionCameraTransform);
            KWS_CoreUtils.SetPlatformSpecificPlanarReflectionParams(_reflectionCamera);
        }

        void RenderPlanar(WaterSystem waterInstance, Camera currentCamera)
        {
            var isStereo = KWS_CoreUtils.SinglePassStereoEnabled;
            if (isStereo && currentCamera.cameraType == CameraType.SceneView) return;

            if (_reflCameraGo == null) CreateCamera();

            var occlusionMeshDefault = false;
#if ENABLE_VR_MODULE && ENABLE_VR && ENABLE_XR_MODULE
            if (isStereo)
            {
                occlusionMeshDefault = XRSettings.useOcclusionMesh;
                XRSettings.useOcclusionMesh = false;
            }
#endif

            currentCamera.CopyReflectionParamsFrom(_reflectionCamera, _waterSettings.PlanarCullingMask, isCubemap: false);
         
            KWS_CoreUtils.UpdatePlatformSpecificPlanarReflectionParams(_reflectionCamera, _waterInstance);

            if (_planarRT == null) InitializeTextures(waterInstance);

            if (isStereo)
            {
                ReflectionUtils.RenderPlanarReflection(currentCamera, waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarLeftRT,  true, Camera.StereoscopicEye.Left);
                ReflectionUtils.RenderPlanarReflection(currentCamera, waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarRightRT, true, Camera.StereoscopicEye.Right);

                if (_stereoBlitter == null) _stereoBlitter = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.CombineSeparatedTexturesToArrayVR);
  
                //ExecuteCommandBuffer doesnt work in VR (slice -1 just ignored and works only with camera command buffers), so I manually send "eye index" and "slice 0/1" 
                if (_stereoCmd == null) _stereoCmd = new CommandBuffer() { name = "KWS_PlanarStereoBlitter" };
                _stereoCmd.Clear();
                _stereoCmd.SetGlobalTexture("KWS_PlanarLeftEye", _planarLeftRT);
                _stereoCmd.SetGlobalTexture("KWS_PlanarRightEye", _planarRightRT);

                for (int slice = 0; slice < 2; slice++)
                {
                    _stereoCmd.SetGlobalFloat("KWS_StereoIndex", slice);
                    _stereoCmd.BlitTriangle(_stereoBlitter, _planarRT, pass: 0, slice: slice);
                }
              
                Graphics.ExecuteCommandBuffer(_stereoCmd); 
            }
            else
            {
                ReflectionUtils.RenderPlanarReflection(currentCamera, waterInstance, _reflectionCamera, _reflectionCameraTransform, _planarRT, false);
            }

           
            if (WaterSharedResources.GlobalSettings.UseAnisotropicReflections == false)
            {
                _planarRT.GenerateMips();
            }

            WaterSharedResources.PlanarReflection = _planarRT;
            Shader.SetGlobalTexture(KWS_ShaderConstants.ReflectionsID.KWS_PlanarReflectionRT, WaterSharedResources.PlanarReflection);

#if ENABLE_VR_MODULE && ENABLE_VR && ENABLE_XR_MODULE
            if (isStereo) XRSettings.useOcclusionMesh = occlusionMeshDefault;
#endif
        }
    }
}