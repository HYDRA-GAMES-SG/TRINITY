using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    internal class ShorelineWavesPass: WaterPass
    {
        internal override string        PassName => "Water.ShorelineWavesPass";

        private           ComputeBuffer _meshPropertiesBuffer;
        private           ComputeBuffer _argsBuffer;
        private           ComputeBuffer _computeFoamBuffer;
        private           ComputeBuffer _computeBufferSurfaceWaves;

        private Mesh _instancedQuadMesh;
        private Material _waveMaterial;
        private RTHandle _displacementRT, _normalRT;

        private int ID_ShorelineDisplacement = Shader.PropertyToID("KWS_ShorelineDisplacement");
        private int ID_ShorelineNormal = Shader.PropertyToID("KWS_ShorelineNormal");
        private int ID_ShorelineAlpha = Shader.PropertyToID("KWS_ShorelineAlpha");


        private Texture2D _shorelineDisplacementTex;
        private Texture2D _shorelineNormalTex;
        private Texture2D _shorelineAlphaTex;

        List<ShorelineWavesScriptableData.ShorelineWave> _visibleSurfaceWaves = new List<ShorelineWavesScriptableData.ShorelineWave>();
        private Vector3 surfaceWavesAreaPos;

       
        public ShorelineWavesPass()
        {
            _waveMaterial = KWS_CoreUtils.CreateMaterial(KWS_ShaderConstants.ShaderNames.ShorelineBakedWavesShaderName);
            InitializeTextures();
        }


        public override void Release()
        {
            //WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            if (_meshPropertiesBuffer != null) _meshPropertiesBuffer.Release();
            _meshPropertiesBuffer = null;

            if (_argsBuffer != null) _argsBuffer.Release();
            _argsBuffer = null;

            if (_computeFoamBuffer != null) _computeFoamBuffer.Dispose();
            _computeFoamBuffer   = null;

            _computeBufferSurfaceWaves?.Release();
            _computeBufferSurfaceWaves = null;

            _displacementRT?.Release();
            _normalRT?.Release();

            _displacementRT = null;
            _normalRT = null;

            Resources.UnloadAsset(_shorelineDisplacementTex);
            Resources.UnloadAsset(_shorelineNormalTex);
            Resources.UnloadAsset(_shorelineAlphaTex);
            KW_Extensions.SafeDestroy(_instancedQuadMesh, _waveMaterial);
            
            KW_Extensions.WaterLog(this, "", KW_Extensions.WaterLogMessageType.Release);
        }
        void InitializeTextures()
        {
            var size = KWS_Settings.Shoreline.ShorelineWavesTextureResolution;
            if (_displacementRT == null)
            {
                _displacementRT = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, name: "_displacement");
                _normalRT = KWS_CoreUtils.RTHandles.Alloc(size, size, colorFormat: GraphicsFormat.R16G16_SFloat, name: "_norm");
                WaterSharedResources.ShorelineWavesDisplacement = _displacementRT;
                WaterSharedResources.ShorelineWavesNormal = _normalRT;
            }

            if (_shorelineDisplacementTex == null) _shorelineDisplacementTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelinePos);
            if (_shorelineNormalTex == null) _shorelineNormalTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelineNorm);
            if (_shorelineAlphaTex == null) _shorelineAlphaTex = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.ShorelineAlpha);

            Shader.SetGlobalTexture(ID_ShorelineDisplacement, _shorelineDisplacementTex);
            Shader.SetGlobalTexture(ID_ShorelineNormal,       _shorelineNormalTex);
            Shader.SetGlobalTexture(ID_ShorelineAlpha,        _shorelineAlphaTex);

            Shader.SetGlobalTexture(KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesDisplacement, WaterSharedResources.ShorelineWavesDisplacement);
            Shader.SetGlobalTexture(KWS_ShaderConstants.ShorelineID.KWS_ShorelineWavesNormal, WaterSharedResources.ShorelineWavesNormal);

            KW_Extensions.WaterLog("ShorelineWavesPass", _displacementRT, _normalRT);
        }

        void UpdateShorelineBuffer(Camera cam)
        {
            _visibleSurfaceWaves.Clear();

            var areaSize = KWS_Settings.Shoreline.ShorelineWavesAreaSize;

            surfaceWavesAreaPos = KW_Extensions.GetRelativeToCameraAreaPos(cam, areaSize, 0.5f);
            var offset = areaSize * 0.5f * Vector3.one;
            var minAreaSize = surfaceWavesAreaPos - offset;
            var maxAreaSize = surfaceWavesAreaPos + offset;


            foreach (var waterInstance in WaterSharedResources.WaterInstances)
            {
                if (!KWS_CoreUtils.IsWaterVisibleAndActive(waterInstance)
                 || !waterInstance.Settings.UseShorelineRendering
                 //|| !waterInstance.Settings.EnabledMeshRendering
                 || waterInstance.Settings.ShorelineWavesScriptableData == null) continue;

                var currentWaves = waterInstance.Settings.ShorelineWavesScriptableData.Waves;
                for (var waveIdx = 0; waveIdx < currentWaves.Count; waveIdx++)
                {
                    var wave = currentWaves[waveIdx];
                    if (wave.IsWaveInsideWorldArea(minAreaSize, maxAreaSize)) _visibleSurfaceWaves.Add(wave);
                }
            }

            MeshUtils.InitializePropertiesBuffer(_visibleSurfaceWaves, ref _computeBufferSurfaceWaves, false);
        }

        public override void ExecuteCommandBuffer(WaterPass.WaterPassContext waterContext)
        {
            UpdateShorelineBuffer(waterContext.cam);
            if (_computeBufferSurfaceWaves == null || _visibleSurfaceWaves.Count == 0) return;

            if (_instancedQuadMesh == null) _instancedQuadMesh = KWS_CoreUtils.CreateQuad();

            WaterSharedResources.ShorelineAreaPosSize = new Vector4(surfaceWavesAreaPos.x, surfaceWavesAreaPos.y, surfaceWavesAreaPos.z, KWS_Settings.Shoreline.ShorelineWavesAreaSize);
            waterContext.cmd.SetGlobalVector(KWS_ShaderConstants.ShorelineID.KWS_ShorelineAreaPosSize, WaterSharedResources.ShorelineAreaPosSize);
            waterContext.cmd.SetGlobalBuffer(KWS_ShaderConstants.StructuredBuffers.KWS_ShorelineDataBuffer, _computeBufferSurfaceWaves);

            _argsBuffer = MeshUtils.InitializeInstanceArgsBuffer(_instancedQuadMesh, _visibleSurfaceWaves.Count, _argsBuffer, false);

            CoreUtils.SetRenderTarget(waterContext.cmd, KWS_CoreUtils.GetMrt(_displacementRT, _normalRT), _displacementRT, ClearFlag.Color, Color.black);
            waterContext.cmd.DrawMeshInstancedIndirect(_instancedQuadMesh, 0, _waveMaterial, 0, _argsBuffer);
        }


    }
}