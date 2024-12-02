using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;


namespace KWS
{
    internal static class ReflectionUtils
    {
        private static PlanarReflectionSettingData _settingsData   = new PlanarReflectionSettingData();

        static float m_ClipPlaneOffset = 0.00f;

        public static GameObject CreateReflectionCamera(string name, WaterSystem waterInstance, out Camera reflectionCamera, out Transform reflectionCamTransform)
        {
            var reflCameraGo = KW_Extensions.CreateHiddenGameObject(name);

            reflCameraGo.transform.parent    = WaterSystem.UpdateManagerObject.transform;
            reflectionCamera                 = reflCameraGo.AddComponent<Camera>();
            reflectionCamTransform           = reflCameraGo.transform;
            reflectionCamera.cameraType      = CameraType.Reflection;
            reflectionCamera.allowMSAA       = false;
            reflectionCamera.enabled         = false;
            reflectionCamera.allowHDR        = true;
            reflectionCamera.stereoTargetEye = StereoTargetEyeMask.None;

            return reflCameraGo;
        }

        public static void RenderPlanarReflection(Camera currentCamera, WaterSystem waterInstance, Camera reflectionCamera, Transform reflectionCameraTransform, RenderTexture target, bool useStereo, Camera.StereoscopicEye eye = Camera.StereoscopicEye.Left)
        {
            var     normal          = Vector3.up;
            float   d               = -Vector3.Dot(normal, waterInstance.WaterPivotWorldPosition) - m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            var reflectedCameraPos = currentCamera.GetCameraPositionFast() - new Vector3(0, waterInstance.WaterRelativeWorldPosition.y * 2, 0);
            reflectedCameraPos.y = -reflectedCameraPos.y;

            SetCameraClampedPosRotation(reflectionCameraTransform, reflectedCameraPos, currentCamera.GetCameraRotationFast());

            if (useStereo)
            {
                reflectionCamera.worldToCameraMatrix = currentCamera.GetStereoViewMatrix(eye) * reflection;
                var clipPlane           = CameraSpacePlane(reflectionCamera, waterInstance.WaterPivotWorldPosition, normal, 1.0f);

                var projMatrix = currentCamera.GetStereoProjectionMatrix(eye);
                MakeProjectionMatrixOblique(ref projMatrix, clipPlane);
                reflectionCamera.projectionMatrix = projMatrix;
            }
            else
            {
                reflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * reflection;
                var clipPlane = CameraSpacePlane(reflectionCamera, waterInstance.WaterPivotWorldPosition, normal, 1.0f);

                var projMatrix = currentCamera.projectionMatrix;
                MakeProjectionMatrixOblique(ref projMatrix, clipPlane);
                reflectionCamera.projectionMatrix = projMatrix;
            }
         
            try
            {
                _settingsData.Set(waterInstance);
                reflectionCamera.targetTexture = target;
               KWS_CoreUtils.UniversalCameraRendering(waterInstance, reflectionCamera);
            }
            finally
            {
                _settingsData.Restore();
            }
        }

        class PlanarReflectionSettingData
        {
            private bool _fog;
            private int _maxLod;
            private float _lodBias;
            private int           _pixelLightsCount;
            private ShadowQuality _shadowQuality;

          
            public void Set(WaterSystem waterInstance)
            {
                _fog              = RenderSettings.fog;
                _shadowQuality    = QualitySettings.shadows;
                _pixelLightsCount = QualitySettings.pixelLightCount;
                _maxLod        = QualitySettings.maximumLODLevel;
                _lodBias       = QualitySettings.lodBias;

                GL.invertCulling                = true;
                RenderSettings.fog = false;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.maximumLODLevel = 1;
                QualitySettings.lodBias = _lodBias * 0.5f;

                if (waterInstance.Settings.RenderPlanarShadows == false) QualitySettings.shadows = ShadowQuality.Disable;
            }

            public void Restore()
            {
                GL.invertCulling                = false;
                RenderSettings.fog = _fog;
                QualitySettings.shadows = _shadowQuality;
                QualitySettings.pixelLightCount = _pixelLightsCount;
                QualitySettings.maximumLODLevel = _maxLod;
                QualitySettings.lodBias = _lodBias;
            }
        }

        public static void CopyReflectionParamsFrom(this Camera currentCamera, Camera reflectionCamera, int cullingMask, bool isCubemap)
        {
            //_reflectionCamera.CopyFrom(currentCamera); //this method have 100500 bugs

            reflectionCamera.orthographic = currentCamera.orthographic;
            
            reflectionCamera.farClipPlane = currentCamera.farClipPlane;
            reflectionCamera.nearClipPlane = currentCamera.nearClipPlane;
            reflectionCamera.rect = currentCamera.rect;
            reflectionCamera.renderingPath = currentCamera.renderingPath;

            if (currentCamera.usePhysicalProperties)
            {
                reflectionCamera.usePhysicalProperties = true;
                reflectionCamera.focalLength = currentCamera.focalLength;
                reflectionCamera.sensorSize = currentCamera.sensorSize;
                reflectionCamera.lensShift = currentCamera.lensShift;
                reflectionCamera.gateFit = currentCamera.gateFit;
            }

            reflectionCamera.cullingMask = cullingMask;
            reflectionCamera.depth = currentCamera.depth - 100;

            reflectionCamera.fieldOfView = currentCamera.fieldOfView;
            reflectionCamera.aspect = currentCamera.aspect;
        }

        public static void SetCameraClampedPosRotation(Transform reflectionCameraT, Vector3 pos, Quaternion rotation)
        {
            reflectionCameraT.position = Vector3.ClampMagnitude(pos, float.MaxValue - 100) + Vector3.one * float.Epsilon;

            if (reflectionCameraT.rotation.eulerAngles == Vector3.zero) reflectionCameraT.rotation = Quaternion.Euler(0.000001f, 0.000001f, 0.000001f);
            else reflectionCameraT.rotation = rotation;
        }


        // Given position/normal of the plane, calculates plane in camera space.
        private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3   offsetPos = pos + normal * m_ClipPlaneOffset;
            Matrix4x4 m         = cam.worldToCameraMatrix;
            Vector3   cpos      = m.MultiplyPoint(offsetPos);
            Vector3   cnormal   = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private static void MakeProjectionMatrixOblique(ref Matrix4x4 matrix, Vector4 clipPlane)
        {
            Vector4 q;
            q.x = (Mathf.Sign(clipPlane.x) + matrix[8]) / matrix[0];
            q.y = (Mathf.Sign(clipPlane.y) + matrix[9]) / matrix[5];
            q.z = -1.0F;
            q.w = (1.0F + matrix[10]) / matrix[14];

            Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

            matrix[2]  = c.x;
            matrix[6]  = c.y;
            matrix[10] = c.z + 1.0F;
            matrix[14] = c.w;
        }
    }
}