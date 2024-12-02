//todo update it when water position changed
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    internal class OrthoDepthPass : WaterPass
    {
        internal override string PassName => "Water.OrthoDepthPass";

        private GameObject _currentCamGO;
        private Camera _depthCamera;
        private Transform _camTransform;

        readonly PassData _currentPassData = new PassData();
        readonly PassData _currentPassDataEditor = new PassData();
        private bool _forceUpdate = false;

        public OrthoDepthPass()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged += OnAnyWaterSettingsChanged;
        }


        public override void Release()
        {
            WaterSharedResources.OnAnyWaterSettingsChanged -= OnAnyWaterSettingsChanged;
            KW_Extensions.SafeDestroy(_currentCamGO);
            _currentPassData.Release();
            _currentPassDataEditor.Release();
        }
        internal class PassData
        {
            public RenderTexture DepthRT;
            public Vector3 LastCamPosition = Vector3.positiveInfinity;
            public Vector4 NearFarSize = new Vector4(-KWS_Settings.Water.OrthoDepthAreaNearOffset, KWS_Settings.Water.OrthoDepthAreaFarOffset, KWS_Settings.Water.OrthoDepthAreaSize, 0);

            public void InitializeTexture()
            {
                if (DepthRT == null) DepthRT = new RenderTexture(KWS_Settings.Water.OrthoDepthResolution, KWS_Settings.Water.OrthoDepthResolution, 24, RenderTextureFormat.Depth);
            }

            public void Release()
            {
                if (DepthRT != null)
                {
                    DepthRT.Release();
                    DepthRT = null;
                }

                LastCamPosition = Vector3.positiveInfinity;
            }
        }


        private void OnAnyWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab waterTab)
        {
            if (waterTab.HasTab(WaterSystem.WaterTab.Transform))
            {
                _forceUpdate = true;
            }
        }

        void InitializeCamera()
        {
            _currentCamGO = KW_Extensions.CreateHiddenGameObject("Ortho depth camera");

            _camTransform = _currentCamGO.transform;
            _camTransform.parent = WaterSystem.UpdateManagerObject.transform;

            _depthCamera = _currentCamGO.AddComponent<Camera>();
            _depthCamera.cameraType = CameraType.Reflection;
            _depthCamera.depthTextureMode = DepthTextureMode.None;
            _depthCamera.backgroundColor = Color.black;
            _depthCamera.clearFlags = CameraClearFlags.Color;
            _depthCamera.aspect = 1.0f;
            _depthCamera.depth = -50;

            _depthCamera.orthographic = true;
            _depthCamera.allowHDR = false;
            _depthCamera.enabled = false;

            _depthCamera.orthographicSize = KWS_Settings.Water.OrthoDepthAreaSize * 0.5f;
            _depthCamera.farClipPlane = KWS_Settings.Water.OrthoDepthAreaFarOffset;
            _depthCamera.nearClipPlane = -KWS_Settings.Water.OrthoDepthAreaNearOffset;
            _depthCamera.cullingMask = ~(1 << KWS_Settings.Water.WaterLayer);

        }


        bool CheckIfCanUpdateDepth(float waterLevel, Vector3 camPos, PassData passData)
        {
            var depthAreaSize = KWS_Settings.Water.OrthoDepthAreaSize;
            if (_forceUpdate
             || (waterLevel + depthAreaSize > camPos.y && KW_Extensions.DistanceXZ(passData.LastCamPosition, camPos) >= depthAreaSize * 0.25f))
            {
                passData.LastCamPosition = camPos;

                _forceUpdate = false;
                return true;
            }

            return false;
        }

        float GetMaxWaterLevelInArea(Vector3 cameraPos, float areaSize)
        {
            float maxLevel = float.MaxValue;
            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                var waterLevel = waterInstance.WaterPivotWorldPosition.y;
                if (KW_Extensions.IsAABBIntersectSphere(waterInstance.WorldSpaceBounds, cameraPos, areaSize)) maxLevel = Mathf.Min(maxLevel, waterLevel);
            }

            return maxLevel;
        }


        public override void ExecutePerFrame(HashSet<Camera> cameras, CustomFixedUpdates fixedUpdates)
        {
            if (WaterSharedResources.IsAnyWaterUseShoreline == false) return;


            foreach (var cam in cameras)
            {
                if (cam == null) continue;

                if (_currentCamGO == null) InitializeCamera();
                var camPos = cam.GetCameraPositionFast();
                var waterLevel = GetMaxWaterLevelInArea(camPos, KWS_Settings.Water.OrthoDepthAreaSize);
                if (Math.Abs(waterLevel - float.MaxValue) < 1) return;


                var isEditorCamera = cam.cameraType == CameraType.SceneView;
                var passData = isEditorCamera ? _currentPassDataEditor : _currentPassData;
                if (passData.DepthRT == null) passData.InitializeTexture();

                if (!CheckIfCanUpdateDepth(waterLevel, camPos, passData)) continue;

                camPos.y = waterLevel;
                _camTransform.position = camPos;
                _camTransform.rotation = Quaternion.Euler(90, 0, 0);

                KWS_CoreUtils.RenderDepth(_depthCamera, passData.DepthRT);
                this.WaterLog("Render ortho depth");

                WaterSharedResources.OrthoDepth = passData.DepthRT;
                WaterSharedResources.OrthoDepthPosition = camPos;
                WaterSharedResources.OrthoDepthNearFarSize = passData.NearFarSize;

                Shader.SetGlobalTexture(KWS_ShaderConstants.OrthoDepth.KWS_WaterOrthoDepthRT, WaterSharedResources.OrthoDepth);
                Shader.SetGlobalVector(KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthPos, WaterSharedResources.OrthoDepthPosition);
                Shader.SetGlobalVector(KWS_ShaderConstants.OrthoDepth.KWS_OrthoDepthNearFarSize, WaterSharedResources.OrthoDepthNearFarSize);
            }
        }
    }
}