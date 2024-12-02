using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWS
{
    [ExecuteAlways]
    [Serializable]
    public class KWS_ExtrudeVolume : MonoBehaviour
    {
        //[SerializeField] public WaterSystemVolumeData VolumeSettings;
        public ExtrudeMaskTypeEnum ExtrudeMaskType       = ExtrudeMaskTypeEnum.Box;
        public float               ExtrudeHeight         = -10;
        public bool                SaveExtrudedSurfaceWaves = true;

        //public Mesh VolumeMesh;

        public enum ExtrudeMaskTypeEnum
        {
            Box,
            //Sphere,
            //CustomMesh
        }

        internal KWS_WaterVolumeVariables.WaterVolumeData _volumeData;
        internal Transform _cachedTransform;
        internal Matrix4x4 _rotationMatrix;

       
        private ComputeBuffer                                  _volumeMaskBufferGPU;
        private List<KWS_WaterVolumeVariables.WaterVolumeData> _volumeMaskBufferCPU = new List<KWS_WaterVolumeVariables.WaterVolumeData>();

        void Awake()
        {
            //LoadOrCreateSettings();
        }

        void OnEnable()
        {
            //LoadOrCreateSettings();
            WaterSharedResources.ExtrudeVolumes.Add(this);
            _cachedTransform = transform;
            UpdateAll();
        }

        void OnDisable()
        {
            WaterSharedResources.ExtrudeVolumes.Remove(this);
            UpdateAll();
        }

        void Update()
        {
            if (_cachedTransform.hasChanged)
            {
                _cachedTransform.hasChanged = false;
                UpdateAll();
            }
        }

        void OnValidate()
        {
            UpdateAll();
        }

        //internal void LoadOrCreateSettings()
        //{
        //    if (VolumeSettings == null) VolumeSettings = ScriptableObject.CreateInstance<WaterSystemVolumeData>();
        //}


        void UpdateAll()
        {
            if (_cachedTransform == null) return;

            var rotationQuaternion = _cachedTransform.rotation;
            _rotationMatrix           = Matrix4x4.Rotate(rotationQuaternion).inverse;
            _rotationMatrix.m03       = 0;
            _rotationMatrix.m13       = 0;
            _rotationMatrix.m23       = 0;
            _rotationMatrix.m30       = 0;
            _rotationMatrix.m31       = 0;
            _rotationMatrix.m32       = 0;
            _rotationMatrix.m33       = 1;

            _volumeData.Position                 = _cachedTransform.position;
            _volumeData.Rotation                 = _rotationMatrix;
            _volumeData.Size                     = _cachedTransform.localScale * 0.5f;
            _volumeData.ExtrudeHeight            = ExtrudeHeight;
            _volumeData.SaveExtrudedSurfaceWaves = SaveExtrudedSurfaceWaves ? 1 : 0;


            UpdateVolumeData();
        }

        void UpdateVolumeData()
        {
            _volumeMaskBufferCPU.Clear();
            foreach (var volumeMask in WaterSharedResources.ExtrudeVolumes)
            {
                if (ExtrudeMaskType == ExtrudeMaskTypeEnum.Box)
                {
                    _volumeMaskBufferCPU.Add(volumeMask._volumeData);
                }
            }

            if (_volumeMaskBufferCPU.Count > 0)
            {
                _volumeMaskBufferGPU = KWS_CoreUtils.GetOrUpdateBuffer<KWS_WaterVolumeVariables.WaterVolumeData>(ref _volumeMaskBufferGPU, _volumeMaskBufferCPU.Count);
                _volumeMaskBufferGPU.SetData(_volumeMaskBufferCPU);

                Shader.SetGlobalBuffer(KWS_ShaderConstants.PrePass.KWS_WaterVolumeDataBuffer, _volumeMaskBufferGPU);
            }
            else if(_volumeMaskBufferGPU != null)
            {
                _volumeMaskBufferGPU.Dispose();
                _volumeMaskBufferGPU = null;
            }
            Shader.SetGlobalInteger(KWS_ShaderConstants.PrePass.KWS_WaterVolumeDataBufferLength, _volumeMaskBufferCPU.Count);

            if (_volumeMaskBufferCPU.Count      > 0) Shader.EnableKeyword(KWS_ShaderConstants.WaterKeywords.KWS_USE_EXTRUDE_MASK);
            else Shader.DisableKeyword(KWS_ShaderConstants.WaterKeywords.KWS_USE_EXTRUDE_MASK);
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }

}
